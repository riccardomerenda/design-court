namespace DesignCourt.Core;

public sealed record EvaluationReport(
    IReadOnlyList<CaseEvaluation> Cases,
    EvaluationMetrics Metrics);

public sealed record CaseEvaluation(
    string Document,
    int TruePositives,
    int FalsePositives,
    int FalseNegatives,
    IReadOnlyList<ExpectedFinding> MatchedExpected,
    IReadOnlyList<ExpectedFinding> MissedExpected,
    IReadOnlyList<JudgedFinding> UnexpectedFindings);
