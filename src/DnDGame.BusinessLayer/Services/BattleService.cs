using DnDGame.BusinessLayer.Common.Errors;
using DnDGame.BusinessLayer.Common.Exceptions;
using DnDGame.BusinessLayer.Dtos.Battles;
using DnDGame.BusinessLayer.Engines.Interfaces;
using DnDGame.BusinessLayer.Repositories.Interfaces;
using DnDGame.BusinessLayer.Services.Interfaces;
using DnDGame.BusinessLayer.Validation;
using DnDGame.Domain.Configuration;
using DnDGame.Domain.Engine.Battle;
using DnDGame.Domain.Engine.Common;
using DnDGame.Domain.Engine.Enums;
using DnDGame.Domain.Engine.Models;
using DnDGame.Domain.Entities.Characters;
using DnDGame.Domain.Entities.Enemies;
using DnDGame.Domain.Entities.Game;
using DnDGame.Domain.Enums;

namespace DnDGame.BusinessLayer.Services;

public class BattleService : IBattleService
{
    private readonly IBattleRepository _battleRepository;
    private readonly IBattleDeckRepository _battleDeckRepository;
    private readonly IGameSessionRepository _gameSessionRepository;
    private readonly IStoryNodeRepository _storyNodeRepository;
    private readonly ICharacterRepository _characterRepository;
    private readonly IEnemyRepository _enemyRepository;
    private readonly IDeckRepository _deckRepository;
    private readonly IDeckEngine _deckEngine;
    private readonly IBattleEngine _battleEngine;
    private readonly PlayRules _playRules;
    private readonly ICurrentPlayerService _currentPlayerService;
    private readonly ICombatExperienceCalculator _combatExperienceCalculator;

    public BattleService(
        IBattleRepository battleRepository,
        IBattleDeckRepository battleDeckRepository,
        IGameSessionRepository gameSessionRepository,
        IStoryNodeRepository storyNodeRepository,
        ICharacterRepository characterRepository,
        IEnemyRepository enemyRepository,
        IDeckRepository deckRepository,
        IDeckEngine deckEngine,
        IBattleEngine battleEngine,
        PlayRules playRules,
        ICurrentPlayerService currentPlayerService,
        ICombatExperienceCalculator combatExperienceCalculator)
    {
        _battleRepository = battleRepository;
        _battleDeckRepository = battleDeckRepository;
        _gameSessionRepository = gameSessionRepository;
        _storyNodeRepository = storyNodeRepository;
        _characterRepository = characterRepository;
        _enemyRepository = enemyRepository;
        _deckRepository = deckRepository;
        _deckEngine = deckEngine;
        _battleEngine = battleEngine;
        _playRules = playRules;
        _currentPlayerService = currentPlayerService;
        _combatExperienceCalculator = combatExperienceCalculator;
    }

