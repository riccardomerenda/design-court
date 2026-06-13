using DesignCourt.Agents;
using DesignCourt.Core;
using DesignCourt.Parsing;

namespace DesignCourt.Agents.Tests;

public sealed class LocalReviewWorkflowFunctionalTests
{
    [Fact]
    public async Task RunAsync_accepts_missing_rollback_finding_for_seeded_rfc()
    {
        var document = ParseSample("samples", "rfcs", "payment-rfc-missing-rollback.md");
        var workflow = CreateWorkflow();

        var result = await workflow.RunAsync(document, CancellationToken.None);

        var finding = Assert.Single(result.Findings);
        Assert.Equal("Missing rollback strategy for database migration", finding.Title);
        Assert.Equal(FindingStatus.Accepted, finding.Status);
        Assert.Equal(Severity.High, finding.Severity);
        Assert.Equal(AgentRole.OperationsEngineer, finding.RaisedBy);

        var evidence = Assert.Single(finding.Evidence);
        Assert.Equal("database-migration-plan", evidence.SectionId);
        Assert.True(evidence.SectionVerified);
        Assert.True(evidence.QuoteVerified);
        Assert.Equal(VerificationLevel.QuoteVerified, evidence.VerificationLevel);
    }

    [Fact]
    public async Task RunAsync_produces_no_findings_for_clean_control()
    {
        var document = ParseSample("samples", "clean-controls", "payment-rfc-clean.md");
        var workflow = CreateWorkflow();

        var result = await workflow.RunAsync(document, CancellationToken.None);

        Assert.Empty(result.Findings);
    }

    private static LocalReviewWorkflow CreateWorkflow()
    {
        return new LocalReviewWorkflow(
            [new DeterministicOperationsEngineerAgent()],
            new EvidenceGatedJudgeAgent(),
            new MarkdownEvidenceVerifier());
    }

    private static ReviewedDocument ParseSample(params string[] pathParts)
    {
        var repositoryRoot = FindRepositoryRoot();
        var samplePath = Path.Combine([repositoryRoot, .. pathParts]);
        var content = File.ReadAllText(samplePath);

        return new MarkdownDocumentParser().Parse(
            Path.GetRelativePath(repositoryRoot, samplePath),
            content);
    }

    private static string FindRepositoryRoot()
    {
        var directory = new DirectoryInfo(AppContext.BaseDirectory);

        while (directory is not null)
        {
            if (File.Exists(Path.Combine(directory.FullName, "DesignCourt.slnx")))
            {
                return directory.FullName;
            }

            directory = directory.Parent;
        }

        throw new InvalidOperationException("Could not locate the repository root.");
    }
}
