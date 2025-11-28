using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Fusion.Service.ViewModels.Companies.Requests
{
    public class CompanyTaskStatsRequest
    {
        public Guid PartnerCompanyId { get; set; }
        public Guid MyCompanyId { get; set; }
    }
}
