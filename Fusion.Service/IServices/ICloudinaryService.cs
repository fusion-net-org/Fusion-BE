
using Microsoft.AspNetCore.Http;

namespace Fusion.Service.IServices;

public interface ICloudinaryService
{
    Task<string> UploadImageAsync(IFormFile file, string folder, CancellationToken cancellationToken = default);
    string? ExtractPublicIdFromUrl(string url);
    Task DeleteImageAsync(string publicId, CancellationToken cancellationToken = default);
}
