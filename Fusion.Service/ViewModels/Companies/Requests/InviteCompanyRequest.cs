using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Fusion.Service.ViewModels.Companies.Requests
{
    public class InviteCompanyRequest
    {
        public Guid CompanyBID { get; set; }

        [DefaultValue(null)]
        public string? Note { get; set; }
    }
}
