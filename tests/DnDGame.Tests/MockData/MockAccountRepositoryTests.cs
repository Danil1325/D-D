using DnDGame.Domain.Entities.Accounts;
using DnDGame.MockData;
using DnDGame.MockData.Repositories;

namespace DnDGame.Tests.MockData;

public class MockAccountRepositoryTests
{
    [Fact]
    public async Task AddAsync_AssignsIncrementingIds()
    {
        var store = new InMemoryGameDataStore();
        var repository = new MockAccountRepository(store);

        var first = await repository.AddAsync(new Account { Username = "alice", Email = "alice@example.com" });
        var second = await repository.AddAsync(new Account { Username = "bob", Email = "bob@example.com" });

        Assert.Equal(1, first.Id);
        Assert.Equal(2, second.Id);
    }

    [Fact]
    public async Task GetByIdAsync_ReturnsAccount_AfterItWasAdded()
    {
        var store = new InMemoryGameDataStore();
        var repository = new MockAccountRepository(store);
        var added = await repository.AddAsync(new Account { Username = "alice", Email = "alice@example.com" });

        var found = await repository.GetByIdAsync(added.Id);

        Assert.NotNull(found);
        Assert.Equal("alice", found!.Username);
    }

    [Fact]
    public async Task GetByIdAsync_UnknownId_ReturnsNull()
    {
        var store = new InMemoryGameDataStore();
        var repository = new MockAccountRepository(store);

        var found = await repository.GetByIdAsync(999);

        Assert.Null(found);
    }

    [Fact]
    public async Task GetByEmailAsync_IsCaseInsensitive()
    {
        var store = new InMemoryGameDataStore();
        var repository = new MockAccountRepository(store);
        await repository.AddAsync(new Account { Username = "alice", Email = "Alice@Example.com" });

        var found = await repository.GetByEmailAsync("alice@example.com");

        Assert.NotNull(found);
        Assert.Equal("alice", found!.Username);
    }

    [Fact]
    public async Task GetByEmailAsync_UnknownEmail_ReturnsNull()
    {
        var store = new InMemoryGameDataStore();
        var repository = new MockAccountRepository(store);

        var found = await repository.GetByEmailAsync("nobody@example.com");

        Assert.Null(found);
    }

    [Fact]
    public async Task GetByUsernameAsync_IsCaseInsensitive()
    {
        var store = new InMemoryGameDataStore();
        var repository = new MockAccountRepository(store);
        await repository.AddAsync(new Account { Username = "Alice", Email = "alice@example.com" });

        var found = await repository.GetByUsernameAsync("alice");

        Assert.NotNull(found);
        Assert.Equal("alice@example.com", found!.Email);
    }

    [Fact]
    public async Task GetByUsernameAsync_UnknownUsername_ReturnsNull()
    {
        var store = new InMemoryGameDataStore();
        var repository = new MockAccountRepository(store);

        var found = await repository.GetByUsernameAsync("nobody");

        Assert.Null(found);
    }
}
