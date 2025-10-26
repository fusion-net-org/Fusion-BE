using Fusion.Repository.Bases.Page;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Fusion.Service.ViewModels.CompanyActivityLog.Requests
{
    public class CompanyActivityLogQuery : PagedRequest
    {
        public string? Keyword { get; set; }
        public DateOnly? From { get; set; }
        public DateOnly? To { get; set; }
    }
}
