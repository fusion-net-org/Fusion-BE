using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Fusion.Repository.ViewModels.Project
{
    public sealed class ProjectProgressResponse
    {
        public Guid ProjectId { get; set; }
        public int TotalTasks { get; set; }
        public int DoneTasks { get; set; }
        public double ProgressPercent { get; set; } // 0..100
    }
    public sealed class ProjectTaskProgressVm
    {
        public int TotalTasks { get; set; }
        public int DoneTasks { get; set; }
    }
}
