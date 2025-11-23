using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Fusion.Service.ViewModels.Task.Request
{
    public class TaskAttachmentUploadRequest
    {
        // tên "files" để FE gửi formData.append("files", file)
        [Required]
        public List<IFormFile> Files { get; set; } = new();

        public string? Description { get; set; }
    }
}
