using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Fusion.Repository.Bases.Page;

namespace Fusion.Repository.Bases.Page.Company_Member
{
    public class CompanyMemberPagedSearchRequest : PagedRequest
    {
        public string? KeyWord { get; set; }

        public DateRange<DateOnly>? DateRange { get; set; }

    }

    public class CompanyMemberPagedSearchAdminRequest : PagedRequest
    {
        public string? MemberName { get; set; }

        public string? CompanyName { get; set; }

        public DateRange<DateOnly>? DateRange { get; set; }

    }
}
