using DnDGame.BusinessLayer.Common.Errors;
using DnDGame.BusinessLayer.Common.Exceptions;
using DnDGame.BusinessLayer.Dtos.Battles;
using DnDGame.BusinessLayer.Engines;
using DnDGame.BusinessLayer.Services;
using DnDGame.BusinessLayer.Services.Interfaces;
using DnDGame.Domain.Configuration;
using DnDGame.Domain.Engine.Battle;
using DnDGame.Domain.Engine.Common;
using DnDGame.Domain.Engine.Enums;
using DnDGame.Domain.Entities.Cards;
using DnDGame.Domain.Entities.Characters;
using DnDGame.Domain.Entities.Enemies;
using DnDGame.Domain.Entities.Game;
using DnDGame.Domain.Entities.Locations;
using DnDGame.Domain.Enums;
using DnDGame.MockData;
using DnDGame.MockData.Repositories;

namespace DnDGame.Tests.BusinessLayer;

/// <summary>
/// Exercises BattleService's own orchestration/reconciliation logic directly,
/// using a hand-written IBattleEngine test double (same technique as
/// DiRegistrationTests.StubDamageCalculator) — the double keeps the service tests
/// focused on BattleService instead of the battle engine itself.
/// </summary>
public class BattleServiceTests
{
    private const int CurrentPlayerId = 1;
    private const int OtherPlayerId = 2;

    [Fact]
    public async Task StartBattleAsync_ValidRequest_CreatesBattleAndBattleDeck()
    {
        var (service, store, engine, session, deck, enemy, _) = CreateScenario();

        var result = await service.StartBattleAsync(new StartBattleRequestDto
        {
            GameSessionId = session.Id,
            DeckId = deck.Id
        });

        Assert.NotEqual(0, result.Id);
        Assert.Equal(session.Id, result.GameSessionId);
        Assert.Single(store.Battles);
        Assert.Single(store.BattleDecks);
        Assert.Equal(result.Id, store.BattleDecks[0].BattleId);
        Assert.Equal(enemy.Id, store.Battles[0].EnemyId);
    }

    [Fact]
    public async Task StartBattleAsync_SessionNotOwnedByCurrentPlayer_ThrowsNotFound()
    {
        var (service, store, _, session, deck, _, _) = CreateScenario();
        session.CharacterId = OtherPlayerId;

        var exception = await Assert.ThrowsAsync<DomainException>(
            () => service.StartBattleAsync(new StartBattleRequestDto { GameSessionId = session.Id, DeckId = deck.Id }));

        Assert.Equal(ErrorCodes.NotFound, exception.ErrorCode);
    }

    [Fact]
    public async Task StartBattleAsync_SessionAlreadyHasActiveBattle_ThrowsConflict()
    {
        var (service, store, _, session, deck, enemy, _) = CreateScenario();
        store.Battles.Add(new Battle { Id = 999, GameSessionId = session.Id, EnemyId = enemy.Id, Status = GameSessionStatus.InProgress });

        var exception = await Assert.ThrowsAsync<DomainException>(
            () => service.StartBattleAsync(new StartBattleRequestDto { GameSessionId = session.Id, DeckId = deck.Id }));

        Assert.Equal(ErrorCodes.Conflict, exception.ErrorCode);
    }

    [Fact]
    public async Task StartBattleAsync_CurrentNodeHasNoEnemy_ThrowsConflict()
    {
        var (service, store, _, session, deck, _, _) = CreateScenario();
        store.StoryNodes.Single(n => n.Id == session.CurrentNodeId).EnemyId = null;

        var exception = await Assert.ThrowsAsync<DomainException>(
            () => service.StartBattleAsync(new StartBattleRequestDto { GameSessionId = session.Id, DeckId = deck.Id }));

        Assert.Equal(ErrorCodes.Conflict, exception.ErrorCode);
    }

    [Fact]
    public async Task StartBattleAsync_DeckNotOwnedByCurrentPlayer_ThrowsNotFound()
    {
        var (service, store, _, session, deck, _, _) = CreateScenario();
        deck.CharacterId = OtherPlayerId;

        var exception = await Assert.ThrowsAsync<DomainException>(
            () => service.StartBattleAsync(new StartBattleRequestDto { GameSessionId = session.Id, DeckId = deck.Id }));

        Assert.Equal(ErrorCodes.NotFound, exception.ErrorCode);
    }

