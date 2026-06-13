namespace DesignCourt.Core;

public sealed record JudgeVerdict(
    FindingStatus Decision,
    string Reason);
