using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace Fusion.Repository.Entities;

public partial class ProjectTask
{
    [Key]
    [Column("id")]
    public Guid Id { get; set; }

    [Column("project_id")]
    public Guid? ProjectId { get; set; }

    [Column("sprint_id")]
    public Guid? SprintId { get; set; }

    [Column("type")]
    [StringLength(20)]
    [Unicode(false)]
    public string? Type { get; set; }

    [Column("img")]
    [StringLength(500)]
    public string? Img { get; set; }

    [Column("title")]
    [StringLength(200)]
    public string? Title { get; set; }

    [Column("description")]
    public string? Description { get; set; }

    [Column("priority")]
    [StringLength(20)]
    [Unicode(false)]
    public string? Priority { get; set; }

    [Column("is_backlog")]
    public bool IsBacklog { get; set; }
    [Column("is_close")]
    public bool IsClose { get; set; }

    [Column("point")]
    public int? Point { get; set; }

    [Column("status")]
    public string? Status { get; set; }
   

    [Column("due_date")]
    [Precision(3)]
    public DateTime? DueDate { get; set; }

    [Column("source")]
    [StringLength(100)]
    public string? Source { get; set; }

    [Column("withdrawn_at")]
    [Precision(3)]
    public DateTime? WithdrawnAt { get; set; }

    [Column("created_by")]
    public Guid? CreatedBy { get; set; }

    [Column("create_at")]
    [Precision(3)]
    public DateTime? CreateAt { get; set; }

    [Column("update_at")]
    [Precision(3)]
    public DateTime? UpdateAt { get; set; }
    [Column("order_in_sprint")]
    public int? OrderInSprint { get; set; }
    [Column("is_deleted")]
    public bool IsDeleted { get; set; }
    [InverseProperty("Task")]
    public virtual ICollection<Comment> Comments { get; set; } = new List<Comment>();

    [ForeignKey("CreatedBy")]
    [InverseProperty("ProjectTasks")]
    public virtual User? CreatedByNavigation { get; set; }

    [ForeignKey("ProjectId")]
    [InverseProperty("ProjectTasks")]
    public virtual Project? Project { get; set; }

    [ForeignKey("SprintId")]
    [InverseProperty("ProjectTasks")]
    public virtual Sprint? Sprint { get; set; }
    //---------------------------------------------------------
    [Column("code"), StringLength(50)] public string? Code { get; set; }
    [Column("severity"), StringLength(20)] public string? Severity { get; set; }
    [Column("estimate_hours")] public int? EstimateHours { get; set; }
    [Column("remaining_hours")] public int? RemainingHours { get; set; }

    [Column("current_status_id")] public Guid? CurrentStatusId { get; set; }
    [ForeignKey(nameof(CurrentStatusId))] public WorkflowStatus? CurrentStatus { get; set; }

    [Column("parent_task_id")] public Guid? ParentTaskId { get; set; }
    [ForeignKey(nameof(ParentTaskId))] public ProjectTask? ParentTask { get; set; }

    [Column("carry_over_count")] public int CarryOverCount { get; set; } = 0;

    [Column("source_task_id")] public Guid? SourceTaskId { get; set; }
    [Column("ticket_id")]
    public Guid? TicketId { get; set; }

    [ForeignKey(nameof(TicketId))]
    [InverseProperty(nameof(Ticket.Tasks))]
    public virtual Ticket? Ticket { get; set; }
    [ForeignKey(nameof(SourceTaskId))] public ProjectTask? SourceTask { get; set; }
    public ICollection<TaskWorkflow> Assignees { get; set; } = new List<TaskWorkflow>();
    public ICollection<ProjectTaskDependency> Dependencies { get; set; } = new List<ProjectTaskDependency>();
    //---------------------------------------------------------
    [Column("component_id")]
    public Guid? ComponentId { get; set; }

    [ForeignKey(nameof(ComponentId))]
    [InverseProperty(nameof(ProjectComponent.Tasks))]
    public virtual ProjectComponent? Component { get; set; }
    [InverseProperty("Task")]
    public virtual ICollection<TaskLogEvent> TaskLogEvents { get; set; } = new List<TaskLogEvent>();

    [InverseProperty("Task")]
    public virtual ICollection<TaskWorkflow> TaskWorkflows { get; set; } = new List<TaskWorkflow>();
    [InverseProperty(nameof(ProjectTaskChecklistItem.Task))]
    public ICollection<ProjectTaskChecklistItem> ChecklistItems { get; set; } = new List<ProjectTaskChecklistItem>();
    [InverseProperty(nameof(ProjectTaskAttachment.Task))]
    public ICollection<ProjectTaskAttachment> Attachments { get; set; } = new List<ProjectTaskAttachment>();

}
