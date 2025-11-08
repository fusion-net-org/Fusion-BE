
using Fusion.Repository.Bases.Exceptions;
using Fusion.Repository.Bases.Page;
using Fusion.Repository.Bases.Page.UserSubscriptions;
using Fusion.Repository.Bases.Responses;
using Fusion.Repository.Data;
using Fusion.Repository.Entities;
using Fusion.Repository.Enums;
using Fusion.Repository.IRepositories;
using Microsoft.EntityFrameworkCore;

namespace Fusion.Repository.Repositories
{
    public class UserSubscriptionRepository : GenericRepository<UserSubscription>, IUserSubscriptionRepository
    {
        private readonly FusionDbContext _context;
        private readonly ITransactionPaymentRepository _transactionPaymentRepository;
        private readonly ISubscriptionPlanRepository _subscriptionPlanRepository;
        public UserSubscriptionRepository(FusionDbContext context, ITransactionPaymentRepository transactionPaymentRepository, ISubscriptionPlanRepository subscriptionPlanRepository) : base(context)
        {
            _context = context;
            _transactionPaymentRepository = transactionPaymentRepository;
            _subscriptionPlanRepository = subscriptionPlanRepository;
        }


        public Task<UserSubscription?> GetByIdWithNavAsync(Guid id, CancellationToken ct = default)
            => _context.UserSubscriptions
               .Include(u => u.UserSubscriptionEntitlements)
               .Include(x => x.TransactionPayment)
                       .ThenInclude(x => x.SubscriptionPlan)
               .FirstOrDefaultAsync(u => u.Id == id, ct);

        public async Task<UserSubscription> CreateAsync(UserSubscription userSubscription, CancellationToken cancellationToken = default)
        {
            var transaction = await _transactionPaymentRepository.GetByIdWithNavAsync(userSubscription.TransactionId);
            if (transaction == null)
                throw CustomExceptionFactory.CreateNotFoundError(ResponseMessages.NOT_FOUND.FormatMessage("Transaction"));

            var plan = await _subscriptionPlanRepository.GetByIdWithNavAsync(transaction.PlanId);
            if (plan == null)
                throw CustomExceptionFactory.CreateNotFoundError(ResponseMessages.NOT_FOUND.FormatMessage("subscription plan"));

            if (plan.Price == null)
                throw CustomExceptionFactory.CreateBadRequestError(ResponseMessages.BAD_REQUEST.FormatMessage("plan has no pricing info"));

            // calculator expried
            var now = DateTime.UtcNow;
            var expiredAt = plan.Price.BillingPeriod switch
            {
                BillingPeriod.Week => now.AddDays(7 * plan.Price.PeriodCount),
                BillingPeriod.Month => now.AddMonths(plan.Price.PeriodCount),
                BillingPeriod.Year => now.AddYears(plan.Price.PeriodCount),
                _ => now
            };

            userSubscription.NamePlan = plan.Name;
            userSubscription.Price = plan.Price.Price;
            userSubscription.Currency = plan.Price.Currency;
            userSubscription.Status = SubscriptionStatus.Active;
            userSubscription.CreatAt = DateTime.UtcNow;
            userSubscription.ExpiredAt = expiredAt;
            userSubscription.UpdateAt = null;

            // create list eentilement 
            if (plan.Features != null && plan.Features.Any())
            {
                userSubscription.UserSubscriptionEntitlements = plan.Features.Select(f => new UserSubscriptionEntitlement
                {
                    FeatureKey = f.FeatureKey,
                    Quantity = f.LimitValue,
                    Remaining = f.LimitValue
                }).ToList();
            }
            else
            {
                userSubscription.UserSubscriptionEntitlements = new List<UserSubscriptionEntitlement>();
            }

            // save
            await _context.UserSubscriptions.AddAsync(userSubscription, cancellationToken);
            //await _context.SaveChangesAsync(cancellationToken);

            return userSubscription;

        }

        public async Task<bool> Delete(Guid id, CancellationToken ct = default)
        {
            var userSubscription = await GetByIdWithNavAsync(id);
            if (userSubscription == null)
                return false;

            _context.UserSubscriptions.Remove(userSubscription);
            await _context.SaveChangesAsync(ct);

            return true;
        }

