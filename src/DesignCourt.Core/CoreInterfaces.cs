namespace DesignCourt.Core;

public interface IReviewAgent
{
    string Name { get; }

    AgentRole Role { get; }

    Task<IReadOnlyList<CandidateFinding>> ReviewAsync(
        ReviewedDocument document,
        CancellationToken cancellationToken);
}

public interface IJudgeAgent
{
    Task<IReadOnlyList<JudgedFinding>> JudgeAsync(
        ReviewedDocument document,
        IReadOnlyList<CandidateFinding> candidates,
        CancellationToken cancellationToken);
}

public interface IEvidenceVerifier
{
    EvidenceVerificationResult Verify(
        ReviewedDocument document,
        EvidenceItem evidence);
}

public interface IAgentWorkflow
{
    Task<ReviewResult> RunAsync(
        ReviewedDocument document,
        CancellationToken cancellationToken);
}
