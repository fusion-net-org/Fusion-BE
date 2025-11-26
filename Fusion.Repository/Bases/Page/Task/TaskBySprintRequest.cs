using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Fusion.Repository.Bases.Page.Task
{
    public class TaskBySprintRequest : PagedRequest
    {
        public string? Title { get; set; }
        public string? Status { get; set; }
        public string? Priority { get; set; }
        public DateTime? CreatedFrom { get; set; }
        public DateTime? CreatedTo { get; set; }
    }
}
