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

        public string? MemberAvatar {  get; set; }
        public string? Email { get; set; }
        public string? Phone {  get; set; }
        public string? Gender { get; set; }

        public string? MemberPhoneNumber { get; set; }

        public int? NumberProductJoin { get; set; } = 0; //Sum of project internal and hired project of member in that company

        public int? NumberCompanyJoin { get; set; } = 0;

        public string? Status { get; set; } //Hien dang o trong cong ty hay khong.

        public bool? IsDeleted { get; set; }

        public DateTime JoinedAt { get; set; }

        public bool IsOwner { get; set; }
    }
}
