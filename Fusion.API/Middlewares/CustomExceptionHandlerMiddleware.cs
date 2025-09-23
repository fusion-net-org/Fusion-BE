using Fusion.Repository.Bases.Exceptions;
using System.Text.Json;
using Fusion.Repository.Bases.Responses;

namespace Travelogue.API.Middlewares;

public class CustomExceptionHandlerMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<CustomExceptionHandlerMiddleware> _logger;

    public CustomExceptionHandlerMiddleware(RequestDelegate next, ILogger<CustomExceptionHandlerMiddleware> logger)
    {
        _next = next;
        _logger = logger;
    }

    public async Task Invoke(HttpContext context)
    {
        try
        {
            await _next(context);
        }
        catch (FluentValidation.ValidationException ex)
        {
            await HandleValidationExceptionAsync(context, ex);
        }
        catch (CustomException ex)
        {
            await HandleCustomExceptionAsync(context, ex);
        }
        catch (Exception ex)
        {
            var internalError = CustomExceptionFactory.CreateInternalServerError(ex.Message);
            await HandleCustomExceptionAsync(context, internalError);
        }
    }

    private void LogError(HttpContext context, Exception ex, string? extra = null)
    {
        _logger.LogError(ex,
            "Error at {Path} | Type: {Type} | Message: {Message} | ClientIP: {ClientIP} | Timestamp: {Timestamp} | Extra: {Extra}",
            context.Request.Path,
            ex.GetType().Name,
            ex.Message,
            context.Connection.RemoteIpAddress,
            DateTime.UtcNow,
            extra);
    }

    private async Task HandleValidationExceptionAsync(HttpContext context, FluentValidation.ValidationException ex)
    {
        var errors = ex.Errors
            .GroupBy(e => e.PropertyName)
            .ToDictionary(
                g => g.Key,
                g => g.Select(e => e.ErrorMessage).ToArray()
            );

        LogError(context, ex, JsonSerializer.Serialize(errors));

        var response = new
        {
            statusCode = StatusCodes.Status400BadRequest,
            message = ResponseMessages.INVALID_INPUT,
            errors
        };

        context.Response.ContentType = "application/json";
        context.Response.StatusCode = StatusCodes.Status400BadRequest;
        await context.Response.WriteAsync(JsonSerializer.Serialize(response));
    }


    private async Task HandleCustomExceptionAsync(HttpContext context, CustomException ex)
    {
        LogError(context, ex);

        var response = new
        {
            statusCode = ex.StatusCode,
            errorCode = ex.ErrorCode,
            message = ex.ErrorMessage?.ToString() ?? "An unexpected error occurred.",
            detail = ex.DetailMessage
        };

        context.Response.ContentType = "application/json";
        context.Response.StatusCode = ex.StatusCode;
        await context.Response.WriteAsync(JsonSerializer.Serialize(response));
    }
}
