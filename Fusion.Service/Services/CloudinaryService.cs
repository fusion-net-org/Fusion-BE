

using CloudinaryDotNet;
using CloudinaryDotNet.Actions;
using Fusion.Repository.Bases.Exceptions;
using Fusion.Repository.Bases.Responses;
using Fusion.Service.IServices;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;

namespace Fusion.Service.Services;

public class CloudinaryService : ICloudinaryService
{
    private readonly Cloudinary _cloudinary;

    public CloudinaryService(IConfiguration configuration)
    {
        var cloudName = configuration["Cloudinary:Cloudname"];
        var apiKey = configuration["Cloudinary:ApiKey"];
        var apiSecret = configuration["Cloudinary:ApiSecret"];

        if (string.IsNullOrWhiteSpace(cloudName) ||
            string.IsNullOrWhiteSpace(apiKey) ||
            string.IsNullOrWhiteSpace(apiSecret))
        {
            throw CustomExceptionFactory.
                CreateInternalServerError(ResponseMessages.INTERNAL_SERVER_ERROR.
                FormatMessage("Cloudinary configuration is missing!"));
        }

        _cloudinary = new Cloudinary(new Account(cloudName, apiKey, apiSecret));
    }

    public async Task<string> UploadImageAsync(IFormFile file, string folder, CancellationToken cancellationToken = default)
    {
        if (file == null || file.Length == 0)
            throw CustomExceptionFactory.CreateBadRequestError(ResponseMessages.INVALID_INPUT);

        var uploadParams = new ImageUploadParams
        {
            File = new FileDescription(file.FileName, file.OpenReadStream()),
            Folder = folder
        };

        var result = await _cloudinary.UploadAsync(uploadParams, cancellationToken);
        if (result == null)
            throw CustomExceptionFactory.CreateInternalServerError(ResponseMessages.INTERNAL_SERVER_ERROR
                .FormatMessage("No response from Cloudinary"));

        if (result.StatusCode != System.Net.HttpStatusCode.OK)
            throw CustomExceptionFactory.CreateInternalServerError(ResponseMessages.INTERNAL_SERVER_ERROR
                .FormatMessage(result.Error?.Message ?? "Upload failed"));

        return result.SecureUrl.ToString();
    }
    public string? ExtractPublicIdFromUrl(string url)
    {
        if (string.IsNullOrWhiteSpace(url)) return null;

        var uri = new Uri(url);
        var segments = uri.Segments.Select(s => s.Trim('/')).ToList();

        int uploadIndex = segments.FindIndex(s => s == "upload");
        if (uploadIndex == -1) return null;

        var publicIdSegments = segments.Skip(uploadIndex + 2);
        var publicIdWithExt = string.Join("/", publicIdSegments);

        int dotIndex = publicIdWithExt.LastIndexOf('.');
        return dotIndex >= 0 ? publicIdWithExt[..dotIndex] : publicIdWithExt;
    }
    public async Task DeleteImageAsync(string publicId, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(publicId))
            throw CustomExceptionFactory.CreateBadRequestError(ResponseMessages.INVALID_INPUT);

        var deletionParams = new DeletionParams(publicId)
        {
            ResourceType = ResourceType.Image
        };

        var deletionResult = await _cloudinary.DestroyAsync(deletionParams);

        if (deletionResult == null)
            throw CustomExceptionFactory.CreateInternalServerError(
                  ResponseMessages.INTERNAL_SERVER_ERROR.
                  FormatMessage(deletionResult.Error?.Message ?? $"Delete failed (PublicId: {publicId})"));

        if (deletionResult.Result != "ok")
            throw CustomExceptionFactory.CreateInternalServerError(
                            ResponseMessages.INTERNAL_SERVER_ERROR.
                            FormatMessage(deletionResult.Error?.Message ?? $"Delete failed (PublicId: {publicId})"));
    }
}



