
using Fusion.Repository.Bases.Exceptions;
using Fusion.Repository.Bases.Page;
using Fusion.Repository.Bases.Page.SubscriptionPlans;
using Fusion.Repository.Bases.Responses;
using Fusion.Repository.Data;
using Fusion.Repository.Entities;
using Fusion.Repository.IRepositories;
using Microsoft.EntityFrameworkCore;

namespace Fusion.Repository.Repositories
{
    public class SubscriptionPlanRepository : GenericRepository<SubscriptionPlan>, ISubscriptionPlanRepository
    {
        private readonly FusionDbContext _context;
        public SubscriptionPlanRepository(FusionDbContext context) : base(context)
        {
            _context = context;
        }

        /* ======= Create / Update ======= */

        public async Task<SubscriptionPlan> CreatePlanAsync(SubscriptionPlan req, CancellationToken ct = default)
        {
            // name unique (nên có unique index)
            bool dup
                = await _context.SubscriptionPlans.AnyAsync(x => x.Name == req.Name, ct);
            if (dup)
                throw CustomExceptionFactory.CreateBadRequestError(ResponseMessages.DUPLICATE.FormatMessage("Plan name already exists."));

            if (req.Id == Guid.Empty)
                req.Id = Guid.NewGuid();

            // Bắt buộc có Price do quan hệ 1-1
            if (req.Price == null)
                throw CustomExceptionFactory.CreateBadRequestError(ResponseMessages.INVALID_INPUT.FormatMessage("Price is required."));

            if (req.Price.Id == Guid.Empty)
                req.Price.Id = Guid.NewGuid();
            req.Price.PlanId = req.Id;

            // Features (nếu có)
            if (req.Features != null)
            {
                foreach (var pf in req.Features)
                {
                    if (pf.Id == Guid.Empty)
                        pf.Id = Guid.NewGuid();
                    pf.PlanId = req.Id;
                }
            }

            using var tx = await _context.Database.BeginTransactionAsync(ct);
            try
            {
                _context.SubscriptionPlans.Add(req);

                await _context.SaveChangesAsync(ct);
                await tx.CommitAsync(ct);
                return req;
            }
            catch
            {
                await tx.RollbackAsync(ct);
                throw;
            }
        }
        public async Task<SubscriptionPlan> UpdatePlanAsync(SubscriptionPlan payload, CancellationToken ct = default)
        {
            var plan = await _context.SubscriptionPlans
               .Include(p => p.Price)
               .Include(p => p.Features)
               .FirstOrDefaultAsync(p => p.Id == payload.Id, ct);

            if (plan == null)
                throw CustomExceptionFactory.CreateNotFoundError(ResponseMessages.NOT_FOUND.FormatMessage("Subscription plan not found."));

            bool dup = await _context.SubscriptionPlans
                                     .AnyAsync(x => x.Id != payload.Id && x.Name == payload.Name, ct);

            if (dup)
                throw CustomExceptionFactory.CreateBadRequestError(ResponseMessages.DUPLICATE.FormatMessage("Plan name already exists."));

            plan.Name = payload.Name;
            plan.Description = payload.Description;
            plan.IsActive = payload.IsActive;
            plan.LicenseScope = payload.LicenseScope;
            plan.IsFullPackage = payload.IsFullPackage;
            plan.CompanyShareLimit = payload.CompanyShareLimit;
            plan.SeatsPerCompanyLimit = payload.SeatsPerCompanyLimit;
            plan.UpdatedAt = DateTime.UtcNow;

            using var tx = await _context.Database.BeginTransactionAsync(ct);
            try
            {
                // Price (1-1) là bắt buộc
                if (payload.Price == null)
                    throw new InvalidOperationException("Price is required.");

                if (plan.Price == null)
                {
                    // Thêm mới
                    var pr = payload.Price;
                    if (pr.Id == Guid.Empty) pr.Id = Guid.NewGuid();
                    pr.PlanId = plan.Id;
                    _context.SubscriptionPlanPrices.Add(pr);
                }
                else
                {
                    // Cập nhật field (giữ Id cũ)
                    plan.Price.BillingPeriod = payload.Price.BillingPeriod;
                    plan.Price.PeriodCount = payload.Price.PeriodCount;
                    plan.Price.ChargeUnit = payload.Price.ChargeUnit;
                    plan.Price.Price = payload.Price.Price;
                    plan.Price.Currency = payload.Price.Currency;
                    plan.Price.PaymentMode = payload.Price.PaymentMode;
                    plan.Price.InstallmentCount = payload.Price.InstallmentCount;
                    plan.Price.InstallmentInterval = payload.Price.InstallmentInterval;
                    plan.Price.PlanId = plan.Id;
                }

                // Features: nếu payload.Features được gửi → replace toàn bộ
                if (payload.Features != null)
                {
                    var old = await _context.SubscriptionPlanFeatures
                        .Where(x => x.PlanId == plan.Id)
                        .ToListAsync(ct);
                    _context.SubscriptionPlanFeatures.RemoveRange(old);

                    foreach (var pf in payload.Features)
                    {
                        if (pf.Id == Guid.Empty) pf.Id = Guid.NewGuid();
                        pf.PlanId = plan.Id;
                        _context.SubscriptionPlanFeatures.Add(pf);
                    }
                }

                await _context.SaveChangesAsync(ct);
                await tx.CommitAsync(ct);
                return plan;
            }
            catch
            {
                await tx.RollbackAsync(ct);
                throw;
            }
        }