        public async Task<PagedResult<UserSubscription>> GetAllAsync(UserSubscriptionPagedRequest request, CancellationToken cancellationToken = default)
        {
            var q = _context.UserSubscriptions
                .AsNoTracking()
                .Include(us => us.UserSubscriptionEntitlements)
                .Include(us => us.TransactionPayment)
                .AsQueryable();

            if (!string.IsNullOrWhiteSpace(request.PlanName))
            {
                var pattern = $"%{request.PlanName.Trim()}%";
                q = q.Where(us => us.NamePlan != null && EF.Functions.Like(us.NamePlan, pattern));
            }

            // Trạng thái
            if (request.status.HasValue)
                q = q.Where(us => us.Status == request.status.Value);

            // Khoảng thời gian tạo
            if (request.CreateAt.From.HasValue)
                q = q.Where(us => us.CreatAt >= request.CreateAt.From.Value);

            if (request.CreateAt.To.HasValue)
                q = q.Where(us => us.CreatAt <= request.CreateAt.To.Value);

            // Khoảng thời gian hết hạn
            if (request.ExpiredAt.From.HasValue)
                q = q.Where(us => us.ExpiredAt >= request.ExpiredAt.From.Value);

            if (request.ExpiredAt.To.HasValue)
                q = q.Where(us => us.ExpiredAt <= request.ExpiredAt.To.Value);

            // Keyword tổng hợp
            if (!string.IsNullOrWhiteSpace(request.Keyword))
            {
                var kw = request.Keyword.Trim();
                var pattern = $"%{kw}%";
                q = q.Where(us =>
                    (us.NamePlan != null && EF.Functions.Like(us.NamePlan, pattern)) ||
                    (us.Currency != null && EF.Functions.Like(us.Currency, pattern)) ||
                    (us.Status.ToString() != null && EF.Functions.Like(us.Status.ToString(), pattern))
                );
            }

            // --- Sorting (nếu client truyền SortColumn, ưu tiên SortMap) ---
            if (string.IsNullOrWhiteSpace(request.SortColumn))
            {
                request.SortColumn = nameof(UserSubscription.CreatAt);
                request.SortDescending = true;
            }

            // --- Paging ---
            return await q.ToPagedResultAsync(request, cancellationToken);
        }

        public async Task<UserSubscription> UpdateAsync(Guid userId, UserSubscription userSubscription, CancellationToken cancellationToken = default)
        {
            var user = await _context.Users
                .AsNoTracking()
                .FirstOrDefaultAsync(x => x.Id == userId, cancellationToken);

            var transaction = await _context.TransactionPayments
                .AsNoTracking()
                .FirstOrDefaultAsync(x => x.Id == userSubscription.TransactionId, cancellationToken);

            if (!user.IsSystemAdmin)
            {
                if (user.Id != transaction.UserId)
                    throw CustomExceptionFactory.CreateForbiddenError();
            }

            var existing = await _context.UserSubscriptions
                          .Include(us => us.UserSubscriptionEntitlements)
                          .FirstOrDefaultAsync(us => us.Id == userSubscription.Id, cancellationToken);

            if (existing == null)
                throw CustomExceptionFactory.CreateNotFoundError(
                    ResponseMessages.NOT_FOUND.FormatMessage("User subscription"));

            // --- Cập nhật thông tin cơ bản ---
            existing.NamePlan = userSubscription.NamePlan ?? existing.NamePlan;
            existing.Price = userSubscription.Price;
            existing.Currency = userSubscription.Currency ?? existing.Currency;
            existing.Status = userSubscription.Status;
            existing.ExpiredAt = userSubscription.ExpiredAt;
            existing.UpdateAt = DateTime.UtcNow;

            // --- Cập nhật entitlements ---
            if (userSubscription.UserSubscriptionEntitlements != null && userSubscription.UserSubscriptionEntitlements.Any())
            {
                // Xóa entitlement cũ
                _context.UserSubscriptionEntitlements.RemoveRange(existing.UserSubscriptionEntitlements ?? []);

                // Add entitlement mới
                foreach (var ent in userSubscription.UserSubscriptionEntitlements)
                {
                    ent.UserSubscriptionId = existing.Id;
                    await _context.UserSubscriptionEntitlements.AddAsync(ent, cancellationToken);
                }
            }

            await _context.SaveChangesAsync(cancellationToken);
            return existing;
        }

        public async Task<UserSubscription> UpdateStatusAsync(Guid id, Guid userId, SubscriptionStatus status, CancellationToken cancellationToken = default)
        {
            var user = await _context.Users
                       .AsNoTracking()
                       .FirstOrDefaultAsync(x => x.Id == userId, cancellationToken);

            var userSubscription = await GetByIdWithNavAsync(id);

            var transaction = await _context.TransactionPayments
                .AsNoTracking()
                .FirstOrDefaultAsync(x => x.Id == userSubscription.TransactionId, cancellationToken);

            if (!user.IsSystemAdmin)
            {
                if (user.Id != transaction.UserId)
                    throw CustomExceptionFactory.CreateForbiddenError();
            }


            if (userSubscription == null)
                throw CustomExceptionFactory.CreateNotFoundError(ResponseMessages.NOT_FOUND.FormatMessage("Transaction"));

            userSubscription.Status = status;
            userSubscription.UpdateAt = DateTime.UtcNow;

            await _context.SaveChangesAsync(cancellationToken);

            return userSubscription;
        }

