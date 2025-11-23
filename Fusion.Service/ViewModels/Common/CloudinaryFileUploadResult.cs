using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Fusion.Service.ViewModels.Common
{
    public class CloudinaryUploadResult
    {
        public string Url { get; set; } = default!;
        public string PublicId { get; set; } = default!;
        public bool IsImage { get; set; }
        public long SizeBytes { get; set; }

    }
}
