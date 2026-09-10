namespace DnDGame.BusinessLayer.Common.Errors;

/// <summary>
/// Generic, infrastructure-level error codes that don't belong to any one feature.
///
/// Deliberately NOT included here: battle/card-specific codes like
/// BATTLE_NOT_FOUND, CARD_NOT_FOUND, CARD_NOT_IN_HAND, INVALID_DICE,
/// NOT_ENOUGH_ENERGY, NOT_PLAYER_TURN, BATTLE_ALREADY_FINISHED. Those belong to
/// Person 1's and Person 2's engine contracts and will be added once those
/// contracts are confirmed (see the team plan) — inventing them here would mean
/// guessing at codes the engines don't actually produce.
/// </summary>
public static class ErrorCodes
{
    /// <summary>Request-shape validation failed (missing field, bad Guid, out-of-range page, etc.).</summary>
    public const string ValidationError = "VALIDATION_ERROR";

    /// <summary>A generic "the requested resource does not exist" — used only until a more specific *_NOT_FOUND code exists for that resource.</summary>
    public const string NotFound = "NOT_FOUND";

    /// <summary>A generic "the request conflicts with the current state" — used only until a more specific code exists.</summary>
    public const string Conflict = "CONFLICT";

    /// <summary>An unexpected/unhandled server-side failure. Never carries exception details.</summary>
    public const string InternalError = "INTERNAL_ERROR";
}
