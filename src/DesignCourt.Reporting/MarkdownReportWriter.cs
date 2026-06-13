using System.Text;
using DesignCourt.Core;

namespace DesignCourt.Reporting;

public sealed class MarkdownReportWriter
{
    public string Write(ReviewResult result)
    {
        ArgumentNullException.ThrowIfNull(result);

        var builder = new StringBuilder();
        builder.AppendLine("# Design Court Report");
        builder.AppendLine();
        builder.AppendLine($"Document: `{result.Document.Path}`");
        builder.AppendLine($"Sections parsed: {result.Document.Sections.Count}");
        builder.AppendLine($"Reportable findings: {result.Findings.Count(FindingIsReportable)}");
        builder.AppendLine();

        var reportableFindings = result.Findings
            .Where(FindingIsReportable)
            .OrderBy(finding => finding.Severity)
            .ThenBy(finding => finding.Id, StringComparer.Ordinal)
            .ToArray();

        if (reportableFindings.Length == 0)
        {
            builder.AppendLine("No reportable findings were produced.");
            return builder.ToString();
        }

        foreach (var finding in reportableFindings)
        {
            AppendFinding(builder, finding);
        }

        return builder.ToString();
    }

    private static bool FindingIsReportable(JudgedFinding finding)
    {
        return FindingValidator.IsReportable(finding.Status);
    }

    private static void AppendFinding(StringBuilder builder, JudgedFinding finding)
    {
        builder.AppendLine($"## {finding.Id}: {finding.Title}");
        builder.AppendLine();
        builder.AppendLine($"- Severity: `{finding.Severity}`");
        builder.AppendLine($"- Status: `{finding.Status}`");
        builder.AppendLine($"- Raised by: `{finding.RaisedBy}`");
        builder.AppendLine($"- Confidence: `{finding.ConfidenceBand}`");
        builder.AppendLine();
        builder.AppendLine(finding.Description);
        builder.AppendLine();
        builder.AppendLine("### Evidence");
        builder.AppendLine();

        foreach (var evidence in finding.Evidence)
        {
            builder.AppendLine($"- `{evidence.SectionId}` lines {evidence.LineStart}-{evidence.LineEnd}: \"{evidence.Quote}\"");
        }

        builder.AppendLine();
        builder.AppendLine("### Recommendation");
        builder.AppendLine();
        builder.AppendLine(finding.Recommendation);
        builder.AppendLine();
        builder.AppendLine("### Verification");
        builder.AppendLine();
        builder.AppendLine($"`{finding.Verification.Kind}`: {finding.Verification.Text}");
        builder.AppendLine();
        builder.AppendLine("### Judge Verdict");
        builder.AppendLine();
        builder.AppendLine(finding.JudgeVerdict.Reason);
        builder.AppendLine();
    }
}
