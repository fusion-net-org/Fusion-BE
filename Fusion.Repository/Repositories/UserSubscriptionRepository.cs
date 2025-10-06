
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

        public async Task<int> GetAllQuotaComapnyRemainingHasActiveAsync(Guid userId, CancellationToken cancellationToken = default)
            => await _context.UserSubscriptions
            .Where(x => x.UserId == userId && x.IsActive)
            .SumAsync(x => x.QuotaProjectRemaining, cancellationToken);

        public async Task<int> GetAllQuotaProjectRemainingHasActiveAsync(Guid userId, CancellationToken cancellationToken = default)
           => await _context.UserSubscriptions
            .Where(x => x.UserId == userId && x.IsActive)
            .SumAsync(x => x.QuotaCompanyRemaining, cancellationToken);

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
