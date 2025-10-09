using Microsoft.AspNetCore.Http;
using Fusion.Repository.Bases.Responses;

namespace Fusion.Repository.Bases.Exceptions;

public static class CustomExceptionFactory
{
    public static CustomException CreateInternalServerError(string? detailMessage = null)
    {
        return new CustomException(
            StatusCodes.Status500InternalServerError,
            ResponseCodeConstants.INTERNAL_SERVER_ERROR,
            ResponseMessages.INTERNAL_SERVER_ERROR,
            detailMessage: detailMessage
        );
    }

    public static CustomException CreateNotFoundError(string objectName)
    {
        return new CustomException(
            StatusCodes.Status404NotFound,
            ResponseCodeConstants.NOT_FOUND,
            ResponseMessages.NOT_FOUND.Replace("{0}", objectName),
             detailMessage: $"{objectName} was not found"
        );
    }

    public static CustomException CreateBadRequestError(string message, string? detailMessage = null)
    {
        return new CustomException(
            StatusCodes.Status400BadRequest,
            ResponseCodeConstants.BAD_REQUEST,
            message,
            detailMessage : detailMessage
        );
    }

    public static CustomException CreateForbiddenError()
    {
        return new CustomException(
            StatusCodes.Status403Forbidden,
            ResponseCodeConstants.FORBIDDEN,
            "Forbidden access!"
        );
    }
    public static CustomException CreateUnauthorizedError(string? detailMessage = null)
    {
        return new CustomException(
            StatusCodes.Status401Unauthorized,
            ResponseCodeConstants.UNAUTHORIZED,
            "Unauthorized access!",
            detailMessage
        );
    }

}