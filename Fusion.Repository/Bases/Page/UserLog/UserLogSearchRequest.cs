
namespace Fusion.Repository.Bases.Page.UserLog;

public class DateRangeRequest
{
    public DateOnly? From { get; set; }
    public DateOnly? To { get; set; }
}
public class UserLogSearchRequest : PagedRequest
{
    public string? Keyword { get; set; }
    public DateRangeRequest? DateRange { get; set; }
}
