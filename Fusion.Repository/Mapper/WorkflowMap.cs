using System.Text.Json;

namespace Fusion.Repository.Repositories;

public static class WorkflowMap
{
    // Chuẩn hoá string, giới hạn về 3 giá trị hợp lệ
    public static string NormalizeType(string? t) =>
        (t ?? "").Trim().ToLowerInvariant() switch
        {
            "success" => "success",
            "failure" => "failure",
            "optional" => "optional",
            _ => "optional"
        };

    public static string ToJson(IEnumerable<string>? list)
        => list == null ? "[]" : System.Text.Json.JsonSerializer.Serialize(list);

    public static List<string> ParseList(string? json)
    {
        if (string.IsNullOrWhiteSpace(json)) return new();
        try { return System.Text.Json.JsonSerializer.Deserialize<List<string>>(json!) ?? new(); }
        catch { return new(); }
    }

    public static string? NormalizeHex(string? hex)
    {
        if (string.IsNullOrWhiteSpace(hex)) return null;
        hex = hex!.Trim();
        if (!hex.StartsWith("#")) hex = "#" + hex;
        return hex.Length is 4 or 7 ? hex : null;
    }
}
