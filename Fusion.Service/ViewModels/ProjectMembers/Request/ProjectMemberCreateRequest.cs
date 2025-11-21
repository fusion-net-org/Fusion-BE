using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Fusion.Service.ViewModels.ProjectMembers.Request
{
    public class ProjectMemberCreateRequest
    {
        public Guid ProjectId { get; set; }
        public Guid CompanyId { get; set; }     // để check user có thuộc company không
        public Guid UserId { get; set; }

        public bool IsPartner { get; set; } = false;
        public bool IsViewAll { get; set; } = false;
    }
}
