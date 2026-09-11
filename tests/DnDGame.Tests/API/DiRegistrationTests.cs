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
    /// Registered but not yet resolvable: building the effect chain requires the
    /// Persona 1 seam — BusinessLayer IDamageCalculator, consumed by DamageEffect —
    /// which has no production implementation yet. Pinned so the expected failure
    /// flips to success automatically once Persona 1 registers that seam.
    /// </summary>
    [Fact]
    public void Persona1_Seam_EffectEngineNotResolvableUntilDamageCalculatorLands()
    {
        using var provider = BuildAll();

        var exception = Assert.Throws<InvalidOperationException>(
            () => provider.GetRequiredService<IEffectEngine>());

        Assert.Contains("IDamageCalculator", exception.Message);
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
    public void MockData_AllRepositoriesResolve(Type serviceType)
    {
        using var provider = BuildAll();
        AssertResolves(provider, serviceType);
    }

    [Theory]
    [InlineData(typeof(ICardService))]
    [InlineData(typeof(IDeckService))]
    public void MockData_ApplicationServicesResolve(Type serviceType)
    {
        using var provider = BuildAll();
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