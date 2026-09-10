using DnDGame.Domain.Entities.Characters;
using DnDGame.Domain.Entities.Enemies;

namespace DnDGame.Domain.Engine.Battle;

/// <summary>
/// Groups the combatants and mutable state required to process a single battle.
/// </summary>
public sealed class BattleContext
{
    public BattleContext(PlayerCharacter player, Enemy enemy, BattleState battleState)
    {
        ArgumentNullException.ThrowIfNull(player);
        ArgumentNullException.ThrowIfNull(enemy);
        ArgumentNullException.ThrowIfNull(battleState);

        Player = player;
        Enemy = enemy;
        BattleState = battleState;
    }

    /// <summary>The player character participating in the battle.</summary>
    public PlayerCharacter Player { get; }

    /// <summary>The enemy participating in the battle.</summary>
    public Enemy Enemy { get; }

    /// <summary>The mutable state of the current battle.</summary>
    public BattleState BattleState { get; }
}
