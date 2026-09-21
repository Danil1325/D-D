using System.Globalization;
using DnDGame.BusinessLayer.Common.Errors;
using DnDGame.BusinessLayer.Common.Exceptions;
using DnDGame.BusinessLayer.Dtos.Scenarios;
using DnDGame.BusinessLayer.Repositories.Interfaces;
using DnDGame.BusinessLayer.Services.Interfaces;
using DnDGame.Domain.Engine.Common;
using DnDGame.Domain.Engine.Scenario;
using DnDGame.Domain.Entities.Characters;
using DnDGame.Domain.Entities.Game;
using DnDGame.Domain.Enums;

namespace DnDGame.BusinessLayer.Services;

/// <summary>
/// Orchestrates the interactive scenario over <see cref="IScenarioEngine"/>.
///
/// Design decisions:
/// - <see cref="PlayerCharacter.OwnerId"/> is how a player id maps to a character
///   (there is no Account-to-Character relationship yet), following the mock-data
///   convention that every character carries a fixed owner id.
/// - A run belongs to a <see cref="GameSession"/>. Starting a run reuses the
///   character's existing session, or creates one for the single seeded adventure.
/// - Visited/selected bookkeeping is not persisted on <see cref="ScenarioProgress"/>,
///   so a resumed run rebuilds it empty; each request rebuilds the state from the
///   progress record and the scene catalog.
/// - Engine failures are translated to DomainException here (the business layer
///   never lets engine codes leak to the HTTP mapper).
/// </summary>
public class ScenarioService : IScenarioService
{
    private readonly IScenarioEngine _engine;
    private readonly ICharacterRepository _characterRepository;
    private readonly IGameSessionRepository _gameSessionRepository;
    private readonly IScenarioProgressRepository _progressRepository;
    private readonly IStorySceneRepository _sceneRepository;
    private readonly IQuestRepository _questRepository;
    private readonly IPlayerQuestRepository _playerQuestRepository;
    private readonly ILocationRepository _locationRepository;
    private readonly IExplicitLocationUnlockService _explicitLocationUnlockService;

    public ScenarioService(
        IScenarioEngine engine,
        ICharacterRepository characterRepository,
        IGameSessionRepository gameSessionRepository,
        IScenarioProgressRepository progressRepository,
        IStorySceneRepository sceneRepository,
        IQuestRepository questRepository,
        IPlayerQuestRepository playerQuestRepository,
        ILocationRepository locationRepository,
        IExplicitLocationUnlockService explicitLocationUnlockService)
    {
        _engine = engine;
        _characterRepository = characterRepository;
        _gameSessionRepository = gameSessionRepository;
        _progressRepository = progressRepository;
        _sceneRepository = sceneRepository;
        _questRepository = questRepository;
        _playerQuestRepository = playerQuestRepository;
        _locationRepository = locationRepository;
        _explicitLocationUnlockService = explicitLocationUnlockService;
    }

    public async Task<StorySceneDto> GetCurrentAsync(int playerId)
    {
        var character = await RequireCharacterAsync(playerId);
        var session = await RequireSessionAsync(character.Id);
        var progress = await RequireProgressAsync(session.Id);

        var scenes = await _sceneRepository.GetAllAsync();

        var state = RequireEngineOk(_engine.ResumeScenario(progress, Array.Empty<int>(), Array.Empty<int>()));
        var currentScene = RequireEngineOk(_engine.GetCurrentScene(state, scenes, character));
        var catalogScene = scenes.First(scene => scene.Id == currentScene.Id);
        var availableChoices = RequireEngineOk(_engine.GetAvailableChoices(state, catalogScene, character));

        return new StorySceneDto
        {
            Id = currentScene.Id,
            Act = currentScene.Act,
            Chapter = currentScene.Chapter,
            Title = currentScene.Title,
            LocationId = currentScene.LocationId,
            BackgroundImage = currentScene.BackgroundImage,
            IsFinalScene = currentScene.IsFinalScene,
            Dialogues = currentScene.Dialogues
                .OrderBy(dialogue => dialogue.DialogueOrder)
                .Select(DialogueDto.FromDomain)
                .ToList(),
            Choices = availableChoices.Select(ChoiceDto.FromDomain).ToList()
        };
    }

