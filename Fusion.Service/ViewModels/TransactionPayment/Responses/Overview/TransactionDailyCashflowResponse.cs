

namespace Fusion.Service.ViewModels.TransactionPayment.Responses.Overview
{
    public class DailyCashflowItem
    {
        public DateOnly Date { get; set; }
        public decimal Revenue { get; set; }     
        public int SuccessCount { get; set; }    
    }
    public class TransactionDailyCashflowResponse
    {
        public DateOnly From { get; set; }
        public DateOnly To { get; set; }

        public List<DailyCashflowItem> Items { get; set; } = new();
    }
}
