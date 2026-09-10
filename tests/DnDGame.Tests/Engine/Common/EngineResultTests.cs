using DnDGame.Domain.Engine.Common;
using Xunit;

namespace DnDGame.Tests.Engine.Common;

public class EngineResultTests
{
    [Fact]
    public void SuccessResultContainsDataWithoutErrorCode()
    {
        var result = EngineResult<string>.Ok("battle-state", "Operation completed.");

        Assert.True(result.Success);
        Assert.Equal("Operation completed.", result.Message);
        Assert.Null(result.ErrorCode);
        Assert.Equal("battle-state", result.Data);
    }

    [Fact]
    public void FailureResultContainsErrorCodeWithoutData()
    {
        var result = EngineResult<string>.Fail(
            "The player turn is not active.",
            EngineErrorCodes.NotPlayerTurn);

        Assert.False(result.Success);
        Assert.Equal("The player turn is not active.", result.Message);
        Assert.Equal(EngineErrorCodes.NotPlayerTurn, result.ErrorCode);
        Assert.Null(result.Data);
    }
}
