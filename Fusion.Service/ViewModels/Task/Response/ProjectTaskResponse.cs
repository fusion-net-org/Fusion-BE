using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;

namespace Fusion.Service.ViewModels.Task.Response;

public class ProjectTaskResponse
{
    public Guid Id { get; set; }
    public string? Code { get; set; }

    public Guid? ProjectId { get; set; }
    public Guid? SprintId { get; set; }

    public string? Title { get; set; }
    public string? Description { get; set; }
    public string? Type { get; set; }
    public string? Priority { get; set; }
    public string? Severity { get; set; }

    public int? Point { get; set; }
    public int? EstimateHours { get; set; }
    public int? RemainingHours { get; set; }
    public DateTime? DueDate { get; set; }

    public Guid? CurrentStatusId { get; set; }
    public string? Status { get; set; } // name/code của status

    public int? OrderInSprint { get; set; }
    public bool IsBacklog { get; set; }
    public bool IsDeleted { get; set; }

    public IEnumerable<Guid> AssigneeIds { get; set; } = Enumerable.Empty<Guid>();

    public DateTime? CreateAt { get; set; }
    public DateTime? UpdateAt { get; set; }
}
