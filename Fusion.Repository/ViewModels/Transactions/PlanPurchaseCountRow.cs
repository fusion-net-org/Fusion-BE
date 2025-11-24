

namespace Fusion.Repository.ViewModels.Transactions
{
    public class PlanPurchaseCountRow
    {
        public Guid SubscriptionPlanId { get; set; }
        public string PlanName { get; set; } = default!;
        public int TransactionsCount { get; set; }
    }
}
