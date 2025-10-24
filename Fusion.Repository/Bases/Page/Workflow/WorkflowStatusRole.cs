using Fusion.Repository.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Fusion.Repository.Bases.Page.Workflow
{
    public class WorkflowStatusRole
    {
        public long Id { get; set; }
        public Guid StatusId { get; set; }
        public string RoleName { get; set; } = default!;
        public WorkflowStatus? Status { get; set; }
    }

    public class WorkflowTransitionRole
    {
        public long Id { get; set; }
        public long TransitionId { get; set; }
        public string RoleName { get; set; } = default!;
        public WorkflowTransition? Transition { get; set; }
    }
}
