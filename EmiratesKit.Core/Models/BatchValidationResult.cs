namespace EmiratesKit.Core.Models;

/// <summary>
/// Pairs a single input string with its validation result.
/// Returned by ParseMany() on all validator classes.
/// </summary>
public class BatchValidationResult<T> where T : ValidationResult
{
    /// <summary>The original input string as provided by the caller.</summary>
    public string? Input { get; init; }

    /// <summary>The full validation result for this input.</summary>
    public T Result { get; init; } = null!;

    /// <summary>Shortcut — same as Result.IsValid.</summary>
    public bool IsValid => Result.IsValid;

    /// <summary>Shortcut — same as Result.ErrorCode.</summary>
    public string? ErrorCode => Result.ErrorCode;
}
