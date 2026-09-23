namespace DnDGame.BusinessLayer.Common.Errors;

/// <summary>
/// Error codes specific to character creation (New Game). Kept separate from the
/// generic ErrorCodes and the other per-feature code classes, following the same
/// per-feature convention as AccountErrorCodes.
/// </summary>
public static class CharacterErrorCodes
{
    /// <summary>New-game creation referenced a race id that does not exist in the store.</summary>
    public const string RaceNotFound = "RACE_NOT_FOUND";

    /// <summary>New-game creation referenced a class id that does not exist in the store.</summary>
    public const string ClassNotFound = "CLASS_NOT_FOUND";
}