namespace DesignCourt.Core;

public sealed class FindingValidator
{
    public FindingValidationResult Validate(JudgedFinding finding)
    {
        ArgumentNullException.ThrowIfNull(finding);

        var errors = new List<string>();

        RequireText(finding.Id, "Finding id is required.", errors);
        RequireText(finding.Fingerprint, "Finding fingerprint is required.", errors);
        RequireText(finding.Title, "Finding title is required.", errors);
        RequireText(finding.Description, "Finding description is required.", errors);
        RequireText(finding.JudgeVerdict.Reason, "Judge verdict reason is required.", errors);

        if (finding.Status != finding.JudgeVerdict.Decision)
        {
            errors.Add("Finding status must match the Judge verdict decision.");
        }

        if (IsReportable(finding.Status))
        {
            ValidateReportableFinding(finding, errors);
        }

        return errors.Count == 0
            ? FindingValidationResult.Valid
            : FindingValidationResult.Invalid(errors);
    }

    public static bool IsReportable(FindingStatus status)
    {
        return status is FindingStatus.Accepted
            or FindingStatus.PartiallyAccepted
            or FindingStatus.NeedsHumanReview;
    }

    private static void ValidateReportableFinding(JudgedFinding finding, List<string> errors)
    {
        if (finding.Evidence.Count == 0)
        {
            errors.Add("Reportable findings require at least one evidence item.");
        }

        RequireText(finding.Recommendation, "Reportable findings require a recommendation.", errors);
        RequireText(finding.Verification.Text, "Reportable findings require a verification step.", errors);

        foreach (var evidence in finding.Evidence)
        {
            if (!evidence.SectionVerified)
            {
                errors.Add($"Evidence section '{evidence.SectionId}' must be verified.");
            }

            if (finding.Status != FindingStatus.NeedsHumanReview && !HasVerifiedQuote(evidence))
            {
                errors.Add($"Evidence quote for section '{evidence.SectionId}' must be verified or fuzzy-verified.");
            }
        }
    }

    private static bool HasVerifiedQuote(EvidenceItem evidence)
    {
        return evidence.QuoteVerified
            || evidence.VerificationLevel == VerificationLevel.QuoteVerified
            || evidence.VerificationLevel == VerificationLevel.QuoteFuzzyVerified;
    }

    private static void RequireText(string value, string message, List<string> errors)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            errors.Add(message);
        }
    }
}
