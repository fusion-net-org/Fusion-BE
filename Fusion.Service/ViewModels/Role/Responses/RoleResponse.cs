using Fusion.Service.ViewModels.Permission.Responses;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Fusion.Service.ViewModels.Role.Responses
{
    public class RoleResponse
    {
        public int RoleId { get; set; }
        public string RoleName { get; set; } = string.Empty;
        public List<PermissionResponse> Permissions { get; set; } = new();
    }

    public class CompanyRoleSummaryResponse
    {
        public int RoleId { get; set; }
        public string RoleName { get; set; } = string.Empty;
        public int TotalMembers { get; set; }
    }


    public class RoleResponseVersion2
    {
        public int Id { get; set; }
        public Guid? CompanyId { get; set; }
        public string RoleName { get; set; }
        public string? Description { get; set; }
        public string Status { get; set; }
        public DateTime CreatedAt { get; set; }
    }

}
