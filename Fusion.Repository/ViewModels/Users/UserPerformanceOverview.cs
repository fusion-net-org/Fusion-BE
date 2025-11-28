using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Fusion.Repository.ViewModels.Users
{
    public class UserPerformanceOverview
    {
        public int TotalTasksAssigned { get; set; }
        public int TotalCompanies { get; set; }
        public int TotalProjects { get; set; }
        public int TotalSubscriptions { get; set; }

    }

    public class UserTaskDashBoard
    {
        public double BugPercent { get; set; }
        public double FeaturePercent { get; set; }
        public double ChorePercent { get; set; }

        public double OverduePercent { get; set; }
        public double OnTimePercent { get; set; }
        public double EarlyCompletedPercent { get; set; }
    }
}
