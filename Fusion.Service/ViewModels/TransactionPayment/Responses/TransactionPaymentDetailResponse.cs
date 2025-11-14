

using Fusion.Repository.Enums;
using System.Text.Json.Serialization;

namespace Fusion.Service.ViewModels.TransactionPayment.Responses;

public class TransactionPaymentDetailResponse
{
    public Guid Id { get; set; }

    // Navs
    public Guid UserId { get; set; }
    public string? UserName { get; set; }

    public Guid PlanId { get; set; }
    public string? PlanName { get; set; }

    // Payment link / gateway
    public long? OrderCode { get; set; }
    public string? PaymentLinkId { get; set; }
    public string? Provider { get; set; }
    public string? PaymentMethod { get; set; }

    // Amount & currency
    public decimal Amount { get; set; }
    public string Currency { get; set; } = "VND";

    // Descriptions / references
    public string? Description { get; set; }
    public string? Reference { get; set; }

    // Timestamps
    public DateTimeOffset CreatedAt { get; set; }
    public DateTimeOffset? TransactionDateTime { get; set; }
    public DateTimeOffset? DueAt { get; set; }
    public DateTimeOffset? PaidAt { get; set; }

    // Status & type
    [JsonConverter(typeof(JsonStringEnumConverter))]
    public PaymentStatus Status { get; set; }

    [JsonConverter(typeof(JsonStringEnumConverter))]
    public TransactionType Type { get; set; }

    // Installments meta
    public int? InstallmentIndex { get; set; }
    public int? InstallmentTotal { get; set; }

    // Snapshot pricing tại thời điểm charge
    [JsonConverter(typeof(JsonStringEnumConverter))]
    public ChargeUnit ChargeUnitSnapshot { get; set; }

    [JsonConverter(typeof(JsonStringEnumConverter))]
    public BillingPeriod BillingPeriodSnapshot { get; set; }

    public int PeriodCountSnapshot { get; set; }

    [JsonConverter(typeof(JsonStringEnumConverter))]
    public PaymentMode PaymentModeSnapshot { get; set; }

    // Bank / counter account info
    public string? AccountNumber { get; set; }           // stk receiving
    public string? CounterAccountBankId { get; set; }
    public string? CounterAccountBankName { get; set; }
    public string? CounterAccountName { get; set; }
    public string? CounterAccountNumber { get; set; }
}
