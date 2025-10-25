using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Fusion.Service.ViewModels.Permission.Responses
{
    public class PermissionResponse
    {
        public string FunctionCode { get; set; } = string.Empty;
        public string FunctionName { get; set; } = string.Empty;
        public string PageCode { get; set; } = string.Empty;
        public bool IsAccess { get; set; }
    }
}
