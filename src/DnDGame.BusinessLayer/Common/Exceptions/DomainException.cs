namespace DnDGame.BusinessLayer.Common.Exceptions;

/// <summary>
/// The exception type Application Services throw for expected, business-level
/// failures (not found, invalid state, failed validation deeper than request-shape
/// checks, etc.). The global error-handling middleware in DnDGame.API catches
/// exactly this type and turns it into an ApiErrorResponse via IErrorCodeHttpMapper —
/// any other exception type is treated as an unexpected/internal error instead.
///
/// ErrorCode is deliberately a plain string, not an enum: this project does not yet
/// know the full set of codes Person 1's and Person 2's engines will raise (e.g.
/// CARD_NOT_IN_HAND, NOT_ENOUGH_ENERGY, NOT_PLAYER_TURN). Once those are confirmed,
/// they can be thrown here without changing this class or the middleware — only
/// IErrorCodeHttpMapper's registered mappings need to grow.
/// </summary>
public class DomainException : Exception
{
    /// <summary>A stable, machine-readable code, e.g. "BATTLE_NOT_FOUND". See ErrorCodes for the ones already known.</summary>
    public string ErrorCode { get; }

    public DomainException(string errorCode, string message)
        : base(message)
    {
        ErrorCode = errorCode;
    }

    public DomainException(string errorCode, string message, Exception innerException)
        : base(message, innerException)
    {
        ErrorCode = errorCode;
    }
}
