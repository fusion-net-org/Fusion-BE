
using Fusion.Service.ViewModels.Common;
using Microsoft.AspNetCore.Http;

namespace Fusion.Service.IServices;

public interface ICloudinaryService
{
    Task<string> UploadImageAsync(IFormFile file, string folder, CancellationToken cancellationToken = default);
    string? ExtractPublicIdFromUrl(string url);
    Task DeleteImageAsync(string publicId, CancellationToken cancellationToken = default);
    Task<(string Url, string PublicId, bool IsImage)>
       UploadFileAsync(
           IFormFile file,
           string folder,
           CancellationToken cancellationToken = default);

    Task<(string Url, string PublicId)> UploadDocumentAsync( IFormFile file, string folder,CancellationToken cancellationToken = default);
    Task<(string Url, string PublicId)> UpdateDocumentAsync(string oldFileUrl,IFormFile newFile,string folder,CancellationToken cancellationToken = default);
}
