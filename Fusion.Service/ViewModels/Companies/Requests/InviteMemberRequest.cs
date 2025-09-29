using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Fusion.Service.ViewModels.Companies.Requests
{
    public class InviteMemberRequest
    {
        public Guid InviteeMemberId { get; set; }
        public Guid CompanyId { get; set; }
    }
}
