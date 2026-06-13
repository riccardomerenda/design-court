namespace DesignCourt.Core;

public sealed record VerificationStep(
    VerificationKind Kind,
    string Text);
