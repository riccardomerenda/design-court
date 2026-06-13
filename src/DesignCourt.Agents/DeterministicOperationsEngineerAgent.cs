using DesignCourt.Core;

namespace DesignCourt.Agents;

public sealed class DeterministicOperationsEngineerAgent : IReviewAgent
{
    private static readonly string[] DestructiveTerms =
    [
        "drop",
        "delete",
        "remove",
        "destructive"
    ];

    private static readonly string[] RecoveryTerms =
    [
        "rollback",
        "roll back",
        "recovery",
        "restore",
        "revert"
    ];

    public string Name => "Operations Engineer";

    public AgentRole Role => AgentRole.OperationsEngineer;

    public Task<IReadOnlyList<CandidateFinding>> ReviewAsync(
        ReviewedDocument document,
        CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(document);

        var findings = new List<CandidateFinding>();

        foreach (var section in document.Sections)
        {
            cancellationToken.ThrowIfCancellationRequested();

            if (!LooksLikeMigrationSection(section)
                || !ContainsAny(section.Text, DestructiveTerms)
                || ContainsAny(section.Text, RecoveryTerms))
            {
                continue;
            }

            var quote = SelectEvidenceQuote(section);

            if (string.IsNullOrWhiteSpace(quote))
            {
                continue;
            }

            findings.Add(CreateMissingRollbackFinding(document, section, quote, findings.Count + 1));
        }

        return Task.FromResult<IReadOnlyList<CandidateFinding>>(findings);
    }

    private static CandidateFinding CreateMissingRollbackFinding(
        ReviewedDocument document,
        DocumentSection section,
        string quote,
        int sequence)
    {
        const string title = "Missing rollback strategy for database migration";

        return new CandidateFinding(
            Id: $"C-OPS-{sequence:000}",
            Fingerprint: FindingFingerprint.Create(FindingCategory.Operability, section.SectionId, title, quote),
            Title: title,
            Description: "The proposal describes a destructive database migration but does not define a rollback or recovery strategy.",
            RaisedBy: AgentRole.OperationsEngineer,
            Category: FindingCategory.Operability,
            Severity: Severity.High,
            ConfidenceBand: ConfidenceBand.High,
            Evidence:
            [
                new EvidenceItem(
                    Document: document.Path,
                    SectionId: section.SectionId,
                    SectionTitle: section.Title,
                    Quote: quote,
                    LineStart: section.LineStart,
                    LineEnd: section.LineStart,
                    SectionVerified: false,
                    QuoteVerified: false,
                    VerificationLevel: VerificationLevel.Unverified)
            ],
            Impact: "A failed migration could leave the system partially migrated and difficult to recover safely.",
            Recommendation: "Add an explicit rollback path, backup validation step, and recovery procedure for failed or partial migrations.",
            Verification: new VerificationStep(
                VerificationKind.Question,
                "What is the recovery procedure if the migration aborts after removing or transforming production data?"),
            Counterarguments: Array.Empty<string>());
    }

    private static bool LooksLikeMigrationSection(DocumentSection section)
    {
        return section.Title.Contains("migration", StringComparison.OrdinalIgnoreCase)
            || section.Text.Contains("migration", StringComparison.OrdinalIgnoreCase);
    }

    private static string SelectEvidenceQuote(DocumentSection section)
    {
        return section.Text
            .Split('\n', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
            .FirstOrDefault(line => ContainsAny(line, DestructiveTerms))
            ?? string.Empty;
    }

    private static bool ContainsAny(string value, IEnumerable<string> terms)
    {
        return terms.Any(term => value.Contains(term, StringComparison.OrdinalIgnoreCase));
    }
}
