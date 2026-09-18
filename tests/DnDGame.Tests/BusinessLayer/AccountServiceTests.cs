using DnDGame.BusinessLayer.Common.Errors;
using DnDGame.BusinessLayer.Common.Exceptions;
using DnDGame.BusinessLayer.Dtos.Accounts;
using DnDGame.BusinessLayer.Services;
using DnDGame.BusinessLayer.Services.Interfaces;
using DnDGame.Domain.Entities.Accounts;
using DnDGame.MockData;
using DnDGame.MockData.Repositories;
using Microsoft.AspNetCore.Identity;

namespace DnDGame.Tests.BusinessLayer;

public class AccountServiceTests
{
    [Fact]
    public async Task RegisterAsync_ValidRequest_PersistsAccountWithHashedPassword()
    {
        var (service, store) = CreateService();

        var result = await service.RegisterAsync(new RegisterRequestDto
        {
            Username = "alice",
            Email = "alice@example.com",
            Password = "correct-horse"
        });

        Assert.NotEqual(0, result.Id);
        Assert.Equal("alice", result.Username);
        Assert.Equal("alice@example.com", result.Email);

        var stored = Assert.Single(store.Accounts);
        Assert.NotEqual("correct-horse", stored.PasswordHash);
        Assert.NotEmpty(stored.PasswordHash);
    }

    [Fact]
    public async Task RegisterAsync_DuplicateEmail_ThrowsEmailAlreadyInUse()
    {
        var (service, _) = CreateService();
        await service.RegisterAsync(new RegisterRequestDto
        {
            Username = "alice",
            Email = "alice@example.com",
            Password = "correct-horse"
        });

        var exception = await Assert.ThrowsAsync<DomainException>(() => service.RegisterAsync(new RegisterRequestDto
        {
            Username = "someoneelse",
            Email = "Alice@Example.com",
            Password = "correct-horse"
        }));

        Assert.Equal(AccountErrorCodes.EmailAlreadyInUse, exception.ErrorCode);
    }

    [Fact]
    public async Task RegisterAsync_DuplicateUsername_ThrowsUsernameAlreadyInUse()
    {
        var (service, _) = CreateService();
        await service.RegisterAsync(new RegisterRequestDto
        {
            Username = "alice",
            Email = "alice@example.com",
            Password = "correct-horse"
        });

        var exception = await Assert.ThrowsAsync<DomainException>(() => service.RegisterAsync(new RegisterRequestDto
        {
            Username = "Alice",
            Email = "someoneelse@example.com",
            Password = "correct-horse"
        }));

        Assert.Equal(AccountErrorCodes.UsernameAlreadyInUse, exception.ErrorCode);
    }

    [Fact]
    public async Task RegisterAsync_BlankUsername_ThrowsValidationError()
    {
        var (service, _) = CreateService();

        var exception = await Assert.ThrowsAsync<DomainException>(() => service.RegisterAsync(new RegisterRequestDto
        {
            Username = " ",
            Email = "alice@example.com",
            Password = "correct-horse"
        }));

        Assert.Equal(ErrorCodes.ValidationError, exception.ErrorCode);
    }

    [Fact]
    public async Task RegisterAsync_PasswordTooShort_ThrowsValidationError()
    {
        var (service, _) = CreateService();

        var exception = await Assert.ThrowsAsync<DomainException>(() => service.RegisterAsync(new RegisterRequestDto
        {
            Username = "alice",
            Email = "alice@example.com",
            Password = "short"
        }));

        Assert.Equal(ErrorCodes.ValidationError, exception.ErrorCode);
    }

    [Fact]
    public async Task LoginAsync_ValidCredentials_ReturnsAccount()
    {
        var (service, _) = CreateService();
        await service.RegisterAsync(new RegisterRequestDto
        {
            Username = "alice",
            Email = "alice@example.com",
            Password = "correct-horse"
        });

        var result = await service.LoginAsync(new LoginRequestDto
        {
            Email = "Alice@Example.com",
            Password = "correct-horse"
        });

        Assert.Equal("alice", result.Username);
    }

    [Fact]
    public async Task LoginAsync_WrongPassword_ThrowsInvalidCredentials()
    {
        var (service, _) = CreateService();
        await service.RegisterAsync(new RegisterRequestDto
        {
            Username = "alice",
            Email = "alice@example.com",
            Password = "correct-horse"
        });

        var exception = await Assert.ThrowsAsync<DomainException>(() => service.LoginAsync(new LoginRequestDto
        {
            Email = "alice@example.com",
            Password = "wrong-password"
        }));

        Assert.Equal(AccountErrorCodes.InvalidCredentials, exception.ErrorCode);
    }

    [Fact]
    public async Task LoginAsync_UnknownEmail_ThrowsInvalidCredentials()
    {
        var (service, _) = CreateService();

        var exception = await Assert.ThrowsAsync<DomainException>(() => service.LoginAsync(new LoginRequestDto
        {
            Email = "nobody@example.com",
            Password = "correct-horse"
        }));

        Assert.Equal(AccountErrorCodes.InvalidCredentials, exception.ErrorCode);
    }

    [Fact]
    public async Task LoginAsync_BlankEmail_ThrowsValidationError()
    {
        var (service, _) = CreateService();

        var exception = await Assert.ThrowsAsync<DomainException>(() => service.LoginAsync(new LoginRequestDto
        {
            Email = " ",
            Password = "correct-horse"
        }));

        Assert.Equal(ErrorCodes.ValidationError, exception.ErrorCode);
    }

    [Fact]
    public async Task GetByIdAsync_ExistingAccount_ReturnsUser()
    {
        var (service, _) = CreateService();
        var registered = await service.RegisterAsync(new RegisterRequestDto
        {
            Username = "alice",
            Email = "alice@example.com",
            Password = "correct-horse"
        });

        var result = await service.GetByIdAsync(registered.Id);

        Assert.NotNull(result);
        Assert.Equal("alice", result!.Username);
    }

    [Fact]
    public async Task GetByIdAsync_UnknownId_ReturnsNull()
    {
        var (service, _) = CreateService();

        var result = await service.GetByIdAsync(999);

        Assert.Null(result);
    }

    private static (IAccountService Service, InMemoryGameDataStore Store) CreateService()
    {
        var store = new InMemoryGameDataStore();
        var repository = new MockAccountRepository(store);
        var service = new AccountService(repository, new PasswordHasher<Account>());
        return (service, store);
    }
}
