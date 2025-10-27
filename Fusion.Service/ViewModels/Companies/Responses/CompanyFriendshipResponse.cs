using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Fusion.Service.ViewModels.Companies.Responses
{
    public class CompanyFriendshipResponse
    {
        public long Id { get; set; }
        public Guid? CompanyAId { get; set; }
        public Guid? CompanyBId { get; set; }
        public Guid? RequesterId { get; set; }
        public string? Status { get; set; }
        public string? Note { get; set; }
        public DateTime? RespondedAt { get; set; }
        public Guid? LastActionBy { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }
        public int? TotalProject { get; set; }
        public int? TotalMember { get; set; }
    }

    public class PartnerResponse 
    {
        public Guid? CompanyId { get; set; }

        public string? Name { get; set; }

        public string? TaxCode { get; set; }

        public string? OwnerUserName { get; set; }

        public DateTime? RespondedAt { get; set; }

        public DateTime CreatedAt { get; set; }


        public int? TotalProject { get; set; }

    }

}
