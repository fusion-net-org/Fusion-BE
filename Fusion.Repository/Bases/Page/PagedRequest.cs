

namespace Fusion.Repository.Bases.Page;

public class PagedRequest
{
    public int PageNumber { get; set; } = 1;
    public int PageSize { get; set; } = 10;
    //public string? Search { get; set; }
    public string? SortColumn { get; set; }
    public bool SortDescending { get; set; } = false;
}
