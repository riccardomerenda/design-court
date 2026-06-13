namespace DesignCourt.Core;

public sealed record CandidateFinding(
    string Id,
    string Fingerprint,
    string Title,
    string Description,
    AgentRole RaisedBy,
    FindingCategory Category,
    Severity Severity,
    ConfidenceBand ConfidenceBand,
    IReadOnlyList<EvidenceItem> Evidence,
    string Impact,
    string Recommendation,
    VerificationStep Verification,
    IReadOnlyList<string> Counterarguments);
