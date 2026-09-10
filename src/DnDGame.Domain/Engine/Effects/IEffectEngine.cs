using DnDGame.Domain.Engine.Battle;
using DnDGame.Domain.Engine.Enums;

namespace DnDGame.Domain.Engine.Effects;

/// <summary>
/// Contract for resolving effects at turn boundaries.
/// </summary>
public interface IEffectEngine
{
    void ApplyStartOfTurnEffects(BattleState battleState, TurnType turn);

    void ApplyEndOfTurnEffects(BattleState battleState, TurnType turn);
}
