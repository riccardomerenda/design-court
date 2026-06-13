using DesignCourt.Core;

namespace DesignCourt.Parsing;

public sealed class MarkdownEvidenceVerifier : IEvidenceVerifier
{
    public EvidenceVerificationResult Verify(ReviewedDocument document, EvidenceItem evidence)
    {
        ArgumentNullException.ThrowIfNull(document);
        ArgumentNullException.ThrowIfNull(evidence);

        var section = document.Sections.FirstOrDefault(candidate =>
            string.Equals(candidate.SectionId, evidence.SectionId, StringComparison.Ordinal));

        if (section is null)
        {
            return new EvidenceVerificationResult(
                evidence with
                {
                    SectionVerified = false,
                    QuoteVerified = false,
                    VerificationLevel = VerificationLevel.Unverified
                },
                SectionFound: false,
                QuoteFound: false);
        }

        var quote = NormalizeNewlines(evidence.Quote);
        var exactQuoteFound = !string.IsNullOrWhiteSpace(quote)
            && section.Text.Contains(quote, StringComparison.Ordinal);
        var fuzzyQuoteFound = !exactQuoteFound
            && ContainsNormalized(section.Text, quote);
        var lineRange = exactQuoteFound
            ? FindQuoteLineRange(section, quote)
            : (evidence.LineStart, evidence.LineEnd);

        var verifiedEvidence = evidence with
        {
            Document = document.Path,
            SectionTitle = section.Title,
            LineStart = lineRange.LineStart,
            LineEnd = lineRange.LineEnd,
            SectionVerified = true,
            QuoteVerified = exactQuoteFound,
            VerificationLevel = exactQuoteFound
                ? VerificationLevel.QuoteVerified
                : fuzzyQuoteFound
                    ? VerificationLevel.QuoteFuzzyVerified
                    : VerificationLevel.SectionVerified
        };

        return new EvidenceVerificationResult(
            verifiedEvidence,
            SectionFound: true,
            QuoteFound: exactQuoteFound || fuzzyQuoteFound);
    }

    private static (int LineStart, int LineEnd) FindQuoteLineRange(DocumentSection section, string quote)
    {
        var index = section.Text.IndexOf(quote, StringComparison.Ordinal);

        if (index < 0)
        {
            return (section.LineStart, section.LineEnd);
        }

        var linesBeforeQuote = section.Text[..index].Count(character => character == '\n');
        var quoteLineCount = quote.Count(character => character == '\n');
        var lineStart = section.LineStart + linesBeforeQuote;

        return (lineStart, lineStart + quoteLineCount);
    }

    private static bool ContainsNormalized(string haystack, string needle)
    {
        if (string.IsNullOrWhiteSpace(needle))
        {
            return false;
        }

        return NormalizeForFuzzyMatch(haystack)
            .Contains(NormalizeForFuzzyMatch(needle), StringComparison.Ordinal);
    }

    private static string NormalizeForFuzzyMatch(string value)
    {
        return string.Concat(
            NormalizeNewlines(value)
                .Where(character => !char.IsWhiteSpace(character)))
            .ToLowerInvariant();
    }

    private static string NormalizeNewlines(string value)
    {
        return value.Replace("\r\n", "\n", StringComparison.Ordinal)
            .Replace('\r', '\n');
    }
}
