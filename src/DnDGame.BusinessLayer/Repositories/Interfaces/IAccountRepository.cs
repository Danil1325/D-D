using DnDGame.Domain.Entities.Accounts;

namespace DnDGame.BusinessLayer.Repositories.Interfaces;

public interface IAccountRepository
{
    Task<Account?> GetByIdAsync(int id);

    Task<Account?> GetByEmailAsync(string email);

    Task<Account?> GetByUsernameAsync(string username);

    Task<Account> AddAsync(Account account);
}
