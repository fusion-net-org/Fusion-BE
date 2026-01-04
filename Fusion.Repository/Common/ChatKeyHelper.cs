

namespace Fusion.Repository.Common;

public static class ChatKeyHelper
{
    public static string BuildDirect(Guid a, Guid b)
        => "Dr_" + PairKeyHelper.Build(a, b);

    public static string BuildGroup(Guid conversationId)
        => "Gr_" + conversationId.ToString("N");

    // Direct anti-spam check: lấy pairKey từ DirectPairKey
    // - NEW: "Dr_{pairKey}"
    // - OLD: "{pairKey}" (backward compatible)
    public static bool TryExtractFriendPairKey(string? chatKey, out string pairKey)
    {
        pairKey = "";
        if (string.IsNullOrWhiteSpace(chatKey)) return false;

        if (chatKey.StartsWith("Dr_", StringComparison.Ordinal))
        {
            pairKey = chatKey.Substring(3);
            return !string.IsNullOrWhiteSpace(pairKey);
        }

        // old stored raw pairKey
        pairKey = chatKey;
        return true;
    }
}