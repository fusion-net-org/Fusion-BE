

namespace Fusion.Repository.ViewModels
{
    public class FriendLiteResponse
    {
        public Guid Id {  get; set; }
        public Guid FriendshipId { get; set; }
        public int Status { get; set; }     // 0 Pending | 1 Accepted
        public string? Email { get; set; }
        public string? Avatar { get; set; }
    }

}
