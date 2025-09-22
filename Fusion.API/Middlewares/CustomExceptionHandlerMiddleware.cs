using Fusion.Repository.Bases.Exceptions;
using Fusion.Service.Commons.BaseResponses;
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
        catch (CustomException ex)
        {
            //_logger.LogError(ex, "Error occurred: {Message} at {StackTrace}", ex.ErrorMessage, ex.StackTrace);
            Console.WriteLine($"Middleware Exception: {ex.GetType().Name} - {ex.Message}");

            _logger.LogError(ex,
               "Error occurred at {Path} | Message: {Message} | ClientIP: {ClientIP} | Timestamp: {Timestamp}",
               context.Request.Path,
               ex.Message,
               context.Connection.RemoteIpAddress,
               DateTime.UtcNow);

            context.Response.StatusCode = ex.StatusCode;

            await HandleExceptionAsync(context, ex);

        }
        catch (Exception ex)
        {
            Console.WriteLine($"Middleware Exception: {ex.GetType().Name} - {ex.Message}");

            _logger.LogError(ex,
               "Error occurred at {Path} | Message: {Message} | ClientIP: {ClientIP} | Timestamp: {Timestamp}",
               context.Request.Path,
               ex.Message,
               context.Connection.RemoteIpAddress,
               DateTime.UtcNow);

            context.Response.StatusCode = StatusCodes.Status500InternalServerError;

            await HandleExceptionAsync(context, new CustomException(
                StatusCodes.Status500InternalServerError,
                ResponseCodeConstants.INTERNAL_SERVER_ERROR,
                ResponseMessages.INTERNAL_SERVER_ERROR,
                ex.Message
            ));
        }
    }

    private async Task HandleExceptionAsync(HttpContext context, CustomException ex)
    {
        // var response = new ResponseModel<object>(ex.StatusCode, null, null, ex.ErrorMessage?.ToString());
        var response = ResponseModel<object>.ErrorResponseModel(
            ex.StatusCode,
            ex.ErrorMessage?.ToString() ?? "An unexpected error occurred.",
            null
        );
        context.Response.ContentType = "application/json";
        context.Response.StatusCode = ex.StatusCode;

        var result = JsonSerializer.Serialize(response);
        await context.Response.WriteAsync(result);
    }
}
