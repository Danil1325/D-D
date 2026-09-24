using DnDGame.BusinessLayer.Common.Errors;
using DnDGame.BusinessLayer.Common.Exceptions;
using DnDGame.BusinessLayer.Services;
using DnDGame.BusinessLayer.Services.Interfaces;
using DnDGame.Domain.Entities.Characters;
using DnDGame.MockData;
using DnDGame.MockData.Repositories;
using DnDGame.MockData.Services;

namespace DnDGame.Tests.BusinessLayer;

public class SkillServiceTests
{
    private const int PlayerCharacterId = 1;
    private const int HumanRaceId = 1;
    private const int MagicianClassId = 3;

    [Fact]
    public async Task GetSkillTreeForCurrentPlayer_ReturnsOnlyTheCharactersRaceAndClassSkills()
    {
        var (service, _) = CreateServiceWithCharacter();

        var tree = await service.GetSkillTreeForCurrentPlayerAsync();

        Assert.Equal(PlayerCharacterId, tree.PlayerId);
        Assert.Equal(7, tree.Skills.Count);
        Assert.All(tree.Skills, skill => Assert.StartsWith("human-mage-", skill.Code));
        Assert.All(tree.Skills, skill => Assert.False(skill.IsUnlocked));
    }

    [Fact]
    public async Task GetSkillTreeForCurrentPlayer_ReflectsTheCharactersSkillPointsBalance()
    {
        var (service, store) = CreateServiceWithCharacter(skillPoints: 5);
        _ = store;

        var tree = await service.GetSkillTreeForCurrentPlayerAsync();

        Assert.Equal(5, tree.AvailableSkillPoints);
    }

    [Fact]
    public async Task GetSkillTreeForCurrentPlayer_WhenNoCharacter_ThrowsNotFound()
    {
        var (service, _) = CreateService();

        var exception = await Assert.ThrowsAsync<DomainException>(() => service.GetSkillTreeForCurrentPlayerAsync());

        Assert.Equal(ErrorCodes.NotFound, exception.ErrorCode);
    }

    [Fact]
    public async Task UnlockSkill_SpendsSkillPointsAndMarksTheSkillUnlocked()
    {
        var (service, store) = CreateServiceWithCharacter(skillPoints: 3);
        var skillId = store.SkillDefinitions.First(s => s.RaceId == HumanRaceId && s.ClassId == MagicianClassId).Id;

        var tree = await service.UnlockSkillForCurrentPlayerAsync(skillId);

        Assert.Equal(2, tree.AvailableSkillPoints);
        var unlocked = Assert.Single(tree.Skills, s => s.Id == skillId);
        Assert.True(unlocked.IsUnlocked);
        Assert.NotNull(unlocked.UnlockedAt);

        var character = store.Characters.Single(c => c.Id == PlayerCharacterId);
        Assert.Equal(2, character.SkillPoints);
    }

    [Fact]
    public async Task UnlockSkill_WhenAlreadyUnlocked_ThrowsAlreadyUnlocked()
    {
        var (service, store) = CreateServiceWithCharacter(skillPoints: 5);
        var skillId = store.SkillDefinitions.First(s => s.RaceId == HumanRaceId && s.ClassId == MagicianClassId).Id;
        await service.UnlockSkillForCurrentPlayerAsync(skillId);

        var exception = await Assert.ThrowsAsync<DomainException>(() => service.UnlockSkillForCurrentPlayerAsync(skillId));

        Assert.Equal(SkillErrorCodes.SkillAlreadyUnlocked, exception.ErrorCode);
    }

    [Fact]
    public async Task UnlockSkill_WhenNotEnoughSkillPoints_ThrowsInsufficientSkillPoints()
    {
        var (service, store) = CreateServiceWithCharacter(skillPoints: 0);
        var skillId = store.SkillDefinitions.First(s => s.RaceId == HumanRaceId && s.ClassId == MagicianClassId).Id;

        var exception = await Assert.ThrowsAsync<DomainException>(() => service.UnlockSkillForCurrentPlayerAsync(skillId));

        Assert.Equal(SkillErrorCodes.InsufficientSkillPoints, exception.ErrorCode);
    }

    [Fact]
    public async Task UnlockSkill_WhenSkillBelongsToAnotherRaceOrClass_ThrowsNotFound()
    {
        var (service, store) = CreateServiceWithCharacter(skillPoints: 5);
        var otherSkillId = store.SkillDefinitions.First(s => s.RaceId != HumanRaceId || s.ClassId != MagicianClassId).Id;

        var exception = await Assert.ThrowsAsync<DomainException>(() => service.UnlockSkillForCurrentPlayerAsync(otherSkillId));

        Assert.Equal(ErrorCodes.NotFound, exception.ErrorCode);
    }

    private static (SkillService Service, InMemoryGameDataStore Store) CreateService()
    {
        var store = MockDataBootstrapper.CreateSeededStore();
        var service = CreateSkillService(store, new MockCurrentPlayerService());
        return (service, store);
    }

    private static (SkillService Service, InMemoryGameDataStore Store) CreateServiceWithCharacter(int skillPoints = 1)
    {
        var (service, store) = CreateService();
        var currentPlayer = new MockCurrentPlayerService();
        store.Characters.Add(new PlayerCharacter
        {
            Id = PlayerCharacterId,
            OwnerId = currentPlayer.GetCurrentPlayerId().ToString(),
            Name = "Hero",
            Level = 1,
            RaceId = HumanRaceId,
            ClassId = MagicianClassId,
            SkillPoints = skillPoints
        });

        return (CreateSkillService(store, currentPlayer), store);
    }

    private static SkillService CreateSkillService(InMemoryGameDataStore store, ICurrentPlayerService currentPlayerService)
    {
        return new SkillService(
            new MockSkillDefinitionRepository(store),
            new MockCharacterSkillRepository(store),
            new MockCharacterRepository(store),
            currentPlayerService);
    }
}
