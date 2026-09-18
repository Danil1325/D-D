using System.Security.Claims;
using DnDGame.API.Controllers;
using DnDGame.BusinessLayer.Dtos.Accounts;
using Microsoft.AspNetCore.Authentication.Cookies;

namespace DnDGame.Tests.API;

public class AuthControllerTests
{
    [Fact]
    public void BuildPrincipal_MapsAccountFieldsToExpectedClaims()
    {
        var account = new CurrentUserDto { Id = 5, Username = "alice", Email = "alice@example.com" };

        var principal = AuthController.BuildPrincipal(account);

        Assert.Equal("5", principal.FindFirstValue(ClaimTypes.NameIdentifier));
        Assert.Equal("alice", principal.FindFirstValue(ClaimTypes.Name));
        Assert.Equal("alice@example.com", principal.FindFirstValue(ClaimTypes.Email));
    }

    [Fact]
    public void BuildPrincipal_ProducesAnAuthenticatedIdentityForTheCookieScheme()
    {
        var account = new CurrentUserDto { Id = 1, Username = "bob", Email = "bob@example.com" };

        var principal = AuthController.BuildPrincipal(account);

        Assert.True(principal.Identity?.IsAuthenticated);
        Assert.Equal(CookieAuthenticationDefaults.AuthenticationScheme, principal.Identity?.AuthenticationType);
    }

    [Fact]
    public void BuildPrincipal_NeverIncludesAPasswordOrHashClaim()
    {
        var account = new CurrentUserDto { Id = 1, Username = "bob", Email = "bob@example.com" };

        var principal = AuthController.BuildPrincipal(account);

        Assert.Equal(3, ((ClaimsIdentity)principal.Identity!).Claims.Count());
    }
}