    public async Task<ScenarioProgressDto> StartAsync(int playerId)
    {
        var character = await RequireCharacterAsync(playerId);
        var session = await RequireSessionOrCreateAsync(character.Id);

        var progressWasCreated = false;
        var progress = await _progressRepository.GetByGameSessionAsync(session.Id);
        if (progress is not null && !progress.IsCompleted)
        {
            throw new DomainException(
                ErrorCodes.Conflict,
                "A scenario is already in progress for this session.");
        }

        if (progress is null)
        {
            progress = new ScenarioProgress { GameSessionId = session.Id };
            progressWasCreated = true;
        }
        else
        {
            // Restarting after a completed run: wipe the previous run's outcome so
            // the new one starts clean (flags, counters and quest-tracked progress
            // must not leak into it).
            progress.CurrentSceneId = 0;
            progress.IsCompleted = false;
            progress.AshClock = 0;
            progress.Corruption = 0;
            progress.WarScore = 0;
            progress.CompanionLoyalty.Clear();
            progress.QuestProgress.Clear();
            progress.StoryFlags.Clear();
        }

        var scenes = await _sceneRepository.GetAllAsync();
        var quests = await _questRepository.GetAllAsync();
        var playerQuests = (await _playerQuestRepository.GetByGameSessionAsync(session.Id)).ToList();

        var state = RequireEngineOk(_engine.StartScenario(
            progress,
            EntrySceneId(scenes),
            scenes,
            character,
            quests,
            playerQuests));

        await PersistRunAsync(progress, character, playerQuests, progressWasCreated);
        return ScenarioProgressDto.FromDomain(state.Progress);
    }

    public async Task<ScenarioProgressDto> SelectChoiceAsync(SelectChoiceRequest request)
    {
        ArgumentNullException.ThrowIfNull(request);

        var character = await RequireCharacterAsync(request.PlayerId);
        var session = await RequireSessionAsync(character.Id);
        var progress = await RequireProgressAsync(session.Id);

        if (progress.IsCompleted)
        {
            throw new DomainException(ErrorCodes.Conflict, "The story has already been completed.");
        }

        if (request.SceneId != progress.CurrentSceneId)
        {
            throw new DomainException(
                ErrorCodes.Conflict,
                $"Scene {request.SceneId} does not match the current scene {progress.CurrentSceneId}.");
        }

        var scenes = await _sceneRepository.GetAllAsync();
        var quests = await _questRepository.GetAllAsync();
        var playerQuests = (await _playerQuestRepository.GetByGameSessionAsync(session.Id)).ToList();

        var scene = scenes.FirstOrDefault(candidate => candidate.Id == request.SceneId);
        if (scene is null)
        {
            throw new DomainException(ErrorCodes.NotFound, $"Scene {request.SceneId} was not found in the catalog.");
        }

        var state = RequireEngineOk(_engine.ResumeScenario(progress, Array.Empty<int>(), Array.Empty<int>()));
        var selection = RequireEngineOk(_engine.SelectChoice(
            state,
            scene,
            character,
            request.ChoiceId,
            scenes,
            quests,
            playerQuests));

        await PersistRunAsync(progress, character, playerQuests, progressWasCreated: false);

        var newLocationIds = await UnlockExplicitLocationsAsync(character, selection.AppliedConsequences);
        return ScenarioProgressDto.FromDomain(progress, newLocationIds);
    }

    public async Task<IReadOnlyList<LocationDto>> GetLocationsAsync()
    {
        var locations = await _locationRepository.GetAllAsync();
        return locations.Select(LocationDto.FromDomain).ToList();
    }

    public async Task<LocationDto> GetLocationAsync(int locationId)
    {
        var location = await _locationRepository.GetByIdAsync(locationId)
            ?? throw new DomainException(ErrorCodes.NotFound, $"Location {locationId} was not found.");
        return LocationDto.FromDomain(location);
    }

