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
        public Guid? RequesterCompanyId { get; set; }

        public Guid? ExecutorCompanyId { get; set; }
        public Guid? ContractId { get; set; }

        //[Required(ErrorMessage = "Name can not empty")]
        public string? Name { get; set; }

        public string? Description { get; set; }

        //[Required(ErrorMessage = "Start Date can not empty")]
        public DateOnly? StartDate { get; set; }

        //[Required(ErrorMessage = "End Date can not empty")]

        public DateOnly? EndDate { get; set; }


    }
}
