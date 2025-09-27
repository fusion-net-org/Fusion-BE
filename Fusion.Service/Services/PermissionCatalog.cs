using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Fusion.Service.Services
{
    public static class PermissionCatalog
    {
        public const string MemberAssignRole = "Member.AssignRole";
        public const string ProjectView = "Project.View";
        public static readonly (int Id, string Code)[] Seed = new[] {
        (1102, MemberAssignRole),
        (2001, ProjectView)
    };
    }
}
