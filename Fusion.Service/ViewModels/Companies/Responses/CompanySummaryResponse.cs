using Fusion.Service.ViewModels.Project.Responses;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Fusion.Service.ViewModels.Companies.Responses
{
    public class CompanySummaryResponse
    {
        public Guid CompanyId { get; set; }
        public int TotalProject { get; set; }
        public int TotalSprint { get; set; }
        public int TotalTask { get; set; }
        public int TotalPoint { get; set; }
        public List<ProjectSummaryResponse> Projects { get; set; } = new();
    }
}
