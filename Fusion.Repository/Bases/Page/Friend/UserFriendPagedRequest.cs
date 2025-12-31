

namespace Fusion.Repository.Bases.Page.Friend
{
    public class UserFriendPagedRequest : PagedRequest
    {
        public string? Email { get; set; }    
        public int? Status { get; set; }
    }
}
