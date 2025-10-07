
using Fusion.Repository.Bases.Exceptions;
using Fusion.Repository.Bases.Page;
using Fusion.Repository.Data;
using Fusion.Repository.Entities;
using Fusion.Repository.IRepositories;
using Microsoft.EntityFrameworkCore;

namespace Fusion.Repository.Repositories
{
    public class UserSubscriptionRepository : GenericRepository<UserSubscription>, IUserSubscriptionRepository
    {
        private readonly FusionDbContext _context;
        public UserSubscriptionRepository(FusionDbContext context) : base(context)
        {
            _context = context;
        }

        public async Task DecreaseCompanyQuotaAsync(Guid userId, CancellationToken cancellationToken = default)
        {
            var subscriptions = await _context.UserSubscriptions
                               .Where(x => x.UserId == userId && x.IsActive && x.QuotaCompanyRemaining > 0)
                               .OrderBy(x => x.ExpiryDate)
                               .ToListAsync(cancellationToken);

            if (!subscriptions.Any())
                throw CustomExceptionFactory.CreateBadRequestError("You have no remaining company quota.");

            var targetSub = subscriptions.First();
            targetSub.QuotaCompanyRemaining -= 1;

            _context.UserSubscriptions.Update(targetSub);
            await _context.SaveChangesAsync(cancellationToken);
        }

        public async Task DecreaseProjectQuotaAsync(Guid userId, CancellationToken cancellationToken = default)
        {
            var subscriptions = await _context.UserSubscriptions
                               .Where(x => x.UserId == userId && x.IsActive && x.QuotaProjectRemaining > 0)
                               .OrderBy(x => x.ExpiryDate)
                               .ToListAsync(cancellationToken);

            if (!subscriptions.Any())
                throw CustomExceptionFactory.CreateBadRequestError("You have no remaining project quota.");

            var targetSub = subscriptions.First();
            targetSub.QuotaProjectRemaining -= 1;

            _context.UserSubscriptions.Update(targetSub);
            await _context.SaveChangesAsync(cancellationToken);
        }
        public async Task<PagedResult<UserSubscription>> GetPagedSubscriptionsByUserIdAsync(Guid userId, PagedRequest request, CancellationToken cancellationToken = default)
        {
            var query = _context.UserSubscriptions
                .Include(x => x.SubscriptionPackage)
                .Where(x => x.UserId == userId)
                .OrderByDescending(x => x.PurchaseDate)
                .AsQueryable();

            return await query.ToPagedResultAsync(request, cancellationToken);
        }
    }
}
