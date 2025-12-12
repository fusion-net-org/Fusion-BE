using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Fusion.Service.ViewModels.Project.Requests
{
    public class ProjectAccessCheckResponse
    {
        public Guid ProjectId { get; set; }
        public Guid UserId { get; set; }

        public bool IsMember { get; set; }  
        public bool IsClosed { get; set; }   

        public bool IsOpen => !IsClosed;
        public bool CanAccess => IsMember && !IsClosed;

        public bool IsOwner { get; set; }
        public bool IsPartner { get; set; }
        public bool IsViewAll { get; set; }
    }
}
