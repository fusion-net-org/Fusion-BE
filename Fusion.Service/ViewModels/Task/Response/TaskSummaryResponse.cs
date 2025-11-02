using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Fusion.Service.ViewModels.Task.Response
{
    public class TaskSummaryResponse
    {
        public Guid Id { get; set; }
        public string? Title { get; set; }
        public int? Point { get; set; }
    }
}
