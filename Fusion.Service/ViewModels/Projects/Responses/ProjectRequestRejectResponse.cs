using Fusion.Repository.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Fusion.Service.ViewModels.Projects.Responses
{
    public class ProjectRequestRejectResponse
    {
        public Guid RequestId { get; set; }
        public string Status { get; set; } = ProjectRequestStatusEnum.Rejected.ToString();
        public string RejectedBy { get; set; } = default!;
        public DateTime RejectedAt { get; set; }
        public string? Reason { get; set; }
    }
}
