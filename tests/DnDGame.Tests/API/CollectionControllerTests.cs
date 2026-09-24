using DnDGame.API.Controllers;
using DnDGame.BusinessLayer.Dtos.Collection;
using DnDGame.BusinessLayer.Services;
using DnDGame.MockData;
using DnDGame.MockData.Repositories;
using Microsoft.AspNetCore.Mvc;
using Xunit;

namespace DnDGame.Tests.API;

public class CollectionControllerTests
{
    [Fact]
    public async Task GetCatalog_ReturnsTheWholeSeededCatalog()
    {
        var store = MockDataBootstrapper.CreateSeededStore();
        var service = new CollectionService(new MockCollectionRepository(store));
        var controller = new CollectionController(service);

        var response = await controller.GetCatalog();

        var result = Assert.IsType<OkObjectResult>(response.Result);
        var catalog = Assert.IsAssignableFrom<IReadOnlyList<CollectionEntryDto>>(result.Value);
        Assert.Equal(33, catalog.Count);
    }
}
