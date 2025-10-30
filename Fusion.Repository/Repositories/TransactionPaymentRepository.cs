

using Fusion.Repository.Bases.Page.TransactionPayment;
using Fusion.Repository.Data;
using Fusion.Repository.Entities;
using Fusion.Repository.IRepositories;
using Microsoft.EntityFrameworkCore;

namespace Fusion.Repository.Repositories
{
    public class TransactionPaymentRepository : GenericRepository<TransactionPayment>, ITransactionPaymentRepository
    {
        private readonly FusionDbContext _context;
        public TransactionPaymentRepository(FusionDbContext context) : base(context)
        {
            _context = context;
        }

        public async Task<IEnumerable<TransactionPayment>> GetListPaymentForCurrentUserAsync(Guid id, CancellationToken cancellationToken = default)
        {
            var query = _context.TransactionPayments
                .Include(t => t.User)
                .Include(t => t.SubscriptionPackage)
                .AsQueryable();

            query = query.Where(t => t.UserId == id);

            return await query
                .OrderByDescending(t => t.CreatedAt)
                .ToListAsync(cancellationToken);
        }
        public IQueryable<TransactionPayment> GetListPaymentForAdminQuery(AdminTransactionSearch request)
        {
            var query = _context.TransactionPayments
                .AsNoTracking()
                .Include(t => t.User)
                .Include(t => t.SubscriptionPackage)
                .AsQueryable();

            if (!string.IsNullOrEmpty(request.TransactionCode))
                query = query.Where(t => t.TransactionCode != null &&
                                         t.TransactionCode.Contains(request.TransactionCode));

            if (!string.IsNullOrEmpty(request.PackageName))
                query = query.Where(t => t.SubscriptionPackage.Name != null &&
                                         t.SubscriptionPackage.Name.Contains(request.PackageName));

            if (request.PaymentDateFrom.HasValue)
                query = query.Where(t => t.CreatedAt >= request.PaymentDateFrom.Value);
            if (request.PaymentDateTo.HasValue)
                query = query.Where(t => t.CreatedAt <= request.PaymentDateTo.Value);

            if (request.AmountMin.HasValue)
                query = query.Where(t => t.Amount >= request.AmountMin.Value);
            if (request.AmountMax.HasValue)
                query = query.Where(t => t.Amount <= request.AmountMax.Value);

            if (!string.IsNullOrEmpty(request.Status))
                query = query.Where(t => t.Status != null && t.Status == request.Status);

            
            return query;
        }

        public async Task<TransactionPayment?> GetLasterTransactionForUserAsync(Guid id, CancellationToken cancellationToken = default)
        {
            var transaction = await _context.TransactionPayments
                              .Where(t => t.UserId == id)
                              .OrderByDescending(t => t.CreatedAt)
                              .FirstOrDefaultAsync(cancellationToken);

            return transaction!;

        }
    }
}
