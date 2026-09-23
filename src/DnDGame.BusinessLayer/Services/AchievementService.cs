using System.Globalization;
using DnDGame.BusinessLayer.Common.Errors;
using DnDGame.BusinessLayer.Common.Exceptions;
using DnDGame.BusinessLayer.Dtos.Achievements;
using DnDGame.BusinessLayer.Repositories.Interfaces;
using DnDGame.BusinessLayer.Services.Interfaces;
using DnDGame.Domain.Entities.Achievements;
using DnDGame.Domain.Enums;

namespace DnDGame.BusinessLayer.Services;

/// <summary>
/// Application-service implementation of the achievements catalog and the
/// per-player progress tracked against it. See <see cref="IAchievementService"/>
/// for the writer discipline: progress only advances from the event hooks.
/// </summary>
public class AchievementService : IAchievementService
{
    private readonly IAchievementRepository _achievementRepository;
    private readonly IAchievementProgressRepository _progressRepository;
    private readonly ICharacterRepository _characterRepository;
    private readonly ICurrentPlayerService _currentPlayerService;

    public AchievementService(
        IAchievementRepository achievementRepository,
        IAchievementProgressRepository progressRepository,
        ICharacterRepository characterRepository,
        ICurrentPlayerService currentPlayerService)
    {
        _achievementRepository = achievementRepository;
        _progressRepository = progressRepository;
        _characterRepository = characterRepository;
        _currentPlayerService = currentPlayerService;
    }

    public async Task<IReadOnlyList<AchievementDto>> GetCatalogAsync()
    {
        var achievements = await _achievementRepository.GetAllAsync();
        return achievements
            .OrderBy(achievement => achievement.Id)
            .Select(AchievementDto.FromDomain)
            .ToList();
    }

    public async Task<PlayerAchievementsDto> GetProgressForCurrentPlayerAsync()
    {
        var currentPlayerId = _currentPlayerService.GetCurrentPlayerId();
        var ownerId = currentPlayerId.ToString(CultureInfo.InvariantCulture);
        var characters = await _characterRepository.GetAllAsync();
        var character = characters.FirstOrDefault(candidate => candidate.OwnerId == ownerId);
        if (character is null)
        {
            throw new DomainException(ErrorCodes.NotFound, $"No character was found for player {currentPlayerId}.");
        }

        return await GetProgressForPlayerAsync(character.Id);
    }

    public async Task<PlayerAchievementsDto> GetProgressForPlayerAsync(int characterId)
    {
        var achievements = await _achievementRepository.GetAllAsync();
        var progress = await _progressRepository.GetByPlayerIdAsync(characterId);
        var progressByAchievement = progress.ToDictionary(row => row.AchievementId);

        var rows = achievements
            .OrderBy(achievement => achievement.Id)
            .Select(achievement => PlayerAchievementDto.FromDomain(
                achievement,
                progressByAchievement.GetValueOrDefault(achievement.Id)))
            .ToList();

        return new PlayerAchievementsDto
        {
            PlayerId = characterId,
            TotalCount = rows.Count,
            CompletedCount = rows.Count(row => row.IsCompleted),
            Achievements = rows
        };
    }

    public Task RegisterCharacterCreatedAsync(int characterId)
        => RegisterEventAsync(characterId, AchievementType.CharacterCreated);

    public Task RegisterQuestCompletedAsync(int characterId)
        => RegisterEventAsync(characterId, AchievementType.QuestsCompleted);

    public Task RegisterBattleVictoryAsync(int characterId)
        => RegisterEventAsync(characterId, AchievementType.BattlesWon);

    public Task RegisterLocationUnlockedAsync(int characterId)
        => RegisterEventAsync(characterId, AchievementType.LocationsUnlocked);

    /// <summary>
    /// Advances every achievement driven by <paramref name="type"/> for the given
    /// character by one. Idempotent per completed achievement: a row that already
    /// has a CompletedAt is left untouched, so re-fired events (e.g. a victory that
    /// is re-evaluated) never inflate an already-completed achievement.
    /// </summary>
    private async Task RegisterEventAsync(int characterId, AchievementType type)
    {
        var achievements = await _achievementRepository.GetByTypeAsync(type);
        if (achievements.Count == 0)
        {
            return;
        }

        var progress = await _progressRepository.GetByPlayerIdAsync(characterId);
        var progressByAchievement = progress.ToDictionary(row => row.AchievementId);

        foreach (var achievement in achievements)
        {
            if (progressByAchievement.TryGetValue(achievement.Id, out var row)
                && row.CompletedAt is not null)
            {
                continue;
            }

            var created = false;
            if (row is null)
            {
                row = new AchievementProgress
                {
                    PlayerId = characterId,
                    AchievementId = achievement.Id
                };
                progressByAchievement[achievement.Id] = row;
                created = true;
            }

            row.CurrentAmount += 1;
            if (row.CurrentAmount >= achievement.TargetAmount)
            {
                row.CompletedAt = DateTime.UtcNow;
            }

            if (created)
            {
                await _progressRepository.AddAsync(row);
            }
            else
            {
                await _progressRepository.UpdateAsync(row);
            }
        }
    }
}