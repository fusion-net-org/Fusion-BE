using Fusion.Repository.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Fusion.Service.ViewModels.Sprint.Responses
{
    public class SprintResponse
    {
        public Guid Id { get; set; }
        public string? Name { get; set; }
        public DateTime? Start { get; set; }
        public DateTime? End { get; set; }
        public string? Color { get; set; }
        public string? Goal { get; set; }
        public int? CapacityHours { get; set; }
        public int? CommittedPoints { get; set; }
        public SprintStatus Status { get; set; }
        public bool IsDeleted { get; set; }
        public DateTime? CreatedAt { get; set; }

    }
}
