using DnDGame.BusinessLayer.Common.Errors;

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
}
