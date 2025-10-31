
namespace Fusion.Service.ViewModels.TransactionPayment.Responses
{
    public class TransactionStatusCountsResponse
    {
        public int Cancel { get; init; }
        public int Pending { get; init; }
        public int Success { get; init; }
    }
}
