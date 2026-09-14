using DnDGame.Domain.Engine.Battle;
using DnDGame.Domain.Engine.Combat;
using DnDGame.Domain.Engine.Common;
using DnDGame.Domain.Engine.Effects;
using DnDGame.Domain.Engine.Enums;
using DnDGame.Domain.Engine.Models;
using DnDGame.Domain.Entities.Cards;
using DnDGame.Domain.Enums;

namespace DnDGame.Domain.Engine.Cards;

/// <summary>
/// Resolves a single card play: affordability, energy consumption, moving the
/// card from the hand to the discard pile, and the card's primary effect.
/// Validation failures leave the battle state unchanged.
/// </summary>
public sealed class CardEngine : ICardEngine
{
    /// <summary>Number of turn boundaries a created active effect survives by default.</summary>
    public const int DefaultEffectDuration = 2;

    private readonly IDamageCalculator _damageCalculator;

    public CardEngine(IDamageCalculator damageCalculator)
    {
        ArgumentNullException.ThrowIfNull(damageCalculator);
        _damageCalculator = damageCalculator;
    }

    public EngineResult<BattleState> PlayCard(BattleContext battleContext, CardInstance card)
    {
        ArgumentNullException.ThrowIfNull(battleContext);
        ArgumentNullException.ThrowIfNull(card);

        var battleState = battleContext.BattleState;

        var failure = ValidatePlay(battleState, card);
        if (failure is not null)
        {
            return failure;
        }

        var damageResult = ResolveDamage(battleState, card);
        if (damageResult is not null && !damageResult.Success)
        {
            return EngineResult<BattleState>.Fail(
                damageResult.Message,
                damageResult.ErrorCode ?? EngineErrorCodes.MissingCombatRule);
        }

        var cost = card.GetEffectiveCost();
        battleState.PlayerEnergy -= cost;
        battleState.Hand.Remove(card);
        battleState.DiscardPile.Add(card);

        var description = ApplyEffect(battleState, card, damageResult?.Data);

        battleState.BattleLog.Add(new BattleLogEntry(
            battleState.TurnNumber,
            actor: "Player",
            action: "Played",
            card: card.Card.Name,
            effects: [description],
            result: description));

        return EngineResult<BattleState>.Ok(battleState, description);
    }

    private static EngineResult<BattleState>? ValidatePlay(BattleState battleState, CardInstance card)
    {
        if (battleState.BattleStatus is BattleStatus.Victory or BattleStatus.Defeat)
        {
            return EngineResult<BattleState>.Fail(
                "The battle has already finished.",
                EngineErrorCodes.BattleAlreadyFinished);
        }

        if (battleState.CurrentTurn != TurnType.Player ||
            battleState.BattleStatus != BattleStatus.PlayerTurn)
        {
            return EngineResult<BattleState>.Fail(
                "Cards can only be played during the player turn.",
                EngineErrorCodes.NotPlayerTurn);
        }

        if (card.Card is null)
        {
            return EngineResult<BattleState>.Fail(
                "The card definition is missing.",
                EngineErrorCodes.InvalidAction);
        }

        if (!battleState.Hand.Contains(card))
        {
            return EngineResult<BattleState>.Fail(
                "The card is not in the player's hand.",
                EngineErrorCodes.InvalidAction);
        }

        if (card.GetEffectiveCost() > battleState.PlayerEnergy)
        {
            return EngineResult<BattleState>.Fail(
                "The player does not have enough energy to play the card.",
                EngineErrorCodes.InvalidAction);
        }

        return null;
    }

    private EngineResult<DamageResult>? ResolveDamage(BattleState battleState, CardInstance card)
    {
        if (card.Card.EffectType != EffectType.Damage)
        {
            return null;
        }

        var targetsPlayer = TargetsPlayer(card);
        return _damageCalculator.Calculate(new DamageRequest(
            baseDamage: card.GetEffectiveDamage(),
            strength: 0,
            defense: 0,
            block: targetsPlayer ? battleState.PlayerBlock : battleState.EnemyBlock));
    }

