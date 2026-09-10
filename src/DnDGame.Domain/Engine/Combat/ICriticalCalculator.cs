using DnDGame.Domain.Engine.Battle;

namespace DnDGame.Domain.Engine.Combat;

/// <summary>
/// Contract for evaluating whether an attack is a critical hit.
/// </summary>
public interface ICriticalCalculator
{
    bool IsCriticalHit(BattleContext battleContext);
}
