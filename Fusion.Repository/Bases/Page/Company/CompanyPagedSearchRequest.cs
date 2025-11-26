using Fusion.Repository.Entities;
using Fusion.Repository.Enums;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Fusion.Repository.Bases.Page.Company
{
    public class CompanyPagedSearchRequest : PagedRequest
    {
        public string? Keyword { get; set; }
        public string? OwnerUserName { get; set; }
        public string? Detail { get; set; }
        public int? TotalProject { get; set; }
        public int? TotalMember { get; set; }
        public ProjectSearchRelationShipEnums? RelationShipEnums { get; set; }
    }
    public class CompanyPagedSearchRequestVersion2 : PagedRequest
    {
        public string? Keyword { get; set; }
        public string? OwnerUserName { get; set; }
        public ProjectSearchRelationShipEnums? RelationShipEnums { get; set; }

        public DateTime? DayFrom { get; set; }
        public DateTime? DayTo { get; set; }   
    }
}
