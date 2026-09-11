namespace DnDGame.BusinessLayer.Effects.Strategies;

using DnDGame.BusinessLayer.Effects.Interfaces;

/// <summary>
/// Effect that deals damage to targets using an injected IDamageCalculator.
/// Delegates all damage calculations to the provided calculator strategy.
/// Does NOT implement damage formula - only prepares data and calls the calculator.
/// </summary>
public class DamageEffect : ICardEffect
{
    private readonly IDamageCalculator _damageCalculator;

    /// <summary>
    /// Creates a new DamageEffect with the specified damage calculator.
    /// The calculator handles all damage modifiers, defense, and block logic.
    /// </summary>
    /// <param name="damageCalculator">The strategy for calculating damage values.</param>
    /// <exception cref="ArgumentNullException">Thrown when damageCalculator is null.</exception>
    public DamageEffect(IDamageCalculator damageCalculator)
    {
        _damageCalculator = damageCalculator ?? throw new ArgumentNullException(nameof(damageCalculator));
    }

    public string EffectName => "Damage";

    /// <summary>
    /// Damage effect can apply if:
    /// - Card has damage value
    /// - Card has a valid target (if required)
    /// </summary>
    public bool CanApply(CardEffectContext context)
    {
        if (context == null)
        {
            throw new ArgumentNullException(nameof(context));
        }

        // Can't apply damage if card has no damage value
        if (context.EffectiveCardDamage <= 0)
        {
            return false;
        }

        // For single-target damage, must have a target
        if (context.CardDefinition.TargetType == Domain.Enums.TargetType.SingleEnemy ||
            context.CardDefinition.TargetType == Domain.Enums.TargetType.SingleAlly)
        {
            return context.HasTarget && context.Target!.TargetId > 0;
        }

        return true;
    }

    /// <summary>
    /// Applies damage to the target(s) using the injected damage calculator.
    /// Prepares the damage calculation input and delegates to calculator.
    /// Then applies the calculated damage to the battle state.
    /// </summary>
    public string Apply(CardEffectContext context)
    {
        if (context == null)
        {
            throw new ArgumentNullException(nameof(context));
        }

        if (!CanApply(context))
        {
            return $"Cannot apply {EffectName} effect.";
        }

        int baseDamage = context.EffectiveCardDamage;

        // For now, assume player strength = 10 (placeholder)
        // In full implementation, this would come from Character/Player stats
        int playerStrength = 10;

        // Enemy defense (placeholder)
        int enemyDefense = 0;

        // Apply damage based on target type
        if (context.TargetsArea)
        {
            // For area effects, apply to all enemies (single enemy for now)
            return ApplyDamageToEnemy(context, baseDamage, playerStrength, enemyDefense);
        }
        else if (context.HasTarget && context.Target!.TargetId > 0)
        {
            // Single target damage
            return ApplyDamageToTarget(context, baseDamage, playerStrength, enemyDefense);
        }
        else
        {
            return "No valid target for damage.";
        }
    }

    /// <summary>
    /// Applies damage to a single target using the damage calculator.
    /// </summary>
    private string ApplyDamageToTarget(CardEffectContext context, int baseDamage, int playerStrength, int enemyDefense)
    {
        // Use the injected calculator to determine final damage
        int finalDamage = _damageCalculator.CalculateDamage(
            baseDamage: baseDamage,
            playerStrength: playerStrength,
            enemyDefense: enemyDefense,
            enemyBlock: context.Battle.EnemyBlock);

        // Apply damage to enemy
        context.Battle.EnemyCurrentHealth = Math.Max(0, context.Battle.EnemyCurrentHealth - finalDamage);

        // Reduce block after damage is applied
        int damageAfterBlock = Math.Max(0, finalDamage - context.Battle.EnemyBlock);
        context.Battle.EnemyBlock = Math.Max(0, context.Battle.EnemyBlock - finalDamage);

        return $"Dealt {finalDamage} damage to target ID {context.Target!.TargetId} " +
               $"(Enemy health: {context.Battle.EnemyCurrentHealth}, Block: {context.Battle.EnemyBlock}).";
    }

    /// <summary>
    /// Applies damage to all enemies (currently assumes single enemy in battle).
    /// </summary>
    private string ApplyDamageToEnemy(CardEffectContext context, int baseDamage, int playerStrength, int enemyDefense)
    {
        // Use the injected calculator to determine final damage
        int finalDamage = _damageCalculator.CalculateDamage(
            baseDamage: baseDamage,
            playerStrength: playerStrength,
            enemyDefense: enemyDefense,
            enemyBlock: context.Battle.EnemyBlock);

        // Apply damage to enemy
        context.Battle.EnemyCurrentHealth = Math.Max(0, context.Battle.EnemyCurrentHealth - finalDamage);

        // Reduce block after damage is applied
        context.Battle.EnemyBlock = Math.Max(0, context.Battle.EnemyBlock - finalDamage);

        return $"Dealt {finalDamage} damage to all enemies " +
               $"(Enemy health: {context.Battle.EnemyCurrentHealth}, Block: {context.Battle.EnemyBlock}).";
    }
}
