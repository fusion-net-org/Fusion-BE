using Fusion.Repository.Bases.Page;
using Fusion.Repository.Bases.Page.ProjectBoard;
using Fusion.Repository.ViewModels.Users;
using Fusion.Service.ViewModels.Task.Response;
using Fusion.Service.ViewModels.UserLog.Responses;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Fusion.Service.ViewModels.Users.Responses
{
    public class AnalyticsUserResponse
    {
        public UserPerformanceOverview UserPerformance { get; set; } = new();
        public List<ProjectTaskResponse> AssignToMe { get; set; } = new();
        public UserTaskDashBoard Dashboard { get; set; } = new();
    }
}
