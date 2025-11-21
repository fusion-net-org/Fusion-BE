using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Fusion.Repository.ViewModels.SubscriptionPlan
{
    public class TransactionPlanRevenueInsightItem
    {
        public Guid PlanId { get; set; }
        public string PlanName { get; set; } = "";

        public int TransactionCount { get; set; }
        public int SuccessCount { get; set; }

        /// <summary>
        /// Tổng tiền của các giao dịch thành công (Paid) trong năm.
        /// </summary>
        public decimal TotalAmount { get; set; }
    }

    public class TransactionPlanRevenueInsightResponse
    {
        public int Year { get; set; }
        public List<TransactionPlanRevenueInsightItem> Items { get; set; } = new();
    }
}
