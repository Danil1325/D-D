using DnDGame.Domain.Entities.Game;

namespace DnDGame.BusinessLayer.Repositories.Interfaces;

/// <summary>Interactive scenario outcome state per game session (Ash Clock, corruption, story flags...).</summary>
public interface IScenarioProgressRepository
{
    Task<ScenarioProgress?> GetByGameSessionAsync(int gameSessionId);
    Task<ScenarioProgress> AddAsync(ScenarioProgress progress);
    Task UpdateAsync(ScenarioProgress progress);
}