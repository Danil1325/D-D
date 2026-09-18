using DnDGame.BusinessLayer.Repositories.Interfaces;
using DnDGame.Domain.Entities.Accounts;

namespace DnDGame.MockData.Repositories;

public class MockAccountRepository : IAccountRepository
{
    private readonly InMemoryGameDataStore _store;

    public MockAccountRepository(InMemoryGameDataStore store)
    {
        _store = store;
    }

    public Task<Account?> GetByIdAsync(int id)
    {
        var account = _store.Accounts.FirstOrDefault(a => a.Id == id);
        return Task.FromResult(account);
    }

    public Task<Account?> GetByEmailAsync(string email)
    {
        var account = _store.Accounts.FirstOrDefault(
            a => string.Equals(a.Email, email, StringComparison.OrdinalIgnoreCase));
        return Task.FromResult(account);
    }

    public Task<Account?> GetByUsernameAsync(string username)
    {
        var account = _store.Accounts.FirstOrDefault(
            a => string.Equals(a.Username, username, StringComparison.OrdinalIgnoreCase));
        return Task.FromResult(account);
    }

    public Task<Account> AddAsync(Account account)
    {
        account.Id = _store.GetNextAccountId();
        _store.Accounts.Add(account);
        return Task.FromResult(account);
    }
}
