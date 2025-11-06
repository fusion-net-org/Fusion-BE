using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Fusion.Service.ViewModels.ProjectMembers.Responses
{
    public class ProjectMemberSummaryResponse
    {
        public Guid MemberId { get; set; }
        public string MemberName { get; set; }
        public string Avatar { get; set; }
    }
}
