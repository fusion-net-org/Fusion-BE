

using System.Runtime.Serialization;

namespace Fusion.Repository.Enums;

public enum SubscriptionStatus
{
    [EnumMember(Value = "active")]
    Active = 1,

    [EnumMember(Value = "inactive")]
    Inactive = 2,

    [EnumMember(Value = "refunded")]
    Refunded = 3,

    [EnumMember(Value = "expired")]
    Expired = 4
}
public enum PaymentStatus
{
    [EnumMember(Value = "pending")]
    Pending = 1,

    [EnumMember(Value = "success")]
    Success = 2,

    [EnumMember(Value = "failed")]
    Failed = 3,

    [EnumMember(Value = "refunded")]
    Refunded = 4,

    [EnumMember(Value = "cancelled")]
    Cancelled = 5
}

public enum BillingPeriod
{
    [EnumMember(Value = "week")]
    Week = 1,

    [EnumMember(Value = "month")]
    Month = 2,

    [EnumMember(Value = "year")]
    Year = 3
}

public enum FeatureKeys
{
    [EnumMember(Value = "project")]
    Project = 1,

    [EnumMember(Value = "company")]
    Company = 2,
    [EnumMember(Value = "share")]
    Share = 3,
}