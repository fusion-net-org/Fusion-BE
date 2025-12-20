using Fusion.Service.ViewModels.Tickets.Responses;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Fusion.Service.ViewModels.Projects.Responses
{
    public class ProjectRequestResponse
    {
        public Guid Id { get; set; }

        public Guid? RequesterCompanyId { get; set; } //MEGA 

        public string? RequesterCompanyName { get; set; }
        public string? RequesterCompanyLogoUrl { get; set; }

        public Guid? ExecutorCompanyId { get; set; } //GOOGOLE

        public string? ExecutorCompanyName { get; set; }
        public Guid? ContractId { get; set; }

        public string? ExecutorCompanyLogoUrl { get; set; }

        public Guid? CreatedBy { get; set; }

        public string? CreatedName { get; set; }

        public string? Code { get; set; }

        public string? ProjectName { get; set; }

        public string? Description { get; set; }

        public string? Status { get; set; }

        public DateOnly? StartDate { get; set; }

        public DateOnly? EndDate { get; set; }

        public DateTime CreateAt { get; set; }

        public DateTime UpdateAt { get; set; }

        public bool? IsDeleted { get; set; }
        public bool? isHaveProject { get; set; }
        public bool? IsClosed { get; set; }
        public string? ClosedBy { get; set; }
        public Guid? ConvertedProjectId { get; set; }
    }

    public class ProjectRequestResponseV2
    {
        public Guid Id { get; set; }

        public Guid? RequesterCompanyId { get; set; } //MEGA 

        public string? RequesterCompanyName { get; set; }
        public string? RequesterCompanyLogoUrl { get; set; }

        public Guid? ExecutorCompanyId { get; set; } //GOOGOLE

        public string? ExecutorCompanyName { get; set; }
        public Guid? ContractId { get; set; }

        public string? ExecutorCompanyLogoUrl { get; set; }

        public Guid? CreatedBy { get; set; }

        public string? CreatedName { get; set; }

        public string? Code { get; set; }

        public string? ProjectName { get; set; }

        public string? Description { get; set; }

        public string? Status { get; set; }

        public DateOnly? StartDate { get; set; }

        public DateOnly? EndDate { get; set; }

        public DateTime CreateAt { get; set; }

        public DateTime UpdateAt { get; set; }

        public bool? IsDeleted { get; set; }
        public bool? isHaveProject { get; set; }

        public Guid? ConvertedProjectId { get; set; }

        public List<TicketResponseV2>? Tickets { get; set; }
    }
}
