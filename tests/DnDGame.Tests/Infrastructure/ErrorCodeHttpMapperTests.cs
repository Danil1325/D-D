using DnDGame.API.CompositionRoot;
using DnDGame.BusinessLayer.Common.Errors;
using DnDGame.Domain.Engine.Common;
using Microsoft.Extensions.DependencyInjection;

namespace DnDGame.Tests.Infrastructure;

public class ErrorCodeHttpMapperTests
{
    [Theory]
    [InlineData(ErrorCodes.ValidationError, 400)]
    [InlineData(ErrorCodes.NotFound, 404)]
    [InlineData(ErrorCodes.Conflict, 409)]
    [InlineData(ErrorCodes.InternalError, 500)]
    public void Map_KnownGenericCode_ReturnsExpectedStatus(string errorCode, int expectedStatus)
    {
        var mapper = new ErrorCodeHttpMapper();

        var status = mapper.Map(errorCode);

        Assert.Equal(expectedStatus, status);
    }

    [Fact]
    public void Map_UnregisteredCode_DefaultsTo500()
    {
        var mapper = new ErrorCodeHttpMapper();

        // Not a real code yet — stands in for a future engine-specific code like
        // BATTLE_NOT_FOUND that hasn't been registered.
        var status = mapper.Map("SOME_FUTURE_ENGINE_CODE");

        Assert.Equal(500, status);
    }

    [Fact]
    public void Register_NewCode_IsThenMapped()
    {
        var mapper = new ErrorCodeHttpMapper();

        mapper.Register("BATTLE_NOT_FOUND", 404);

        Assert.Equal(404, mapper.Map("BATTLE_NOT_FOUND"));
    }

    [Fact]
    public void Register_ExistingCode_Overwrites()
    {
        var mapper = new ErrorCodeHttpMapper();

        mapper.Register(ErrorCodes.NotFound, 418);

        Assert.Equal(418, mapper.Map(ErrorCodes.NotFound));
    }

    [Fact]
    public void CompositionRoot_RegistersInvalidDiceAs400()
    {
        var services = new ServiceCollection();
        services.AddCardBattleServices();

        using var provider = services.BuildServiceProvider();
        var mapper = provider.GetRequiredService<IErrorCodeHttpMapper>();

        Assert.Equal(400, mapper.Map(EngineErrorCodes.InvalidDice));
    }

    [Theory]
    [InlineData(nameof(DnDGame.Domain.Enums.ErrorCode.DECK_TOO_SMALL))]
    [InlineData(nameof(DnDGame.Domain.Enums.ErrorCode.DECK_TOO_LARGE))]
    [InlineData(nameof(DnDGame.Domain.Enums.ErrorCode.CARD_COPY_LIMIT_REACHED))]
    public void CompositionRoot_RegistersDeckValidationCodesAs400(string errorCode)
    {
        var services = new ServiceCollection();
        services.AddCardBattleServices();

        using var provider = services.BuildServiceProvider();
        var mapper = provider.GetRequiredService<IErrorCodeHttpMapper>();

        Assert.Equal(400, mapper.Map(errorCode));
    }

    [Theory]
    [InlineData(EngineErrorCodes.BattleNotFound, 404)]
    [InlineData(EngineErrorCodes.BattleAlreadyFinished, 409)]
    [InlineData(EngineErrorCodes.NotPlayerTurn, 409)]
    [InlineData(EngineErrorCodes.InvalidAction, 400)]
    [InlineData(EngineErrorCodes.PlayerDead, 409)]
    [InlineData(EngineErrorCodes.EnemyDead, 409)]
    [InlineData(EngineErrorCodes.MissingCombatRule, 500)]
    [InlineData(EngineErrorCodes.RewardsAlreadyGranted, 409)]
    public void CompositionRoot_RegistersBattleEngineCodes(string errorCode, int expectedStatus)
    {
        var services = new ServiceCollection();
        services.AddCardBattleServices();

        using var provider = services.BuildServiceProvider();
        var mapper = provider.GetRequiredService<IErrorCodeHttpMapper>();

        Assert.Equal(expectedStatus, mapper.Map(errorCode));
    }

    [Theory]
    [InlineData(AccountErrorCodes.EmailAlreadyInUse, 409)]
    [InlineData(AccountErrorCodes.UsernameAlreadyInUse, 409)]
    [InlineData(AccountErrorCodes.InvalidCredentials, 401)]
    public void CompositionRoot_RegistersAccountCodes(string errorCode, int expectedStatus)
    {
        var services = new ServiceCollection();
        services.AddCardBattleServices();

        using var provider = services.BuildServiceProvider();
        var mapper = provider.GetRequiredService<IErrorCodeHttpMapper>();

        Assert.Equal(expectedStatus, mapper.Map(errorCode));
    }
}
