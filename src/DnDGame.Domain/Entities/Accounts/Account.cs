using DnDGame.Domain.Common;

namespace DnDGame.Domain.Entities.Accounts;

/// <summary>
/// A login account: the credentials a person uses to authenticate. Deliberately
/// separate from PlayerCharacter — there is no Account-to-PlayerCharacter
/// relationship yet (that link is a pending team decision, same as the
/// Player-vs-PlayerCharacter question noted in docs/ARCHITECTURE.md). This entity
/// exists only to support login/registration.
/// </summary>
public class Account : BaseEntity
{
    public string Username { get; set; } = string.Empty;

    public string Email { get; set; } = string.Empty;

    /// <summary>
    /// The hashed (never plaintext) password, produced by
    /// Microsoft.AspNetCore.Identity.PasswordHasher&lt;Account&gt;.
    /// </summary>
    public string PasswordHash { get; set; } = string.Empty;

    public DateTime CreatedAt { get; set; }
}
