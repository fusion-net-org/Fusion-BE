using Fusion.Repository.Bases.Page;
using Fusion.Repository.Entities;


namespace Fusion.Repository.ViewModels.Transactions;

public class TransactionPaymentPagedRepoResult : PagedResult<TransactionPayment>
{
    public decimal TotalRevenue { get; set; }
    public int TotalSuccess { get; set; }
    public int TotalFailed { get; set; }
    public int TotalPending { get; set; }
    public int TotalTransactions => TotalCount;

}
