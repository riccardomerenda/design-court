using DesignCourt.Core;

namespace DesignCourt.Agents;

public sealed class LocalReviewWorkflow : IAgentWorkflow
{
    private readonly IReadOnlyList<IReviewAgent> agents;
    private readonly IJudgeAgent judge;
    private readonly IEvidenceVerifier evidenceVerifier;
    private readonly FindingValidator validator;

    public LocalReviewWorkflow(
        IReadOnlyList<IReviewAgent> agents,
        IJudgeAgent judge,
        IEvidenceVerifier evidenceVerifier,
        FindingValidator? validator = null)
    {
        ArgumentNullException.ThrowIfNull(agents);

        this.agents = agents;
        this.judge = judge ?? throw new ArgumentNullException(nameof(judge));
        this.evidenceVerifier = evidenceVerifier ?? throw new ArgumentNullException(nameof(evidenceVerifier));
        this.validator = validator ?? new FindingValidator();
    }

    public async Task<ReviewResult> RunAsync(
        ReviewedDocument document,
        CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(document);

        var candidates = new List<CandidateFinding>();

        foreach (var agent in agents)
        {
            cancellationToken.ThrowIfCancellationRequested();
            var agentCandidates = await agent.ReviewAsync(document, cancellationToken);
            candidates.AddRange(agentCandidates.Select(candidate => VerifyEvidence(document, candidate)));
        }

        var judgedFindings = await judge.JudgeAsync(document, candidates, cancellationToken);
        ValidateJudgedFindings(judgedFindings);

        return new ReviewResult(document, judgedFindings);
    }

    private CandidateFinding VerifyEvidence(ReviewedDocument document, CandidateFinding candidate)
    {
        var verifiedEvidence = candidate.Evidence
            .Select(evidence => evidenceVerifier.Verify(document, evidence).Evidence)
            .ToArray();

        return candidate with
        {
            Evidence = verifiedEvidence
        };
    }

    private void ValidateJudgedFindings(IEnumerable<JudgedFinding> judgedFindings)
    {
        foreach (var finding in judgedFindings)
        {
            var result = validator.Validate(finding);

            if (!result.IsValid)
            {
                throw new InvalidOperationException(
                    $"Judge produced invalid finding '{finding.Id}': {string.Join("; ", result.Errors)}");
            }
        }
    }
}
