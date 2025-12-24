using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Fusion.Service.ViewModels.TaskLogEventQuery
{
    public sealed class ProjectActivityVm
    {
        public long Id { get; set; }
        public Guid TaskId { get; set; }
        public string? Action { get; set; }
        public Guid? ActorId { get; set; }
        public string? ActorName { get; set; }
        public string? ActorEmail { get; set; }
        public string? ChangedCols { get; set; }
        public string? OldRow { get; set; }
        public string? NewRow { get; set; }
        public string? Metadata { get; set; }
        public DateTimeOffset CreatedAt { get; set; }
        public bool IsView { get; set; }
    }
}
