

using Fusion.Repository.Entities;

namespace Fusion.Repository.Bases.Page.FeatureCatalog;

public class FeatureCatalogPagedRequest : PagedRequest
{
    public string? Keyword { get; set; }
    public bool? IsActive { get; set; }
    public string? Category { get; set; }

    public static readonly Dictionary<string, string> SortMap = new(StringComparer.OrdinalIgnoreCase)
    {
        ["category"] = nameof(Feature.Category),
        ["createdAt"] = nameof(Feature.CreatedAt),
    };
}
