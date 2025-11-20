

using System.Runtime.Serialization;

namespace Fusion.Repository.Enums;

/// <summary>
/// Phạm vi cấp phép của gói
/// </summary>
public enum LicenseScope
{
    [EnumMember(Value = "user_limits")]
    Userlimits = 1,

    [EnumMember(Value = "entire_company")]
    EntireCompany = 2

}

/// <summary>
/// Kỳ hạn billing cho Price (áp dụng PeriodCount x BillingPeriod)
/// </summary>
/// 
public enum BillingPeriod
{
    [EnumMember(Value = "week")]
     Week = 1,

   [EnumMember(Value = "month")]
    Month = 2,

    [EnumMember(Value = "year")]
    Year = 3,
}


/// <summary>
/// Đơn vị tính phí
/// </summary>
public enum ChargeUnit
{
    [EnumMember(Value = "per_subscription")]
    PerSubscription = 1,

    //[EnumMember(Value = "per_seat")]
    //PerSeat = 2
}


/// <summary>
/// Phương thức thanh toán của Price
/// </summary>
public enum PaymentMode
{
    [EnumMember(Value = "prepaid")]
    Prepaid = 1,

    [EnumMember(Value = "installments")]
    Installments = 2
}

/// <summary>
/// Trạng thái của Subscription (khác với PaymentStatus)
/// </summary>
public enum SubscriptionStatus
{
    [EnumMember(Value = "pending")]
    Pending = 0,          // chờ kích hoạt / chờ thanh toán đầu

    [EnumMember(Value = "active")]
    Active = 1,

    [EnumMember(Value = "paused")]
    Paused = 2,           // tạm dừng (vi phạm, admin pause, v.v.)

    [EnumMember(Value = "canceled")]
    Canceled = 3,         // người dùng/hệ thống hủy trước hạn

    [EnumMember(Value = "expired")]
    Expired = 4           // quá hạn
                        
}

/// <summary>
/// Trạng thái của giao dịch thanh toán
/// </summary>
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

public enum TransactionType
{
    /// <summary>Thu tiền (mua mới, gia hạn, từng kỳ trả góp)</summary>
    [EnumMember(Value = "charge")]
    Charge = 1,

    /// <summary>Hoàn tiền (toàn phần hoặc một phần)</summary>
    [EnumMember(Value = "refund")]
    Refund = 2,

    /// <summary>Điều chỉnh sổ sách (giảm trừ/khuyến mãi/bù trừ thủ công…)</summary>
    [EnumMember(Value = "adjustment")]
    Adjustment = 3 
}

