using Fusion.Repository.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Fusion.Service.ViewModels.Sprint
{
    public class SprintCreateRequest
    {
        public Guid ProjectId { get; set; }
        public DateTime StartDate { get; set; } // ngày bắt đầu (date-only)
        public byte DurationWeeks { get; set; } = 2; // 1 hoặc 2
        public string? Name { get; set; }
        public string? Goal { get; set; }
        public bool LockBacklogOnStart { get; set; } = true;
        public List<Guid>? TaskIds { get; set; } // Optional: add task ngay khi tạo
    }


    public class SprintVm
    {
        public Guid Id { get; set; }
        public Guid? ProjectId { get; set; }
        public string Name { get; set; } = default!;
        public DateTime? StartDate { get; set; }
        public DateTime? EndDate { get; set; }
        public byte Status { get; set; }
        public int TaskCount { get; set; }
    }
   
}
