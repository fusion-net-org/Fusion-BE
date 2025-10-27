
using Microsoft.EntityFrameworkCore;
using System.Linq.Dynamic.Core;

namespace Fusion.Repository.Bases.Page;

public static class IQueryableExtensions
{
    public static async Task<PagedResult<T>> ToPagedResultAsync<T>(
       this IQueryable<T> query,
       PagedRequest request,
       CancellationToken cancellationToken = default)
    {
        // total count
        var totalCount = await query.CountAsync(cancellationToken);

        // sort 
        if (!string.IsNullOrEmpty(request.SortColumn))
        {
            var sort = request.SortColumn + (request.SortDescending ? " desc" : " asc");
            query = query.OrderBy(sort);
        }
        else
        {
            var firstProperty = typeof(T).GetProperties().FirstOrDefault();
            if (firstProperty != null)
                query = query.OrderBy(firstProperty.Name);
        }

        // paging
        var items = await query
            .Skip((request.PageNumber - 1) * request.PageSize)
            .Take(request.PageSize)
            .ToListAsync(cancellationToken);

        return new PagedResult<T>
        {
            Items = items,
            TotalCount = totalCount,
            PageNumber = request.PageNumber,
            PageSize = request.PageSize
        };
    }
}
