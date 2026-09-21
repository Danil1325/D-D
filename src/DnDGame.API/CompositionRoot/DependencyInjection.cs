// Composition root for the whole API. One class per concern keeps the DI wiring
// grouped by owner instead of a single monolithic Program.cs:
//
//   - AddCardBattleServices()      -> Card-battle feature components (Persoana 2, branch catalina).
//   - AddBattleTurnSystemServices()-> Battle/Turn system integration surface (Persoana 1, branch battle_models).
//   - AddMockData()                -> In-memory store + Mock* repositories for the mock-data phase.
//   - AddScenarioServices()        -> Interactive scenario engine (Persoana 3).

// Program.cs only calls these four methods; every game-logic registration lives
// here. The container stays lazy (no ValidateOnBuild), so registrations whose
// dependency graph is not completed by Persoana 1 yet still let the app boot — they
// throw only if someone actually resolves them. Each such gap is marked as a
// "Person 1 seam" instead of being implemented here.

using DnDGame.BusinessLayer.Common.Errors;
using DnDGame.BusinessLayer.Effects;
using DnDGame.BusinessLayer.Effects.Interfaces;
using DnDGame.BusinessLayer.Effects.Strategies;
using DnDGame.BusinessLayer.Engines;
using DnDGame.BusinessLayer.Engines.Interfaces;
using DnDGame.BusinessLayer.Repositories;
using DnDGame.BusinessLayer.Repositories.Interfaces;
using DnDGame.BusinessLayer.Services;
using DnDGame.BusinessLayer.Services.Interfaces;
using DnDGame.Domain.Configuration;
using DnDGame.Domain.Engine.Battle;
using DnDGame.Domain.Engine.Combat;
using DnDGame.Domain.Engine.Common;
using DnDGame.Domain.Engine.Dice;
using DnDGame.Domain.Engine.EnemyActions;
using DnDGame.Domain.Engine.Initiative;
using DnDGame.Domain.Engine.Locations;
using DnDGame.Domain.Engine.SavingThrows;
using DnDGame.Domain.Engine.Scenario;
using DnDGame.Domain.Engine.Turn;
using DnDGame.Domain.Entities.Accounts;
using DnDGame.Domain.Entities.Locations;
using DnDGame.MockData;
using DnDGame.MockData.Repositories;
using DnDGame.MockData.Services;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.DependencyInjection;

// There are two IDamageCalculator contracts on this branch:
//   - DnDGame.BusinessLayer.Effects.Interfaces.IDamageCalculator (card effects take raw numeric inputs)
//   - DnDGame.Domain.Engine.Combat.IDamageCalculator              (battle engine takes a DamageRequest)
// This alias lets both using-directives coexist without making the simple name
// ambiguous below.
using DomainDamageCalculator = DnDGame.Domain.Engine.Combat.IDamageCalculator;
using DomainErrorCode = DnDGame.Domain.Enums.ErrorCode;

namespace DnDGame.API.CompositionRoot;

