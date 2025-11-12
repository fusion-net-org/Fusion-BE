
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
        public async Task<List<SubscriptionPlan>> GetAllForCusromerAsync(CancellationToken cancellationToken = default)
        {
            var subscriptions = await _context.SubscriptionPlans
                     .AsNoTracking()
                     .Include(p => p.Price)
                     .Include(p => p.Features)
                     .Where(x => x.IsActive && x.Price.Price > 0)
                     .ToListAsync(cancellationToken);
            return subscriptions;   
        }
        public Task<SubscriptionPlan?> GetByIdWithNavAsync(Guid id, CancellationToken ct = default)
           => _context.SubscriptionPlans
            .Include(p => p.Price)    // 1–1
            .Include(p => p.Features) // 1–N
            .FirstOrDefaultAsync(p => p.Id == id, ct);

        public async Task<bool> ExistsCodeOrNameExceptAsync(Guid excludeId, string code, string name, CancellationToken cancellationToken = default)
        {
            return await _context.SubscriptionPlans
                .AsNoTracking()
                .AnyAsync(p => p.Id != excludeId && (p.Code == code || p.Name == name), cancellationToken);
        }
        public async Task<SubscriptionPlan> UpdatePlanAsync(SubscriptionPlan payload, CancellationToken cancellationToken = default)
        {
            var plan = await _context.SubscriptionPlans
                .Include(p => p.Price)
                .Include(p => p.Features)
                .FirstOrDefaultAsync(p => p.Id == payload.Id, cancellationToken);

            if (plan == null)
                throw CustomExceptionFactory.CreateNotFoundError(
                    ResponseMessages.NOT_FOUND.FormatMessage("Subscription Plan"));

            // Check trùng Code/Name
            if (await ExistsCodeOrNameExceptAsync(payload.Id, payload.Code, payload.Name, cancellationToken))
                throw CustomExceptionFactory.CreateBadRequestError(ResponseMessages.EXISTED, "Plan code/name");

            // Bắt buộc có Price
            if (payload.Price == null)
                throw CustomExceptionFactory.CreateBadRequestError("Price is required.");

            using var tx = await _context.Database.BeginTransactionAsync(cancellationToken);
            try
            {
                // 1) Update fields cơ bản
                plan.Code = payload.Code;
                plan.Name = payload.Name;
                plan.Description = payload.Description;
                plan.IsActive = payload.IsActive;
                plan.UpdatedAt = DateTime.UtcNow;

                // 2) Price (1-1)
                if (plan.Price == null)
                {
                    plan.Price = new SubscriptionPlanPrice
                    {
                        Id = Guid.NewGuid(),
                        PlanId = plan.Id
                    };
                }

                plan.Price.BillingPeriod = payload.Price!.BillingPeriod;
                plan.Price.PeriodCount = payload.Price.PeriodCount;
                plan.Price.Price = payload.Price.Price;
                plan.Price.Currency = payload.Price.Currency;
                plan.Price.RefundWindowDays = payload.Price.RefundWindowDays;
                plan.Price.RefundFeePercent = payload.Price.RefundFeePercent;

                // 3) Features — chỉ đồng bộ khi client gửi (null => giữ nguyên)
                if (payload.Features is not null)
                {
                    var existing = (plan.Features ?? new List<SubscriptionPlanFeature>()).ToList();

                    // chuẩn hoá input: loại duplicates theo (Id hoặc FeatureKey)
                    var normalized = payload.Features
                        .GroupBy(f => f.Id != Guid.Empty ? $"ID:{f.Id}" : $"KEY:{f.FeatureKey}")
                        .Select(g => g.First())
                        .ToList();

                    // Build index để tra nhanh
                    var byId = existing.Where(e => e.Id != Guid.Empty).ToDictionary(e => e.Id, e => e);
                    var byKey = existing.GroupBy(e => e.FeatureKey).ToDictionary(g => g.Key, g => g.First());

                    var keepIds = new HashSet<Guid>();            // những existing sẽ được giữ lại
                    var toAdd = new List<SubscriptionPlanFeature>();

                    foreach (var n in normalized)
                    {
                        var isNew = n.Id == Guid.Empty;

                        if (!isNew && byId.TryGetValue(n.Id, out var exById))
                        {
                            // Update theo Id
                            exById.FeatureKey = n.FeatureKey;
                            exById.LimitValue = n.LimitValue;
                            keepIds.Add(exById.Id);
                        }
                        else if (isNew && byKey.TryGetValue(n.FeatureKey, out var exByKey))
                        {
                            // Id rỗng nhưng đã có feature cùng key => coi là UPDATE theo key
                            exByKey.FeatureKey = n.FeatureKey;
                            exByKey.LimitValue = n.LimitValue;
                            keepIds.Add(exByKey.Id);
                        }
                        else
                        {
                            // Thêm mới
                            toAdd.Add(new SubscriptionPlanFeature
                            {
                                Id = Guid.NewGuid(),
                                PlanId = plan.Id,
                                FeatureKey = n.FeatureKey,
                                LimitValue = n.LimitValue
                            });
                        }
                    }

                    // Xoá những cái không nằm trong keepIds (khi client gửi [] rỗng => xoá hết)
                    var toRemove = existing.Where(e => !keepIds.Contains(e.Id) && !toAdd.Any(a => a.FeatureKey == e.FeatureKey)).ToList();
                    if (toRemove.Count > 0)
                        _context.SubscriptionPlanFeatures.RemoveRange(toRemove);

                    if (toAdd.Count > 0)
                        await _context.SubscriptionPlanFeatures.AddRangeAsync(toAdd, cancellationToken);
                }

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
