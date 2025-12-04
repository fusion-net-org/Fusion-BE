using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Fusion.Service.ViewModels.Role.Request
{
    public class UpdateRoleRequest
    {
        public string RoleName { get; set; }
        public string? Description { get; set; }
    }
    public class DeleteRoleRequest
    {
        public string Reason { get; set; }
    }

}
