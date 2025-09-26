using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Fusion.Service.ViewModels.Users.Requests
{
    public class CompanyFriendshipRequest
    {
        public Guid CompanyAId { get; set; }
        public Guid CompanyBId { get; set; }
        public Guid RequesterId { get; set; }
    }
}
