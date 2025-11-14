

using Fusion.Repository.Bases.Page;
using Fusion.Repository.Bases.Page.TransactionPayment;
using Fusion.Service.ViewModels.TransactionPayment.Requests;
using Fusion.Service.ViewModels.TransactionPayment.Responses;

namespace Fusion.Service.IServices;

public interface ITransactionPaymentService
{
    // === Create checkout (sinh draft kỳ 1; nếu installments thì sinh cả N kỳ) ===
    Task<TransactionPaymentDetailResponse> CreateAsync(TransactionPaymentCreateRequest req, CancellationToken ct = default);

    // === Đính link thanh toán cho một draft (kỳ 1 hoặc kỳ đến hạn) ===
    Task<bool> AttachPaymentLinkAsync(Guid id, long orderCode, string paymentLinkId, string? provider, CancellationToken ct = default);

    // === Đánh dấu kết quả thanh toán ===
    Task<bool> MarkSuccessAsync(Guid id, decimal? amount, DateTimeOffset paidAt, string? reference, CancellationToken ct = default);
    Task<bool> MarkFailedAsync(Guid id, string? description, string? reference, CancellationToken ct = default);

    // === Đọc dữ liệu ===
    Task<TransactionPaymentDetailResponse?> GetDetailAsync(Guid id, CancellationToken ct = default);
    Task<PagedResult<TransactionPaymentResponse>> GetPagedAsync(TransactionPaymentPagedRequest request, CancellationToken ct = default);

    // === Liệt kê các draft đến hạn để phát hành link (scheduler sử dụng) ===
    Task<List<TransactionPaymentResponse>> GetDueAsync(DateTimeOffset asOf, int take = 100, CancellationToken ct = default);

}