    [Fact]
    public async Task StartBattleAsync_EngineFailure_ThrowsDomainExceptionWithEngineErrorCode()
    {
        var (service, _, engine, session, deck, _, _) = CreateScenario();
        engine.OnStartBattle = _ => EngineResult<BattleState>.Fail("no rule", EngineErrorCodes.MissingCombatRule);

        var exception = await Assert.ThrowsAsync<DomainException>(
            () => service.StartBattleAsync(new StartBattleRequestDto { GameSessionId = session.Id, DeckId = deck.Id }));

        Assert.Equal(EngineErrorCodes.MissingCombatRule, exception.ErrorCode);
    }

    // --- StartLocationEncounterBattleAsync (BACK-LOC-06) ---

    [Fact]
    public async Task StartLocationEncounterBattleAsync_ValidEnemyFromPool_CreatesBattleWithLocationIdSet()
    {
        var (service, store, _, session, deck, enemy, _) = CreateScenario();
        store.LocationEncounterDefinitions.Add(new LocationEncounterDefinition
        {
            Id = 1,
            LocationId = LocationId.Ashtonia,
            Enemies = new List<EncounterEnemyDefinition>
            {
                new() { EnemyId = enemy.Id, Tier = EncounterTier.Normal }
            }
        });

        var result = await service.StartLocationEncounterBattleAsync(new StartLocationEncounterBattleRequestDto
        {
            GameSessionId = session.Id,
            DeckId = deck.Id,
            LocationId = LocationId.Ashtonia,
            EnemyId = enemy.Id
        });

        Assert.NotEqual(0, result.Id);
        Assert.Single(store.Battles);
        Assert.Equal(enemy.Id, store.Battles[0].EnemyId);
        Assert.Equal(LocationId.Ashtonia, store.Battles[0].LocationId);
    }

    [Fact]
    public async Task StartLocationEncounterBattleAsync_EnemyNotCurrentlyAvailableAtThatLocation_ThrowsConflict()
    {
        var (service, _, _, session, deck, enemy, _) = CreateScenario();
        // No LocationEncounterDefinition seeded at all, so nothing is ever available.

        var exception = await Assert.ThrowsAsync<DomainException>(() => service.StartLocationEncounterBattleAsync(
            new StartLocationEncounterBattleRequestDto
            {
                GameSessionId = session.Id,
                DeckId = deck.Id,
                LocationId = LocationId.Ashtonia,
                EnemyId = enemy.Id
            }));

        Assert.Equal(ErrorCodes.Conflict, exception.ErrorCode);
    }

    [Fact]
    public async Task StartLocationEncounterBattleAsync_SessionAlreadyHasActiveBattle_ThrowsConflict()
    {
        var (service, store, _, session, deck, enemy, _) = CreateScenario();
        store.Battles.Add(new Battle { Id = 999, GameSessionId = session.Id, EnemyId = enemy.Id, Status = GameSessionStatus.InProgress });

        var exception = await Assert.ThrowsAsync<DomainException>(() => service.StartLocationEncounterBattleAsync(
            new StartLocationEncounterBattleRequestDto
            {
                GameSessionId = session.Id,
                DeckId = deck.Id,
                LocationId = LocationId.Ashtonia,
                EnemyId = enemy.Id
            }));

        Assert.Equal(ErrorCodes.Conflict, exception.ErrorCode);
    }

    [Fact]
    public async Task GetBattleStateAsync_UnknownBattle_ThrowsNotFound()
    {
        var (service, _, _, _, _, _, _) = CreateScenario();

        var exception = await Assert.ThrowsAsync<DomainException>(() => service.GetBattleStateAsync(999));

        Assert.Equal(ErrorCodes.NotFound, exception.ErrorCode);
    }

    [Fact]
    public async Task GetBattleStateAsync_ExistingBattle_ComposesFromBattleAndBattleDeck()
    {
        var (service, store, _, session, deck, enemy, _) = CreateScenario();
        var started = await service.StartBattleAsync(new StartBattleRequestDto { GameSessionId = session.Id, DeckId = deck.Id });

        var state = await service.GetBattleStateAsync(started.Id);

        Assert.Equal(started.Id, state.Id);
        Assert.Equal(enemy.Health, state.EnemyMaxHealth);
    }

