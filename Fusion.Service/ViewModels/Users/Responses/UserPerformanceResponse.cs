using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Fusion.Service.ViewModels.Users.Responses
{
    public class UserPerformanceResponse
    {
        public Guid UserId { get; set; }
        public string UserName { get; set; } = string.Empty;
        public int OnTimeCount { get; set; }
        public int LateCount { get; set; }
        public int NotCompletedCount { get; set; }
        public double OnTimePercent { get; set; }
        public double LatePercent { get; set; }
    }
}
