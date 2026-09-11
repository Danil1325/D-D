using DnDGame.BusinessLayer.Models;
using DnDGame.Domain.Entities.Cards;
using DnDGame.Domain.Entities.Characters;

namespace DnDGame.BusinessLayer.Engines.Interfaces;

/// <summary>Validates and pays for ability/spell use without applying the effect itself.</summary>
public interface IAbilityUseEngine
{
    EngineResult<AbilityUseResult> UseAbility(
        PlayerCharacter playerCharacter,
        PlayerCard playerCard,
        AbilityUseContext context);
}
