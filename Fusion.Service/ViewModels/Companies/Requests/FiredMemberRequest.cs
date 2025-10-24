using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Fusion.Service.ViewModels.Companies.Requests
{
    public class FiredMemberRequest
    {
        public string? FiredMemberMail { get; set; }
        public string? Reason { get; set; }  
        public Guid CompanyId { get; set; }
    }
}
