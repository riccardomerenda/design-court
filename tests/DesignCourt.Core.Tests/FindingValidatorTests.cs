using DesignCourt.Core;

namespace DesignCourt.Core.Tests;

public sealed class FindingValidatorTests
{
    [Fact]
    public void Validate_accepts_reportable_finding_with_verified_evidence()
    {
        var finding = CreateFinding();

        var result = new FindingValidator().Validate(finding);

        Assert.True(result.IsValid);
        Assert.Empty(result.Errors);
    }

    [Fact]
    public void Validate_rejects_accepted_finding_without_verified_quote()
    {
        var finding = CreateFinding(evidence: VerifiedEvidence() with
        {
            QuoteVerified = false,
            VerificationLevel = VerificationLevel.SectionVerified
        });

        var result = new FindingValidator().Validate(finding);

        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, error => error.Contains("quote", StringComparison.OrdinalIgnoreCase));
    }

    [Fact]
    public void Validate_allows_needs_human_review_when_quote_is_not_verified()
    {
        var finding = CreateFinding(
            status: FindingStatus.NeedsHumanReview,
            evidence: VerifiedEvidence() with
            {
                QuoteVerified = false,
                VerificationLevel = VerificationLevel.SectionVerified
            });

        var result = new FindingValidator().Validate(finding);

        Assert.True(result.IsValid);
    }

    private static JudgedFinding CreateFinding(
        FindingStatus status = FindingStatus.Accepted,
        EvidenceItem? evidence = null)
    {
        return new JudgedFinding(
            Id: "F-001",
            Fingerprint: "sha256:missing-rollback-strategy",
            Title: "Missing rollback strategy for database migration",
            Description: "The proposal describes a destructive migration without a rollback strategy.",
            RaisedBy: AgentRole.OperationsEngineer,
            Category: FindingCategory.Operability,
            Severity: Severity.High,
            Status: status,
            ConfidenceBand: ConfidenceBand.High,
            Evidence: new[] { evidence ?? VerifiedEvidence() },
            Impact: "A failed migration could leave production data partially migrated.",
            Recommendation: "Add a rollback path, backup validation step, and recovery procedure.",
            Verification: new VerificationStep(
                VerificationKind.Question,
                "What is the recovery procedure if the migration aborts in production?"),
            Counterarguments: Array.Empty<string>(),
            JudgeVerdict: new JudgeVerdict(status, "The cited section lacks a recovery plan."));
    }

    private static EvidenceItem VerifiedEvidence()
    {
        return new EvidenceItem(
            Document: "payment-rfc.md",
            SectionId: "database-migration-plan",
            SectionTitle: "Database Migration Plan",
            Quote: "The migration will remove deprecated columns after data transformation.",
            LineStart: 42,
            LineEnd: 42,
            SectionVerified: true,
            QuoteVerified: true,
            VerificationLevel: VerificationLevel.QuoteVerified);
    }
}
