namespace Fusion.API.Context
{
    public sealed class CompanyContext
    {
        public Guid UserId { get; init; }
        public Guid? CurrentCompanyId { get; init; }
        public bool IsSystemAdmin { get; init; }
        public HashSet<string> Permissions { get; } = new(StringComparer.OrdinalIgnoreCase);
    }

    public interface ICompanyContextAccessor
    {
        CompanyContext? Current { get; }
    }

    internal sealed class HttpCompanyContextAccessor : ICompanyContextAccessor
    {
        private readonly IHttpContextAccessor _http;
        public HttpCompanyContextAccessor(IHttpContextAccessor http) => _http = http;
        public CompanyContext? Current =>
            _http.HttpContext?.Items[nameof(CompanyContext)] as CompanyContext;
    }
}