    [Fact]
    public async Task PlayCardAsync_CardInHand_PassesResolvedCardToEngineAndReconciles()
    {
        var (service, store, engine, session, deck, _, _) = CreateScenario();
        var started = await service.StartBattleAsync(new StartBattleRequestDto { GameSessionId = session.Id, DeckId = deck.Id });
        var battleDeck = store.BattleDecks.Single(bd => bd.BattleId == started.Id);
        var cardInHand = battleDeck.DrawPile.Concat(battleDeck.Hand).First();
        battleDeck.Hand.Add(cardInHand);

        engine.OnPlayCard = (ctx, card) =>
        {
            ctx.BattleState.PlayerEnergy -= 1;
            return EngineResult<BattleState>.Ok(ctx.BattleState);
        };

        var result = await service.PlayCardAsync(started.Id, new PlayCardRequestDto { CardInstanceId = cardInHand.InstanceId });

        Assert.Equal(cardInHand.InstanceId, engine.LastPlayedCard?.InstanceId);
        Assert.Equal(started.PlayerCurrentEnergy - 1, result.PlayerCurrentEnergy);
    }

    [Fact]
    public async Task PlayCardAsync_CardNotInHand_PassesNullToEngine()
    {
        var (service, store, engine, session, deck, _, _) = CreateScenario();
        var started = await service.StartBattleAsync(new StartBattleRequestDto { GameSessionId = session.Id, DeckId = deck.Id });

        await service.PlayCardAsync(started.Id, new PlayCardRequestDto { CardInstanceId = Guid.NewGuid() });

        Assert.Null(engine.LastPlayedCard);
    }

    [Fact]
    public async Task EndTurnAsync_StaysPlayerTurn_DoesNotExecuteEnemyTurn()
    {
        var (service, _, engine, session, deck, _, _) = CreateScenario();
        var started = await service.StartBattleAsync(new StartBattleRequestDto { GameSessionId = session.Id, DeckId = deck.Id });
        engine.OnEndTurn = ctx =>
        {
            ctx.BattleState.BattleStatus = BattleStatus.PlayerTurn;
            return EngineResult<BattleState>.Ok(ctx.BattleState);
        };

        await service.EndTurnAsync(started.Id);

        Assert.False(engine.ExecuteEnemyTurnCalled);
    }

    [Fact]
    public async Task EndTurnAsync_HandsToEnemy_ExecutesEnemyTurnAndReconcilesFinalState()
    {
        var (service, store, engine, session, deck, _, _) = CreateScenario();
        var started = await service.StartBattleAsync(new StartBattleRequestDto { GameSessionId = session.Id, DeckId = deck.Id });
        engine.OnEndTurn = ctx =>
        {
            ctx.BattleState.BattleStatus = BattleStatus.EnemyTurn;
            return EngineResult<BattleState>.Ok(ctx.BattleState);
        };
        engine.OnExecuteEnemyTurn = ctx =>
        {
            ctx.BattleState.BattleStatus = BattleStatus.PlayerTurn;
            ctx.BattleState.PlayerHealth -= 5;
            return EngineResult<BattleState>.Ok(ctx.BattleState);
        };

        var result = await service.EndTurnAsync(started.Id);

        Assert.True(engine.ExecuteEnemyTurnCalled);
        Assert.Equal(started.PlayerCurrentHealth - 5, result.PlayerCurrentHealth);
        Assert.Equal(GameSessionStatus.InProgress, store.Battles.Single(b => b.Id == started.Id).Status);
    }

    [Fact]
    public async Task EndTurnAsync_BattleEndsInVictory_SetsCompletedAt()
    {
        var (service, store, engine, session, deck, _, _) = CreateScenario();
        var started = await service.StartBattleAsync(new StartBattleRequestDto { GameSessionId = session.Id, DeckId = deck.Id });
        engine.OnEndTurn = ctx =>
        {
            ctx.BattleState.BattleStatus = BattleStatus.Victory;
            return EngineResult<BattleState>.Ok(ctx.BattleState);
        };

        await service.EndTurnAsync(started.Id);

        var battle = store.Battles.Single(b => b.Id == started.Id);
        Assert.Equal(GameSessionStatus.Victory, battle.Status);
        Assert.NotNull(battle.CompletedAt);
        Assert.False(engine.ExecuteEnemyTurnCalled);
    }

