using Microsoft.AspNetCore.Http;
namespace Fusion.Service.Commons.BaseResponses;
public class PagedResponseModel<T> : ResponseModel<T>
{
    public int PageNumber { get; set; }
    public int PageSize { get; set; }
    public int TotalCount { get; set; } = 0;

    public PagedResponseModel(int statusCode, T? data, string? message, int totalCount = 0, int pageNumber = 1, int pageSize = 10)
    {
        this.StatusCode = statusCode;
        this.Succeeded = true;
        this.Data = data;
        this.Message = message;
        this.PageNumber = pageNumber;
        this.PageSize = pageSize;
        this.TotalCount = totalCount;
    }

    //public static PagedResponseModel<T> OkResponseModel(T? data, string? message = null, int pageNumber = 1, int pageSize = 10)
    //{
    //    return new PagedResponseModel<T>(StatusCodes.Status200OK, data, message, pageNumber, pageSize);
    //}

    public static PagedResponseModel<T> OkResponseModel(T? data, string? message, int totalCount = 0, int pageNumber = 1, int pageSize = 10)
    {
        return new PagedResponseModel<T>(StatusCodes.Status200OK, data, message, totalCount, pageNumber, pageSize);
    }
}
