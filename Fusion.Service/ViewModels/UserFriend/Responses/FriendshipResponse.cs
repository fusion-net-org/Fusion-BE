namespace Fusion.Service.ViewModels.UserFriend.Responses;

public class FriendshipResponse
{
    public Guid Id { get; set; }

    public Guid RequesterId { get; set; }
    public Guid AddresseeId { get; set; }

    public int Status { get; set; } // 0 Pending | 1 Accepted | 2 Rejected

    public DateTime? RequestedAt { get; set; }
    public DateTime? RespondedAt { get; set; }
}

public class FriendshipResponseV2
{
    public Guid Id { get; set; }

    public Guid RequesterId { get; set; }
    public string? RequesterName { get; set; }
    public string? RequesterEmail { get; set; }

    public Guid AddresseeId { get; set; }

    public int Status { get; set; } // 0 Pending | 1 Accepted | 2 Rejected

    public DateTime? RequestedAt { get; set; }
    public DateTime? RespondedAt { get; set; }
}
