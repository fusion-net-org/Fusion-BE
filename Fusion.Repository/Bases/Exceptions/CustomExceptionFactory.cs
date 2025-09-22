using Microsoft.AspNetCore.Http;
using Fusion.Repository.Bases.Responses;

namespace Fusion.Repository.Bases.Exceptions;

public static class CustomExceptionFactory
{
    public static CustomException CreateInternalServerError()
    {
        return new CustomException(StatusCodes.Status500InternalServerError,
                                   ResponseCodeConstants.INTERNAL_SERVER_ERROR,
                                   ResponseMessages.INTERNAL_SERVER_ERROR);
    }

    public static CustomException CreateInternalServerError(string ex)
    {
        return new CustomException(StatusCodes.Status500InternalServerError,
                                   ResponseCodeConstants.INTERNAL_SERVER_ERROR,
                                   ResponseMessages.INTERNAL_SERVER_ERROR,
                                   ex);
    }

    public static CustomException CreateNotFoundError(string objectName)
    {
        return new CustomException(StatusCodes.Status404NotFound,
                                   ResponseCodeConstants.NOT_FOUND,
                                   ResponseMessages.NOT_FOUND.Replace("{0}", $"{objectName}"));
    }

    public static CustomException CreateBadRequestError(string message)
    {
        return new CustomException(StatusCodes.Status400BadRequest,
                                   ResponseCodeConstants.BAD_REQUEST,
                                   message);
    }

    public static Exception CreateForbiddenError()
    {
        return new CustomException(StatusCodes.Status403Forbidden,
                                   ResponseCodeConstants.FORBIDDEN,
                                   ResponseMessages.UNAUTHORIZED);
    }
}