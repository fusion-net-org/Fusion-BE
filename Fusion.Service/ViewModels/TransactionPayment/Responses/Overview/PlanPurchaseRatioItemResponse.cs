using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Fusion.Service.ViewModels.TransactionPayment.Responses.Overview
{
    public class PlanPurchaseRatioItemResponse
    {
        public Guid SubscriptionPlanId { get; set; }
        public string PlanName { get; set; } = default!;
        public int TransactionsCount { get; set; }
        public decimal Percentage { get; set; }
        public bool IsOther { get; set; }
    }
}