    private static string ApplyEffect(BattleState battleState, CardInstance card, DamageResult? damageResult)
    {
        var magnitude = Math.Max(1, card.GetEffectiveDamage());

        switch (card.Card.EffectType)
        {
            case EffectType.Damage:
                if (damageResult is null)
                {
                    return "No effect.";
                }

                if (TargetsPlayer(card))
                {
                    battleState.PlayerBlock = damageResult.RemainingBlock;
                    battleState.PlayerHealth = Math.Max(0, battleState.PlayerHealth - damageResult.FinalDamage);
                    return $"Dealt {damageResult.FinalDamage} damage to the player.";
                }

                battleState.EnemyBlock = damageResult.RemainingBlock;
                battleState.EnemyHealth = Math.Max(0, battleState.EnemyHealth - damageResult.FinalDamage);
                return $"Dealt {damageResult.FinalDamage} damage to the enemy.";

            case EffectType.Healing:
                var healing = Math.Min(magnitude, Math.Max(0, battleState.PlayerMaxHealth - battleState.PlayerHealth));
                battleState.PlayerHealth += healing;
                return $"Healed the player for {healing}.";

            case EffectType.Protect:
                battleState.PlayerBlock += magnitude;
                return $"Gained {magnitude} block.";

            case EffectType.Strength or EffectType.BuffAttribute:
                battleState.ActiveEffects.Add(new StrengthEffect(magnitude, DefaultEffectDuration, EffectTarget.Player));
                return "Gained Strength.";

            case EffectType.DefenseUp:
                battleState.ActiveEffects.Add(new DefenseUpEffect(magnitude, DefaultEffectDuration, EffectTarget.Player));
                return "Gained DefenseUp.";

            case EffectType.Weak or EffectType.DebuffAttribute:
                battleState.ActiveEffects.Add(new WeakEffect(magnitude, DefaultEffectDuration, EffectTarget.Enemy));
                return "Applied Weak to the enemy.";

            case EffectType.Vulnerable:
                battleState.ActiveEffects.Add(new VulnerableEffect(magnitude, DefaultEffectDuration, EffectTarget.Enemy));
                return "Applied Vulnerable to the enemy.";

            case EffectType.StatusEffect:
                battleState.ActiveEffects.Add(CreateStatusEffect(card, magnitude));
                return "Applied a status effect to the enemy.";

            case EffectType.Draw:
                var drawn = DrawUpTo(battleState, magnitude);
                return $"Drew {drawn} card(s).";

            case EffectType.Discard:
                var discarded = DiscardUpTo(battleState, magnitude);
                return $"Discarded {discarded} card(s) from hand.";

            default:
                return "No additional effect.";
        }
    }

    private static ActiveEffect CreateStatusEffect(CardInstance card, int magnitude)
    {
        return card.Card.ActiveEffect.Contains("Burn", StringComparison.OrdinalIgnoreCase)
            ? new BurnEffect(magnitude, DefaultEffectDuration, EffectTarget.Enemy)
            : new PoisonEffect(magnitude, DefaultEffectDuration, EffectTarget.Enemy);
    }

    private static int DrawUpTo(BattleState battleState, int count)
    {
        var drawn = 0;

        for (var i = 0; i < count; i++)
        {
            if (battleState.DrawPile.Count == 0)
            {
                ReshuffleDiscardPile(battleState);
            }

            if (battleState.DrawPile.Count == 0)
            {
                break;
            }

            var card = battleState.DrawPile[^1];
            battleState.DrawPile.RemoveAt(battleState.DrawPile.Count - 1);
            battleState.Hand.Add(card);
            drawn++;
        }

        return drawn;
    }

    private static int DiscardUpTo(BattleState battleState, int count)
    {
        var cardsToDiscard = battleState.Hand.Take(count).ToList();

        foreach (var card in cardsToDiscard)
        {
            battleState.Hand.Remove(card);
            battleState.DiscardPile.Add(card);
        }

        return cardsToDiscard.Count;
    }

    private static void ReshuffleDiscardPile(BattleState battleState)
    {
        while (battleState.DiscardPile.Count > 0)
        {
            var card = battleState.DiscardPile[0];
            battleState.DiscardPile.RemoveAt(0);
            battleState.DrawPile.Add(card);
        }

        Shuffle(battleState.DrawPile);
    }

    private static void Shuffle(IList<CardInstance> cards)
    {
        for (var i = cards.Count - 1; i > 0; i--)
        {
            var randomIndex = Random.Shared.Next(0, i + 1);
            (cards[i], cards[randomIndex]) = (cards[randomIndex], cards[i]);
        }
    }

    private static bool TargetsPlayer(CardInstance card) =>
        card.Card.TargetType is TargetType.Self or TargetType.SingleAlly or TargetType.AllAllies;
}