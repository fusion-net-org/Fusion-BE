using Fusion.Repository.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Fusion.Repository.Bases.Page.Sprint
{
    // Internal repo-only VM shape to avoid Service layer needing IQueryable
    public sealed class SprintVmData
    {
        public Guid Id { get; init; }
        public Guid? ProjectId { get; init; }
        public string Name { get; init; } = default!;
        public DateTime? StartDate { get; init; }
        public DateTime? EndDate { get; init; }
        public SprintStatus Status { get; init; }
        public int TaskCount { get; init; }
    }
}
