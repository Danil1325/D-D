using DnDGame.BusinessLayer.Dtos.Common;
using DnDGame.BusinessLayer.Validation;

namespace DnDGame.Tests.Infrastructure;

public class RequestValidationHelpersTests
{
    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void RequireNonEmpty_BlankValue_Fails(string? value)
    {
        var result = RequestValidationHelpers.RequireNonEmpty(value, "Name");

        Assert.False(result.IsValid);
        Assert.Contains("Name", result.Errors[0]);
    }

    [Fact]
    public void RequireNonEmpty_RealValue_Succeeds()
    {
        var result = RequestValidationHelpers.RequireNonEmpty("TestPlayer", "Name");

        Assert.True(result.IsValid);
    }

    [Fact]
    public void RequireValidGuid_EmptyGuidString_Fails()
    {
        var result = RequestValidationHelpers.RequireValidGuid(Guid.Empty.ToString(), "BattleId", out var parsed);

        Assert.False(result.IsValid);
        Assert.Equal(Guid.Empty, parsed);
    }

    [Fact]
    public void RequireValidGuid_NotAGuid_Fails()
    {
        var result = RequestValidationHelpers.RequireValidGuid("not-a-guid", "BattleId", out _);

        Assert.False(result.IsValid);
    }

    [Fact]
    public void RequireValidGuid_ValidGuid_Succeeds()
    {
        var guid = Guid.NewGuid();

        var result = RequestValidationHelpers.RequireValidGuid(guid.ToString(), "BattleId", out var parsed);

        Assert.True(result.IsValid);
        Assert.Equal(guid, parsed);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    [InlineData(null)]
    public void RequirePositiveId_NonPositive_Fails(int? id)
    {
        var result = RequestValidationHelpers.RequirePositiveId(id, "EnemyId");

        Assert.False(result.IsValid);
    }

    [Fact]
    public void ValidatePagination_WithinBounds_Succeeds()
    {
        var request = new PagedRequest { Page = 2, PageSize = 20 };

        var result = RequestValidationHelpers.ValidatePagination(request);

        Assert.True(result.IsValid);
    }

    [Fact]
    public void ValidatePagination_PageBelowOne_Fails()
    {
        var request = new PagedRequest { Page = 0, PageSize = 20 };

        var result = RequestValidationHelpers.ValidatePagination(request);

        Assert.False(result.IsValid);
    }

    [Fact]
    public void ValidatePagination_PageSizeTooLarge_Fails()
    {
        var request = new PagedRequest { Page = 1, PageSize = PagedRequest.MaxPageSize + 1 };

        var result = RequestValidationHelpers.ValidatePagination(request);

        Assert.False(result.IsValid);
    }
}
