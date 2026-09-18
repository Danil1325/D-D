namespace DnDGame.BusinessLayer.Common.Errors;

/// <summary>
/// Error codes specific to the auth/account feature (registration, login).
/// Kept separate from the generic ErrorCodes — these describe account-specific
/// conflicts, not generic infrastructure failures — following the same
/// per-feature convention as Domain.Enums.ErrorCode (card-battle) and
/// EngineErrorCodes (battle engine).
/// </summary>
public static class AccountErrorCodes
{
    /// <summary>Registration attempted with an email that already belongs to an account.</summary>
    public const string EmailAlreadyInUse = "EMAIL_ALREADY_IN_USE";

    /// <summary>Registration attempted with a username that already belongs to an account.</summary>
    public const string UsernameAlreadyInUse = "USERNAME_ALREADY_IN_USE";

    /// <summary>
    /// Login failed because the email/username or password did not match any
    /// account. Deliberately a single generic code for both cases, so a client
    /// can never distinguish "unknown email" from "wrong password" — this avoids
    /// leaking which emails/usernames are registered.
    /// </summary>
    public const string InvalidCredentials = "INVALID_CREDENTIALS";
}
