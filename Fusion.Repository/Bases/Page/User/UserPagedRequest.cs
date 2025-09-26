

namespace Fusion.Repository.Bases.Page.User
{
    public class UserPagedRequest : PagedRequest
    {
        public string? Email { get; set; }
        public string? Company { get; set; }
        public bool? Stauts { get; set; }
    }
}
