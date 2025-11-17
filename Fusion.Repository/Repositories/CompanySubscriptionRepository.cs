

using Fusion.Repository.Bases.Exceptions;
using Fusion.Repository.Data;
using Fusion.Repository.Entities;
using Fusion.Repository.Enums;
using Fusion.Repository.IRepositories;
using Microsoft.EntityFrameworkCore;

namespace Fusion.Repository.Repositories
{
    public class CompanySubscriptionRepository : GenericRepository<CompanySubscription>, ICompanySubscriptionRepository
    {
        private readonly FusionDbContext _context;
        private readonly IUserSubscriptionRepository _userSubscriptionRepository;
        public CompanySubscriptionRepository(FusionDbContext context, IUserSubscriptionRepository userSubscriptionRepository) : base(context)
        {
            _context = context;
            _userSubscriptionRepository = userSubscriptionRepository;
        }

        public async Task<CompanySubscription> CreateAsync(CompanySubscription companySubscription, CancellationToken cancellationToken = default)
        {
            // 1. Load UserSubscription + Entitlements
            var userSub = await _userSubscriptionRepository
                .GetByIdWithNavAsync(companySubscription.UserSubscriptionId, cancellationToken);

            if (userSub == null)
                throw CustomExceptionFactory.CreateNotFoundError("User subscription not found");

            if (userSub.Status != SubscriptionStatus.Active)
                throw CustomExceptionFactory.CreateBadRequestError("User subscription is not active.");

            if (userSub.TermEnd.HasValue && userSub.TermEnd.Value < DateTimeOffset.UtcNow)
                throw CustomExceptionFactory.CreateBadRequestError("User subscription has expired.");

            // 2. Check share limit
            if (userSub.CompanyShareLimitSnapshot.HasValue)
            {
                if (userSub.CompanyShareLimitSnapshot.Value <= 0)
                    throw CustomExceptionFactory.CreateBadRequestError("No remaining company shares for this subscription.");
            }

            // 3. Kiểm tra share trùng
            var exists = await _context.CompanySubscriptions
                .AnyAsync(cs =>
                    cs.UserSubscriptionId == companySubscription.UserSubscriptionId &&
                    cs.CompanyId == companySubscription.CompanyId,
                    cancellationToken);

            if (exists)
                throw CustomExceptionFactory.CreateBadRequestError("This subscription has already been shared to the specified company.");

            using var transaction = await _context.Database.BeginTransactionAsync(cancellationToken);
            try
            {
                // 4. Trừ hạn mức share nếu có
                if (userSub.CompanyShareLimitSnapshot.HasValue)
                {
                    userSub.CompanyShareLimitSnapshot -= 1;
                    _context.UserSubscriptions.Update(userSub);
                }

                // 5. Hoàn thiện entity CompanySubscription
                companySubscription.Status = SubscriptionStatus.Active;
                companySubscription.SharedOn = DateTimeOffset.UtcNow;
                companySubscription.UpdatedAt = DateTimeOffset.UtcNow;

                // nếu OwnerUserId chưa set (Guid.Empty) thì lấy từ userSub
                if (companySubscription.OwnerUserId == Guid.Empty)
                {
                    companySubscription.OwnerUserId = userSub.UserId;
                }

                companySubscription.SeatsLimitSnapshot = userSub.SeatsPerCompanyLimitSnapshot;
                // SeatsLimitUnit hiện chưa dùng, để null hoặc bỏ property khỏi entity

                await _context.CompanySubscriptions.AddAsync(companySubscription, cancellationToken);

                // 6. Copy entitlements từ UserSubscription -> CompanySubscription
                var userEntitlements = userSub.Entitlements
                    .Where(e => e.Enabled)
                    .ToList();

                if (userEntitlements.Count > 0)
                {
                    var companyEntitlements = userEntitlements.Select(e => new CompanySubscriptionEntitlement
                    {
                        CompanySubscriptionId = companySubscription.Id,
                        FeatureId = e.FeatureId,
                        Enabled = true
                    }).ToList();

                    await _context.CompanySubscriptionEntitlements
                        .AddRangeAsync(companyEntitlements, cancellationToken);
                }

                // 7. Save + Commit
                await _context.SaveChangesAsync(cancellationToken);
                await transaction.CommitAsync(cancellationToken);

                return companySubscription;
            }
            catch
            {
                await transaction.RollbackAsync(cancellationToken);
                throw;
            }
        }

        public async Task<CompanySubscription?> GetByIdWithNavAsync(Guid id, CancellationToken ct = default)
            => await _context.CompanySubscriptions
              .Include(cs => cs.Company)
                .Include(cs => cs.UserSubscription)
                    .ThenInclude(us => us.Plan)
                .Include(cs => cs.Entitlements)
                    .ThenInclude(e => e.Feature)
                .FirstOrDefaultAsync(cs => cs.Id == id, ct);

    }
}
