namespace DnDGame.Domain.Entities.Game;

using DnDGame.Domain.Common;
using DnDGame.Domain.Engine.Enums;
using DnDGame.Domain.Engine.Models;
using DnDGame.Domain.Entities.Enemies;
using DnDGame.Domain.Enums;

/// <summary>
/// Represents an active battle in the game.
/// Tracks battle state, participants, turn order, and energy/resource pools.
/// </summary>
public class Battle : BaseEntity
{
    /// <summary>
    /// The ID of the game session this battle belongs to.
    /// </summary>
    public int GameSessionId { get; set; }

    /// <summary>
    /// Navigation property to the game session.
    /// </summary>
    public virtual GameSession? GameSession { get; set; }

    /// <summary>
    /// The current status of the battle.
    /// </summary>
    public GameSessionStatus Status { get; set; } = GameSessionStatus.InProgress;

    /// <summary>
    /// The ID of the player currently taking a turn.
    /// For single-player battles, this is the player character ID.
    /// </summary>
    public int CurrentPlayerTurnId { get; set; }

    /// <summary>
    /// The current round/turn number of the battle.
    /// Increments each time the turn passes to a new player.
    /// </summary>
    public int CurrentRound { get; set; } = 1;

    /// <summary>
    /// The current energy pool of the active player.
    /// Resets at the start of each turn and is spent playing cards.
    /// </summary>
    public int CurrentPlayerEnergy { get; set; }

    /// <summary>
    /// The maximum energy pool for the current player this turn.
    /// Used to limit card plays.
    /// </summary>
    public int MaxPlayerEnergyPerTurn { get; set; } = 5;

    /// <summary>
    /// The timestamp when this battle started.
    /// </summary>
    public DateTime StartedAt { get; set; }

    /// <summary>
    /// The timestamp when this battle ended (if completed).
    /// </summary>
    public DateTime? CompletedAt { get; set; }

    /// <summary>
    /// Notes or description about the battle outcome/events.
    /// </summary>
    public string? Notes { get; set; } = string.Empty;

    /// <summary>
    /// The ID of the enemy this battle is fought against. Resolved once at battle
    /// start (from the game session's current story node) and persisted, so it can
    /// be re-resolved on every later request without re-walking the story graph.
    /// </summary>
    public int EnemyId { get; set; }

    /// <summary>
    /// Stable identity of the individual summoner across encounters in this session.
    /// Null for ordinary enemies; all repeat summons from that individual share this value.
    /// This is an instance identifier, not an enemy template ID.
    /// </summary>
    public Guid? SummonerInstanceId { get; set; }

    /// <summary>
    /// Navigation property to the enemy.
    /// </summary>
    public virtual Enemy? Enemy { get; set; }

    /// <summary>
    /// The current block/shield value for the enemy or opponent.
    /// Temporary shield that reduces incoming damage. Resets each turn.
    /// </summary>
    public int EnemyBlock { get; set; } = 0;

    /// <summary>
    /// The current health of the enemy or opponent.
    /// Updated as damage is dealt. Battle ends when this reaches 0.
    /// </summary>
    public int EnemyCurrentHealth { get; set; }

    /// <summary>
    /// The current health of the player.
    /// Updated as damage is dealt. Battle ends when this reaches 0.
    /// </summary>
    public int PlayerCurrentHealth { get; set; }

    /// <summary>
    /// The maximum health of the player.
    /// Used for heal effect cap calculations.
    /// </summary>
    public int PlayerMaxHealth { get; set; }

    /// <summary>
    /// Whether end-of-battle rewards (XP, loot, etc.) have already been granted for
    /// this battle. Prevents granting rewards more than once for the same battle.
    /// </summary>
    public bool RewardsGranted { get; set; } = false;

    /// <summary>
    /// Which combatant is currently acting. Distinct from <see cref="Status"/>,
    /// which tracks the battle's terminal outcome (in progress/victory/defeat),
    /// not turn ownership.
    /// </summary>
    public TurnType CurrentTurn { get; set; } = TurnType.Player;

    /// <summary>
    /// Chronological history of meaningful battle events (attacks, card plays,
    /// dice rolls, etc.) for both combatants.
    /// </summary>
    public List<BattleLogEntry> BattleLog { get; set; } = new();

    /// <summary>
    /// Buffs/debuffs currently active on either combatant, discriminated by
    /// <see cref="ActiveEffect.Target"/>.
    /// </summary>
    public IList<ActiveEffect> ActiveEffects { get; set; } = new List<ActiveEffect>();
}
