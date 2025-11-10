using Fusion.Service.ViewModels.Users.Responses;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Fusion.Service.ViewModels.Companies.Responses
{
    public class CompanyPerformanceResponse
    {
        public Guid CompanyId { get; set; }
        public int TotalMembers { get; set; }
        public List<UserPerformanceResponse> Data { get; set; } = new();

    }
}
