using DnDGame.MockData.Services;

namespace DnDGame.Tests.Infrastructure;

public class MockCurrentPlayerServiceTests
{
    [Fact]
    public void GetCurrentPlayerId_AlwaysReturnsTheFixedMockId()
    {
        var service = new MockCurrentPlayerService();

        var id = service.GetCurrentPlayerId();

        Assert.Equal(MockCurrentPlayerService.MockPlayerId, id);
    }
}
