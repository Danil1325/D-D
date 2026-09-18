using DnDGame.BusinessLayer.Models;
using DnDGame.BusinessLayer.Services.Interfaces;
using DnDGame.Domain.Entities.Characters;
using DnDGame.Domain.Entities.Enemies;
using DnDGame.Domain.Entities.Game;
using DnDGame.Domain.Enums;

namespace DnDGame.BusinessLayer.Services;

public sealed class CombatExperienceCalculator : ICombatExperienceCalculator
{
    private readonly IExperienceService _experienceService;

    public CombatExperienceCalculator(IExperienceService experienceService)
    {
        ArgumentNullException.ThrowIfNull(experienceService);
        _experienceService = experienceService;
    }

    public int GetBaseExperience(Enemy enemy)
    {
        ArgumentNullException.ThrowIfNull(enemy);
        return (enemy.Family, enemy.Tier) switch
        {
            (EnemyFamily.Goblin or EnemyFamily.Skeleton or EnemyFamily.Slime or EnemyFamily.Wraith,
                EnemyTier.Base) => 20,
            (EnemyFamily.Goblin or EnemyFamily.Skeleton or EnemyFamily.Slime or EnemyFamily.Wraith,
                EnemyTier.Evolved) => 45,
            (EnemyFamily.Demon or EnemyFamily.Troll or EnemyFamily.Chimera, EnemyTier.Base) => 55,
            (EnemyFamily.Demon or EnemyFamily.Troll or EnemyFamily.Chimera, EnemyTier.Evolved) => 90,
            (EnemyFamily.Goblin or EnemyFamily.Skeleton or EnemyFamily.Slime or EnemyFamily.Wraith
                or EnemyFamily.Troll or EnemyFamily.Chimera, EnemyTier.Elite) => 160,
            // No new balance value was specified for Demon Lord or other unlisted enemies.
            _ => Math.Max(0, enemy.XpReward)
        };
    }

    public CombatExperienceResult CalculateAndAward(
        Battle battle, Enemy enemy, GameSession session, PlayerCharacter player,
        bool permanentlyResolvedWithoutCombat = false)
    {
        ArgumentNullException.ThrowIfNull(battle);
        ArgumentNullException.ThrowIfNull(enemy);
        ArgumentNullException.ThrowIfNull(session);
        ArgumentNullException.ThrowIfNull(player);
        if (battle.Id <= 0 || battle.GameSessionId != session.Id ||
            battle.EnemyId != enemy.Id || session.CharacterId != player.Id)
            throw new ArgumentException("Battle, enemy, session and character must describe the same saved encounter.");
        if (battle.SummonerInstanceId == Guid.Empty)
            throw new ArgumentException("A summoned enemy requires a non-empty summoner instance identifier.", nameof(battle));

        var baseExperience = GetBaseExperience(enemy);

        // Mock repositories share the session instance. Serialize reward claims across
        // calculator instances; a future database implementation must use a transaction.
        lock (session)
        {
            if (battle.Status is GameSessionStatus.Abandoned or GameSessionStatus.Defeat ||
                session.Status is GameSessionStatus.Abandoned or GameSessionStatus.Defeat)
                return new(baseExperience);

            if (battle.Status != GameSessionStatus.Victory &&
                !(battle.Status == GameSessionStatus.InProgress && permanentlyResolvedWithoutCombat))
                return new(baseExperience);

            if (battle.RewardsGranted || session.ExperienceRewardedBattleIds.Contains(battle.Id))
                return new(baseExperience, AlreadyGranted: true);

            var repeatedSummon = battle.SummonerInstanceId is Guid summoner &&
                session.ExperienceRewardedSummonerIds.Contains(summoner);

            // Do not claim a reward if progression fails.
            var progression = repeatedSummon ? null : _experienceService.AddExperience(player, baseExperience);
            session.ExperienceRewardedBattleIds.Add(battle.Id);
            if (battle.SummonerInstanceId is Guid summonerId)
                session.ExperienceRewardedSummonerIds.Add(summonerId);

            // Non-combat resolution is terminal too, preventing a subsequent combat payout.
            battle.Status = GameSessionStatus.Victory;
            battle.CompletedAt ??= DateTime.UtcNow;
            battle.RewardsGranted = true;
            return new(baseExperience, progression, RepeatedSummon: repeatedSummon);
        }
    }
}
