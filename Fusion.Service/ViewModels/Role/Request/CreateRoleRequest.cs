using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Fusion.Service.ViewModels.Role.Request
{
    public class CreateRoleRequest
    {
        public Guid? CompanyId { get; set; }
        public string RoleName { get; set; }
        public string? Description { get; set; }
    }

}