/// <summary>
/// Extensions that register every DI service in the API.
/// </summary>
public static class DependencyInjection
{
    /// <summary>
    /// Registers the card-battle feature created by Persoana 2 (branch catalina):
    /// configurable rules, the BusinessLayer engines, deck validation and the
    /// card-effect strategy registry.
    /// </summary>
    public static IServiceCollection AddCardBattleServices(this IServiceCollection services)
    {
        // --- Configurable game rules (singletons; replace with real configuration later) ---
        // Parameterless constructors exist for DI purposes, but DeckRules' defaults
        // (0/0/0) would accept any deck, so explicit placeholder tuning is provided.
        // These are sensible starting bounds — move them to appsettings once the card
        // set and balancing are finalised.
        services.AddSingleton(new DeckRules(
            minimumDeckSize: 20,
            maximumDeckSize: 40,
            maximumCopiesPerCard: 4));
        services.AddSingleton(new HandRules(maxHandSize: 10));
        services.AddSingleton(new PlayRules(maxEnergyPerTurn: 5, allowOverdraft: false));
        services.AddSingleton(new CardVisibilityRules());
        services.AddSingleton(new LevelProgressionRules());
        services.AddScoped<IExperienceService, ExperienceService>();
        services.AddScoped<ICombatExperienceCalculator, CombatExperienceCalculator>();

        // --- Engines (scoped, per-request) ---
        services.AddScoped<IDeckEngine, DeckEngine>();
        services.AddScoped<IHandEngine, HandEngine>();
        services.AddScoped<IPlayEngine, PlayEngine>();
        services.AddScoped<IEffectEngine, EffectEngine>();
        services.AddScoped<ICardEngine, CardEngine>();
        services.AddScoped<IAbilityUseEngine, AbilityUseEngine>();

        // --- Rules / cross-cutting ---
        services.AddScoped<IDeckValidator, DeckValidator>();

        // Registered here (rather than a 4th composition method) since this is the
        // one shared IErrorCodeHttpMapper instance for the whole API. Feature-specific
        // codes are added via Register(...) as each feature starts throwing them —
        // currently the Dice feature (see AddBattleTurnSystemServices) and the Deck
        // feature's IDeckValidator codes (see DeckService).
        services.AddSingleton<IErrorCodeHttpMapper>(_ =>
        {
            var mapper = new ErrorCodeHttpMapper();
            mapper.Register(EngineErrorCodes.InvalidDice, StatusCodes.Status400BadRequest);
            mapper.Register(DomainErrorCode.DECK_TOO_SMALL.ToString(), StatusCodes.Status400BadRequest);
            mapper.Register(DomainErrorCode.DECK_TOO_LARGE.ToString(), StatusCodes.Status400BadRequest);
            mapper.Register(DomainErrorCode.CARD_COPY_LIMIT_REACHED.ToString(), StatusCodes.Status400BadRequest);

            // Domain.Engine.Common.EngineErrorCodes — what IBattleEngine's EngineResult
            // failures actually carry (see BattleService). Only the codes its public
            // methods (StartBattle/PlayCard/EndTurn/ExecuteEnemyTurn) can plausibly
            // return are registered here; ConsequenceAlreadyApplied belongs to the
            // unrelated adventure/story-node flow and isn't guessed at.
            mapper.Register(EngineErrorCodes.BattleNotFound, StatusCodes.Status404NotFound);
            mapper.Register(EngineErrorCodes.BattleAlreadyFinished, StatusCodes.Status409Conflict);
            mapper.Register(EngineErrorCodes.NotPlayerTurn, StatusCodes.Status409Conflict);
            mapper.Register(EngineErrorCodes.InvalidAction, StatusCodes.Status400BadRequest);
            mapper.Register(EngineErrorCodes.PlayerDead, StatusCodes.Status409Conflict);
            mapper.Register(EngineErrorCodes.EnemyDead, StatusCodes.Status409Conflict);
            mapper.Register(EngineErrorCodes.MissingCombatRule, StatusCodes.Status500InternalServerError);
            mapper.Register(EngineErrorCodes.RewardsAlreadyGranted, StatusCodes.Status409Conflict);

            // Account/auth codes (see AccountErrorCodes remarks for why login uses one
            // generic code instead of distinguishing "unknown account" from "wrong password").
            mapper.Register(AccountErrorCodes.EmailAlreadyInUse, StatusCodes.Status409Conflict);
            mapper.Register(AccountErrorCodes.UsernameAlreadyInUse, StatusCodes.Status409Conflict);
            mapper.Register(AccountErrorCodes.InvalidCredentials, StatusCodes.Status401Unauthorized);
            return mapper;
        });

        // BusinessLayer IDamageCalculator (raw-int signature, consumed by DamageEffect
        // below) — distinct from the aliased Domain one registered in
        // AddBattleTurnSystemServices. Closing this seam is what lets CardEffectRegistry/
        // IEffectEngine/ICardEngine resolve (see DiRegistrationTests).
        services.AddScoped<DnDGame.BusinessLayer.Effects.Interfaces.IDamageCalculator, AdditiveDamageCalculator>();

        // --- Card effects (strategy pattern) ---
        // Every ICardEffect is registered so CardEffectRegistry can be built from
        // the container instead of a hand-maintained list (Open/Closed Principle).
        services.AddScoped<ICardEffect, DamageEffect>();
        services.AddScoped<ICardEffect, HealEffect>();
        services.AddScoped<ICardEffect, DrawEffect>();
        services.AddScoped<ICardEffect, EnergyEffect>();
        services.AddScoped<ICardEffect, BlockEffect>();
        services.AddScoped<ICardEffect, DefenseUpCardEffect>();
        services.AddScoped<ICardEffect, StrengthCardEffect>();
        services.AddScoped<ICardEffect, VulnerableCardEffect>();
        services.AddScoped<ICardEffect, WeakCardEffect>();

        services.AddSingleton<CardEffectRegistry>(provider =>
            new CardEffectRegistry(provider.GetServices<ICardEffect>()));
        return services;
    }

