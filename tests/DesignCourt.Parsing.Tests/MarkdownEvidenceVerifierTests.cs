using DesignCourt.Core;
using DesignCourt.Parsing;

namespace DesignCourt.Parsing.Tests;

public sealed class MarkdownEvidenceVerifierTests
{
    [Fact]
    public void Verify_marks_exact_quote_as_verified_and_sets_line_range()
    {
        var document = new MarkdownDocumentParser().Parse("payment-rfc.md", SampleDocument());
        var evidence = UnverifiedEvidence("The migration will remove deprecated columns after data transformation.");

        var result = new MarkdownEvidenceVerifier().Verify(document, evidence);

        Assert.True(result.SectionFound);
        Assert.True(result.QuoteFound);
        Assert.True(result.Evidence.SectionVerified);
        Assert.True(result.Evidence.QuoteVerified);
        Assert.Equal(VerificationLevel.QuoteVerified, result.Evidence.VerificationLevel);
        Assert.Equal(5, result.Evidence.LineStart);
        Assert.Equal(5, result.Evidence.LineEnd);
    }

    [Fact]
    public void Verify_marks_whitespace_normalized_quote_as_fuzzy_verified()
    {
        var document = new MarkdownDocumentParser().Parse("payment-rfc.md", SampleDocument());
        var evidence = UnverifiedEvidence("The migration will remove deprecated columns after data\ntransformation.");

        var result = new MarkdownEvidenceVerifier().Verify(document, evidence);

        Assert.True(result.SectionFound);
        Assert.True(result.QuoteFound);
        Assert.False(result.Evidence.QuoteVerified);
        Assert.Equal(VerificationLevel.QuoteFuzzyVerified, result.Evidence.VerificationLevel);
    }

    [Fact]
    public void Verify_marks_missing_section_as_unverified()
    {
        var document = new MarkdownDocumentParser().Parse("payment-rfc.md", SampleDocument());
        var evidence = UnverifiedEvidence("Missing quote") with
        {
            SectionId = "unknown-section"
        };

        var result = new MarkdownEvidenceVerifier().Verify(document, evidence);

        Assert.False(result.SectionFound);
        Assert.False(result.QuoteFound);
        Assert.Equal(VerificationLevel.Unverified, result.Evidence.VerificationLevel);
    }

    private static EvidenceItem UnverifiedEvidence(string quote)
    {
        return new EvidenceItem(
            Document: "payment-rfc.md",
            SectionId: "database-migration-plan",
            SectionTitle: "",
            Quote: quote,
            LineStart: 0,
            LineEnd: 0,
            SectionVerified: false,
            QuoteVerified: false,
            VerificationLevel: VerificationLevel.Unverified);
    }

    private static string SampleDocument()
    {
        return """
            # Payment RFC
            Intro

            ## Database Migration Plan
            The migration will remove deprecated columns after data transformation.
            This line is outside the cited quote.
            """;
    }
}
