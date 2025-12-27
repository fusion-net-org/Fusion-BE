

using Fusion.Repository.Bases.Page;
using Fusion.Repository.Bases.Page.TransactionPayment;
using Fusion.Repository.ViewModels.SubscriptionPlan;
using Fusion.Repository.ViewModels.Transactions;
using Fusion.Service.Commons.BaseResponses;
using Fusion.Service.ViewModels.SubscriptionPlan.Responses;
using Fusion.Service.ViewModels.TransactionPayment.Requests;
using Fusion.Service.ViewModels.TransactionPayment.Responses;
using Fusion.Service.ViewModels.TransactionPayment.Responses.Overview;

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
    Task<TransactionPaymentPagedSummaryResponse> GetPagedAsync(TransactionPaymentPagedRequest request, CancellationToken ct = default);
    Task<TransactionPaymentPagedSummaryResponse> GetPagedByUserIdAsync(TransactionPaymentUserPagedRequest request, CancellationToken ct = default);

    // === Liệt kê các draft đến hạn để phát hành link (scheduler sử dụng) ===
    Task<List<TransactionPaymentResponse>> GetDueAsync(DateTimeOffset asOf, int take = 100, CancellationToken ct = default);

    Task<TransactionPaymentDetailResponse?> FindEarliestPendingInstallmentAsync(Guid planId, Guid? userSubscriptionId = null, CancellationToken ct = default);

    // =================================OVERVIEW ==================================
    #region Transaction
    Task<TransactionMonthlyRevenueResponse> GetMonthlyRevenueAsync(int? year,CancellationToken ct = default);
    Task<TransactionMonthlyRevenueThreeYearsResponse> GetMonthlyRevenueThreeYearsAsync(int? year,CancellationToken ct = default);
    Task<TransactionMonthlyStatusResponse> GetMonthlyStatusAsync(int year, CancellationToken ct = default);
    Task<TransactionDailyCashflowResponse> GetDailyCashflowAsync(int lastDays,CancellationToken ct = default);
    Task<TransactionInstallmentAgingResponse> GetInstallmentAgingAsync(DateTimeOffset? asOf,CancellationToken ct = default);
    Task<TransactionTopCustomersResponse> GetTopCustomersAsync( int year, int topN,CancellationToken ct = default);
    #endregion

    #region SubsciptionPlan
    Task<TransactionPaymentModeInsightResponse> GetPaymentModeInsightAsync(int year,CancellationToken ct = default);
    Task<TransactionPlanRevenueInsightResponse> GetPlanRevenueInsightAsync(int year, CancellationToken ct = default);
    Task<List<SubscriptionPlanPurchaseStatResponse>> GetPlanPurchaseStatsAsync(CancellationToken ct = default);
    Task<List<SubscriptionPlanPurchaseStatResponse>> GetTopPlanPurchaseStatsAsync( int top = 3, bool includeOther = true,CancellationToken ct = default);
    Task<List<PlanMonthlyPurchaseCountRow>> GetPlanMonthlyPurchaseStatsAsync( int year,CancellationToken ct = default);
    #endregion
}
