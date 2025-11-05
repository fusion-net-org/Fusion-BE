

using Fusion.Repository.Bases.Exceptions;
using Fusion.Repository.Bases.Responses;
using Fusion.Repository.Data;
using Fusion.Repository.Entities;
using Fusion.Repository.IRepositories;
using Microsoft.EntityFrameworkCore;

namespace Fusion.Repository.Repositories
{
    public class SubscriptionRepository : GenericRepository<SubscriptionPlan>, ISubscriptionRepository
    {
        private readonly FusionDbContext _context;
        public SubscriptionRepository(FusionDbContext context) : base(context)
        {
            _context = context;
        }
        public async Task<bool> ExistsByNameAsync(string name, CancellationToken cancellationToken = default)
            => await _context.SubscriptionPackages
                             .AnyAsync(p => p.Name == name, cancellationToken);
      
    }
}
