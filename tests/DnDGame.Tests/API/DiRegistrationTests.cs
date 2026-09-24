using DnDGame.API.CompositionRoot;
using DnDGame.BusinessLayer.Common.Errors;
using DnDGame.BusinessLayer.Effects.Interfaces;
using DnDGame.BusinessLayer.Effects.Strategies;
using DnDGame.BusinessLayer.Engines.Interfaces;
using DnDGame.BusinessLayer.Repositories.Interfaces;
using DnDGame.BusinessLayer.Services.Interfaces;
using DnDGame.Domain.Engine.Battle;
using DnDGame.Domain.Engine.Dice;
using DnDGame.Domain.Engine.EnemyActions;
using DnDGame.Domain.Engine.Initiative;
using DnDGame.Domain.Engine.SavingThrows;
using DnDGame.Domain.Engine.Turn;
using DnDGame.MockData;
using DomainCombat = DnDGame.Domain.Engine.Combat;
using Microsoft.Extensions.DependencyInjection;

namespace DnDGame.Tests.API;

/// <summary>
/// Verifies the composition root registers every Persona 2 component and the
/// Persona 1 integration surface correctly. The container is intentionally lazy —
/// registrations whose graph cannot be completed yet (because Persona 1 has not
/// landed those implementations) must throw on resolve, not at startup.
/// These tests are the guard rails for that contract.
/// </summary>
public class DiRegistrationTests
{
    private static ServiceProvider Build(Action<IServiceCollection> configure)
    {
        var services = new ServiceCollection();
        configure(services);
        return services.BuildServiceProvider();
    }

    private static ServiceProvider BuildAll()
    {
        return Build(services =>
        {
            services.AddCardBattleServices();
            services.AddBattleTurnSystemServices();
            services.AddMockData();
        });
    }

    private static void AssertResolves(ServiceProvider provider, Type serviceType)
    {
        Assert.NotNull(provider.GetService(serviceType));
    }

    // --- Persona 2: card-battle feature ---

    [Theory]
    [InlineData(typeof(IDeckEngine))]
    [InlineData(typeof(IHandEngine))]
    [InlineData(typeof(IPlayEngine))]
    [InlineData(typeof(IAbilityUseEngine))]
    [InlineData(typeof(IErrorCodeHttpMapper))]
    [InlineData(typeof(IDeckValidator))]
    public void Persona2_CardBattleCore_AllResolve(Type serviceType)
    {
        using var provider = BuildAll();
        AssertResolves(provider, serviceType);
    }

    [Fact]
    public void Persona2_AllNineCardEffectsAreRegistered()
    {
        var services = new ServiceCollection();
        services.AddCardBattleServices();

        var effectTypes = services
            .Where(descriptor => descriptor.ServiceType == typeof(ICardEffect))
            .Select(descriptor => descriptor.ImplementationType)
            .ToList();

        Assert.Equal(
            new[]
            {
                typeof(DamageEffect),
                typeof(HealEffect),
                typeof(DrawEffect),
                typeof(EnergyEffect),
                typeof(BlockEffect),
                typeof(DefenseUpCardEffect),
                typeof(StrengthCardEffect),
                typeof(VulnerableCardEffect),
                typeof(WeakCardEffect)
            },
            effectTypes);
    }

    /// <summary>
    /// Once Persona 1 provides a BusinessLayer IDamageCalculator (the card-effect
    /// seam), the whole effect chain — CardEffectRegistry, IEffectEngine and
    /// ICardEngine — must resolve and every registered effect must be available.
    /// </summary>
    [Fact]
    public void Persona2_EffectChain_ResolvesWhenDamageCalculatorSeamProvided()
    {
        using var provider = Build(services =>
        {
            services.AddCardBattleServices();
            services.AddBattleTurnSystemServices();
            services.AddMockData();
            services.AddScoped<IDamageCalculator, StubDamageCalculator>();
        });

        var cardEngine = provider.GetRequiredService<ICardEngine>();
        Assert.NotNull(cardEngine);

        var registry = provider.GetRequiredService<DnDGame.BusinessLayer.Effects.CardEffectRegistry>();
        Assert.Equal(9, registry.Count);
        Assert.Contains("Damage", registry.GetRegisteredEffectNames());
    }

