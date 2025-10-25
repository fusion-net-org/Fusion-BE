using Fusion.Service.ViewModels.Role.Responses;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Fusion.Service.ViewModels.UserRole.Responses
{
    public class AddMemberRoleInCompanyResponse
    {
        public Guid UserId { get; set; }
        public string UserName { get; set; } = string.Empty;
        public List<RoleResponse> Roles { get; set; } = new();
    }
}
