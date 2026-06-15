using System.Globalization;
using System.Text;
using DesignCourt.Core;

namespace DesignCourt.Reporting;

public sealed class MarkdownEvaluationReportWriter
{
    public string Write(EvaluationReport report)
    {
        ArgumentNullException.ThrowIfNull(report);

        var metrics = report.Metrics;
        var builder = new StringBuilder();

        builder.AppendLine("# Design Court Evaluation");
        builder.AppendLine();
        builder.AppendLine($"Benchmark cases: {metrics.DocumentCount}");
        builder.AppendLine();
        builder.AppendLine("## Metrics");
        builder.AppendLine();
        builder.AppendLine("| Metric | Value |");
        builder.AppendLine("| --- | --- |");
        builder.AppendLine($"| Precision | {Format(metrics.Precision)} |");
        builder.AppendLine($"| Recall | {Format(metrics.Recall)} |");
        builder.AppendLine($"| F1 | {Format(metrics.F1)} |");
        builder.AppendLine($"| False-positive rate | {Format(metrics.FalsePositiveRate)} |");
        builder.AppendLine($"| True positives | {metrics.TruePositives} |");
        builder.AppendLine($"| False positives | {metrics.FalsePositives} |");
        builder.AppendLine($"| False negatives | {metrics.FalseNegatives} |");
        builder.AppendLine();
        builder.AppendLine("## Cases");
        builder.AppendLine();

        foreach (var caseEvaluation in report.Cases)
        {
            AppendCase(builder, caseEvaluation);
        }

        return builder.ToString();
    }

    private static void AppendCase(StringBuilder builder, CaseEvaluation caseEvaluation)
    {
        builder.AppendLine($"### `{caseEvaluation.Document}`");
        builder.AppendLine();
        builder.AppendLine($"- True positives: {caseEvaluation.TruePositives}");
        builder.AppendLine($"- False positives: {caseEvaluation.FalsePositives}");
        builder.AppendLine($"- False negatives: {caseEvaluation.FalseNegatives}");

        foreach (var missed in caseEvaluation.MissedExpected)
        {
            builder.AppendLine($"- Missed expected `{missed.Id}`: {missed.Title}");
        }

        foreach (var unexpected in caseEvaluation.UnexpectedFindings)
        {
            builder.AppendLine($"- Unexpected `{unexpected.Id}`: {unexpected.Title}");
        }

        builder.AppendLine();
    }

    private static string Format(double value)
    {
        return value.ToString("0.00", CultureInfo.InvariantCulture);
    }
}
