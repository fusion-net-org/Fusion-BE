
using Fusion.Repository.Bases.Page;
using Fusion.Repository.Bases.Page.TransactionPayment;
using Fusion.Repository.Data;
using Fusion.Repository.Entities;
using Fusion.Repository.ViewModels.SubscriptionPlan;
using Fusion.Repository.ViewModels.Transactions;

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
    Task<TransactionPaymentPagedRepoResult> GetPagedAsync(TransactionPaymentPagedRequest request, CancellationToken ct = default);

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

    Task<int> AttachSubscriptionToInstallmentBatchAsync(Guid userId, Guid planId, Guid userSubscriptionId, CancellationToken ct = default);
    Task<TransactionPayment?> FindEarliestPendingInstallmentAsync(Guid userId, Guid planId, Guid? userSubscriptionId, CancellationToken ct = default);

    // OVerView
    #region Transaction
    Task<List<TransactionMonthlyRevenueRepoItem>> GetMonthlyRevenueAsync(int year, CancellationToken ct = default);
    Task<TransactionMonthlyStatusRepoResult> GetMonthlyStatusAsync( int year,CancellationToken ct = default);
    Task<List<DailyCashflowAgg>> GetDailyCashflowAggAsync(DateTimeOffset from, DateTimeOffset toExclusive, CancellationToken ct = default);
    Task<TransactionInstallmentAgingResult> GetInstallmentAgingAsync( DateTimeOffset? asOf = null, CancellationToken ct = default);
    Task<List<TransactionTopCustomerItemResponse>> GetTopCustomersAsync(int year, int topN, CancellationToken ct = default);
    Task<decimal> GetTotalRevenueAsync(CancellationToken ct = default);
    Task<List<TransactionMonthlyAmountPoint>> GetMonthlyAmountInYearAsync(int year,CancellationToken ct = default);
    #endregion

    #region SubsciptionPlan
    Task<List<TransactionPaymentModeInsightItemDto>> GetPaymentModeInsightAsync(int year,CancellationToken ct = default);
    Task<List<TransactionPlanRevenueInsightItem>> GetPlanRevenueInsightAsync(int year,CancellationToken ct);
    Task<List<PlanPurchaseCountRow>> GetPlanPurchaseCountsAsync(CancellationToken ct = default);
    Task<List<SubscriptionPlanPurchaseRow>> GetSubscriptionPlanPurchaseStatsAsync(CancellationToken ct = default);
    Task<List<PlanMonthlyPurchaseCountRow>> GetPlanMonthlyPurchaseCountsAsync(int year,CancellationToken ct = default);
    #endregion

}

