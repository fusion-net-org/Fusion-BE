using Fusion.Repository.Enums;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Fusion.Service.ViewModels.Projects.Requests
{
    public class CreateProjectRequestRequest
    {
        public Guid? ExecutorCompanyId { get; set; }

        public Guid? RequesterCompanyId { get; set; }

        public string? Name { get; set; }

        public string? Description { get; set; }

        public ProjectRequestStatusEnum? Status { get; set; }

        public DateOnly? StartDate { get; set; }

        public DateOnly? EndDate { get; set; }
    }
}
