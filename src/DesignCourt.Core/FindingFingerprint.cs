using System.Security.Cryptography;
using System.Text;

namespace DesignCourt.Core;

public static class FindingFingerprint
{
    public static string Create(
        FindingCategory category,
        string sectionId,
        string title,
        string quote)
    {
        var normalized = string.Join(
            "\n",
            category.ToString().ToLowerInvariant(),
            Normalize(sectionId),
            Normalize(title),
            Normalize(quote));

        var hash = SHA256.HashData(Encoding.UTF8.GetBytes(normalized));
        return $"sha256:{Convert.ToHexString(hash).ToLowerInvariant()}";
    }

    private static string Normalize(string value)
    {
        return string.Join(
            ' ',
            value.Trim()
                .ToLowerInvariant()
                .Split((char[]?)null, StringSplitOptions.RemoveEmptyEntries));
    }
}
