using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Fusion.Repository.Bases.Page.ProjectMember
{
    public class ProjectMemberChartResponse
    {
        public List<JoinedOverTimeItem> JoinedOverTime { get; set; } = new();
        public List<GenderDistributionItem> GenderDistribution { get; set; } = new();
        public List<StatusDistributionItem> StatusDistribution { get; set; } = new();
        public List<PartnerDistributionItem> PartnerDistribution { get; set; } = new();
    }

    public class JoinedOverTimeItem
    {
        public string Month { get; set; } = string.Empty;
        public int Members { get; set; }
    }

    public class GenderDistributionItem
    {
        public string Gender { get; set; } = string.Empty;
        public int Count { get; set; }
    }

    public class StatusDistributionItem
    {
        public string Status { get; set; } = string.Empty;
        public int Count { get; set; }
    }

    public class PartnerDistributionItem
    {
        public string Type { get; set; } = string.Empty;
        public int Count { get; set; }
    }

}
