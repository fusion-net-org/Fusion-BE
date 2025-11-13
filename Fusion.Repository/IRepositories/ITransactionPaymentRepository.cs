
//using Fusion.Repository.Bases.Page;
//using Fusion.Repository.Bases.Page.TransactionPayment;
//using Fusion.Repository.Data;
//using Fusion.Repository.Entities;

//namespace Fusion.Repository.IRepositories
//{
//    public interface ITransactionPaymentRepository : IGenericRepository<TransactionPayment>
//    {
//        Task<TransactionPayment?> GetByIdWithNavAsync(Guid id, CancellationToken ct = default);
//        Task<TransactionPayment> CreateAsync(TransactionPayment entity, CancellationToken ct = default);
//        Task<bool> UpdateAsync(TransactionPayment entity, CancellationToken ct = default);
//        Task<bool> DeleteAsync(Guid id, CancellationToken ct = default);

//        Task<TransactionPayment?> GetByOrderCodeAsync(long orderCode, CancellationToken ct = default);
//        Task<TransactionPayment?> GetByPaymentLinkIdAsync(string paymentLinkId, CancellationToken ct = default);
//        Task<bool> ExistsOrderCodeAsync(long orderCode, CancellationToken ct = default);
//        Task<bool> ExistsPaymentLinkIdAsync(string paymentLinkId, CancellationToken ct = default);

//        // Paged list
//        Task<PagedResult<TransactionPayment>> GetPagedAsync(TransactionPaymentPagedRequest request, CancellationToken ct = default);
//    }
//}
