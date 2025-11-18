
using Fusion.Repository.Enums;
using System.Text.Json.Serialization;

namespace Fusion.Service.ViewModels.TransactionPayment.Responses;

public class TransactionPaymentResponse
{
    public Guid Id { get; set; }

    public Guid UserId { get; set; }
    public string? UserName { get; set; }         // map from tp.User.UserName (Include User)

    public Guid PlanId { get; set; }
    public string? PlanName { get; set; }         // map from tp.SubscriptionPlan.Name (Include Plan)

    public long? OrderCode { get; set; }
    public string? PaymentLinkId { get; set; }
    public string? Provider { get; set; }         // PayOS / VNPAY / Momo...
    public string? PaymentMethod { get; set; }    // Bank / Wallet...

    public decimal Amount { get; set; }
    public string Currency { get; set; } = "VND";

    public DateTimeOffset CreatedAt { get; set; }
    public DateTimeOffset? TransactionDateTime { get; set; }
    public DateTimeOffset? DueAt { get; set; }    // kỳ đến hạn (installments)
    public DateTimeOffset? PaidAt { get; set; }

    [JsonConverter(typeof(JsonStringEnumConverter))]
    public PaymentStatus Status { get; set; }

    [JsonConverter(typeof(JsonStringEnumConverter))]
    public TransactionType Type { get; set; }

    // Installments helper
    public int? InstallmentIndex { get; set; }    // 1..N
    public int? InstallmentTotal { get; set; }

    // Snapshot ngắn gọn để render nhãn
    [JsonConverter(typeof(JsonStringEnumConverter))]
    public ChargeUnit ChargeUnitSnapshot { get; set; }

    [JsonConverter(typeof(JsonStringEnumConverter))]
    public BillingPeriod BillingPeriodSnapshot { get; set; }

    public int PeriodCountSnapshot { get; set; }

    [JsonConverter(typeof(JsonStringEnumConverter))]
    public PaymentMode PaymentModeSnapshot { get; set; }
}
