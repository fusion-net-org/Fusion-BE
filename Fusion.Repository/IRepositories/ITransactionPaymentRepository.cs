
using Fusion.Repository.Bases.Page.TransactionPayment;
using Fusion.Repository.Data;
using Fusion.Repository.Entities;

namespace Fusion.Repository.IRepositories
{
    public interface ITransactionPaymentRepository : IGenericRepository<TransactionPayment>
    {
        IQueryable<TransactionPayment> GetListPaymentForAdminQuery(AdminTransactionSearch request);
        Task<IEnumerable<TransactionPayment>> GetListPaymentForCurrentUserAsync(Guid id, CancellationToken cancellationToken = default);
        Task<TransactionPayment?> GetLasterTransactionForUserAsync(Guid id, CancellationToken cancellationToken = default);
        Task<decimal> GetTotalRevenueSuccessAsync(CancellationToken cancellationToken = default);
        Task<(int Cancel, int Pending, int Success)> CountTransactionByStatusAsync(CancellationToken cancellationToken = default);
    }
}
