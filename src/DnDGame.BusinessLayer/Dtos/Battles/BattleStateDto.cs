using DnDGame.Domain.Engine.Enums;
using DnDGame.Domain.Entities.Game;
using DnDGame.Domain.Enums;

namespace DnDGame.BusinessLayer.Dtos.Battles;

/// <summary>
/// Response shape for GET /api/battle/{battleId} and the mutating battle
/// endpoints. Assembled from both Battle and its BattleDeck, since the two
/// together hold everything BattleState tracks (see docs/ARCHITECTURE.md §15).
/// DrawPile/DiscardPile are exposed only as counts, not contents — deck order
/// shouldn't be visible to the client, same spirit as the Card feature's
/// visibility rules.
/// </summary>
public class BattleStateDto
{
    public int Id { get; init; }
    public int GameSessionId { get; init; }
    public GameSessionStatus Status { get; init; }
    public TurnType CurrentTurn { get; init; }
    public int CurrentRound { get; init; }

    public int PlayerCurrentHealth { get; init; }
    public int PlayerMaxHealth { get; init; }
    public int PlayerCurrentEnergy { get; init; }
    public int PlayerMaxEnergyPerTurn { get; init; }
    public int PlayerBlock { get; init; }

    public int EnemyCurrentHealth { get; init; }
    public int EnemyMaxHealth { get; init; }
    public int EnemyBlock { get; init; }

    public IReadOnlyList<CardInstanceDto> Hand { get; init; } = Array.Empty<CardInstanceDto>();
    public int DrawPileCount { get; init; }
    public int DiscardPileCount { get; init; }

    public IReadOnlyList<ActiveEffectDto> ActiveEffects { get; init; } = Array.Empty<ActiveEffectDto>();

    public bool RewardsGranted { get; init; }

    /// <summary>
    /// Enemy max health is not persisted on Battle (only current health is — see
    /// docs/ARCHITECTURE.md §15), so callers resolve it separately (e.g. from the
    /// Enemy repository) and pass it in.
    /// </summary>
    public static BattleStateDto FromDomain(Battle battle, BattleDeck battleDeck, int enemyMaxHealth) => new()
    {
        Id = battle.Id,
        GameSessionId = battle.GameSessionId,
        Status = battle.Status,
        CurrentTurn = battle.CurrentTurn,
        CurrentRound = battle.CurrentRound,

        PlayerCurrentHealth = battle.PlayerCurrentHealth,
        PlayerMaxHealth = battle.PlayerMaxHealth,
        PlayerCurrentEnergy = battle.CurrentPlayerEnergy,
        PlayerMaxEnergyPerTurn = battle.MaxPlayerEnergyPerTurn,
        PlayerBlock = battleDeck.CurrentBlock,

        EnemyCurrentHealth = battle.EnemyCurrentHealth,
        EnemyMaxHealth = enemyMaxHealth,
        EnemyBlock = battle.EnemyBlock,

        Hand = battleDeck.Hand.Select(CardInstanceDto.FromDomain).ToList(),
        DrawPileCount = battleDeck.DrawPile.Count,
        DiscardPileCount = battleDeck.DiscardPile.Count,

        ActiveEffects = battle.ActiveEffects.Select(ActiveEffectDto.FromDomain).ToList(),

        RewardsGranted = battle.RewardsGranted
    };
}