        public async Task<PagedResult<UserSubscription>> GetAllByUserIdAsync(Guid userId, UserSubscriptionPagedRequest request, CancellationToken cancellationToken = default)
        {
            var q = _context.UserSubscriptions
                    .AsNoTracking()
                    .Include(us => us.UserSubscriptionEntitlements)
                    .Include(us => us.TransactionPayment)
                    .Where(us => us.TransactionPayment.UserId == userId)
                    .AsQueryable();

            if (!string.IsNullOrWhiteSpace(request.PlanName))
            {
                var pattern = $"%{request.PlanName.Trim()}%";
                q = q.Where(us => us.NamePlan != null && EF.Functions.Like(us.NamePlan, pattern));
            }

            // Trạng thái
            if (request.status.HasValue)
                q = q.Where(us => us.Status == request.status.Value);

            // Khoảng thời gian tạo
            if (request.CreateAt.From.HasValue)
                q = q.Where(us => us.CreatAt >= request.CreateAt.From.Value);

            if (request.CreateAt.To.HasValue)
                q = q.Where(us => us.CreatAt <= request.CreateAt.To.Value);

            // Khoảng thời gian hết hạn
            if (request.ExpiredAt.From.HasValue)
                q = q.Where(us => us.ExpiredAt >= request.ExpiredAt.From.Value);

            if (request.ExpiredAt.To.HasValue)
                q = q.Where(us => us.ExpiredAt <= request.ExpiredAt.To.Value);

            // Keyword tổng hợp
            if (!string.IsNullOrWhiteSpace(request.Keyword))
            {
                var kw = request.Keyword.Trim();
                var pattern = $"%{kw}%";
                q = q.Where(us =>
                    (us.NamePlan != null && EF.Functions.Like(us.NamePlan, pattern)) ||
                    (us.Currency != null && EF.Functions.Like(us.Currency, pattern)) ||
                    (us.Status.ToString() != null && EF.Functions.Like(us.Status.ToString(), pattern))
                );
            }

            // --- Sorting (nếu client truyền SortColumn, ưu tiên SortMap) ---
            if (string.IsNullOrWhiteSpace(request.SortColumn))
            {
                request.SortColumn = nameof(UserSubscription.CreatAt);
                request.SortDescending = true;
            }

            // --- Paging ---
            return await q.ToPagedResultAsync(request, cancellationToken);
        }

        public async Task ValidateAndConsumeEntitlementsAsync(Guid userSubscriptionId, IEnumerable<CompanySubscriptionEntitlement> requestedEntitlements, CancellationToken cancellationToken = default)
        {
            if(requestedEntitlements == null || !requestedEntitlements.Any())
                return;

            // 1 Get userSubscription with entitlement
            var userSub = await _context.UserSubscriptions
                .Include(x => x.UserSubscriptionEntitlements)
                .FirstOrDefaultAsync(u => u.Id == userSubscriptionId, cancellationToken);

            if (userSub == null)
                throw CustomExceptionFactory.CreateNotFoundError(ResponseMessages.NOT_FOUND.FormatMessage("User subscription"));

            if(userSub.Status != SubscriptionStatus.Active)
                throw CustomExceptionFactory.CreateBadRequestError(ResponseMessages.BAD_REQUEST.FormatMessage("User subscription not active"));

            //2 validate each feature
            foreach (var ent in requestedEntitlements)
            {
                var userEnt = userSub.UserSubscriptionEntitlements
                    .FirstOrDefault(x => x.FeatureKey == ent.FeatureKey);

                if(userEnt == null)
                    throw CustomExceptionFactory.CreateNotFoundError(ResponseMessages.NOT_FOUND.FormatMessage($"Feature {ent.FeatureKey} not found in user subscription"));

                if (ent.Quantity > userEnt.Remaining)
                    throw CustomExceptionFactory.CreateBadRequestError(ResponseMessages.BAD_REQUEST.FormatMessage($"Not enough quota for feature {ent.FeatureKey}"));
            }

            foreach( var ent in requestedEntitlements)
            {
                var userEnt = userSub.UserSubscriptionEntitlements
                    .First(x => x.FeatureKey == ent.FeatureKey);
                userEnt.Remaining -= ent.Quantity;
            }
            _context.UserSubscriptionEntitlements.UpdateRange(userSub.UserSubscriptionEntitlements);
        }
    }
}