    [Fact]
    public async Task GetBattleLogAsync_ReturnsEntriesMappedFromBattle()
    {
        var (service, store, engine, session, deck, _, _) = CreateScenario();
        var started = await service.StartBattleAsync(new StartBattleRequestDto { GameSessionId = session.Id, DeckId = deck.Id });
        var battle = store.Battles.Single(b => b.Id == started.Id);
        battle.BattleLog.Add(new DnDGame.Domain.Engine.Models.BattleLogEntry(1, "Player", "Attack"));

        var log = await service.GetBattleLogAsync(started.Id);

        var entry = Assert.Single(log);
        Assert.Equal("Player", entry.Actor);
        Assert.Equal("Attack", entry.Action);
    }

    private static (
        IBattleService Service,
        InMemoryGameDataStore Store,
        FakeBattleEngine Engine,
        GameSession Session,
        Deck Deck,
        Enemy Enemy,
        PlayerCharacter Player) CreateScenario()
    {
        var store = new InMemoryGameDataStore();

        var player = new PlayerCharacter { Id = CurrentPlayerId, Name = "Hero", MaxHealth = 30, CurrentHealth = 30 };
        store.Characters.Add(player);

        var enemy = new Enemy { Id = 1, Name = "Goblin", Family = EnemyFamily.Goblin, Tier = EnemyTier.Base, Health = 15 };
        store.Enemies.Add(enemy);

        var node = new StoryNode { Id = 1, NodeType = NodeType.Combat, EnemyId = enemy.Id };
        store.StoryNodes.Add(node);

        var session = new GameSession { Id = 1, CharacterId = CurrentPlayerId, CurrentNodeId = node.Id };
        store.GameSessions.Add(session);

        var card = new Card { Id = 1, Name = "Strike", BaseCost = 1, BaseDamage = 5 };
        store.Cards.Add(card);
        var deck = new Deck { Id = 1, Name = "Starter", CharacterId = CurrentPlayerId, Cards = [card] };
        store.Decks.Add(deck);

        var battleRepository = new MockBattleRepository(store);
        var battleDeckRepository = new MockBattleDeckRepository(store);
        var gameSessionRepository = new MockGameSessionRepository(store);
        var storyNodeRepository = new MockStoryNodeRepository(store);
        var characterRepository = new MockCharacterRepository(store);
        var enemyRepository = new MockEnemyRepository(store);
        var deckRepository = new MockDeckRepository(store);
        var deckEngine = new DeckEngine();
        var engine = new FakeBattleEngine();
        var playRules = new PlayRules(maxEnergyPerTurn: 5, allowOverdraft: false);
        var currentPlayerService = new FixedCurrentPlayerService(CurrentPlayerId);

        var locationEncounterService = new LocationEncounterService(
            new MockLocationEncounterRepository(store),
            new MockLocationDefinitionRepository(store),
            enemyRepository,
            gameSessionRepository,
            characterRepository,
            new MockScenarioProgressRepository(store),
            new MockPlayerQuestRepository(store),
            battleRepository,
            currentPlayerService);

        var service = new BattleService(
            battleRepository,
            battleDeckRepository,
            gameSessionRepository,
            storyNodeRepository,
            characterRepository,
            enemyRepository,
            deckRepository,
            deckEngine,
            engine,
            playRules,
            currentPlayerService,
            new CombatExperienceCalculator(new ExperienceService(new LevelProgressionRules())),
            locationEncounterService);

        return (service, store, engine, session, deck, enemy, player);
    }


