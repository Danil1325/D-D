using DnDGame.BusinessLayer.Models;
using DnDGame.Domain.Entities.Characters;
using DnDGame.Domain.Entities.Enemies;
using DnDGame.Domain.Entities.Game;

namespace DnDGame.BusinessLayer.Services.Interfaces;

public interface ICombatExperienceCalculator
{
    int GetBaseExperience(Enemy enemy);

    /// <summary>
    /// Calculates and applies EXP once per battle in the session. The caller must persist
    /// the character, session reward ledger and battle together.
    /// Set permanentlyResolvedWithoutCombat only after game logic confirms permanent
    /// removal of the danger; an escape or temporary truce does not qualify.
    /// </summary>
    CombatExperienceResult CalculateAndAward(
        Battle battle, Enemy enemy, GameSession session, PlayerCharacter player,
        bool permanentlyResolvedWithoutCombat = false);
}
