using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Fusion.Repository.ViewModels.Companies
{
    public class CompanyTaskStatsResponse
    {
        public int OnTime { get; set; }
        public int Violations { get; set; }
        public int Completed { get; set; }
    }
}