    /// <summary>
    /// Used to be registered-but-not-resolvable: building the effect chain required
    /// the Persona 1 seam — BusinessLayer IDamageCalculator, consumed by DamageEffect —
    /// which had no production implementation. AdditiveDamageCalculator now closes it
    /// (registered in AddCardBattleServices), so IEffectEngine resolves for real. This
    /// test used to be a pin asserting a resolve failure; it now asserts the seam is
    /// closed, same convention as the IInitiativeRule/IEnemyActionRule seam flips.
    /// </summary>
    [Fact]
    public void Persona1_Seam_EffectEngineResolvesNowThatDamageCalculatorLands()
    {
        using var provider = BuildAll();

        var effectEngine = provider.GetRequiredService<IEffectEngine>();
        Assert.NotNull(effectEngine);

        var cardEngine = provider.GetRequiredService<ICardEngine>();
        Assert.NotNull(cardEngine);
    }

    /// <summary>
    /// The Persona 1 turn-boundary engines (Domain Deck/Hand/Card/Effect engines and
    /// IEnemyDefenseRule) have landed, so TurnEngine, BattleEngine and — in turn —
    /// BattleService now resolve from the composition root. This test used to be a
    /// pin asserting a resolve failure; it now asserts the seam is closed.
    /// </summary>
    [Fact]
    public void Persona1_Seam_BattleServiceResolvesNowThatBattleEngineLands()
    {
        using var provider = BuildAll();

        Assert.NotNull(provider.GetRequiredService<IBattleEngine>());
        Assert.NotNull(provider.GetRequiredService<IBattleService>());
    }

    /// <summary>
    /// Resolving IBattleEngine used to not be the same as it working: StartBattle
    /// delegates to IInitiativeEngine.DetermineFirstTurn, which unconditionally
    /// requires an IInitiativeRule, and no production implementation existed
    /// anywhere in the codebase (only a test double in InitiativeEngineTests). Now
    /// that AdditiveInitiativeRule is registered in AddBattleTurnSystemServices, a
    /// real StartBattle call resolves an actual starting turn instead of failing
    /// with MissingCombatRule. This test used to be a pin asserting that failure;
    /// it now asserts the seam is closed, same convention as
    /// Persona1_Seam_BattleServiceResolvesNowThatBattleEngineLands above.
    /// </summary>
    [Fact]
    public void Persona1_Seam_StartBattleSucceedsNowThatInitiativeRuleLands()
    {
        using var provider = BuildAll();
        var battleEngine = provider.GetRequiredService<IBattleEngine>();

        var player = new DnDGame.Domain.Entities.Characters.PlayerCharacter
        {
            Id = 1,
            Name = "Hero",
            MaxHealth = 30,
            CurrentHealth = 30
        };
        var enemy = new DnDGame.Domain.Entities.Enemies.Enemy { Id = 1, Name = "Goblin", Health = 15 };
        var state = new BattleState
        {
            PlayerEnergy = 5,
            PlayerMaxEnergy = 5,
            Hand = new List<DnDGame.Domain.Entities.Cards.CardInstance>(),
            DrawPile = new List<DnDGame.Domain.Entities.Cards.CardInstance>(),
            DiscardPile = new List<DnDGame.Domain.Entities.Cards.CardInstance>(),
            ActiveEffects = new List<DnDGame.Domain.Engine.Models.ActiveEffect>(),
            BattleLog = new List<DnDGame.Domain.Engine.Models.BattleLogEntry>()
        };

        var result = battleEngine.StartBattle(new BattleContext(player, enemy, state));

        Assert.True(result.Success);
        Assert.NotNull(result.Data);
    }

    // --- Persona 1: battle/turn surface (implemented parts resolve) ---

    [Theory]
    [InlineData(typeof(IRandomNumberSource))]
    [InlineData(typeof(IDiceEngine))]
    [InlineData(typeof(IDiceService))]
    [InlineData(typeof(DomainCombat.IDamageCalculator))]
    [InlineData(typeof(DomainCombat.IDamageRule))]
    [InlineData(typeof(DomainCombat.IDodgeCalculator))]
    [InlineData(typeof(DomainCombat.ICriticalCalculator))]
    [InlineData(typeof(IInitiativeEngine))]
    [InlineData(typeof(ISavingThrowEngine))]
    [InlineData(typeof(IEnemyActionSelector))]
    [InlineData(typeof(IBattleLogWriter))]
    [InlineData(typeof(DnDGame.Domain.Engine.Deck.IDeckEngine))]
    [InlineData(typeof(DnDGame.Domain.Engine.Hand.IHandEngine))]
    [InlineData(typeof(DnDGame.Domain.Engine.Cards.ICardEngine))]
    [InlineData(typeof(DnDGame.Domain.Engine.Effects.IEffectEngine))]
    [InlineData(typeof(IEnemyDefenseRule))]
    [InlineData(typeof(ITurnEngine))]
    [InlineData(typeof(IBattleEngine))]
    [InlineData(typeof(IBattleService))]
    public void Persona1_ImplementedComponents_AllResolve(Type serviceType)
    {
        using var provider = BuildAll();
        AssertResolves(provider, serviceType);
    }

