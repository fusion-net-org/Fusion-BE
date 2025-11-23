using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Fusion.Service.ViewModels.Task.Response
{

    public class TaskAttachmentResponse
    {
        public Guid Id { get; set; }
        public Guid TaskId { get; set; }

        public string FileName { get; set; } = default!;
        public string Url { get; set; } = default!;
        public string? ContentType { get; set; }
        public long Size { get; set; }
        public bool IsImage { get; set; }
        public string? Description { get; set; }

        public DateTime UploadedAt { get; set; }
        public Guid UploadedBy { get; set; }
        public string? UploadedByName { get; set; }
    }
}
