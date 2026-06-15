namespace DesignCourt.Core;

public sealed class BenchmarkEvaluator
{
    public EvaluationReport Evaluate(IReadOnlyList<EvaluationCase> cases)
    {
        ArgumentNullException.ThrowIfNull(cases);

        var caseEvaluations = cases.Select(EvaluateCase).ToArray();

        var truePositives = caseEvaluations.Sum(evaluation => evaluation.TruePositives);
        var falsePositives = caseEvaluations.Sum(evaluation => evaluation.FalsePositives);
        var falseNegatives = caseEvaluations.Sum(evaluation => evaluation.FalseNegatives);
        var documentCount = caseEvaluations.Length;
        var documentsWithFalsePositives = caseEvaluations.Count(evaluation => evaluation.FalsePositives > 0);

        var precision = Ratio(truePositives, truePositives + falsePositives);
        var recall = Ratio(truePositives, truePositives + falseNegatives);
        var f1 = precision + recall == 0d
            ? 0d
            : 2d * precision * recall / (precision + recall);
        var falsePositiveRate = documentCount == 0
            ? 0d
            : (double)documentsWithFalsePositives / documentCount;

        var metrics = new EvaluationMetrics(
            truePositives,
            falsePositives,
            falseNegatives,
            documentCount,
            documentsWithFalsePositives,
            precision,
            recall,
            f1,
            falsePositiveRate);

        return new EvaluationReport(caseEvaluations, metrics);
    }

    private static CaseEvaluation EvaluateCase(EvaluationCase evaluationCase)
    {
        var reportableFindings = evaluationCase.ActualFindings
            .Where(finding => FindingValidator.IsReportable(finding.Status))
            .ToArray();

        var matched = new bool[reportableFindings.Length];
        var matchedExpected = new List<ExpectedFinding>();
        var missedExpected = new List<ExpectedFinding>();

        foreach (var expected in evaluationCase.ExpectedFindings)
        {
            var matchIndex = FindMatch(expected, reportableFindings, matched);

            if (matchIndex < 0)
            {
                missedExpected.Add(expected);
                continue;
            }

            matched[matchIndex] = true;
            matchedExpected.Add(expected);
        }

        var unexpectedFindings = reportableFindings
            .Where((_, index) => !matched[index])
            .ToArray();

        return new CaseEvaluation(
            evaluationCase.Document,
            TruePositives: matchedExpected.Count,
            FalsePositives: unexpectedFindings.Length,
            FalseNegatives: missedExpected.Count,
            MatchedExpected: matchedExpected,
            MissedExpected: missedExpected,
            UnexpectedFindings: unexpectedFindings);
    }

    private static int FindMatch(
        ExpectedFinding expected,
        IReadOnlyList<JudgedFinding> findings,
        bool[] matched)
    {
        for (var index = 0; index < findings.Count; index++)
        {
            if (!matched[index] && Matches(expected, findings[index]))
            {
                return index;
            }
        }

        return -1;
    }

    private static bool Matches(ExpectedFinding expected, JudgedFinding finding)
    {
        return finding.Category == expected.Category
            && finding.Evidence.Any(evidence =>
                string.Equals(evidence.SectionId, expected.SectionId, StringComparison.Ordinal));
    }

    private static double Ratio(int numerator, int denominator)
    {
        return denominator == 0 ? 1d : (double)numerator / denominator;
    }
}
