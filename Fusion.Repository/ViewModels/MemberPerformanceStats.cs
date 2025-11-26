using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Fusion.Repository.ViewModels
{
    public class MemberPerformanceStats
    {
        public int Productivity { get; set; }
        public int Communication { get; set; }
        public int Teamwork { get; set; }
        public int ProblemSolving { get; set; }
    }

    public class MemberStats
    {
        public int Score { get; set; }
        public int HoursPerWeek { get; set; }

        public EfficiencyChart? Efficiency {  get; set; }
        public PieChart? PriorityDistribution { get; set; }
        public LineChart? ScoreTrendChart { get; set; }
    }

    public class EfficiencyChart
    {
        public int OnTimePercent { get; set; }
        public int LatePercent { get; set; }
        public int PendingPercent { get; set; } 
    };

    public class PieChartSegment
    {
        public string Name { get; set; } = null!;
        public int Value { get; set; }
        public string Color { get; set; } = "#000000";
    }

    public class PieChart
    {
        public List<PieChartSegment> Segments { get; set; } = new();
    }

    public class ScoreTrend
    {
        public string Period { get; set; } = null!;
        public int UserScore { get; set; }
        public int MaxScore { get; set; }
    }

    public class LineChart
    {
        public List<ScoreTrend> Data { get; set; } = new();
    }

}
