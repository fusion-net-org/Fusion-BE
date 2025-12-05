using Microsoft.AspNetCore.Http;
namespace Fusion.Service.ViewModels
{
    public class UpdateDocumentRequest
    {
        public string OldFileUrl { get; set; }
        public IFormFile NewFile { get; set; }
    }
}
