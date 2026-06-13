namespace DesignCourt.Core;

public sealed record EvidenceItem(
    string Document,
    string SectionId,
    string SectionTitle,
    string Quote,
    int LineStart,
    int LineEnd,
    bool SectionVerified,
    bool QuoteVerified,
    VerificationLevel VerificationLevel);
