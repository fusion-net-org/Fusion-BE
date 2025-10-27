

namespace Fusion.Repository.Bases.Page.User
{
    public class AdminUserPagedSearch : PagedRequest
    {
        public string? Email { get; set; }
        public string? Company { get; set; }
        public bool? Stauts { get; set; }
    }
}
