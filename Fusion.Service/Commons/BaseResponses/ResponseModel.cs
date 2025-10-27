using System.Text.Json.Serialization;

namespace Fusion.Service.Commons.BaseResponses
{
    public class ResponseModel<T>
    {
        public bool Succeeded { get; set; } = false;
        public int StatusCode { get; set; }
        public string? Message { get; set; }
        public T? Data { get; set; }

        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public object? ErrorData { get; set; }

        // OK response
        public static ResponseModel<T> Ok(T data, string? message = "")
        {
            return new ResponseModel<T>
            {
                Succeeded = true,
                StatusCode = 200,
                Message = message,
                Data = data
            };
        }

        // Error response
        public static ResponseModel<T> Error(int statusCode, string message, object? additionalData = null)
        {
            return new ResponseModel<T>
            {
                Succeeded = false,
                StatusCode = statusCode,
                Message = message,
                Data = default,
                ErrorData = additionalData
            };
        }
    }
}