    /// <summary>
    /// Registers the battle/turn subsystem components supplied by Persoana 1
    /// (branch battle_models). Only the pieces with an implementation present on
    /// this branch are wired; everything Persoana 1 has not implemented yet stays
    /// an unregistered interface (a "seam") so the container remains bootable.
    /// </summary>
    public static IServiceCollection AddBattleTurnSystemServices(this IServiceCollection services)
    {
        // --- Dice (foundation of everything random in battle) ---
        services.AddSingleton<IRandomNumberSource, CryptographicRandomNumberSource>();
        services.AddSingleton<IDiceEngine, DiceEngine>();
        services.AddScoped<IDiceService, DiceService>();

        // --- Combat (damage, dodge, criticals) ---
        // IDamageRule/IDodgeRule are optional policy seams — their default
        // implementations are wired only if they exist. The concrete rules below
        // are the ones already implemented on this branch. (IInitiativeRule used
        // to be in this category too; AdditiveInitiativeRule below closes it.)
        services.AddSingleton<IDamageRule, AdditiveDamageRule>();
        services.AddSingleton<DomainDamageCalculator, DamageCalculator>();
        services.AddSingleton<IDodgeCalculator, DodgeCalculator>();
        services.AddSingleton<ICriticalCalculator, CriticalCalculator>();

        // --- Turn / battle orchestration building blocks ---
        services.AddSingleton<IInitiativeRule, AdditiveInitiativeRule>();
        services.AddSingleton<IInitiativeEngine, InitiativeEngine>();
        services.AddSingleton<ISavingThrowEngine, SavingThrowEngine>();
        services.AddSingleton<IEnemyActionRule, WeightedEnemyActionRule>();
        services.AddSingleton<IEnemyActionSelector, EnemyActionSelector>();
        services.AddSingleton<IBattleLogWriter, BattleLogWriter>();

        // --- Domain turn-boundary engines, ITurnEngine and IBattleEngine (Person 1) ---
        // BattleEngine (DnDGame.Domain.Engine.Battle) and TurnEngine
        // (DnDGame.Domain.Engine.Turn) only resolve once the engine set they delegate
        // to — Deck/Hand/Card/Effect engines and IEnemyDefenseRule — is wired in.
        // Their interfaces collide with the identically-named BusinessLayer ones in
        // AddCardBattleServices, so they are written fully qualified here.
        services.AddSingleton<DnDGame.Domain.Engine.Deck.IDeckEngine, DnDGame.Domain.Engine.Deck.DeckEngine>();
        services.AddSingleton<DnDGame.Domain.Engine.Hand.IHandEngine, DnDGame.Domain.Engine.Hand.HandEngine>();
        services.AddSingleton<DnDGame.Domain.Engine.Cards.ICardEngine, DnDGame.Domain.Engine.Cards.CardEngine>();
        services.AddSingleton<DnDGame.Domain.Engine.Effects.IEffectEngine, DnDGame.Domain.Engine.Effects.EffectEngine>();
        services.AddSingleton<IEnemyDefenseRule, EnemyDefenseRule>();
        services.AddSingleton<ITurnEngine, TurnEngine>();
        services.AddSingleton<IBattleEngine, BattleEngine>();

        // --- Remaining Person 1 seam (deliberately NOT registered/implemented here) ---
        // BattleContext (DnDGame.Domain.Engine.Battle.BattleContext): the in-memory
        // hand-off between the two layers — Persoana 2's card flow produces
        // Battle/BattleDeck/CardInstance, Persoana 1's IBattleEngine consumes a
        // BattleContext wrapping PlayerCharacter/Enemy/BattleState. It is a per-battle
        // data holder, created on demand, so it is NOT a DI service.
        //
        // (BusinessLayer IDamageCalculator used to be listed here too. It's now
        // registered — see AddCardBattleServices — so CardEffectRegistry/IEffectEngine/
        // ICardEngine resolve.)
        return services;
    }

