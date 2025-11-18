using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Fusion.Repository.Enums;

namespace Fusion.Service.ViewModels.Sprint.Responses
{
    public class SprintChartDto
    {
        public string SprintName { get; set; } = null!;
        public int EstimatedHours { get; set; }
        public int RemainingHours { get; set; }
        public int TodoCount { get; set; }
        public int InProgressCount { get; set; }
        public int DoneCount { get; set; }
        public int Review { get; set; }

    }
    public class SprintStatusDistributionDto
    {
        public SprintStatus Status { get; set; }
        public int Count { get; set; }
    }

    public class SprintChartsVm
    {
        public List<SprintStatusDistributionDto> StatusDistribution { get; set; } = new();
        public List<SprintChartDto> SprintWorkload { get; set; } = new();
    }
}
