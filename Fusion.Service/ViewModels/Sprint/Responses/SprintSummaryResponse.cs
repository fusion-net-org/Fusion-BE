using Fusion.Service.ViewModels.Task.Response;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Fusion.Service.ViewModels.Sprint.Responses
{
    public class SprintSummaryResponse
    {
        public Guid Id { get; set; }
        public string Name { get; set; }
        public int TaskCount { get; set; }
        public int TotalPoint { get; set; }
        public List<TaskSummaryResponse> Tasks { get; set; } = new();
    }
}
