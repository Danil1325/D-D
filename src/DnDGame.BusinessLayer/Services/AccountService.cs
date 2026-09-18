using DnDGame.BusinessLayer.Common.Errors;
using DnDGame.BusinessLayer.Common.Exceptions;
using DnDGame.BusinessLayer.Dtos.Accounts;
using DnDGame.BusinessLayer.Repositories.Interfaces;
using DnDGame.BusinessLayer.Services.Interfaces;
using DnDGame.BusinessLayer.Validation;
using DnDGame.Domain.Entities.Accounts;
using Microsoft.AspNetCore.Identity;

namespace DnDGame.BusinessLayer.Services;

public class AccountService : IAccountService
{
    /// <summary>
    /// Deliberately short for a university project's mock-data phase — not a
    /// production password policy. Long enough to reject empty/trivial input
    /// during manual testing without getting in the way.
    /// </summary>
    private const int MinimumPasswordLength = 8;

    private readonly IAccountRepository _accountRepository;
    private readonly IPasswordHasher<Account> _passwordHasher;

    public AccountService(IAccountRepository accountRepository, IPasswordHasher<Account> passwordHasher)
    {
        _accountRepository = accountRepository;
        _passwordHasher = passwordHasher;
    }

    public async Task<CurrentUserDto> RegisterAsync(RegisterRequestDto request)
    {
        ArgumentNullException.ThrowIfNull(request);
        ValidateRegistration(request);

        if (await _accountRepository.GetByEmailAsync(request.Email) is not null)
        {
            throw new DomainException(AccountErrorCodes.EmailAlreadyInUse, "This email is already registered.");
        }

        if (await _accountRepository.GetByUsernameAsync(request.Username) is not null)
        {
            throw new DomainException(AccountErrorCodes.UsernameAlreadyInUse, "This username is already taken.");
        }

        var account = new Account
        {
            Username = request.Username,
            Email = request.Email,
            CreatedAt = DateTime.UtcNow
        };
        account.PasswordHash = _passwordHasher.HashPassword(account, request.Password);

        var created = await _accountRepository.AddAsync(account);
        return CurrentUserDto.FromDomain(created);
    }

    public async Task<CurrentUserDto> LoginAsync(LoginRequestDto request)
    {
        ArgumentNullException.ThrowIfNull(request);
        ValidateLogin(request);

        var account = await _accountRepository.GetByEmailAsync(request.Email);
        if (account is null)
        {
            throw new DomainException(AccountErrorCodes.InvalidCredentials, "Invalid email or password.");
        }

        var verification = _passwordHasher.VerifyHashedPassword(account, account.PasswordHash, request.Password);
        if (verification == PasswordVerificationResult.Failed)
        {
            throw new DomainException(AccountErrorCodes.InvalidCredentials, "Invalid email or password.");
        }

        return CurrentUserDto.FromDomain(account);
    }

    public async Task<CurrentUserDto?> GetByIdAsync(int accountId)
    {
        var account = await _accountRepository.GetByIdAsync(accountId);
        return account is null ? null : CurrentUserDto.FromDomain(account);
    }

    private static void ValidateRegistration(RegisterRequestDto request)
    {
        var validation = ValidationResult.Combine(
            RequestValidationHelpers.RequireNonEmpty(request.Username, "Username"),
            RequestValidationHelpers.RequireNonEmpty(request.Email, "Email"),
            ValidatePassword(request.Password));

        ThrowIfInvalid(validation);
    }

    private static void ValidateLogin(LoginRequestDto request)
    {
        var validation = ValidationResult.Combine(
            RequestValidationHelpers.RequireNonEmpty(request.Email, "Email"),
            RequestValidationHelpers.RequireNonEmpty(request.Password, "Password"));

        ThrowIfInvalid(validation);
    }

    private static ValidationResult ValidatePassword(string? password)
    {
        if (string.IsNullOrWhiteSpace(password))
        {
            return ValidationResult.Failure("Password is required.");
        }

        return password.Length < MinimumPasswordLength
            ? ValidationResult.Failure($"Password must be at least {MinimumPasswordLength} characters.")
            : ValidationResult.Success();
    }

    private static void ThrowIfInvalid(ValidationResult validation)
    {
        if (!validation.IsValid)
        {
            throw new DomainException(ErrorCodes.ValidationError, string.Join(" ", validation.Errors));
        }
    }
}
