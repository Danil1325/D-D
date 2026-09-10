namespace DnDGame.BusinessLayer.Common.Errors;

/// <summary>
/// Default IErrorCodeHttpMapper implementation. Registered as a DI singleton so its
/// mapping table is built once at startup and shared by every request.
///
/// Seeded here only with the generic codes this project already knows about
/// (see ErrorCodes). Feature-specific codes (battle/card/dice) are added via
/// Register(...) once Person 1/2's contracts are confirmed — see Task doc's
/// "HTTP Error Mapping" example (BATTLE_NOT_FOUND -> 404, NOT_ENOUGH_ENERGY -> 400,
/// NOT_PLAYER_TURN -> 409, etc.), none of which are guessed at here.
/// </summary>
public class ErrorCodeHttpMapper : IErrorCodeHttpMapper
{
    private const int DefaultStatusCode = 500;

    private readonly Dictionary<string, int> _map = new(StringComparer.OrdinalIgnoreCase)
    {
        [ErrorCodes.ValidationError] = 400,
        [ErrorCodes.NotFound] = 404,
        [ErrorCodes.Conflict] = 409,
        [ErrorCodes.InternalError] = DefaultStatusCode
    };

    public void Register(string errorCode, int httpStatusCode)
    {
        _map[errorCode] = httpStatusCode;
    }

    public int Map(string errorCode)
    {
        return _map.TryGetValue(errorCode, out var statusCode) ? statusCode : DefaultStatusCode;
    }
}
