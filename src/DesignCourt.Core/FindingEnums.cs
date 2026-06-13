namespace DesignCourt.Core;

public enum FindingCategory
{
    Operability,
    Security,
    Ambiguity,
    Architecture,
    Compliance,
    ApiDesign,
    CostAndComplexity
}

public enum Severity
{
    Critical,
    High,
    Medium,
    Low,
    Info
}

public enum FindingStatus
{
    Accepted,
    PartiallyAccepted,
    Rejected,
    NeedsHumanReview
}

public enum ConfidenceBand
{
    Low,
    Medium,
    High
}

public enum VerificationKind
{
    Question,
    Test,
    Experiment,
    Checklist,
    DocumentUpdate
}

public enum VerificationLevel
{
    SectionVerified,
    QuoteVerified,
    QuoteFuzzyVerified,
    Unverified
}
