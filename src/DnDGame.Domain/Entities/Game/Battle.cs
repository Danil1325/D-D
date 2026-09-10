namespace DnDGame.Domain.Entities.Game;

using DnDGame.Domain.Common;
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
}
