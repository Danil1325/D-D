using DnDGame.BusinessLayer.Engines.Interfaces;
using DnDGame.BusinessLayer.Models;
using DnDGame.Domain.Entities.Cards;
using DnDGame.Domain.Entities.Characters;
using DnDGame.Domain.Enums;

namespace DnDGame.BusinessLayer.Engines;

/// <summary>
/// Enforces ability/spell eligibility before spending its resource. Effect
/// resolution remains delegated to the existing card-effect pipeline.
/// </summary>
public sealed class AbilityUseEngine : IAbilityUseEngine
{
    private readonly IPlayEngine _playEngine;

    public AbilityUseEngine(IPlayEngine playEngine)
    {
        _playEngine = playEngine ?? throw new ArgumentNullException(nameof(playEngine));
    }

    public EngineResult<AbilityUseResult> UseAbility(
        PlayerCharacter playerCharacter,
        PlayerCard playerCard,
        AbilityUseContext context)
    {
        ArgumentNullException.ThrowIfNull(playerCharacter);
        ArgumentNullException.ThrowIfNull(playerCard);
        ArgumentNullException.ThrowIfNull(context);

        if (!playerCard.Unlocked)
        {
            return EngineResult<AbilityUseResult>.Failure(
                ErrorCode.ABILITY_LOCKED,
                "This ability or spell is locked.");
        }

        var card = playerCard.Card;
        if (card is null || playerCard.CardId != card.Id)
        {
            return EngineResult<AbilityUseResult>.Failure(
                ErrorCode.CARD_CANNOT_BE_PLAYED,
                "The ability or spell card definition is missing or does not match the player card.");
        }

        if (card.CardType is not (CardType.Ability or CardType.Spell))
        {
            return EngineResult<AbilityUseResult>.Failure(
                ErrorCode.CARD_CANNOT_BE_PLAYED,
                "Only cards of type Ability or Spell can be used through the ability pipeline.");
        }

        if (card.RequiredClassId is not null && playerCharacter.ClassId != card.RequiredClassId)
        {
            return EngineResult<AbilityUseResult>.Failure(
                ErrorCode.CLASS_REQUIREMENT_NOT_MET,
                "The character does not have the required class for this ability or spell.");
        }

        if (card.RequiredLevel < 1 || playerCharacter.Level < card.RequiredLevel)
        {
            return EngineResult<AbilityUseResult>.Failure(
                ErrorCode.LEVEL_REQUIREMENT_NOT_MET,
                "The character does not meet the required level for this ability or spell.");
        }

        if (card.ResourceCost < 0 || context.AvailableResource < card.ResourceCost)
        {
            return EngineResult<AbilityUseResult>.Failure(
                ErrorCode.INSUFFICIENT_ENERGY,
                "The character does not have enough resource to use this ability or spell.");
        }

        if (!Enum.IsDefined(card.EffectType))
        {
            return EngineResult<AbilityUseResult>.Failure(
                ErrorCode.CARD_CANNOT_BE_PLAYED,
                "The ability or spell has an invalid effect.");
        }

        var cardInstance = new CardInstance { Card = card };
        if (!_playEngine.IsValidTarget(cardInstance, context.Target))
        {
            return EngineResult<AbilityUseResult>.Failure(
                ErrorCode.INVALID_TARGET,
                "The selected target is invalid for this ability or spell.");
        }

        context.Consume(card.ResourceCost);

        return EngineResult<AbilityUseResult>.Success(new AbilityUseResult(
            card.ResourceCost,
            context.AvailableResource,
            card.TargetType,
            card.EffectType));
    }
}
