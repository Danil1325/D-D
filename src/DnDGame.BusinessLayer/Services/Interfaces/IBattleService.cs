using DnDGame.BusinessLayer.Dtos.Battles;

namespace DnDGame.BusinessLayer.Services.Interfaces;

/// <summary>
/// Orchestrates the battle lifecycle: resolves the current player's Battle/BattleDeck
/// (Domain.Entities.Game — persisted) into the runtime BattleContext/BattleState
/// (Domain.Engine.Battle — transient) IBattleEngine operates on, then reconciles the
/// result back. See docs/ARCHITECTURE.md §15 for why both models exist.
/// </summary>
public interface IBattleService
{
    /// <summary>
    /// Starts a battle for the current player's game session against the enemy
    /// resolved from the session's current story node. Throws DomainException(CONFLICT)
    /// if the session already has an active battle or its current node has no
    /// combat encounter.
    /// </summary>
    Task<BattleStateDto> StartBattleAsync(StartBattleRequestDto request);

    /// <summary>Throws DomainException(NOT_FOUND) if the battle doesn't exist or isn't owned by the current player.</summary>
    Task<BattleStateDto> GetBattleStateAsync(int battleId);

    Task<BattleStateDto> PlayCardAsync(int battleId, PlayCardRequestDto request);

    /// <summary>
    /// Ends the current player's turn. If that hands the turn to the enemy, the
    /// enemy's turn is resolved immediately in the same call — there is no separate
    /// "execute enemy turn" endpoint for the client to call.
    /// </summary>
    Task<BattleStateDto> EndTurnAsync(int battleId);

    Task<IReadOnlyList<BattleLogEntryDto>> GetBattleLogAsync(int battleId);
}
