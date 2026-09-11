// Composition root for the whole API. One class per concern keeps the DI wiring
// grouped by owner instead of a single monolithic Program.cs:
//
//   - AddCardBattleServices()      -> Card-battle feature components (Persoana 2, branch catalina).
//   - AddBattleTurnSystemServices()-> Battle/Turn system integration surface (Persoana 1, branch battle_models).
//   - AddMockData()                -> In-memory store + Mock* repositories for the mock-data phase.
//
// Program.cs only calls these three methods; every game-logic registration lives
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
using DnDGame.BusinessLayer.Services.Interfaces;
using DnDGame.Domain.Configuration;
using DnDGame.Domain.Engine.Battle;
using DnDGame.Domain.Engine.Combat;
using DnDGame.Domain.Engine.Dice;
using DnDGame.Domain.Engine.EnemyActions;
using DnDGame.Domain.Engine.Initiative;
using DnDGame.Domain.Engine.SavingThrows;
using DnDGame.MockData;
using DnDGame.MockData.Repositories;
using DnDGame.MockData.Services;
using Microsoft.Extensions.DependencyInjection;

// There are two IDamageCalculator contracts on this branch:
//   - DnDGame.BusinessLayer.Effects.Interfaces.IDamageCalculator (card effects take raw numeric inputs)
//   - DnDGame.Domain.Engine.Combat.IDamageCalculator              (battle engine takes a DamageRequest)
// This alias lets both using-directives coexist without making the simple name
// ambiguous below.
using DomainDamageCalculator = DnDGame.Domain.Engine.Combat.IDamageCalculator;

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

        // --- Engines (scoped, per-request) ---
        services.AddScoped<IDeckEngine, DeckEngine>();
        services.AddScoped<IHandEngine, HandEngine>();
        services.AddScoped<IPlayEngine, PlayEngine>();
        services.AddScoped<IEffectEngine, EffectEngine>();
        services.AddScoped<ICardEngine, CardEngine>();
        services.AddScoped<IAbilityUseEngine, AbilityUseEngine>();

        // --- Rules / cross-cutting ---
        services.AddScoped<IDeckValidator, DeckValidator>();
        services.AddSingleton<IErrorCodeHttpMapper, ErrorCodeHttpMapper>();

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

        // --- Combat (damage, dodge, criticals) ---
        // IDamageRule/IDodgeRule/IInitiativeRule are optional policy seams — their
        // default implementations are wired only if they exist. The concrete rules
        // below are the ones already implemented on this branch.
        services.AddSingleton<IDamageRule, AdditiveDamageRule>();
        services.AddSingleton<DomainDamageCalculator, DamageCalculator>();
        services.AddSingleton<IDodgeCalculator, DodgeCalculator>();
        services.AddSingleton<ICriticalCalculator, CriticalCalculator>();

        // --- Turn / battle orchestration building blocks ---
        services.AddSingleton<IInitiativeEngine, InitiativeEngine>();
        services.AddSingleton<ISavingThrowEngine, SavingThrowEngine>();
        services.AddSingleton<IEnemyActionSelector, EnemyActionSelector>();
        services.AddSingleton<IBattleLogWriter, BattleLogWriter>();

        // --- Person 1 seams (deliberately NOT registered/implemented here) ---
        // BattleContext (DnDGame.Domain.Engine.Battle.BattleContext): the in-memory
        // hand-off between the two layers — Persoana 2's card flow produces
        // Battle/BattleDeck/CardInstance, Persoana 1's IBattleEngine consumes a
        // BattleContext wrapping PlayerCharacter/Enemy/BattleState. It is a per-battle
        // data holder, created on demand, so it is NOT a DI service.
        //
        // BusinessLayer IDamageCalculator (DnDGame.BusinessLayer.Effects.Interfaces):
        //   consumed by DamageEffect / the card-effect chain. No production
        //   implementation exists — Persoana 1 provides it; the moment it is
        //   registered, CardEffectRegistry/IEffectEngine/ICardEngine resolve.
        //
        // Domain turn-boundary engines: DnDGame.Domain.Engine.Deck.IDeckEngine,
        //   Hand.IHandEngine, Cards.ICardEngine, Effects.IEffectEngine — used by
        //   IBattleEngine/ITurnEngine (also Person 1 seams). No implementations exist
        //   on this branch; once they land, BattleEngine and TurnEngine become
        //   resolvable and can be registered here.
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

        services.AddScoped<ICurrentPlayerService, MockCurrentPlayerService>();
        return services;
    }
}