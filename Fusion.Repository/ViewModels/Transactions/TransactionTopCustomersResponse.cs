

namespace Fusion.Repository.ViewModels.Transactions;

public class TransactionTopCustomerItemResponse
{
    public Guid UserId { get; set; }
    public string? UserName { get; set; }
    public string? Email { get; set; }

    public decimal TotalAmount { get; set; }
    public int SuccessCount { get; set; }
    public decimal MaxPayment { get; set; }
    public DateTimeOffset? LastPaymentAt { get; set; }
}
public class TransactionTopCustomersResponse
{
    public int Year { get; set; }
    public int TopN { get; set; }
    public List<TransactionTopCustomerItemResponse> Items { get; set; } = new();
}