        public async Task<PagedResult<SubscriptionPlan>> GetAllAsync(SubscriptionPlanPagedRequest request, CancellationToken ct = default)
        {
            var q = _context.SubscriptionPlans
               .AsNoTracking()
               .Include(p => p.Price)
               .Include(p => p.Features).ThenInclude(pf => pf.Feature)
               .AsQueryable();

            // Keyword: name/description
            if (!string.IsNullOrWhiteSpace(request.Keyword))
            {
                var kw = $"%{request.Keyword.Trim()}%";
                q = q.Where(p =>
                    EF.Functions.Like(p.Name, kw) ||
                    (p.Description != null && EF.Functions.Like(p.Description, kw)));
            }

            // IsActive
            if (request.IsActive.HasValue)
                q = q.Where(p => p.IsActive == request.IsActive.Value);

            // BillingPeriod (tồn tại price có period này)
            if (request.BillingPeriod.HasValue)
                q = q.Where(p => p.Price != null && p.Price.BillingPeriod == request.BillingPeriod.Value);

            // Sort
            string sortColumn = nameof(SubscriptionPlan.CreatedAt);
            if (!string.IsNullOrWhiteSpace(request.SortColumn) &&
                SubscriptionPlanPagedRequest.SortMap.TryGetValue(request.SortColumn, out var mapped))
            {
                sortColumn = mapped;
            }

            q = request.SortDescending
                ? q.OrderByDescending(e => EF.Property<object>(e, sortColumn))
                : q.OrderBy(e => EF.Property<object>(e, sortColumn));

            // Page
            return await q.ToPagedResultAsync(request, ct);
        }

        public async Task<List<SubscriptionPlan>> GetAllForCusromerAsync(CancellationToken ct = default)
        {
            return await _context.SubscriptionPlans
              .AsNoTracking()
              .Include(p => p.Price)
              .Include(p => p.Features)
                 .ThenInclude(pf => pf.Feature)
              .Where(p => p.IsActive)
              .OrderBy(p => p.Name)
              .ToListAsync(ct);
        }

        public Task<SubscriptionPlan?> GetByIdWithNavAsync(Guid id, CancellationToken ct = default)
        {
            return _context.SubscriptionPlans
              .AsNoTracking()
              .Include(p => p.Price)
              .Include(p => p.Features)
              .ThenInclude(pf => pf.Feature)
              .FirstOrDefaultAsync(p => p.Id == id, ct);
        }

        public async Task<bool> DeleteAsync(Guid id, CancellationToken ct = default)
        {
            var plan = await _context.SubscriptionPlans
               .Include(p => p.Price)
               .Include(p => p.Features)
               .FirstOrDefaultAsync(p => p.Id == id, ct);

            if (plan == null) return false;

            using var tx = await _context.Database.BeginTransactionAsync(ct);
            try
            {
                if (plan.Price != null)
                    _context.SubscriptionPlanPrices.Remove(plan.Price);

                if (plan.Features?.Count > 0)
                    _context.SubscriptionPlanFeatures.RemoveRange(plan.Features);

                _context.SubscriptionPlans.Remove(plan);

                await _context.SaveChangesAsync(ct);
                await tx.CommitAsync(ct);
                return true;
            }
            catch
            {
                await tx.RollbackAsync(ct);
                throw;
            }
        }

    }
}
