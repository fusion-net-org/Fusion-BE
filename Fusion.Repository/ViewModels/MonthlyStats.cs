using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Fusion.Repository.ViewModels
{
    public class MonthlyStats
    {
        public int Month { get; set; }
        public int Users { get; set; }
        public int Companies { get; set; }
        public decimal Revenue { get; set; }
    }
}
