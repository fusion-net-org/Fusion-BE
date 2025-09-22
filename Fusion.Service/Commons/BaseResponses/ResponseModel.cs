using System.Text.Json.Serialization;
using Microsoft.AspNetCore.Http;
namespace Fusion.Service.Commons.BaseResponses;

public class ResponseModel<T>
{
    public T? Data { get; set; }

    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public object? AdditionalData { get; set; }
    public string? Message { get; set; }
    public bool Succeeded { get; set; } = false;
    public int StatusCode { get; set; }

    public ResponseModel()
    {

    }

    // Full properties
    public ResponseModel(int statusCode, T? data = default, object? additionalData = null, string? message = null)
    {
        StatusCode = statusCode;
        Succeeded = true;
        Data = data;
        AdditionalData = additionalData;
        Message = message;
    }

    // Without additionalData prop
    public ResponseModel(int statusCode, T? data = default, string? message = null)
    {
        StatusCode = statusCode;
        Succeeded = true;
        Data = data;
        Message = message;
    }

    public ResponseModel(int statusCode, T? data = default, bool succeeded = false, string? message = null)
    {
        StatusCode = statusCode;
        Succeeded = false;
        Data = data;
        Message = message;
    }

    // OK Response
    public static ResponseModel<T> OkResponseModel(T? data, object? additionalData = null, string? message = null)
    {
        return new ResponseModel<T>(StatusCodes.Status200OK, data, message);
    }

    // Error Response
    public static ResponseModel<T> ErrorResponseModel(int statusCode, string message, object? additionalData = null)
    {
        return new ResponseModel<T>(statusCode, default, false, message);
    }
}
