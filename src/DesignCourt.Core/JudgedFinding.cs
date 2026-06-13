namespace DesignCourt.Core;

public sealed record JudgedFinding(
    string Id,
    string Fingerprint,
    string Title,
    string Description,
    AgentRole RaisedBy,
    FindingCategory Category,
    Severity Severity,
    FindingStatus Status,
    ConfidenceBand ConfidenceBand,
    IReadOnlyList<EvidenceItem> Evidence,
    string Impact,
    string Recommendation,
    VerificationStep Verification,
    IReadOnlyList<string> Counterarguments,
    JudgeVerdict JudgeVerdict);
