
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

        public async Task<SubscriptionPlan> CreatePlanAsync(SubscriptionPlan req, CancellationToken cancellationToken = default)
        {
            bool exists = await _context.SubscriptionPlans
                .AnyAsync( p => p.Name == req.Name || p.Code == req.Code , cancellationToken);

            if (exists)
                throw CustomExceptionFactory.CreateBadRequestError(ResponseMessages.EXISTED, "Plan name");

            using var transaction = await _context.Database.BeginTransactionAsync(cancellationToken);
            try
            {
                // feature
                if (req.Features != null)
                {
                    foreach (var f in req.Features)
                    {
                        f.Id = default;     
                        f.PlanId = default; 
                    }
                }

                //3. if has prices
                req.Price.Id = default;          
                req.Price.PlanId = req.Id;

                _context.SubscriptionPlans.Add(req);
                await _context.SaveChangesAsync(cancellationToken);
                await transaction.CommitAsync(cancellationToken);
                return req;
            }
            catch
            {
                await transaction.RollbackAsync(cancellationToken);
                throw;
            }
        }

        public async Task<bool> DeleteAsync(Guid id, CancellationToken ct = default)
        {
            var plan = await GetByIdWithNavAsync(id, ct);
            if (plan is null) return false;

            _context.SubscriptionPlans.Remove(plan);

            await _context.SaveChangesAsync(ct);
            return true;

        }

        public async Task<PagedResult<SubscriptionPlan>> GetAllAsync(SubscriptionPlanPagedRequest request, CancellationToken cancellationToken = default)
        {
            var q = _context.SubscriptionPlans
                     .AsNoTracking()
                     .Include(p => p.Price)    // 1–1
                     .Include(p => p.Features) // 1–N
                     .AsQueryable();

            // --- Filters ---
            if (!string.IsNullOrWhiteSpace(request.Keyword))
            {
                var kw = request.Keyword.Trim();
                var pattern = $"%{kw}%";
                q = q.Where(p =>
                    (p.Code != null && EF.Functions.Like(p.Code, pattern)) ||
                    (p.Name != null && EF.Functions.Like(p.Name, pattern)) ||
                    (p.Description != null && EF.Functions.Like(p.Description, pattern)));
            }

            if (request.IsActive.HasValue)
                q = q.Where(p => p.IsActive == request.IsActive.Value);

            if (request.BillingPeriod.HasValue)
                q = q.Where(p => p.Price != null && p.Price.BillingPeriod == request.BillingPeriod.Value);

            if (request.CreatedAt.From.HasValue)
                q = q.Where(p => p.CreatedAt >= request.CreatedAt.From.Value);

            if (request.CreatedAt.To.HasValue)
                q = q.Where(p => p.CreatedAt <= request.CreatedAt.To.Value);

            return await q.ToPagedResultAsync(request, cancellationToken);
        }

        public Task<SubscriptionPlan?> GetByIdWithNavAsync(Guid id, CancellationToken ct = default)
           => _context.SubscriptionPlans
            .Include(p => p.Price)    // 1–1
            .Include(p => p.Features) // 1–N
            .FirstOrDefaultAsync(p => p.Id == id, ct);

        public async Task<SubscriptionPlan> UpdatePlan(SubscriptionPlan req, CancellationToken cancellationToken = default)
        {
            var plan = await _context.SubscriptionPlans
                           .Include(p => p.Features)
                           .Include(p => p.Price) // 1–1
                           .FirstOrDefaultAsync(p => p.Id == req.Id, cancellationToken);

            if (plan == null)
                throw CustomExceptionFactory.CreateNotFoundError(
                    ResponseMessages.NOT_FOUND.FormatMessage("Subscription Plan"));

            // Chặn trùng Code/Name với plan khác
            bool duplicated = await _context.SubscriptionPlans
                .AnyAsync(p => p.Id != req.Id && (p.Code == req.Code || p.Name == req.Name), cancellationToken);
            if (duplicated)
                throw CustomExceptionFactory.CreateBadRequestError(ResponseMessages.EXISTED, "Plan code/name");

            // Bắt buộc có 1 Price theo nghiệp vụ 1–1
            if (req.Price == null)
                throw CustomExceptionFactory.CreateBadRequestError("Price is required.");

            using var tx = await _context.Database.BeginTransactionAsync(cancellationToken);
            try
            {
                // Update plan fields
                plan.Code = req.Code;
                plan.Name = req.Name;
                plan.Description = req.Description;
                plan.IsActive = req.IsActive;
                plan.UpdatedAt = DateTime.UtcNow;

                // Replace-all features
                if (plan.Features?.Count > 0)
                    _context.SubscriptionPlanFeatures.RemoveRange(plan.Features);

                if (req.Features != null && req.Features.Any())
                {
                    var newFeatures = req.Features.Select(f => new SubscriptionPlanFeature
                    {
                        Id = default,
                        PlanId = plan.Id,
                        FeatureKey = f.FeatureKey,
                        LimitValue = f.LimitValue
                    }).ToList();

                    await _context.SubscriptionPlanFeatures.AddRangeAsync(newFeatures, cancellationToken);
                }

                // Update 1–1 price (in-place)
                if (plan.Price == null)
                {
                    plan.Price = new SubscriptionPlanPrice
                    {
                        Id = default,   // nếu cấu hình NEWID()
                        PlanId = plan.Id
                    };
                }

                plan.Price.BillingPeriod = req.Price.BillingPeriod;
                plan.Price.PeriodCount = req.Price.PeriodCount;
                plan.Price.Price = req.Price.Price;
                plan.Price.Currency = req.Price.Currency;
                plan.Price.RefundWindowDays = req.Price.RefundWindowDays;
                plan.Price.RefundFeePercent = req.Price.RefundFeePercent;

                await _context.SaveChangesAsync(cancellationToken);
                await tx.CommitAsync(cancellationToken);
                return plan;
            }
            catch
            {
                await tx.RollbackAsync(cancellationToken);
                throw;
            }
        }

       
    }
}
