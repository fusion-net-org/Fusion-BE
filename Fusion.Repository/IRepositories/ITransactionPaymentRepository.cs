
using Fusion.Repository.Bases.Page;
using Fusion.Repository.Bases.Page.TransactionPayment;
using Fusion.Repository.Data;
using Fusion.Repository.Entities;
using Fusion.Repository.ViewModels;

namespace Fusion.Repository.IRepositories;

public interface ITransactionPaymentRepository : IGenericRepository<TransactionPayment>
{
    // Gets
    Task<TransactionPayment?> GetByIdWithNavAsync(Guid id, CancellationToken ct = default);
    Task<TransactionPayment?> GetByOrderCodeAsync(long orderCode, CancellationToken ct = default);
    Task<TransactionPayment?> GetByPaymentLinkIdAsync(string paymentLinkId, CancellationToken ct = default);
    Task<bool> ExistsOrderCodeAsync(long orderCode, CancellationToken ct = default);
    Task<bool> ExistsPaymentLinkIdAsync(string paymentLinkId, CancellationToken ct = default);
    Task<bool> LinkToSubscriptionAsync(Guid transactionId, Guid userSubscriptionId, CancellationToken ct = default);
    // Paged
    Task<PagedResult<TransactionPayment>> GetPagedAsync(TransactionPaymentPagedRequest request, CancellationToken ct = default);

    // Create
    Task<TransactionPayment> CreateDraftChargeAsync(TransactionPayment draft, CancellationToken ct = default);
    // Lưu các kỳ còn lại (cùng 1 plan) cho trả góp
    Task<int> BulkCreateAsync(IEnumerable<TransactionPayment> rows, CancellationToken ct = default);

    Task<TransactionPayment?> FindNextPendingInstallmentAsync(
    Guid userId, Guid planId, Guid? userSubscriptionId, int currentInstallmentIndex, CancellationToken ct = default);
    // Transitions (no refund)
    Task<bool> AttachPaymentLinkAsync(Guid id, long orderCode, string paymentLinkId, string? provider, CancellationToken ct = default);
    Task<bool> MarkSuccessAsync(Guid id, decimal? amount, DateTimeOffset paidAt, string? reference, CancellationToken ct = default);
    Task<bool> MarkFailedAsync(Guid id, string? description, string? reference, CancellationToken ct = default);

    // Scheduled queries
    Task<List<TransactionPayment>> GetDueAsync(DateTimeOffset asOf, int take = 100, CancellationToken ct = default);

    Task<int> AttachSubscriptionToInstallmentBatchAsync( Guid userId, Guid planId, Guid userSubscriptionId,CancellationToken ct = default);

    /// <summary>
    /// Lấy transaction installment tiếp theo cần thanh toán:
    /// transaction có Status = Pending, nhỏ nhất InstallmentIndex (và DueAt).
    /// </summary>
    Task<TransactionPayment?> FindEarliestPendingInstallmentAsync( Guid userId,Guid planId,Guid? userSubscriptionId, CancellationToken ct = default);
}
