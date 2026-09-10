namespace DnDGame.BusinessLayer.Validation;

/// <summary>
/// Generic outcome of validating an incoming request's shape (required fields,
/// parseable Guid, in-range pagination, etc. — see Task 3.18). Deliberately not
/// tied to ASP.NET's ModelState, so this can be unit-tested without spinning up a
/// controller/HTTP context.
/// </summary>
public class ValidationResult
{
    public bool IsValid => Errors.Count == 0;
    public IReadOnlyList<string> Errors { get; }

    private ValidationResult(IReadOnlyList<string> errors)
    {
        Errors = errors;
    }

    public static ValidationResult Success() => new(Array.Empty<string>());

    public static ValidationResult Failure(params string[] errors) => new(errors);

    public static ValidationResult Failure(IEnumerable<string> errors) => new(errors.ToList());

    /// <summary>Combines several validation results into one — useful for validating a request with multiple independent fields.</summary>
    public static ValidationResult Combine(params ValidationResult[] results) =>
        new(results.SelectMany(r => r.Errors).ToList());
}
