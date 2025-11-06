using Fusion.Service.ViewModels.ProjectMembers.Responses;
using Fusion.Service.ViewModels.Sprint.Responses;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Fusion.Service.ViewModels.Project.Responses
{
    public class ProjectSummaryResponse
    {
        public Guid Id { get; set; }
        public string Name { get; set; }
        public int SprintCount { get; set; }
        public int TotalTask { get; set; }
        public int TotalPoint { get; set; }
        public List<SprintSummaryResponse> Sprints { get; set; } = new();
    }

    public class ProjectSummaryResponseV2
    {
        public Guid Id { get; set; }
        public string Name { get; set; }

        public string ProjectType { get; set; }


        public Guid CompanyId { get; set; }
        public string? CompanyName { get; set; }


        public Guid? CompanyHiredId { get; set; }
        public string? CompanyHiredName { get; set; }


        public Guid WorkflowId { get; set; }
        public string? WorkflowName { get; set; }

        public Guid OwnerId { get; set; }
        public string? OwnerName { get; set; }

        public List<ProjectMemberSummaryResponse> Members { get; set; } = new();

        public int SprintCount { get; set; }
        public int TotalTask { get; set; }
        public int TotalPoint { get; set; }

        public double Progress { get; set; } // % tiến độ
        public List<SprintSummaryResponse> Sprints { get; set; }
    }
}
