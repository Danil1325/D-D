namespace DnDGame.BusinessLayer.Common.Errors;

/// <summary>
/// Maps a DomainException's ErrorCode to an HTTP status code, so the global error
/// middleware never has to know about individual codes itself. Registrations are
/// additive: BusinessLayer registers the generic codes it owns (see ErrorCodes);
/// once Person 1/2's engine error codes are confirmed, their DI registration (or a
/// startup extension of their own) can call Register(...) for each one without
/// touching the middleware or this interface.
/// </summary>
public interface IErrorCodeHttpMapper
{
    /// <summary>Registers (or overwrites) the HTTP status code for a given ErrorCode.</summary>
    void Register(string errorCode, int httpStatusCode);

    /// <summary>
    /// Returns the HTTP status code for the given ErrorCode, or the mapper's default
    /// (500) if the code hasn't been registered — an unregistered code is treated as
    /// a bug to fix (register it) rather than silently guessed at.
    /// </summary>
    int Map(string errorCode);
}
