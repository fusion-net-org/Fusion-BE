
namespace Fusion.Repository.Bases.Page.Project
{
    public class DateRangeRequest
    {
        public DateOnly? From { get; set; }
        public DateOnly? To { get; set; }
    }
    public class ProjectSearchRequest : PagedRequest
    {
        public string? Keyword { get; set; }
        public string? Status { get; set; }
        public Guid? CompanyId { get; set; }
        public DateRangeRequest? DateRange { get; set; }
    }
}
