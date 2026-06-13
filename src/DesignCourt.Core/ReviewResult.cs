namespace DesignCourt.Core;

public sealed record ReviewResult(
    ReviewedDocument Document,
    IReadOnlyList<JudgedFinding> Findings);
