using DnDGame.API.Controllers;
using DnDGame.BusinessLayer.Dtos.Skills;
using DnDGame.BusinessLayer.Services;
using DnDGame.Domain.Entities.Characters;
using DnDGame.MockData;
using DnDGame.MockData.Repositories;
using DnDGame.MockData.Services;
using Microsoft.AspNetCore.Mvc;
using Xunit;

namespace DnDGame.Tests.API;

/// <summary>
/// API-surface tests for the skill-tree endpoints. The player identity comes
/// from MockCurrentPlayerService (same as the rest of the API); the store is
/// the seeded singleton backing the service instance.
/// </summary>
public class SkillControllerTests
{
    private const int HumanRaceId = 1;
    private const int MagicianClassId = 3;

    [Fact]
    public async Task GetCurrent_ReturnsTheCharactersSkillTree()
    {
        var (service, store) = CreateServiceWithCharacter();
        var controller = new SkillController(service);

        var response = await controller.GetCurrent();

        var result = Assert.IsType<OkObjectResult>(response.Result);
        var tree = Assert.IsType<CharacterSkillsDto>(result.Value);
        Assert.Equal(1, tree.PlayerId);
        Assert.Equal(7, tree.Skills.Count);
        _ = store;
    }

    [Fact]
    public async Task Unlock_SpendsAPointAndReturnsTheUpdatedTree()
    {
        var (service, store) = CreateServiceWithCharacter();
        var skillId = store.SkillDefinitions.First(s => s.RaceId == HumanRaceId && s.ClassId == MagicianClassId).Id;
        var controller = new SkillController(service);

        var response = await controller.Unlock(skillId);

        var result = Assert.IsType<OkObjectResult>(response.Result);
        var tree = Assert.IsType<CharacterSkillsDto>(result.Value);
        Assert.Equal(4, tree.AvailableSkillPoints);
        Assert.True(tree.Skills.Single(s => s.Id == skillId).IsUnlocked);
    }

    private static (SkillService Service, InMemoryGameDataStore Store) CreateServiceWithCharacter()
    {
        var store = MockDataBootstrapper.CreateSeededStore();
        var currentPlayer = new MockCurrentPlayerService();
        store.Characters.Add(new PlayerCharacter
        {
            Id = 1,
            OwnerId = currentPlayer.GetCurrentPlayerId().ToString(),
            Name = "Hero",
            Level = 1,
            RaceId = HumanRaceId,
            ClassId = MagicianClassId,
            SkillPoints = 5
        });

        var service = new SkillService(
            new MockSkillDefinitionRepository(store),
            new MockCharacterSkillRepository(store),
            new MockCharacterRepository(store),
            currentPlayer);

        return (service, store);
    }
}
