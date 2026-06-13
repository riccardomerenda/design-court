using DesignCourt.Parsing;

namespace DesignCourt.Parsing.Tests;

public sealed class MarkdownDocumentParserTests
{
    [Fact]
    public void Parse_creates_stable_section_ids_and_disambiguates_duplicates()
    {
        var document = new MarkdownDocumentParser().Parse("payment-rfc.md", SampleDocument());

        Assert.Collection(
            document.Sections,
            section => Assert.Equal("payment-rfc", section.SectionId),
            section => Assert.Equal("database-migration-plan", section.SectionId),
            section => Assert.Equal("database-migration-plan-2", section.SectionId));
    }

    [Fact]
    public void Parse_tracks_section_line_ranges()
    {
        var document = new MarkdownDocumentParser().Parse("payment-rfc.md", SampleDocument());

        var section = document.Sections[1];

        Assert.Equal(4, section.LineStart);
        Assert.Equal(6, section.LineEnd);
    }

    private static string SampleDocument()
    {
        return """
            # Payment RFC
            Intro

            ## Database Migration Plan
            The migration will remove deprecated columns after data transformation.

            ## Database Migration Plan
            Second section
            """;
    }
}
