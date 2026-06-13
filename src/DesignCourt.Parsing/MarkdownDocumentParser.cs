using System.Text;
using DesignCourt.Core;

namespace DesignCourt.Parsing;

public sealed class MarkdownDocumentParser
{
    public ReviewedDocument Parse(string path, string content)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(path);
        ArgumentNullException.ThrowIfNull(content);

        var normalizedContent = NormalizeNewlines(content);
        var lines = normalizedContent.Split('\n');
        var headings = FindHeadings(lines);

        if (headings.Count == 0)
        {
            return new ReviewedDocument(
                path,
                normalizedContent,
                new[]
                {
                    new DocumentSection(
                        "document",
                        "Document",
                        0,
                        1,
                        Math.Max(lines.Length, 1),
                        normalizedContent)
                });
        }

        var slugCounts = new Dictionary<string, int>(StringComparer.Ordinal);
        var sections = new List<DocumentSection>();

        for (var index = 0; index < headings.Count; index++)
        {
            var heading = headings[index];
            var nextLine = index + 1 < headings.Count
                ? headings[index + 1].LineNumber - 1
                : lines.Length;
            var sectionText = JoinLines(lines, heading.LineNumber, nextLine);
            var sectionId = CreateUniqueSlug(heading.Title, slugCounts);

            sections.Add(new DocumentSection(
                sectionId,
                heading.Title,
                heading.Level,
                heading.LineNumber,
                nextLine,
                sectionText));
        }

        return new ReviewedDocument(path, normalizedContent, sections);
    }

    private static IReadOnlyList<Heading> FindHeadings(string[] lines)
    {
        var headings = new List<Heading>();

        for (var index = 0; index < lines.Length; index++)
        {
            if (TryParseHeading(lines[index], out var level, out var title))
            {
                headings.Add(new Heading(index + 1, level, title));
            }
        }

        return headings;
    }

    private static bool TryParseHeading(string line, out int level, out string title)
    {
        level = 0;
        title = string.Empty;

        while (level < line.Length && line[level] == '#')
        {
            level++;
        }

        if (level is < 1 or > 6 || level >= line.Length || line[level] != ' ')
        {
            return false;
        }

        title = line[level..].Trim().TrimEnd('#').Trim();
        return title.Length > 0;
    }

    private static string JoinLines(string[] lines, int lineStart, int lineEnd)
    {
        var builder = new StringBuilder();

        for (var lineNumber = lineStart; lineNumber <= lineEnd; lineNumber++)
        {
            if (lineNumber > lineStart)
            {
                builder.Append('\n');
            }

            builder.Append(lines[lineNumber - 1]);
        }

        return builder.ToString();
    }

    private static string CreateUniqueSlug(string title, Dictionary<string, int> slugCounts)
    {
        var slug = Slugify(title);

        if (!slugCounts.TryGetValue(slug, out var count))
        {
            slugCounts[slug] = 1;
            return slug;
        }

        count++;
        slugCounts[slug] = count;
        return $"{slug}-{count}";
    }

    private static string Slugify(string value)
    {
        var builder = new StringBuilder();
        var previousWasSeparator = false;

        foreach (var character in value.Trim().ToLowerInvariant())
        {
            if (char.IsAsciiLetterOrDigit(character))
            {
                builder.Append(character);
                previousWasSeparator = false;
                continue;
            }

            if (char.IsWhiteSpace(character) || character is '-' or '_')
            {
                if (!previousWasSeparator && builder.Length > 0)
                {
                    builder.Append('-');
                    previousWasSeparator = true;
                }
            }
        }

        return builder.ToString().Trim('-') is { Length: > 0 } slug
            ? slug
            : "section";
    }

    private static string NormalizeNewlines(string value)
    {
        return value.Replace("\r\n", "\n", StringComparison.Ordinal)
            .Replace('\r', '\n');
    }

    private sealed record Heading(int LineNumber, int Level, string Title);
}
