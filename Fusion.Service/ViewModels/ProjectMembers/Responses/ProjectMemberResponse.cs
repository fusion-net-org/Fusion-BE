using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Fusion.Service.ViewModels.ProjectMembers.Responses
{
    public class ProjectBelongToMemberResponse
    {
        public Guid Id { get; set; }
        public string Name { get; set; }
        public string? Code { get; set; }
        public string? Status { get; set; }
        public bool IsHired { get; set; }
        public DateOnly? StartDate { get; set; }
        public DateOnly? EndDate { get; set; }
    }

    public class MemberProjectListResponse
    {
        public Guid CompanyId { get; set; }

        public Guid UserId { get; set; }

        public int TotalProject { get; set; }

        public List<ProjectBelongToMemberResponse> Projects { get; set; } = new();
    }
    public class ProjectMemberResponseV2
    {
        public long Id { get; set; }
        public Guid? UserId { get; set; }
        public string? UserName { get; set; }
        public string? Email { get; set; }
        public string? Phone {  get; set; }
        public string? Avatar { get; set; }
        public string? Status { get; set; }
        public string? Gender { get; set; }
        public bool IsPartner { get; set; }
        public bool IsViewAll { get; set; }
        public DateTime JoinedAt { get; set; }
    }
}
