using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Fusion.Service.ViewModels.ProjectMembers.Responses;
using Fusion.Service.ViewModels.Sprint.Responses;
using Microsoft.EntityFrameworkCore;

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
        public string? Code { get; set; }

        public string? Description { get; set; }

        public string? Status { get; set; }


        public string ProjectType { get; set; }


        public Guid CompanyExecutorId { get; set; }
        public string? CompanyExecutorName { get; set; }

        public Guid? CompanyRequestId { get; set; }
        public string? CompanyRequestName { get; set; }

        public DateOnly? StartDate { get; set; }
        public DateOnly? EndDate { get; set; }

        public Guid WorkflowId { get; set; }
        public string? WorkflowName { get; set; }

        public Guid OwnerId { get; set; }
        public string? OwnerName { get; set; }

        public List<ProjectMemberSummaryResponse> Members { get; set; } = new();

        public int MembersCount { get; set; }
        public int SprintCount { get; set; }
        public int TotalTask { get; set; }
        public int TotalPoint { get; set; }

        public double Progress { get; set; } // % tiến độ
        public List<SprintSummaryResponse> Sprints { get; set; }
    }

    public class ProjectResponseVersion3
    {
        public Guid Id { get; set; }
        public Guid? CompanyId { get; set; }
        public bool IsHired { get; set; }
        public Guid? CompanyRequestId { get; set; }
        public Guid? ProjectRequestId { get; set; }
        public string? CompanyRequestName { get; set; }
        public string? CompanyExecutorName { get; set; }
        public string? Code { get; set; }
        public string? Name { get; set; }
        public string? Description { get; set; }
        public string? Status { get; set; }
        public Guid? WorkflowId { get; set; }
        public DateOnly? StartDate { get; set; }
        public DateOnly? EndDate { get; set; }
        public bool IsClosed { get; set; } = false;

        public Guid? ClosedBy { get; set; }
        public Guid? CreatedBy { get; set; }
        public string? CreateByName { get; set; }
        public DateTime CreateAt { get; set; }
        public DateTime UpdateAt { get; set; }
        public decimal? ContractBudget { get; set; }
        public decimal TicketTotalBudget { get; set; }
        public bool IsMaintenance { get; set; } = false;
        public Guid? MaintenanceForProjectId { get; set; }
    }
}
