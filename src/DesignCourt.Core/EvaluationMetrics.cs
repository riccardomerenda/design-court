namespace DesignCourt.Core;

public sealed record EvaluationMetrics(
    int TruePositives,
    int FalsePositives,
    int FalseNegatives,
    int DocumentCount,
    int DocumentsWithFalsePositives,
    double Precision,
    double Recall,
    double F1,
    double FalsePositiveRate);
