using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Fusion.Service.ViewModels.Sprint
{
    public class SprintListItemVm
    {
        public Guid Id { get; set; }
        public Guid? ProjectId { get; set; }
        public string ProjectName { get; set; } = "";
        public string Name { get; set; } = "";
        public DateTime? StartDate { get; set; }
        public DateTime? EndDate { get; set; }
        public string Status { get; set; } = "Planned";
        public string? Color { get; set; }
    }

    public class SprintDetailVm : SprintListItemVm
    {
        public string? Goal { get; set; }
        public Guid? CreatedBy { get; set; }
        public DateTime? CreatedAt { get; set; }
    }
}
