using DnDGame.BusinessLayer.Services;
using DnDGame.Domain.Enums;
using DnDGame.MockData;
using DnDGame.MockData.Repositories;

namespace DnDGame.Tests.BusinessLayer;

public class CollectionServiceTests
{
    [Fact]
    public async Task GetCatalogAsync_ReturnsTheSeededCollection()
    {
        var service = CreateService();

        var catalog = await service.GetCatalogAsync();

        Assert.Equal(33, catalog.Count);
        Assert.Contains(catalog, entry => entry.Code == "orc" && entry.Category == CollectionCategory.Heroes);
        Assert.Contains(catalog, entry => entry.Code == "iron-sword" && entry.Category == CollectionCategory.Weapons);
    }

    [Fact]
    public async Task GetCatalogAsync_CarriesStatsAndLoreWhenPresent()
    {
        var service = CreateService();

        var catalog = await service.GetCatalogAsync();

        var orc = Assert.Single(catalog, entry => entry.Code == "orc");
        Assert.Equal(CollectionRarity.Legendary, orc.Rarity);
        Assert.NotNull(orc.Stats);
        Assert.Equal("200", orc.Stats!["HP"]);
        Assert.NotNull(orc.Lore);

        var troll = Assert.Single(catalog, entry => entry.Code == "troll");
        Assert.Null(troll.Stats);
        Assert.Null(troll.Lore);
    }

    private static CollectionService CreateService()
    {
        var store = MockDataBootstrapper.CreateSeededStore();
        return new CollectionService(new MockCollectionRepository(store));
    }
}
