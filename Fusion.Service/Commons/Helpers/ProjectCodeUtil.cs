using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Fusion.Service.Commons.Helpers
{
    public static class ProjectCodeUtil
    {
        public static string GenerateProjectRequestCode()
        {
            var year = DateTime.UtcNow.Year;
            var shortGuid = Guid.NewGuid().ToString("N")[..6].ToUpper();
            return $"REQ-{year}-{shortGuid}";
        }
    }
}
