

using System.ComponentModel.DataAnnotations;

namespace Fusion.Service.ViewModels.FeatureCatalog.Requests;

public class FeatureUpdateRequest
{
    public Guid Id { get; set; }

    public string Code { get; set; } = string.Empty;

    public string Name { get; set; } = string.Empty;

    public string? Description { get; set; }

    public string? Category { get; set; }

    public bool IsActive { get; set; } = true;
}
