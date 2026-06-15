using DesignCourt.Core;
using DesignCourt.Parsing;

namespace DesignCourt.Agents.Tests;

public sealed class BenchmarkEvaluationFunctionalTests
{
    [Fact]
    public async Task Evaluate_reports_perfect_metrics_for_seeded_corpus()
    {
        var cases = new[]
        {
            await BuildCaseAsync(
                Path.Combine("samples", "rfcs", "payment-rfc-missing-rollback.md"),
                [
                    new ExpectedFinding(
                        Id: "EXP-001",
                        Title: "Missing rollback strategy for database migration",
                        Category: FindingCategory.Operability,
                        Severity: Severity.High,
                        SectionId: "database-migration-plan",
                        Quote: "The migration will remove deprecated columns after data transformation.",
                        Reason: "The RFC describes a destructive schema migration but does not define a rollback or recovery procedure.")
                ]),
            await BuildCaseAsync(
                Path.Combine("samples", "clean-controls", "payment-rfc-clean.md"),
                [])
        };

        var report = new BenchmarkEvaluator().Evaluate(cases);

        Assert.Equal(1, report.Metrics.TruePositives);
        Assert.Equal(0, report.Metrics.FalsePositives);
        Assert.Equal(0, report.Metrics.FalseNegatives);
        Assert.Equal(1.0, report.Metrics.Precision, 6);
        Assert.Equal(1.0, report.Metrics.Recall, 6);
        Assert.Equal(1.0, report.Metrics.F1, 6);
        Assert.Equal(0.0, report.Metrics.FalsePositiveRate, 6);
    }

    private static async Task<EvaluationCase> BuildCaseAsync(
        string relativePath,
        IReadOnlyList<ExpectedFinding> expectedFindings)
    {
        var repositoryRoot = FindRepositoryRoot();
        var documentPath = Path.Combine(repositoryRoot, relativePath);
        var content = await File.ReadAllTextAsync(documentPath);
        var document = new MarkdownDocumentParser().Parse(
            Path.GetRelativePath(repositoryRoot, documentPath),
            content);

        var workflow = new LocalReviewWorkflow(
            [new DeterministicOperationsEngineerAgent()],
            new EvidenceGatedJudgeAgent(),
            new MarkdownEvidenceVerifier());

        var result = await workflow.RunAsync(document, CancellationToken.None);

        return new EvaluationCase(document.Path, expectedFindings, result.Findings);
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
