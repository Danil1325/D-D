using System.Security.Claims;
using DnDGame.BusinessLayer.Dtos.Accounts;
using DnDGame.BusinessLayer.Services.Interfaces;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace DnDGame.API.Controllers;

[ApiController]
[Route("api/auth")]
public class AuthController : ControllerBase
{
    private readonly IAccountService _accountService;

    public AuthController(IAccountService accountService)
    {
        _accountService = accountService;
    }

    [HttpPost("register")]
    public async Task<ActionResult<CurrentUserDto>> Register([FromBody] RegisterRequestDto request)
    {
        var result = await _accountService.RegisterAsync(request);
        await SignInAsync(result);
        return StatusCode(StatusCodes.Status201Created, result);
    }

    [HttpPost("login")]
    public async Task<ActionResult<CurrentUserDto>> Login([FromBody] LoginRequestDto request)
    {
        var result = await _accountService.LoginAsync(request);
        await SignInAsync(result);
        return Ok(result);
    }

    /// <summary>
    /// Deliberately not [Authorize] — signing out an already-signed-out (or never
    /// signed-in) session is a harmless no-op, not an error worth a 401 for.
    /// </summary>
    [HttpPost("logout")]
    public async Task<IActionResult> Logout()
    {
        await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
        return NoContent();
    }

    [HttpGet("me")]
    [Authorize]
    public async Task<ActionResult<CurrentUserDto>> Me()
    {
        var accountId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
        var result = await _accountService.GetByIdAsync(accountId);

        // Only reachable in this mock-data phase if the in-memory store was reset
        // (e.g. app restart) while the browser still holds a previously issued,
        // still-valid cookie — the account it points to no longer exists.
        return result is null ? Unauthorized() : Ok(result);
    }

    private async Task SignInAsync(CurrentUserDto account)
    {
        var principal = BuildPrincipal(account);
        await HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, principal);
    }

    /// <summary>
    /// Maps an account onto the claims ASP.NET Core cookie auth stores in the
    /// encrypted cookie. Extracted as its own (testable without a real
    /// HttpContext) method since it's the one piece of real logic in this
    /// otherwise-thin controller.
    /// </summary>
    public static ClaimsPrincipal BuildPrincipal(CurrentUserDto account)
    {
        var claims = new[]
        {
            new Claim(ClaimTypes.NameIdentifier, account.Id.ToString()),
            new Claim(ClaimTypes.Name, account.Username),
            new Claim(ClaimTypes.Email, account.Email)
        };
        var identity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
        return new ClaimsPrincipal(identity);
    }
}