    // --- Mock data ---

    [Fact]
    public void MockData_StoreAndCurrentPlayerResolve()
    {
        using var provider = BuildAll();
        AssertResolves(provider, typeof(InMemoryGameDataStore));
        Assert.NotNull(provider.GetService<ICurrentPlayerService>());
    }

    [Theory]
    [InlineData(typeof(IEnemyRepository))]
    [InlineData(typeof(IRaceRepository))]
    [InlineData(typeof(IClassRepository))]
    [InlineData(typeof(ICharacterRepository))]
    [InlineData(typeof(ICharacterPortraitRepository))]
    [InlineData(typeof(IAdventureRepository))]
    [InlineData(typeof(ITalentRepository))]
    [InlineData(typeof(IGameSessionRepository))]
    [InlineData(typeof(IStoryNodeRepository))]
    [InlineData(typeof(ICardCollectionRepository))]
    [InlineData(typeof(IDeckRepository))]
    [InlineData(typeof(IBattleRepository))]
    [InlineData(typeof(IBattleDeckRepository))]
    [InlineData(typeof(IAccountRepository))]
    [InlineData(typeof(IAchievementRepository))]
    [InlineData(typeof(IAchievementProgressRepository))]
    [InlineData(typeof(ISkillDefinitionRepository))]
    [InlineData(typeof(ICharacterSkillRepository))]
    [InlineData(typeof(ICollectionRepository))]
    public void MockData_AllRepositoriesResolve(Type serviceType)
    {
        using var provider = BuildAll();
        AssertResolves(provider, serviceType);
    }

    [Theory]
    [InlineData(typeof(ICardService))]
    [InlineData(typeof(IDeckService))]
    [InlineData(typeof(IAccountService))]
    [InlineData(typeof(ICurrentPlayerService))]
    [InlineData(typeof(ICharacterService))]
    [InlineData(typeof(IAchievementService))]
    [InlineData(typeof(ISkillService))]
    [InlineData(typeof(ICollectionService))]
    public void MockData_ApplicationServicesResolve(Type serviceType)
    {
        using var provider = BuildAll();
        AssertResolves(provider, serviceType);
    }

    [Fact]
    public void ScenarioServices_ExplicitLocationUnlockServiceResolves()
    {
        using var provider = Build(services =>
        {
            services.AddCardBattleServices();
            services.AddBattleTurnSystemServices();
            services.AddMockData();
            services.AddScenarioServices();
        });

        AssertResolves(provider, typeof(IExplicitLocationUnlockService));
    }

    [Theory]
    [InlineData(typeof(IScenarioService))]
    [InlineData(typeof(IQuestService))]
    [InlineData(typeof(ILocationService))]
    [InlineData(typeof(ILocationProgressionService))]
    [InlineData(typeof(IProgressionService))]
    public void ScenarioServices_ApplicationServicesResolve(Type serviceType)
    {
        using var provider = Build(services =>
        {
            services.AddCardBattleServices();
            services.AddBattleTurnSystemServices();
            services.AddMockData();
            services.AddScenarioServices();
        });

        AssertResolves(provider, serviceType);
    }

    /// <summary>
    /// Deterministic stand-in for the real combat rules. Mirrors the test fake used
    /// by CardEffectStrategyTests so the DI tests never depend on Persona 1's
    /// formulas — they only prove the wiring works when the seam is populated.
    /// </summary>
    private sealed class StubDamageCalculator : IDamageCalculator
    {
        public int CalculateDamage(int baseDamage, int playerStrength, int enemyDefense, int enemyBlock)
        {
            return Math.Max(0, baseDamage + playerStrength - enemyDefense - enemyBlock);
        }
    }
}
