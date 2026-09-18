using DnDGame.Domain.Entities.Accounts;

namespace DnDGame.BusinessLayer.Dtos.Accounts;

/// <summary>
/// Safe, public projection of an Account — never carries PasswordHash.
/// </summary>
public class CurrentUserDto
{
    public int Id { get; init; }
    public string Username { get; init; } = string.Empty;
    public string Email { get; init; } = string.Empty;

    public static CurrentUserDto FromDomain(Account account) => new()
    {
        Id = account.Id,
        Username = account.Username,
        Email = account.Email
    };
}
