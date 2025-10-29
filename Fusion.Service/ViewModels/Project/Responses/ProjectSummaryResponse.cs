using Fusion.Service.ViewModels.Sprint.Responses;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Fusion.Service.ViewModels.Project.Responses
{
    public class ProjectSummaryResponse
    {
        public Guid Id { get; set; }
        public string Name { get; set; }
        public int SprintCount { get; set; }
        public int TotalTask { get; set; }
        public int TotalPoint { get; set; }
        public List<SprintSummaryResponse> Sprints { get; set; } = new();
    }
}
