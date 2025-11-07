using Fusion.Repository.Enums;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Fusion.Service.ViewModels.Sprint
{
    public class SprintQuery
    {
        public Guid? ProjectId { get; set; }

        // match FE kiểu 'DateRange.From' / 'DateRange.To'
        [FromQuery(Name = "DateRange.From")]
        public DateTime? DateFrom { get; set; }

        [FromQuery(Name = "DateRange.To")]
        public DateTime? DateTo { get; set; }

        public List<SprintStatus>? Statuses { get; set; }
        public string? SortColumn { get; set; } = "start_date";
        public bool SortDescending { get; set; } = false;

        public string? Q { get; set; } // keyword
        public int PageNumber { get; set; } = 1;
        public int PageSize { get; set; } = 50;
    }
}
