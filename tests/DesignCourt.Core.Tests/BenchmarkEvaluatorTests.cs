using DesignCourt.Core;

namespace DesignCourt.Core.Tests;

public sealed class BenchmarkEvaluatorTests
{
    [Fact]
    public void Evaluate_reports_true_positive_when_finding_matches_expected()
    {
        var report = EvaluateSingleCase(
            ExpectedOperability("database-migration-plan"),
            ReportableFinding("database-migration-plan", FindingStatus.Accepted));

        Assert.Equal(1, report.Metrics.TruePositives);
        Assert.Equal(0, report.Metrics.FalsePositives);
        Assert.Equal(0, report.Metrics.FalseNegatives);
        Assert.Equal(1.0, report.Metrics.Precision, 6);
        Assert.Equal(1.0, report.Metrics.Recall, 6);
        Assert.Equal(1.0, report.Metrics.F1, 6);
        Assert.Equal(0.0, report.Metrics.FalsePositiveRate, 6);
    }

    [Fact]
    public void Evaluate_reports_false_negative_when_expected_finding_is_missing()
    {
        var report = EvaluateSingleCase(ExpectedOperability("database-migration-plan"));

        Assert.Equal(0, report.Metrics.TruePositives);
        Assert.Equal(1, report.Metrics.FalseNegatives);
        Assert.Equal(0.0, report.Metrics.Recall, 6);
        Assert.Equal(0.0, report.Metrics.F1, 6);
    }

    [Fact]
    public void Evaluate_counts_rejected_finding_as_missing_expected()
    {
        var report = EvaluateSingleCase(
            ExpectedOperability("database-migration-plan"),
            ReportableFinding("database-migration-plan", FindingStatus.Rejected));

        Assert.Equal(0, report.Metrics.TruePositives);
        Assert.Equal(0, report.Metrics.FalsePositives);
        Assert.Equal(1, report.Metrics.FalseNegatives);
    }

    [Fact]
    public void Evaluate_reports_false_positive_for_clean_control_with_finding()
    {
        var report = new BenchmarkEvaluator().Evaluate(
        [
            new EvaluationCase(
                "clean.md",
                Array.Empty<ExpectedFinding>(),
                [ReportableFinding("database-migration-plan", FindingStatus.Accepted)])
        ]);

        Assert.Equal(0, report.Metrics.TruePositives);
        Assert.Equal(1, report.Metrics.FalsePositives);
        Assert.Equal(0.0, report.Metrics.Precision, 6);
        Assert.Equal(1, report.Metrics.DocumentsWithFalsePositives);
        Assert.Equal(1.0, report.Metrics.FalsePositiveRate, 6);
    }

    [Fact]
    public void Evaluate_does_not_match_expected_in_a_different_section()
    {
        var report = EvaluateSingleCase(
            ExpectedOperability("database-migration-plan"),
            ReportableFinding("observability", FindingStatus.Accepted));

        Assert.Equal(0, report.Metrics.TruePositives);
        Assert.Equal(1, report.Metrics.FalsePositives);
        Assert.Equal(1, report.Metrics.FalseNegatives);
    }

    private static EvaluationReport EvaluateSingleCase(
        ExpectedFinding expected,
        params JudgedFinding[] actual)
    {
        return new BenchmarkEvaluator().Evaluate(
        [
            new EvaluationCase("doc.md", [expected], actual)
        ]);
    }

    private static ExpectedFinding ExpectedOperability(string sectionId)
    {
        return new ExpectedFinding(
            Id: "EXP-001",
            Title: "Missing rollback strategy for database migration",
            Category: FindingCategory.Operability,
            Severity: Severity.High,
            SectionId: sectionId,
            Quote: "The migration will remove deprecated columns after data transformation.",
            Reason: "Destructive migration without a rollback strategy.");
    }

    private static JudgedFinding ReportableFinding(string sectionId, FindingStatus status)
    {
        return new JudgedFinding(
            Id: "F-OPS-001",
            Fingerprint: "sha256:test",
            Title: "Missing rollback strategy for database migration",
            Description: "Description.",
            RaisedBy: AgentRole.OperationsEngineer,
            Category: FindingCategory.Operability,
            Severity: Severity.High,
            Status: status,
            ConfidenceBand: ConfidenceBand.High,
            Evidence:
            [
                new EvidenceItem(
                    Document: "doc.md",
                    SectionId: sectionId,
                    SectionTitle: "Section",
                    Quote: "The migration will remove deprecated columns after data transformation.",
                    LineStart: 1,
                    LineEnd: 1,
                    SectionVerified: true,
                    QuoteVerified: true,
                    VerificationLevel: VerificationLevel.QuoteVerified)
            ],
            Impact: "Impact.",
            Recommendation: "Recommendation.",
            Verification: new VerificationStep(VerificationKind.Question, "Question?"),
            Counterarguments: Array.Empty<string>(),
            JudgeVerdict: new JudgeVerdict(status, "Reason."));
    }
}
