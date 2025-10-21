using Fusion.Repository.Entities;
using Fusion.Service.ViewModels.Projects.Responses;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Fusion.Service.ViewModels.Companies.Responses
{
    public class CompanyResponse
    {
        public Guid Id { get; set; }

        public string? Name { get; set; }

        public Guid? OwnerUserId { get; set; }

        public string? OwnerUserName { get; set; }

        public string? OwnerUserAvatar { get; set; }

        public string? TaxCode { get; set; }

        public string? PhoneNumber { get; set; }

        public string? Address { get; set; }

        public string? Website { get; set; }

        public string? Email { get; set; }

        public string? Detail { get; set; }

        public string? ImageCompany { get; set; }

        public string? AvatarCompany { get; set; }

        public DateTime CreateAt { get; set; }

        public DateTime UpdateAt { get; set; }

        public bool? IsDeleted { get; set; }

        public int? TotalMember {  get; set; }

        public int? TotalProject {  get; set; }

        public int? TotalPartners { get; set; }

        public int? TotalApproved { get; set; }

        public int? TotalWaitForApprove { get; set; }


        public ICollection<CompanyMemberResponse>? ListMembers { get; set; }
        public ICollection<ProjectResponse>? ListProjects { get; set; }
    }

    public class CompanyResponseVersion2
    {
        public Guid Id { get; set; }

        public string? Name { get; set; }

        public Guid? OwnerUserId { get; set; }

        public string? OwnerUserName { get; set; }

        public string? OwnerUserAvatar { get; set; }

        public string? TaxCode { get; set; }

        public string? PhoneNumber { get; set; }

        public string? Address { get; set; }

        public string? Website { get; set; }

        public string? Email { get; set; }

        public string? Detail { get; set; }

        public string? ImageCompany { get; set; }

        public string? AvatarCompany { get; set; }

        public bool? isOwner { get; set; }
        public bool? isPartner { get; set; }
        public bool? isPendingAprovePartner { get; set; }

        public DateTime CreateAt { get; set; }

        public DateTime UpdateAt { get; set; }

        public bool? IsDeleted { get; set; }


        public int? TotalMember { get; set; }
        public int? TotalProject { get; set; }
        public int? TotalPartners { get; set; }
        public int? TotalApproved { get; set; }
        public int? TotalWaitForApprove { get; set; }
    }
}
