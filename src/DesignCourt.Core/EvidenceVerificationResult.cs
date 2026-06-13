namespace DesignCourt.Core;

public sealed record EvidenceVerificationResult(
    EvidenceItem Evidence,
    bool SectionFound,
    bool QuoteFound);
