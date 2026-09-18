using DnDGame.BusinessLayer.Dtos.Accounts;

namespace DnDGame.BusinessLayer.Services.Interfaces;

/// <summary>
/// Registration and login for accounts. Deliberately has no notion of "who is
/// currently logged in" or of cookies/claims — that is the Controller's job, once
/// it calls HttpContext.SignInAsync with the id/username this returns (see Step 4/5
/// of the auth plan). Also deliberately unrelated to ICurrentPlayerService/
/// PlayerCharacter — see Account's remarks.
/// </summary>
public interface IAccountService
{
    /// <summary>
    /// Throws DomainException(VALIDATION_ERROR) for a malformed request,
    /// EMAIL_ALREADY_IN_USE / USERNAME_ALREADY_IN_USE for a conflicting account.
    /// </summary>
    Task<CurrentUserDto> RegisterAsync(RegisterRequestDto request);

    /// <summary>
    /// Throws DomainException(VALIDATION_ERROR) for a malformed request,
    /// INVALID_CREDENTIALS if the email/password pair doesn't match any account
    /// (deliberately the same code either way — see AccountErrorCodes).
    /// </summary>
    Task<CurrentUserDto> LoginAsync(LoginRequestDto request);

    /// <summary>Returns null if no account with that id exists.</summary>
    Task<CurrentUserDto?> GetByIdAsync(int accountId);
}
