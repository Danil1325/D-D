using System.Globalization;
using DnDGame.BusinessLayer.Common.Errors;
using DnDGame.BusinessLayer.Common.Exceptions;
using DnDGame.BusinessLayer.Dtos.Achievements;
using DnDGame.BusinessLayer.Repositories.Interfaces;
using DnDGame.BusinessLayer.Services.Interfaces;
using DnDGame.Domain.Entities.Achievements;
using DnDGame.Domain.Entities.Locations;
using DnDGame.Domain.Enums;

namespace DnDGame.BusinessLayer.Services;

/// <summary>
/// Application-service implementation of the achievements catalog and the
/// per-player progress tracked against it. See <see cref="IAchievementService"/>
/// for the writer discipline: progress only advances from the event hooks, and
/// every event identity is ledgered exactly-once per player.
/// </summary>
public class AchievementService : IAchievementService
{
    private readonly IAchievementRepository _achievementRepository;
    private readonly IAchievementProgressRepository _progressRepository;
    private readonly IAchievementEventRepository _eventRepository;
    private readonly ICharacterRepository _characterRepository;
    private readonly ICurrentPlayerService _currentPlayerService;

    public AchievementService(
        IAchievementRepository achievementRepository,
        IAchievementProgressRepository progressRepository,
        IAchievementEventRepository eventRepository,
        ICharacterRepository characterRepository,
        ICurrentPlayerService currentPlayerService)
    {
        _achievementRepository = achievementRepository;
        _progressRepository = progressRepository;
        _eventRepository = eventRepository;
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
        var characterId = await ResolveCurrentCharacterIdAsync();
        return await GetProgressForPlayerAsync(characterId);
    }

    public async Task<AchievementsOverviewDto> GetOverviewForCurrentPlayerAsync()
    {
        var characterId = await ResolveCurrentCharacterIdAsync();
        var rows = await BuildProgressRowsAsync(characterId);

        return new AchievementsOverviewDto
        {
            PlayerId = characterId,
            TotalCount = rows.Count,
            CompletedCount = rows.Count(row => row.IsCompleted),
            Locked = rows.Where(row => !row.IsCompleted && row.CurrentAmount == 0).ToList(),
            InProgress = rows.Where(row => !row.IsCompleted && row.CurrentAmount > 0).ToList(),
            Unlocked = rows.Where(row => row.IsCompleted).ToList()
        };
    }

    public async Task<PlayerAchievementsDto> GetProgressForPlayerAsync(int characterId)
    {
        var rows = await BuildProgressRowsAsync(characterId);

        return new PlayerAchievementsDto
        {
            PlayerId = characterId,
            TotalCount = rows.Count,
            CompletedCount = rows.Count(row => row.IsCompleted),
            Achievements = rows
        };
    }

    private async Task<int> ResolveCurrentCharacterIdAsync()
    {
        var currentPlayerId = _currentPlayerService.GetCurrentPlayerId();
        var ownerId = currentPlayerId.ToString(CultureInfo.InvariantCulture);
        var characters = await _characterRepository.GetAllAsync();
        var character = characters.FirstOrDefault(candidate => candidate.OwnerId == ownerId);
        if (character is null)
        {
            throw new DomainException(ErrorCodes.NotFound, $"No character was found for player {currentPlayerId}.");
        }

        return character.Id;
    }

    private async Task<List<PlayerAchievementDto>> BuildProgressRowsAsync(int characterId)
    {
        var achievements = await _achievementRepository.GetAllAsync();
        var progress = await _progressRepository.GetByPlayerIdAsync(characterId);
        var progressByAchievement = progress.ToDictionary(row => row.AchievementId);

        return achievements
            .OrderBy(achievement => achievement.Id)
            .Select(achievement => PlayerAchievementDto.FromDomain(
                achievement,
                progressByAchievement.GetValueOrDefault(achievement.Id)))
            .ToList();
    }

    public Task RegisterCharacterCreatedAsync(int characterId)
        => RegisterEventAsync(characterId, AchievementType.CharacterCreated, characterId.ToString(CultureInfo.InvariantCulture));

    public Task RegisterQuestCompletedAsync(int characterId, int questId)
        => RegisterEventAsync(characterId, AchievementType.QuestsCompleted, questId.ToString(CultureInfo.InvariantCulture));

    public Task RegisterBattleVictoryAsync(int characterId, int battleId)
        => RegisterEventAsync(characterId, AchievementType.BattlesWon, battleId.ToString(CultureInfo.InvariantCulture));

    public Task RegisterLocationUnlockedAsync(int characterId, LocationId locationId)
        => RegisterEventAsync(characterId, AchievementType.LocationsUnlocked, ((int)locationId).ToString(CultureInfo.InvariantCulture));

    /// <summary>
    /// Advances every achievement driven by <paramref name="type"/> for the given
    /// character by one, but only if the specific event has not been counted for
    /// this player before.
    ///
    /// Idempotency is enforced twice:
    ///  1) the event's identity (Type + EventKey) is ledgered exactly-once per
    ///     player — re-reporting the same event is a no-op, so it can never inflate
    ///     progress toward a threshold; and
    ///  2) a row that already has a CompletedAt is left untouched, so the unlock
    ///     timestamp is the first time the threshold was reached and is never
    ///     overwritten by later events.
    /// </summary>
    private async Task RegisterEventAsync(int characterId, AchievementType type, string eventKey)
    {
        var recorded = await _eventRepository.AddIfMissingAsync(new AchievementEvent
        {
            PlayerId = characterId,
            Type = type,
            EventKey = eventKey,
            RecordedAt = DateTime.UtcNow
        });

        if (!recorded)
        {
            return;
        }

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