    public async Task<BattleStateDto> StartBattleAsync(StartBattleRequestDto request)
    {
        ArgumentNullException.ThrowIfNull(request);
        RequirePositiveId(request.GameSessionId, "gameSessionId");
        RequirePositiveId(request.DeckId, "deckId");

        var currentPlayerId = _currentPlayerService.GetCurrentPlayerId();

        var session = await _gameSessionRepository.GetByIdAsync(request.GameSessionId);
        if (session is null || session.CharacterId != currentPlayerId)
        {
            throw new DomainException(ErrorCodes.NotFound, $"Game session {request.GameSessionId} was not found.");
        }

        var existingActiveBattle = await _battleRepository.GetActiveByGameSessionIdAsync(session.Id);
        if (existingActiveBattle is not null)
        {
            throw new DomainException(ErrorCodes.Conflict, "This game session already has a battle in progress.");
        }

        var node = await _storyNodeRepository.GetByIdAsync(session.CurrentNodeId);
        if (node?.EnemyId is null)
        {
            throw new DomainException(ErrorCodes.Conflict, "The current story node has no combat encounter.");
        }

        var enemy = await _enemyRepository.GetByIdAsync(node.EnemyId.Value);
        if (enemy is null)
        {
            throw new DomainException(ErrorCodes.NotFound, $"Enemy {node.EnemyId} was not found.");
        }

        var player = await _characterRepository.GetByIdAsync(currentPlayerId);
        if (player is null)
        {
            throw new DomainException(ErrorCodes.NotFound, $"Character {currentPlayerId} was not found.");
        }

        var deck = await _deckRepository.GetByIdAsync(request.DeckId);
        if (deck is null || deck.CharacterId != currentPlayerId)
        {
            throw new DomainException(ErrorCodes.NotFound, $"Deck {request.DeckId} was not found.");
        }

        var battleDeck = _deckEngine.CreateBattleDeck(deck);

        // Health/block/turn-number/RewardsGranted are reset by IBattleEngine.StartBattle
        // itself (from BattleContext.Player/Enemy) — only seed what it doesn't own.
        var initialState = new BattleState
        {
            PlayerEnergy = _playRules.MaxEnergyPerTurn,
            PlayerMaxEnergy = _playRules.MaxEnergyPerTurn,
            Hand = battleDeck.Hand,
            DrawPile = battleDeck.DrawPile,
            DiscardPile = battleDeck.DiscardPile,
            ActiveEffects = new List<ActiveEffect>(),
            BattleLog = new List<BattleLogEntry>()
        };

        var context = new BattleContext(player, enemy, initialState);
        var result = _battleEngine.StartBattle(context);
        if (!result.Success || result.Data is null)
        {
            throw new DomainException(result.ErrorCode ?? EngineErrorCodes.InvalidAction, result.Message);
        }

        var finalState = result.Data;

        var battle = new Battle
        {
            GameSessionId = session.Id,
            EnemyId = enemy.Id,
            StartedAt = DateTime.UtcNow
        };
        ApplyState(battle, finalState);
        var createdBattle = await _battleRepository.AddAsync(battle);

        battleDeck.BattleId = createdBattle.Id;
        ApplyState(battleDeck, finalState);
        var createdBattleDeck = await _battleDeckRepository.AddAsync(battleDeck);
        await AwardCombatExperienceAsync(createdBattle, player, enemy);
        await _battleRepository.UpdateAsync(createdBattle);

        return BattleStateDto.FromDomain(createdBattle, createdBattleDeck, enemy.Health);
    }

    public async Task<BattleStateDto> GetBattleStateAsync(int battleId)
    {
        var (battle, battleDeck, _, enemy) = await LoadBattleAsync(battleId);
        return BattleStateDto.FromDomain(battle, battleDeck, enemy.Health);
    }

    public async Task<BattleStateDto> PlayCardAsync(int battleId, PlayCardRequestDto request)
    {
        ArgumentNullException.ThrowIfNull(request);
        var (battle, battleDeck, player, enemy) = await LoadBattleAsync(battleId);

        // Looking up the card is just id resolution — whether it's legal to play
        // (in hand, affordable, valid turn, etc.) is IBattleEngine's call, not ours.
        // A missing card is passed through as null and left for the engine to reject.
        var card = battleDeck.Hand.FirstOrDefault(c => c.InstanceId == request.CardInstanceId);

        if (battle.Status == GameSessionStatus.Abandoned)
            throw new DomainException(ErrorCodes.Conflict, "An abandoned battle cannot be resumed.");

        var state = BuildState(battle, battleDeck, enemy);
        var context = new BattleContext(player, enemy, state);

        var result = _battleEngine.PlayCard(context, card);
        if (!result.Success || result.Data is null)
        {
            throw new DomainException(result.ErrorCode ?? EngineErrorCodes.InvalidAction, result.Message);
        }

        ApplyState(battle, result.Data);
        ApplyState(battleDeck, result.Data);
        await AwardCombatExperienceAsync(battle, player, enemy);
        await _battleRepository.UpdateAsync(battle);
        await _battleDeckRepository.UpdateAsync(battleDeck);

        return BattleStateDto.FromDomain(battle, battleDeck, enemy.Health);
    }