    [Theory]
    [InlineData("card")]
    [InlineData("turn")]
    [InlineData("enemy")]
    public async Task Victory_FromEachAction_AwardsExperienceOnce(string action)
    {
        var (service, store, engine, session, deck, _, player) = CreateScenario();
        player.CurrentXp = 110;
        var started = await service.StartBattleAsync(new StartBattleRequestDto { GameSessionId = session.Id, DeckId = deck.Id });

        EngineResult<BattleState> Win(BattleContext context)
        {
            context.BattleState.BattleStatus = BattleStatus.Victory;
            return EngineResult<BattleState>.Ok(context.BattleState);
        }

        if (action == "card")
        {
            engine.OnPlayCard = (context, _) => Win(context);
            await service.PlayCardAsync(started.Id, new PlayCardRequestDto());
        }
        else
        {
            engine.OnEndTurn = context =>
            {
                if (action == "turn") return Win(context);
                context.BattleState.BattleStatus = BattleStatus.EnemyTurn;
                return EngineResult<BattleState>.Ok(context.BattleState);
            };
            engine.OnExecuteEnemyTurn = Win;
            await service.EndTurnAsync(started.Id);
        }

        Assert.Equal(130, player.CurrentXp);
        Assert.Equal(2, player.Level);
        Assert.Equal(3, player.SkillPoints);
        Assert.True(store.Battles.Single().RewardsGranted);
        await service.GetBattleStateAsync(started.Id);
        Assert.Equal(130, player.CurrentXp);
    }

    [Fact]
    public async Task AbandonedBattle_CannotBeResumedToEarnExperience()
    {
        var (service, store, engine, session, deck, _, player) = CreateScenario();
        var started = await service.StartBattleAsync(new StartBattleRequestDto { GameSessionId = session.Id, DeckId = deck.Id });
        store.Battles.Single().Status = GameSessionStatus.Abandoned;
        engine.OnEndTurn = context =>
        {
            context.BattleState.BattleStatus = BattleStatus.Victory;
            return EngineResult<BattleState>.Ok(context.BattleState);
        };

        await Assert.ThrowsAsync<DomainException>(() => service.EndTurnAsync(started.Id));
        await Assert.ThrowsAsync<DomainException>(() => service.PlayCardAsync(started.Id, new PlayCardRequestDto()));
        Assert.Equal(0, player.CurrentXp);
    }

    private sealed class FixedCurrentPlayerService : ICurrentPlayerService
    {
        private readonly int _playerId;

        public FixedCurrentPlayerService(int playerId)
        {
            _playerId = playerId;
        }

        public int GetCurrentPlayerId() => _playerId;
    }

    /// <summary>
    /// Deterministic stand-in for Domain.Engine.Battle.IBattleEngine. Defaults every
    /// operation to a success that returns the BattleState unchanged; tests override
    /// the On* delegates to simulate specific engine outcomes.
    /// </summary>
    private sealed class FakeBattleEngine : IBattleEngine
    {
        public Func<BattleContext, EngineResult<BattleState>> OnStartBattle { get; set; } =
            ctx => EngineResult<BattleState>.Ok(ctx.BattleState);

        public Func<BattleContext, CardInstance?, EngineResult<BattleState>> OnPlayCard { get; set; } =
            (ctx, _) => EngineResult<BattleState>.Ok(ctx.BattleState);

        public Func<BattleContext, EngineResult<BattleState>> OnEndTurn { get; set; } =
            ctx => EngineResult<BattleState>.Ok(ctx.BattleState);

        public Func<BattleContext, EngineResult<BattleState>> OnExecuteEnemyTurn { get; set; } =
            ctx => EngineResult<BattleState>.Ok(ctx.BattleState);

        public CardInstance? LastPlayedCard { get; private set; }

        public bool ExecuteEnemyTurnCalled { get; private set; }

        public EngineResult<BattleState> StartBattle(BattleContext battleContext) => OnStartBattle(battleContext);

        public EngineResult<BattleState> PlayCard(BattleContext battleContext, CardInstance? card)
        {
            LastPlayedCard = card;
            return OnPlayCard(battleContext, card);
        }

        public EngineResult<BattleState> EndTurn(BattleContext battleContext) => OnEndTurn(battleContext);

        public EngineResult<BattleState> ExecuteEnemyTurn(BattleContext battleContext)
        {
            ExecuteEnemyTurnCalled = true;
            return OnExecuteEnemyTurn(battleContext);
        }

        public EngineResult<bool> MarkRewardsGranted(BattleContext battleContext) =>
            EngineResult<bool>.Ok(true);

        public EngineResult<BattleStatus> CheckBattleStatus(BattleContext battleContext) =>
            EngineResult<BattleStatus>.Ok(battleContext.BattleState.BattleStatus);

        public bool CheckVictory(BattleState battleState) => battleState.BattleStatus == BattleStatus.Victory;

        public bool CheckDefeat(BattleState battleState) => battleState.BattleStatus == BattleStatus.Defeat;
    }
}
