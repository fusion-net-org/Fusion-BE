using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Fusion.Repository.ViewModels.ProjectRequest
{
    public class ReviewCloseProjectRequest
    {
        public bool IsApproved { get; set; }
        public string? ReasonReject { get; set; }
    }
}
