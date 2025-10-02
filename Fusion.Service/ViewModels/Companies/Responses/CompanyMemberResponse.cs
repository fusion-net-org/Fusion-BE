using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Fusion.Service.ViewModels.Companies.Responses
{
    public class CompanyMemberResponse
    {
        public long Id { get; set; }

        public Guid? CompanyId { get; set; }

        public string? CompanyName { get; set; }

        public Guid? MemberId { get; set; }

        public string? MemberName { get; set; }

        public bool Status { get; set; }

        public DateTime JoinedAt { get; set; }
    }
}
