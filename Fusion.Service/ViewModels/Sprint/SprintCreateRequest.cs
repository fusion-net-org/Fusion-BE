using System;
using System.Collections.Generic;

namespace Fusion.Service.ViewModels.Sprint
{
    public class SprintCreateRequest
    {
        public Guid ProjectId { get; set; }

        /// <summary>
        /// Nếu null/empty -> BE tự generate "Sprint yyyy-MM-dd"
        /// </summary>
        public string? Name { get; set; }

        public string? Goal { get; set; }

        /// <summary>
        /// 1 hoặc 2 tuần. Nếu FE không gửi lên (0) -> mặc định = 2
        /// </summary>
        public byte DurationWeeks { get; set; } = 2;

        /// <summary>
        /// Nếu FE không gửi lên (default 0001-01-01) -> BE tự tính StartDate
        /// </summary>
        public DateTime StartDate { get; set; }

        public bool LockBacklogOnStart { get; set; } = true;

        /// <summary>
        /// Optional: add task ngay khi tạo
        /// </summary>
        public List<Guid>? TaskIds { get; set; }
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
