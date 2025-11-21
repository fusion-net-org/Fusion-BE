
namespace Fusion.Repository.ViewModels.Transactions;

public class TransactionInstallmentAgingBucket
{
    public string BucketKey { get; set; } = default!; // "NotDue", "1-7", "8-14", "15-30", "31-60", "60+"
    public int InstallmentCount { get; set; }
    public decimal OutstandingAmount { get; set; }
}
public class TransactionInstallmentAgingResult
{
    public DateTimeOffset AsOf { get; set; }
    public List<TransactionInstallmentAgingBucket> Items { get; set; } = new();
    public int TotalInstallments { get; set; }
    public decimal TotalOutstandingAmount { get; set; }
}