namespace DesignCourt.Core;

public sealed record ExpectedFinding(
    string Id,
    string Title,
    FindingCategory Category,
    Severity Severity,
    string SectionId,
    string Quote,
    string Reason);
