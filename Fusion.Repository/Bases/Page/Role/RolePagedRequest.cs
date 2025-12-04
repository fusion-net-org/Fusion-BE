using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Fusion.Repository.Bases.Page.Role
{
    public class RolePagedRequest : PagedRequest
    {
        public Guid? CompanyId { get; set; }
        public string? Keyword { get; set; }
        public string? Status { get; set; }
        public DateTime? CreatedAtFrom { get; set; }
        public DateTime? CreatedAtTo { get; set; }
    }
}
