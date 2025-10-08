
using Fusion.Repository.Bases.Page.TransactionPayment;
using Fusion.Repository.Data;
using Fusion.Repository.Entities;

namespace Fusion.Repository.IRepositories
{
    public interface ITransactionPaymentRepository : IGenericRepository<TransactionPayment>
    {
        Task<IEnumerable<TransactionPayment>> GetListPaymentForAdminAsync(AdminTransactionSearch request, CancellationToken cancellationToken = default);
        Task<IEnumerable<TransactionPayment>> GetListPaymentForCurrentUserAsync(Guid id, CancellationToken cancellationToken = default);
    }
}