    private async Task<PlayerCharacter> RequireCharacterAsync(int playerId)
    {
        var characters = await _characterRepository.GetAllAsync();
        return characters.FirstOrDefault(character => character.OwnerId == playerId.ToString(CultureInfo.InvariantCulture))
            ?? throw new DomainException(ErrorCodes.NotFound, $"No character was found for player {playerId}.");
    }

    private async Task<GameSession> RequireSessionAsync(int characterId)
    {
        var sessions = await _gameSessionRepository.GetByCharacterIdAsync(characterId);
        return sessions.FirstOrDefault(session => session.Status == GameSessionStatus.InProgress)
            ?? sessions.FirstOrDefault()
            ?? throw new DomainException(ErrorCodes.NotFound, $"No game session was found for character {characterId}.");
    }

    private async Task<GameSession> RequireSessionOrCreateAsync(int characterId)
    {
        var existing = await _gameSessionRepository.GetByCharacterIdAsync(characterId);
        if (existing.Count > 0)
        {
            return existing.FirstOrDefault(session => session.Status == GameSessionStatus.InProgress)
                ?? existing.First();
        }

        var session = new GameSession
        {
            CharacterId = characterId,
            AdventureId = 1, // the single seeded adventure
            Status = GameSessionStatus.InProgress,
            StartedAt = DateTime.UtcNow
        };
        return await _gameSessionRepository.AddAsync(session);
    }

    private async Task<ScenarioProgress> RequireProgressAsync(int gameSessionId)
    {
        return await _progressRepository.GetByGameSessionAsync(gameSessionId)
            ?? throw new DomainException(ErrorCodes.NotFound, "No scenario has been started for this player yet.");
    }

    private async Task PersistRunAsync(
        ScenarioProgress progress,
        PlayerCharacter character,
        ICollection<PlayerQuest> playerQuests,
        bool progressWasCreated)
    {
        if (progressWasCreated)
        {
            await _progressRepository.AddAsync(progress);
        }
        else
        {
            await _progressRepository.UpdateAsync(progress);
        }

        await _characterRepository.UpdateAsync(character);

        // The engine appends freshly-started quests (Id == 0) to the collection.
        foreach (var playerQuest in playerQuests.Where(quest => quest.Id == 0))
        {
            await _playerQuestRepository.AddAsync(playerQuest);
        }
    }

    private async Task<IReadOnlyCollection<int>> UnlockExplicitLocationsAsync(
        PlayerCharacter character,
        IReadOnlyList<ChoiceConsequence> appliedConsequences)
    {
        var locationIds = appliedConsequences.SelectMany(consequence => consequence.NewLocationIds);
        var newlyUnlocked = await _explicitLocationUnlockService.UnlockExplicitLocationsAsync(character, locationIds);
        return newlyUnlocked.Select(locationId => (int)locationId).ToList();
    }

    private static int EntrySceneId(IReadOnlyList<StoryScene> scenes)
    {
        if (scenes.Count == 0)
        {
            throw new DomainException(ErrorCodes.InternalError, "The scenario catalog is empty.");
        }

        return scenes.Min(scene => scene.Id);
    }

    private static T RequireEngineOk<T>(EngineResult<T> result)
    {
        if (result.Success && result.Data is not null)
        {
            return result.Data;
        }

        throw new DomainException(MapEngineError(result.ErrorCode), result.Message ?? "The scenario operation failed.");
    }

    private static string MapEngineError(string? engineCode) => engineCode switch
    {
        EngineErrorCodes.ScenarioSceneNotFound or EngineErrorCodes.ScenarioChoiceNotFound => ErrorCodes.NotFound,
        EngineErrorCodes.ScenarioInvalidState
            or EngineErrorCodes.ScenarioChoiceAlreadySelected
            or EngineErrorCodes.ScenarioRequirementNotMet
            or EngineErrorCodes.ScenarioStoryCompleted => ErrorCodes.Conflict,
        _ => ErrorCodes.Conflict
    };
}
