

namespace Fusion.Service.ViewModels.TransactionPayment.Responses;

public class TransactionPaymentDetailResponse : TransactionPaymentResponse
{
    // Bank / counter account info
    public string? AccountNumber { get; set; }           // stk receiving
    public string? CounterAccountBankId { get; set; }
    public string? CounterAccountBankName { get; set; }
    public string? CounterAccountName { get; set; }
    public string? CounterAccountNumber { get; set; }
}
