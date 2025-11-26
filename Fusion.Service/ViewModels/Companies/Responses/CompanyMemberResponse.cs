using Fusion.Repository.ViewModels;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Fusion.Service.ViewModels.Companies.Responses
{
    public class CompanyMemberResponse
    {
        public long Id { get; set; }

        public Guid? CompanyId { get; set; }

        public string? CompanyName { get; set; }

        public Guid? MemberId { get; set; }

        public string? MemberName { get; set; }

        public string? MemberAvatar {  get; set; }
        public string? Email { get; set; }
        public string? Phone {  get; set; }
        public string? Gender { get; set; }

        public string? MemberPhoneNumber { get; set; }
        public string? roleName { get; set; }

        public int? NumberProductJoin { get; set; } = 0; //Sum of project internal and hired project of member in that company

        public int? NumberCompanyJoin { get; set; } = 0;

        public string? Status { get; set; } //Hien dang o trong cong ty hay khong.

        public int Productivity { get; set; }
        public int Communication { get; set; }
        public int Teamwork { get; set; }
        public int ProblemSolving { get; set; }

        public int Score { get; set; }
        public int HoursPerWeek { get; set; }

        public EfficiencyChart? Efficiency { get; set; }
        public PieChart? PriorityDistribution { get; set; }
        public LineChart? ScoreTrendChart { get; set; }

        public bool? IsDeleted { get; set; }

        public DateTime JoinedAt { get; set; }

        public bool IsOwner { get; set; }
    }

    public class CompanyMemberResponseV2
    {
        public long Id { get; set; }

        // Company Info
        public Guid? CompanyId { get; set; }
        public string? CompanyName { get; set; }
        public string? CompanyEmail { get; set; }
        public string? CompanyOwner { get; set; }
        public string? CompanyAvatar { get; set; }

        public string? CompanyPhone { get; set; }
        public string? CompanyAddress { get; set; }
        public DateTime CompanyCreateAt { get; set; }

        // Member Info (From CompanyMember)
        public Guid? UserId { get; set; }
        public DateTime MemberJoinAt { get; set; }
        public string? Status { get; set; }

        // User 
        public string? UserName { get; set; }
        public string? UserEmail { get; set; }
        public string? UserPhone { get; set; }
        public string? UserAvatar { get; set; }
    }
}
