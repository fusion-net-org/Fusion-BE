using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Fusion.Service.ViewModels.UserRole.Requests
{
    public class AddUserRoleToCompanyRequest
    {
        public Guid UserId { get; set; }
        public List<int> RoleIds { get; set; } = new();
    }
    public class RemoveUserRoleFromCompanyRequest
    {
        public Guid UserId { get; set; }
        public List<int> RoleIds { get; set; } = new();
    }
}
