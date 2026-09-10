using DnDGame.BusinessLayer.Dtos.Common;

namespace DnDGame.BusinessLayer.Validation;

/// <summary>
/// Reusable, feature-agnostic request-shape checks (Task 3.18). These only validate
/// that a request is well-formed — a parseable Guid, a required field that isn't
/// blank, a page/pageSize within sane bounds. They deliberately know nothing about
/// whether an id actually exists, whether a card is in hand, whose turn it is, etc.
/// — those are business-level checks that belong in a Service (and, once available,
/// in Person 1/2's engines), not here.
/// </summary>
public static class RequestValidationHelpers
{
    /// <summary>Fails if the value is null, empty, or whitespace-only.</summary>
    public static ValidationResult RequireNonEmpty(string? value, string fieldName)
    {
        return string.IsNullOrWhiteSpace(value)
            ? ValidationResult.Failure($"{fieldName} is required.")
            : ValidationResult.Success();
    }

    /// <summary>Fails if the value isn't a parseable, non-empty Guid.</summary>
    public static ValidationResult RequireValidGuid(string? value, string fieldName, out Guid parsed)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            parsed = Guid.Empty;
            return ValidationResult.Failure($"{fieldName} is required.");
        }

        if (!Guid.TryParse(value, out parsed) || parsed == Guid.Empty)
        {
            return ValidationResult.Failure($"{fieldName} must be a valid, non-empty GUID.");
        }

        return ValidationResult.Success();
    }

    /// <summary>Fails if the id is not a positive integer.</summary>
    public static ValidationResult RequirePositiveId(int? id, string fieldName)
    {
        return id is null or <= 0
            ? ValidationResult.Failure($"{fieldName} must be a positive integer.")
            : ValidationResult.Success();
    }

    /// <summary>
    /// Validates a PagedRequest's Page/PageSize are within sane bounds. Unlike
    /// PagedRequest.Normalize() (which silently clamps), this reports the problem so
    /// the caller can return 400 Bad Request instead of quietly changing what the
    /// client asked for — Task 3.18 requires invalid payloads to be rejected, not
    /// silently "fixed".
    /// </summary>
    public static ValidationResult ValidatePagination(PagedRequest request)
    {
        var errors = new List<string>();

        if (request.Page < 1)
        {
            errors.Add("Page must be 1 or greater.");
        }

        if (request.PageSize < 1)
        {
            errors.Add("PageSize must be 1 or greater.");
        }
        else if (request.PageSize > PagedRequest.MaxPageSize)
        {
            errors.Add($"PageSize must not exceed {PagedRequest.MaxPageSize}.");
        }

        return errors.Count == 0 ? ValidationResult.Success() : ValidationResult.Failure(errors);
    }
}
