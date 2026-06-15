namespace DesignCourt.Core;

public sealed record EvaluationCase(
    string Document,
    IReadOnlyList<ExpectedFinding> ExpectedFindings,
    IReadOnlyList<JudgedFinding> ActualFindings);
