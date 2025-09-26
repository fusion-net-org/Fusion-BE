using FluentValidation;
using Fusion.Repository.Bases.Exceptions;
using Fusion.Service.Commons.BaseResponses;
using System.Text.Json;

namespace Fusion.API.Middlewares
{
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
            catch (ValidationException ex)
            {
                if (context.Response.HasStarted)
                {
                    LogError(context, ex, ex.Errors);
                    throw;
                }

                await HandleValidationExceptionAsync(context, ex);
            }
            catch (CustomException ex)
            {
                if (context.Response.HasStarted)
                {
                    LogError(context, ex);
                    throw;
                }

                await HandleCustomExceptionAsync(context, ex);
            }
            catch (Exception ex)
            {
                if (context.Response.HasStarted)
                {
                    LogError(context, ex);
                    throw;
                }

                var internalError = CustomExceptionFactory.CreateInternalServerError(ex.Message);
                await HandleCustomExceptionAsync(context, internalError);
            }
        }

        private void LogError(HttpContext context, Exception ex, object? extra = null)
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

        private async Task HandleValidationExceptionAsync(HttpContext context, ValidationException ex)
        {
            LogError(context, ex, ex.Errors);

            var errors = ex.Errors
                .GroupBy(e => e.PropertyName)
                .ToDictionary(g => g.Key, g => g.Select(e => e.ErrorMessage).ToArray());

            var response = ResponseModel<object>.Error(
                statusCode: StatusCodes.Status400BadRequest,
                message: "Validation failed",
                additionalData: errors
            );

            context.Response.ContentType = "application/json";
            context.Response.StatusCode = StatusCodes.Status400BadRequest;
            await context.Response.WriteAsJsonAsync(response);
        }

        private async Task HandleCustomExceptionAsync(HttpContext context, CustomException ex)
        {
            LogError(context, ex);

            var response = ResponseModel<object>.Error(
                statusCode: ex.StatusCode,
                message: ex.ErrorMessage,
                additionalData: ex.DetailMessage
            );

            context.Response.ContentType = "application/json";
            context.Response.StatusCode = ex.StatusCode;
            await context.Response.WriteAsJsonAsync(response);
        }
    }
}
