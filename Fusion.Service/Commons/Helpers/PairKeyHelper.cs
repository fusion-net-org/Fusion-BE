

namespace Fusion.Service.Commons.Helpers;

public static class PairKeyHelper
{
    public static string Build(Guid a, Guid b)
    {
        var sa = a.ToString("N");
        var sb = b.ToString("N");
        return string.CompareOrdinal(sa, sb) < 0 ? $"{sa}_{sb}" : $"{sb}_{sa}";
    }
}
