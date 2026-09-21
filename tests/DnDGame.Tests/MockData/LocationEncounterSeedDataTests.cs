using DnDGame.Domain.Entities.Locations;
using DnDGame.Domain.Enums;
using DnDGame.MockData;
using Xunit;

namespace DnDGame.Tests.MockData;

public class LocationEncounterSeedDataTests
{
    [Fact]
    public void SeededStore_HasOneEncounterDefinitionPerLocationId()
    {
        var store = MockDataBootstrapper.CreateSeededStore();
        var allLocationIds = Enum.GetValues<LocationId>();

        Assert.Equal(allLocationIds.Length, store.LocationEncounterDefinitions.Count);
        Assert.Equal(
            allLocationIds.OrderBy(id => id),
            store.LocationEncounterDefinitions.Select(definition => definition.LocationId).OrderBy(id => id));
    }

    [Fact]
    public void EveryEnemyId_ReferencesARealSeededEnemy()
    {
        var store = MockDataBootstrapper.CreateSeededStore();
        var realEnemyIds = store.Enemies.Select(enemy => enemy.Id).ToHashSet();

        foreach (var definition in store.LocationEncounterDefinitions)
        {
            foreach (var enemy in definition.Enemies)
            {
                Assert.Contains(enemy.EnemyId, realEnemyIds);
            }
        }
    }

    [Fact]
    public void NamedCampaignBosses_ResolveToTheirPlaceholderEnemyRows()
    {
        var store = MockDataBootstrapper.CreateSeededStore();
        var enemiesById = store.Enemies.ToDictionary(enemy => enemy.Id);
        var allSlots = store.LocationEncounterDefinitions.SelectMany(definition => definition.Enemies);

        var expectedBossIds = new[] { 22, 23, 24, 25, 26, 27 };
        var expectedNames = new[] { "The Herald", "Vharruk", "Karnyx", "Nerath-Dur the Lich", "Grommash-Vurr the Troll King", "Greater Demon Mayor" };

        foreach (var (enemyId, name) in expectedBossIds.Zip(expectedNames))
        {
            Assert.Equal(name, enemiesById[enemyId].Name);

            var slots = allSlots.Where(slot => slot.EnemyId == enemyId).ToList();
            Assert.NotEmpty(slots);
            Assert.All(slots, slot => Assert.Equal(EncounterTier.Boss, slot.Tier));
        }
    }

    [Fact]
    public void WhisperingWoods_HasDreadWraithAsItsSoleBoss()
    {
        var store = MockDataBootstrapper.CreateSeededStore();
        var definition = Assert.Single(store.LocationEncounterDefinitions, d => d.LocationId == LocationId.WhisperingWoods);

        var bosses = definition.Enemies.Where(enemy => enemy.Tier == EncounterTier.Boss).ToList();
        var boss = Assert.Single(bosses);
        Assert.Equal(21, boss.EnemyId);
    }

    [Fact]
    public void MisthavenPort_HasNoBossAndRestrictsEncountersToNamedSubLocations()
    {
        var store = MockDataBootstrapper.CreateSeededStore();
        var definition = Assert.Single(store.LocationEncounterDefinitions, d => d.LocationId == LocationId.MisthavenPort);

        Assert.DoesNotContain(definition.Enemies, enemy => enemy.Tier == EncounterTier.Boss);
        Assert.All(definition.Enemies, enemy =>
        {
            Assert.NotNull(enemy.Availability);
            Assert.Equal(new[] { "arena", "archives", "port", "wrecks" }, enemy.Availability!.RequiredSubLocations);
        });
    }

    [Fact]
    public void Oakheaven_GoblinAndHobgoblin_OnlyAvailableWhenAllianceFailedFlagIsSet()
    {
        var store = MockDataBootstrapper.CreateSeededStore();
        var definition = Assert.Single(store.LocationEncounterDefinitions, d => d.LocationId == LocationId.Oakheaven);

        foreach (var enemyId in new[] { 4, 5 }) // Goblin, Hobgoblin
        {
            var slot = Assert.Single(definition.Enemies, enemy => enemy.EnemyId == enemyId);
            Assert.NotNull(slot.Availability);
            Assert.Equal("OakheavenAllianceFailed", slot.Availability!.RequiredFlag);
            Assert.True(slot.Availability.RequiredFlagValue);
        }

        Assert.DoesNotContain(definition.Enemies, enemy => (enemy.EnemyId == 16 || enemy.EnemyId == 7) && enemy.Availability != null);
    }
}
