using DnDGame.BusinessLayer.Engines;
using DnDGame.BusinessLayer.Engines.Interfaces;
using DnDGame.BusinessLayer.Models;
using DnDGame.Domain.Configuration;
using DnDGame.Domain.Entities.Cards;
using DnDGame.Domain.Entities.Characters;
using DnDGame.Domain.Enums;
using Xunit;

namespace DnDGame.Tests.Engine;

public class AbilityUseEngineTests
{
    private readonly AbilityUseEngine _engine = new(new PlayEngine(new PlayRules()));

    [Fact]
    public void UseAbility_WhenLocked_ReturnsAbilityLockedAndDoesNotConsumeResource()
    {
        var context = new AbilityUseContext(availableResource: 10);
        var playerCard = new PlayerCard(5) { Card = CreateAbility() };

        var result = _engine.UseAbility(CreateCharacter(), playerCard, context);

        Assert.False(result.IsSuccess);
        Assert.Equal(ErrorCode.ABILITY_LOCKED, result.ErrorCode);
        Assert.Equal(10, context.AvailableResource);
    }

    [Fact]
    public void UseAbility_WhenRequirementFails_DoesNotConsumeResource()
    {
        var character = CreateCharacter(level: 1);
        var playerCard = Unlock(CreateAbility(requiredLevel: 2), character);
        var context = new AbilityUseContext(availableResource: 10);

        var result = _engine.UseAbility(character, playerCard, context);

        Assert.False(result.IsSuccess);
        Assert.Equal(ErrorCode.LEVEL_REQUIREMENT_NOT_MET, result.ErrorCode);
        Assert.Equal(10, context.AvailableResource);
    }

    [Fact]
    public void UseAbility_WhenTargetIsInvalid_DoesNotConsumeResource()
    {
        var character = CreateCharacter();
        var ability = CreateAbility();
        ability.TargetType = TargetType.SingleEnemy;
        var playerCard = Unlock(ability, character);
        var context = new AbilityUseContext(availableResource: 10);

        var result = _engine.UseAbility(character, playerCard, context);

        Assert.False(result.IsSuccess);
        Assert.Equal(ErrorCode.INVALID_TARGET, result.ErrorCode);
        Assert.Equal(10, context.AvailableResource);
    }

    [Fact]
    public void UseAbility_WhenAllRulesPass_ConsumesResourceAndReturnsTargetAndEffect()
    {
        var character = CreateCharacter();
        var playerCard = Unlock(CreateAbility(resourceCost: 4), character);
        var context = new AbilityUseContext(availableResource: 10);

        var result = _engine.UseAbility(character, playerCard, context);

        Assert.True(result.IsSuccess);
        Assert.Equal(6, context.AvailableResource);
        Assert.Equal(4, result.Data!.ResourceCost);
        Assert.Equal(TargetType.Self, result.Data.TargetType);
        Assert.Equal(EffectType.Healing, result.Data.EffectType);
    }

    private static PlayerCharacter CreateCharacter(int level = 3) => new() { Id = 1, ClassId = 2, Level = level };

    private static PlayerCard Unlock(Card card, PlayerCharacter character)
    {
        var playerCard = new PlayerCard(card.Id) { Card = card };
        playerCard.Unlock(new LevelCardUnlockRule(card.Id, 1), character);
        return playerCard;
    }

    private static Card CreateAbility(int requiredLevel = 2, int resourceCost = 3) => new()
    {
        Id = 5,
        CardType = CardType.Ability,
        RequiredClassId = 2,
        RequiredLevel = requiredLevel,
        ResourceCost = resourceCost,
        TargetType = TargetType.Self,
        EffectType = EffectType.Healing
    };
}