    /// <summary>
    /// Registers the in-memory data store, the Mock* repositories and the current
    /// player service used during the mock-data phase.
    /// </summary>
    public static IServiceCollection AddMockData(this IServiceCollection services)
    {
        // One shared store instance backs all Mock* repositories, so data written
        // by one request survives into the next (see MockDataBootstrapper remarks).
        var mockDataStore = MockDataBootstrapper.CreateSeededStore();
        services.AddSingleton(mockDataStore);

        services.AddScoped<IEnemyRepository, MockEnemyRepository>();
        services.AddScoped<IRaceRepository, MockRaceRepository>();
        services.AddScoped<IClassRepository, MockClassRepository>();
        services.AddScoped<ICharacterRepository, MockCharacterRepository>();
        services.AddScoped<ICharacterPortraitRepository, MockCharacterPortraitRepository>();
        services.AddScoped<IAdventureRepository, MockAdventureRepository>();
        services.AddScoped<ITalentRepository, MockTalentRepository>();
        services.AddScoped<IGameSessionRepository, MockGameSessionRepository>();
        services.AddScoped<IStoryNodeRepository, MockStoryNodeRepository>();
        services.AddScoped<ICardCollectionRepository, MockCardCollectionRepository>();
        services.AddScoped<IDeckRepository, MockDeckRepository>();
        services.AddScoped<IBattleRepository, MockBattleRepository>();
        services.AddScoped<IBattleDeckRepository, MockBattleDeckRepository>();
        services.AddScoped<IAccountRepository, MockAccountRepository>();
        services.AddScoped<IQuestRepository, MockQuestRepository>();
        services.AddScoped<IPlayerQuestRepository, MockPlayerQuestRepository>();
        services.AddScoped<IScenarioProgressRepository, MockScenarioProgressRepository>();
        services.AddScoped<IStorySceneRepository, MockStorySceneRepository>();
        services.AddScoped<ILocationRepository, MockLocationRepository>();
        services.AddScoped<ILocationDefinitionRepository, MockLocationDefinitionRepository>();
        services.AddScoped<ILocationEncounterRepository, MockLocationEncounterRepository>();
        services.AddScoped<ILocationProgressRepository, MockLocationProgressRepository>();

        services.AddScoped<ICurrentPlayerService, MockCurrentPlayerService>();
        services.AddScoped<ICardService, CardService>();
        services.AddScoped<IDeckService, DeckService>();

        // Auth: PasswordHasher<Account> is the real, non-mock implementation (no
        // "mock hashing" phase — there's nothing database-specific about it), kept
        // here only because this is where every other feature service is wired.
        services.AddSingleton<IPasswordHasher<Account>, PasswordHasher<Account>>();
        services.AddScoped<IAccountService, AccountService>();

        // Registered here (rather than AddScenarioServices) because BattleService
        // (below, same method) depends on it directly — keeping both in AddMockData
        // means any composition that needs IBattleService stays self-sufficient
        // without also requiring AddScenarioServices.
        services.AddScoped<ILocationEncounterService, LocationEncounterService>();

        // BattleService depends on Domain.Engine.Battle.IBattleEngine (the engine
        // set it needs is registered in AddBattleTurnSystemServices) and on
        // ILocationEncounterService (registered just above), so both must resolve.
        services.AddScoped<IBattleService, BattleService>();
        return services;
    }

    /// <summary>
    /// Registers the interactive scenario engine (Persoana 3). The engine is a
    /// stateless state machine and is registered as a singleton.
    /// </summary>
    public static IServiceCollection AddScenarioServices(this IServiceCollection services)
    {
        services.AddSingleton<IScenarioEngine, ScenarioEngine>();

        // Stateless/pure, same as IScenarioEngine above — QuestService (below) uses
        // both to check which locations a completed quest newly unlocks (BACK-LOC-07).
        services.AddSingleton<ILocationRouteProvider, LocationRouteProvider>();
        services.AddSingleton<ILocationUnlockEngine, LocationUnlockEngine>();

        services.AddScoped<IQuestService, QuestService>();
        services.AddScoped<IScenarioService, ScenarioService>();
        services.AddScoped<IProgressionService, ProgressionService>();
        return services;
    }
}