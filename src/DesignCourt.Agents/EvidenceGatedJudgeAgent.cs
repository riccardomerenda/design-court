using DesignCourt.Core;

namespace DesignCourt.Agents;

public sealed class EvidenceGatedJudgeAgent : IJudgeAgent
{
    public Task<IReadOnlyList<JudgedFinding>> JudgeAsync(
        ReviewedDocument document,
        IReadOnlyList<CandidateFinding> candidates,
        CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(document);
        ArgumentNullException.ThrowIfNull(candidates);

        var judgedFindings = candidates
            .GroupBy(candidate => candidate.Fingerprint, StringComparer.Ordinal)
            .Select(group => group.First())
            .Select(candidate =>
            {
                cancellationToken.ThrowIfCancellationRequested();
                return Judge(candidate);
            })
            .ToArray();

        return Task.FromResult<IReadOnlyList<JudgedFinding>>(judgedFindings);
    }

    private static JudgedFinding Judge(CandidateFinding candidate)
    {
        var decision = Decide(candidate);

        return new JudgedFinding(
            Id: candidate.Id.Replace("C-", "F-", StringComparison.Ordinal),
            Fingerprint: candidate.Fingerprint,
            Title: candidate.Title,
            Description: candidate.Description,
            RaisedBy: candidate.RaisedBy,
            Category: candidate.Category,
            Severity: candidate.Severity,
            Status: decision.Status,
            ConfidenceBand: candidate.ConfidenceBand,
            Evidence: candidate.Evidence,
            Impact: candidate.Impact,
            Recommendation: candidate.Recommendation,
            Verification: candidate.Verification,
            Counterarguments: candidate.Counterarguments,
            JudgeVerdict: new JudgeVerdict(decision.Status, decision.Reason));
    }

    private static JudgeDecision Decide(CandidateFinding candidate)
    {
        if (candidate.Evidence.Count == 0)
        {
            return new JudgeDecision(
                FindingStatus.Rejected,
                "Rejected because the candidate finding does not cite evidence.");
        }

        if (candidate.Evidence.Any(evidence => !evidence.SectionVerified))
        {
            return new JudgeDecision(
                FindingStatus.Rejected,
                "Rejected because one or more cited sections could not be verified.");
        }

        if (candidate.Evidence.Any(evidence => !EvidenceHasVerifiedQuote(evidence)))
        {
            return new JudgeDecision(
                FindingStatus.NeedsHumanReview,
                "The cited section was verified, but the quote could not be machine-verified.");
        }

        return new JudgeDecision(
            FindingStatus.Accepted,
            "Accepted because the finding cites verified evidence and describes an actionable design risk.");
    }

    private static bool EvidenceHasVerifiedQuote(EvidenceItem evidence)
    {
        return evidence.QuoteVerified
            || evidence.VerificationLevel == VerificationLevel.QuoteVerified
            || evidence.VerificationLevel == VerificationLevel.QuoteFuzzyVerified;
    }

    private sealed record JudgeDecision(FindingStatus Status, string Reason);
}