    public async Task<BattleStateDto> EndTurnAsync(int battleId)
    {
        var (battle, battleDeck, player, enemy) = await LoadBattleAsync(battleId);
        if (battle.Status == GameSessionStatus.Abandoned)
            throw new DomainException(ErrorCodes.Conflict, "An abandoned battle cannot be resumed.");

        var state = BuildState(battle, battleDeck, enemy);
        var context = new BattleContext(player, enemy, state);

        var endResult = _battleEngine.EndTurn(context);
        if (!endResult.Success || endResult.Data is null)
        {
            throw new DomainException(endResult.ErrorCode ?? EngineErrorCodes.InvalidAction, endResult.Message);
        }

        var finalState = endResult.Data;

        // No separate "execute enemy turn" endpoint exists for the client to call,
        // so if ending the player's turn hands play to the enemy, resolve the
        // enemy's turn immediately in the same request.
        if (finalState.BattleStatus == BattleStatus.EnemyTurn)
        {
            var enemyTurnContext = new BattleContext(player, enemy, finalState);
            var enemyResult = _battleEngine.ExecuteEnemyTurn(enemyTurnContext);
            if (!enemyResult.Success || enemyResult.Data is null)
            {
                throw new DomainException(enemyResult.ErrorCode ?? EngineErrorCodes.InvalidAction, enemyResult.Message);
            }

            finalState = enemyResult.Data;
        }

        ApplyState(battle, finalState);
        ApplyState(battleDeck, finalState);
        await AwardCombatExperienceAsync(battle, player, enemy);
        await _battleRepository.UpdateAsync(battle);
        await _battleDeckRepository.UpdateAsync(battleDeck);

        return BattleStateDto.FromDomain(battle, battleDeck, enemy.Health);
    }

    public async Task<IReadOnlyList<BattleLogEntryDto>> GetBattleLogAsync(int battleId)
    {
        var (battle, _, _, _) = await LoadBattleAsync(battleId);
        return battle.BattleLog.Select(BattleLogEntryDto.FromDomain).ToList();
    }

    private async Task<(Battle Battle, BattleDeck BattleDeck, PlayerCharacter Player, Enemy Enemy)> LoadBattleAsync(int battleId)
    {
        RequirePositiveId(battleId, "battleId");

        var currentPlayerId = _currentPlayerService.GetCurrentPlayerId();

        var battle = await _battleRepository.GetByIdAsync(battleId);
        if (battle is null)
        {
            throw new DomainException(ErrorCodes.NotFound, $"Battle {battleId} was not found.");
        }

        var session = await _gameSessionRepository.GetByIdAsync(battle.GameSessionId);
        if (session is null || session.CharacterId != currentPlayerId)
        {
            // Deliberately the same NOT_FOUND as "doesn't exist" — same convention
            // as DeckService.GetOwnedDeckOrThrowAsync — so ownership isn't leaked.
            throw new DomainException(ErrorCodes.NotFound, $"Battle {battleId} was not found.");
        }

        var battleDeck = await _battleDeckRepository.GetByBattleIdAsync(battleId);
        if (battleDeck is null)
        {
            throw new DomainException(ErrorCodes.NotFound, $"Battle {battleId} has no battle deck.");
        }

        var player = await _characterRepository.GetByIdAsync(session.CharacterId);
        if (player is null)
        {
            throw new DomainException(ErrorCodes.NotFound, $"Character {session.CharacterId} was not found.");
        }

        var enemy = await _enemyRepository.GetByIdAsync(battle.EnemyId);
        if (enemy is null)
        {
            throw new DomainException(ErrorCodes.NotFound, $"Enemy {battle.EnemyId} was not found.");
        }

        return (battle, battleDeck, player, enemy);
    }

    private async Task AwardCombatExperienceAsync(Battle battle, PlayerCharacter player, Enemy enemy)
    {
        if (battle.Status != GameSessionStatus.Victory)
            return;

        var session = await _gameSessionRepository.GetByIdAsync(battle.GameSessionId);
        if (session is null)
            throw new DomainException(ErrorCodes.NotFound, "The battle session was not found.");

        var reward = _combatExperienceCalculator.CalculateAndAward(battle, enemy, session, player);
        if (reward.WasGranted || reward.RepeatedSummon)
        {
            await _characterRepository.UpdateAsync(player);
            await _gameSessionRepository.UpdateAsync(session);
        }
    }

