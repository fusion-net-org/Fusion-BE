using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Fusion.Service.ViewModels.Task.Request
{
    public class DraftMaterializeRequest
    {
        public Guid SprintId { get; set; }

        public Guid? WorkflowStatusId { get; set; }
        public string? StatusCode { get; set; }

        public int? OrderInSprint { get; set; }
    }
}
