namespace CSharpLatest.AsyncEventHandler;

/// <summary>
/// Represents the result of an attribute validity check.
/// </summary>
/// <param name="Result">The validity check result.</param>
/// <param name="ArgumentValues">The list of argument values if successful.</param>
/// <param name="PositionOfFirstInvalidArgument">The 0-based position of the first invalid argument if not successful.</param>
public record AttributeValidityCheckResult(AttributeGeneration Result, (bool, bool) ArgumentValues, int PositionOfFirstInvalidArgument)
{
    /// <summary>
    /// Creates the invalid attribute result.
    /// </summary>
    /// <param name="positionOfFirstInvalidArgument">The 0-based position of the invalid argument. -1 if no argument.</param>
    public static AttributeValidityCheckResult Invalid(int positionOfFirstInvalidArgument) => new(AttributeGeneration.Invalid, (false, false), positionOfFirstInvalidArgument);
}
