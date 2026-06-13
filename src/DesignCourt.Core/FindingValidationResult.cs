namespace DesignCourt.Core;

public sealed record FindingValidationResult(
    bool IsValid,
    IReadOnlyList<string> Errors)
{
    public static FindingValidationResult Valid { get; } = new(true, Array.Empty<string>());

    public static FindingValidationResult Invalid(IReadOnlyList<string> errors)
    {
        ArgumentNullException.ThrowIfNull(errors);

        return new FindingValidationResult(false, errors);
    }
}
