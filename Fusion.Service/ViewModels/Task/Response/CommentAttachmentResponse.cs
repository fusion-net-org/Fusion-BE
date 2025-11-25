using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Fusion.Service.ViewModels.Task.Response
{
    public class CommentAttachmentResponse
    {
        public Guid Id { get; set; }
        public long CommentId { get; set; }
        public string FileName { get; set; } = null!;
        public string Url { get; set; } = null!;
        public string? ContentType { get; set; }
        public long Size { get; set; }
        public bool IsImage { get; set; }
    }
}