    private static BattleState BuildState(Battle battle, BattleDeck battleDeck, Enemy enemy) => new()
    {
        PlayerHealth = battle.PlayerCurrentHealth,
        PlayerMaxHealth = battle.PlayerMaxHealth,
        PlayerEnergy = battle.CurrentPlayerEnergy,
        PlayerMaxEnergy = battle.MaxPlayerEnergyPerTurn,
        PlayerBlock = battleDeck.CurrentBlock,
        EnemyHealth = battle.EnemyCurrentHealth,
        EnemyMaxHealth = enemy.Health,
        EnemyBlock = battle.EnemyBlock,
        TurnNumber = battle.CurrentRound,
        CurrentTurn = battle.CurrentTurn,
        BattleStatus = ToEngineBattleStatus(battle.Status, battle.CurrentTurn),
        Hand = battleDeck.Hand,
        DrawPile = battleDeck.DrawPile,
        DiscardPile = battleDeck.DiscardPile,
        ActiveEffects = battle.ActiveEffects,
        BattleLog = battle.BattleLog,
        RewardsGranted = battle.RewardsGranted
    };

    private static void ApplyState(Battle battle, BattleState state)
    {
        battle.PlayerCurrentHealth = state.PlayerHealth;
        battle.PlayerMaxHealth = state.PlayerMaxHealth;
        battle.CurrentPlayerEnergy = state.PlayerEnergy;
        battle.MaxPlayerEnergyPerTurn = state.PlayerMaxEnergy;
        battle.EnemyCurrentHealth = state.EnemyHealth;
        battle.EnemyBlock = state.EnemyBlock;
        battle.CurrentRound = state.TurnNumber;
        battle.RewardsGranted = state.RewardsGranted;
        battle.ActiveEffects = state.ActiveEffects;
        battle.BattleLog = state.BattleLog;

        ApplyBattleStatus(battle, state.BattleStatus);
        if (battle.Status != GameSessionStatus.InProgress)
        {
            battle.CompletedAt ??= DateTime.UtcNow;
        }
    }

    private static void ApplyState(BattleDeck battleDeck, BattleState state)
    {
        battleDeck.Hand = state.Hand;
        battleDeck.DrawPile = state.DrawPile;
        battleDeck.DiscardPile = state.DiscardPile;
        battleDeck.CurrentBlock = state.PlayerBlock;
    }

    /// <summary>
    /// Battle.Status (GameSessionStatus) tracks the terminal outcome; Battle.CurrentTurn
    /// (TurnType) tracks turn ownership. BattleState.BattleStatus (Engine.Enums.BattleStatus)
    /// conflates both into one enum, so composing/decomposing it is this boundary's job.
    /// </summary>
    private static BattleStatus ToEngineBattleStatus(GameSessionStatus status, TurnType currentTurn) => status switch
    {
        GameSessionStatus.Victory => BattleStatus.Victory,
        GameSessionStatus.Defeat => BattleStatus.Defeat,
        _ => currentTurn == TurnType.Player ? BattleStatus.PlayerTurn : BattleStatus.EnemyTurn
    };

    private static void ApplyBattleStatus(Battle battle, BattleStatus engineStatus)
    {
        switch (engineStatus)
        {
            case BattleStatus.Victory:
                battle.Status = GameSessionStatus.Victory;
                break;
            case BattleStatus.Defeat:
                battle.Status = GameSessionStatus.Defeat;
                break;
            case BattleStatus.PlayerTurn:
                battle.Status = GameSessionStatus.InProgress;
                battle.CurrentTurn = TurnType.Player;
                break;
            case BattleStatus.EnemyTurn:
                battle.Status = GameSessionStatus.InProgress;
                battle.CurrentTurn = TurnType.Enemy;
                break;
        }
    }

    private static void RequirePositiveId(int id, string fieldName)
    {
        var validation = RequestValidationHelpers.RequirePositiveId(id, fieldName);
        if (!validation.IsValid)
        {
            throw new DomainException(ErrorCodes.ValidationError, string.Join(" ", validation.Errors));
        }
    }
}
