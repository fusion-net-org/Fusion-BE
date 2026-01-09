using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Fusion.Service.ViewModels.ProjectComponent
{
    public class CreateProjectComponent
    {
        public Guid? ProjectId { get; set; }

        public Guid? ProjectRequestId { get; set; }

        public string Name { get; set; } = null!;

        public string? Description { get; set; }

    }
    public class ProjectComponentResponse
    {
        public Guid Id { get; set; }

        public Guid? ProjectId { get; set; }

        public Guid? ProjectRequestId { get; set; }

        public string Name { get; set; } = null!;

        public string? Description { get; set; }

        public DateTime CreatedAt { get; set; }

        public Guid? CreatedBy { get; set; }
    }
    public class CreateProjectComponentList
    {
        public List<CreateProjectComponent> Items { get; set; } = new();
    }

}
