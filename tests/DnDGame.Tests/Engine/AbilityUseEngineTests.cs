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

    [Fact]
    public void UseAbility_ClassMismatch_ReturnsClassRequirementNotMet()
    {
        var character = new PlayerCharacter { Id = 1, ClassId = 99, Level = 5 };
        var ability = CreateAbility(requiredLevel: 1);
        var playerCard = Unlock(ability, character);
        var context = new AbilityUseContext(availableResource: 10);

        var result = _engine.UseAbility(character, playerCard, context);

        Assert.False(result.IsSuccess);
        Assert.Equal(ErrorCode.CLASS_REQUIREMENT_NOT_MET, result.ErrorCode);
        Assert.Equal(10, context.AvailableResource);
    }

    [Fact]
    public void UseAbility_InsufficientResource_DoesNotConsumeResource()
    {
        var character = CreateCharacter();
        var playerCard = Unlock(CreateAbility(resourceCost: 20), character);
        var context = new AbilityUseContext(availableResource: 5);

        var result = _engine.UseAbility(character, playerCard, context);

        Assert.False(result.IsSuccess);
        Assert.Equal(ErrorCode.INSUFFICIENT_ENERGY, result.ErrorCode);
        Assert.Equal(5, context.AvailableResource);
    }

    [Fact]
    public void UseAbility_WrongCardType_ReturnsCardCannotBePlayed()
    {
        var character = CreateCharacter();
        var weapon = new Card
        {
            Id = 10,
            CardType = CardType.Weapon,
            RequiredLevel = 1,
            ResourceCost = 1,
            TargetType = TargetType.Self,
            EffectType = EffectType.Damage
        };
        var playerCard = Unlock(weapon, character);
        var context = new AbilityUseContext(availableResource: 10);

        var result = _engine.UseAbility(character, playerCard, context);

        Assert.False(result.IsSuccess);
        Assert.Equal(ErrorCode.CARD_CANNOT_BE_PLAYED, result.ErrorCode);
        Assert.Equal(10, context.AvailableResource);
    }

    [Fact]
    public void UseAbility_ExactlyMeetsResourceCost_ReturnsSuccess()
    {
        var character = CreateCharacter();
        var playerCard = Unlock(CreateAbility(resourceCost: 5), character);
        var context = new AbilityUseContext(availableResource: 5);

        var result = _engine.UseAbility(character, playerCard, context);

        Assert.True(result.IsSuccess);
        Assert.Equal(0, context.AvailableResource);
        Assert.Equal(5, result.Data!.ResourceCost);
        Assert.Equal(0, result.Data.RemainingResource);
    }

    [Fact]
    public void UseAbility_NullCardDefinition_ReturnsCardCannotBePlayed()
    {
        var character = CreateCharacter();
        var playerCard = new PlayerCard(99);
        playerCard.Unlock(new LevelCardUnlockRule(99, requiredLevel: 1), character);
        playerCard.Card = null;

        var result = _engine.UseAbility(character, playerCard, new AbilityUseContext(availableResource: 10));

        Assert.False(result.IsSuccess);
        Assert.Equal(ErrorCode.CARD_CANNOT_BE_PLAYED, result.ErrorCode);
    }

    [Fact]
    public void UseAbility_NullCharacter_Throws()
    {
        var playerCard = new PlayerCard(1) { Card = CreateAbility() };
        var context = new AbilityUseContext(availableResource: 10);

        Assert.Throws<ArgumentNullException>(() => _engine.UseAbility(null!, playerCard, context));
    }

    [Fact]
    public void UseAbility_NullPlayerCard_Throws()
    {
        var character = CreateCharacter();
        var context = new AbilityUseContext(availableResource: 10);

        Assert.Throws<ArgumentNullException>(() => _engine.UseAbility(character, null!, context));
    }

    [Fact]
    public void UseAbility_NullContext_Throws()
    {
        var character = CreateCharacter();
        var playerCard = Unlock(CreateAbility(), character);

        Assert.Throws<ArgumentNullException>(() => _engine.UseAbility(character, playerCard, null!));
    }

    [Fact]
    public void UseAbility_SpellCardType_ReturnsSuccess()
    {
        var character = CreateCharacter();
        var spell = CreateAbility();
        spell.CardType = CardType.Spell;
        var playerCard = Unlock(spell, character);
        var context = new AbilityUseContext(availableResource: 10);

        var result = _engine.UseAbility(character, playerCard, context);

        Assert.True(result.IsSuccess);
        Assert.Equal(CardType.Spell, playerCard.Card!.CardType);
    }

    [Fact]
    public void UseAbility_TargetedSingleEnemy_ReturnsSuccessWithTarget()
    {
        var character = CreateCharacter();
        var ability = CreateAbility(resourceCost: 2);
        ability.TargetType = TargetType.SingleEnemy;
        var playerCard = Unlock(ability, character);
        var target = new Target(42);
        var context = new AbilityUseContext(availableResource: 10, target: target);

        var result = _engine.UseAbility(character, playerCard, context);

        Assert.True(result.IsSuccess);
        Assert.Equal(TargetType.SingleEnemy, result.Data!.TargetType);
        Assert.Equal(8, context.AvailableResource);
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

    private sealed class Target(int targetId) : ICardTarget
    {
        public int TargetId { get; } = targetId;
        public string TargetType => "Enemy";
    }
}
