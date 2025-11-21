using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Fusion.Service.ViewModels.ProjectMembers.Request
{
    public class ProjectMemberRoleResponse
    {
        public Guid ProjectId { get; set; }
        public Guid UserId { get; set; }

        public string? UserName { get; set; }
        public string? Email { get; set; }
        public string? AvatarUrl { get; set; }

        public string? CompanyRoleName { get; set; } // <- role trong company

        public bool IsPartner { get; set; }
        public bool IsViewAll { get; set; }
        public DateTime JoinedAt { get; set; }
    }
}
