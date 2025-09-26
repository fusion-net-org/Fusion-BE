namespace Fusion.Repository.Bases.Exceptions;

public class CustomException : Exception
{
    public int StatusCode { get; }
    public string ErrorCode { get; }
    public string ErrorMessage { get; }
    public string? DetailMessage { get; }

    public CustomException(int statusCode, string errorCode, string errorMessage, string? detailMessage = null)
        : base(errorMessage)
    {
        StatusCode = statusCode;
        ErrorCode = errorCode;
        ErrorMessage = errorMessage;
        DetailMessage = detailMessage;
    }
}
