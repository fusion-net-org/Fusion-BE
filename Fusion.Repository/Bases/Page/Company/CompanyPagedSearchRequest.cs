using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Fusion.Repository.Bases.Page.Company
{
    public class CompanyPagedSearchRequest : PagedRequest
    {
        public string? Name { get; set; }
        public string? TaxCode { get; set; }
    }
}
