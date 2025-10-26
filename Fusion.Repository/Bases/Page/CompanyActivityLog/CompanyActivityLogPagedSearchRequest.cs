

namespace Fusion.Repository.Bases.Page.CompanyActivityLog;

public class DateRangeRequest
{
    public DateOnly? From { get; set; }
    public DateOnly? To { get; set; }
}

public class CompanyActivityLogPagedSearchRequest : PagedRequest
{
    public string? KeyWord { get; set; }
    public DateRangeRequest? DateRange { get; set; }
}
