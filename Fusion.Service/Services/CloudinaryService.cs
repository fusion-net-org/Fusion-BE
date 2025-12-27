

using CloudinaryDotNet;
using CloudinaryDotNet.Actions;
using Fusion.Repository.Bases.Exceptions;
using Fusion.Repository.Bases.Responses;
using Fusion.Service.IServices;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using System.Net;

namespace Fusion.Service.Services;

public class CloudinaryService : ICloudinaryService
{
    private readonly Cloudinary _cloudinary;

    public CloudinaryService(IConfiguration configuration)
    {
        var cloudName = configuration["CloudinarySettings:Cloudname"];
        var apiKey = configuration["CloudinarySettings:ApiKey"];
        var apiSecret = configuration["CloudinarySettings:ApiSecret"];

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
    // ================== FILE (task attachments) ==================
    public async Task<(string Url, string PublicId, bool IsImage)>
       UploadFileAsync(
           IFormFile file,
           string folder,
           CancellationToken cancellationToken = default)
    {
        if (file == null || file.Length == 0)
            throw CustomExceptionFactory.CreateBadRequestError(
                ResponseMessages.INVALID_INPUT);

        var ext = Path.GetExtension(file.FileName).ToLowerInvariant();
        var imageExts = new[] { ".jpg", ".jpeg", ".png", ".gif", ".webp", ".bmp", ".tiff", ".svg" };
        bool isImage = imageExts.Contains(ext);

        UploadResult uploadResult;

        if (isImage)
        {
            // ẢNH
            var imageParams = new ImageUploadParams
            {
                File = new FileDescription(file.FileName, file.OpenReadStream()),
                Folder = folder
            };

            uploadResult = await _cloudinary.UploadAsync(imageParams, cancellationToken);
        }
        else
        {
            // FILE “RAW”: pdf, docx, xlsx, zip,...
            var rawParams = new RawUploadParams
            {
                File = new FileDescription(file.FileName, file.OpenReadStream()),
                Folder = folder
            };

            // type = "auto" để Cloudinary tự chọn raw/video/image phù hợp
            uploadResult = await _cloudinary.UploadAsync(rawParams, "auto", cancellationToken);
        }

        if (uploadResult == null)
        {
            throw CustomExceptionFactory.CreateInternalServerError(
                ResponseMessages.INTERNAL_SERVER_ERROR
                    .FormatMessage("No response from Cloudinary"));
        }

        // Tránh case StatusCode = 0, check 200 / 201
        if (uploadResult.StatusCode != HttpStatusCode.OK &&
            uploadResult.StatusCode != HttpStatusCode.Created)
        {
            throw CustomExceptionFactory.CreateInternalServerError(
                ResponseMessages.INTERNAL_SERVER_ERROR
                    .FormatMessage(uploadResult.Error?.Message ?? "Upload failed"));
        }

        var secureUrl = uploadResult.SecureUrl?.ToString()
                        ?? uploadResult.Url?.ToString()
                        ?? throw CustomExceptionFactory.CreateInternalServerError(
                            ResponseMessages.INTERNAL_SERVER_ERROR
                                .FormatMessage("Upload succeeded but URL is empty"));

        return (secureUrl, uploadResult.PublicId, isImage);
    }
    public async Task<(string Url, string PublicId)> UploadDocumentAsync(
       IFormFile file,
       string folder,
       CancellationToken cancellationToken = default)
    {
        if (file == null || file.Length == 0)
            throw CustomExceptionFactory.CreateBadRequestError(ResponseMessages.INVALID_INPUT);

        var ext = Path.GetExtension(file.FileName).ToLowerInvariant();
        var allowedExts = new[] { ".pdf", ".doc", ".docx" };

        if (!allowedExts.Contains(ext))
            throw CustomExceptionFactory.CreateBadRequestError(
                ResponseMessages.INVALID_INPUT.FormatMessage("Only PDF, DOC, DOCX are allowed."));

        long maxSize = 100 * 1024 * 1024; // 100MB
        if (file.Length > maxSize)
            throw CustomExceptionFactory.CreateBadRequestError(
                ResponseMessages.INVALID_INPUT.FormatMessage("File size must not exceed 100MB"));

        var uploadParams = new RawUploadParams
        {
            File = new FileDescription(file.FileName, file.OpenReadStream()),
            Folder = folder
        };

        var result = await _cloudinary.UploadAsync(uploadParams, "raw", cancellationToken);
        if (result == null)
            throw CustomExceptionFactory.CreateInternalServerError(
                ResponseMessages.INTERNAL_SERVER_ERROR.FormatMessage("No response from Cloudinary"));

        if (result.Error != null)
            throw CustomExceptionFactory.CreateInternalServerError(
                ResponseMessages.INTERNAL_SERVER_ERROR.FormatMessage(result.Error.Message));

        if (result.StatusCode != HttpStatusCode.OK &&
            result.StatusCode != HttpStatusCode.Created)
            throw CustomExceptionFactory.CreateInternalServerError(
                ResponseMessages.INTERNAL_SERVER_ERROR.FormatMessage("Upload failed"));

        var url = result.SecureUrl?.ToString()
                  ?? result.Url?.ToString()
                  ?? throw CustomExceptionFactory.CreateInternalServerError(
                      ResponseMessages.INTERNAL_SERVER_ERROR.FormatMessage("Upload succeeded but URL is empty"));

        return (url, result.PublicId);
    }

    public async Task<(string Url, string PublicId)> UpdateDocumentAsync(
    string oldFileUrl,
    IFormFile newFile,
    string folder,
    CancellationToken cancellationToken = default)
    {
        if (newFile == null || newFile.Length == 0)
            throw CustomExceptionFactory.CreateBadRequestError(ResponseMessages.INVALID_INPUT);

        var oldPublicId = ExtractPublicIdFromUrl(oldFileUrl);

        if (!string.IsNullOrWhiteSpace(oldPublicId))
        {
            await DeleteImageAsync(oldPublicId, cancellationToken);
        }
        var (url, newPublicId) = await UploadDocumentAsync(newFile, folder, cancellationToken);

        return (url, newPublicId);
    }
}



