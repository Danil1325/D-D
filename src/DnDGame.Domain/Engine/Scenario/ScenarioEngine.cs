using DnDGame.Domain.Engine.Common;
using DnDGame.Domain.Entities.Characters;
using DnDGame.Domain.Entities.Game;
using DnDGame.Domain.Enums;

namespace DnDGame.Domain.Engine.Scenario;

/// <summary>
/// Pure scenario state machine over <see cref="StoryScene"/> data. See
/// <see cref="IScenarioEngine"/> for the contract and boundary notes.
/// </summary>
public sealed class ScenarioEngine : IScenarioEngine
{
    private const string UserNicknamePlaceholder = "{user_nickname}";

    public EngineResult<ScenarioState> StartScenario(
        ScenarioProgress progress,
        int startSceneId,
        IReadOnlyCollection<StoryScene> scenes,
        PlayerCharacter character,
        IReadOnlyCollection<Quest> quests,
        ICollection<PlayerQuest> playerQuests)
    {
        if (progress is null)
        {
            return InvalidState("Scenario progress is required to start a scenario.");
        }

        if (scenes?.FirstOrDefault(s => s.Id == startSceneId) is not { } entryScene)
        {
            return EngineResult<ScenarioState>.Fail(
                $"Scene {startSceneId} was not found in the scenario catalog.",
                EngineErrorCodes.ScenarioSceneNotFound);
        }

        var state = new ScenarioState(progress)
        {
            Progress =
            {
                CurrentSceneId = startSceneId,
                IsCompleted = false
            }
        };
        state.VisitedSceneIds.Add(startSceneId);

        StartQuestsForScene(state, character, entryScene, quests, playerQuests);

        return OkState(state, "Scenario started.");
    }

    public EngineResult<ScenarioState> ResumeScenario(
        ScenarioProgress progress,
        IReadOnlyCollection<int> visitedSceneIds,
        IReadOnlyCollection<int> selectedChoiceIds)
    {
        if (progress is null)
        {
            return InvalidState("Scenario progress is required to resume a scenario.");
        }

        var state = new ScenarioState(progress);
        if (visitedSceneIds is not null)
        {
            state.VisitedSceneIds.UnionWith(visitedSceneIds);
        }

        if (selectedChoiceIds is not null)
        {
            state.SelectedChoiceIds.UnionWith(selectedChoiceIds);
        }

        return OkState(state, "Scenario resumed.");
    }

    public EngineResult<StoryScene> GetCurrentScene(
        ScenarioState state,
        IReadOnlyCollection<StoryScene> scenes,
        PlayerCharacter character)
    {
        var sceneResult = FindCurrentScene(state, scenes);
        if (!sceneResult.Success || sceneResult.Data is null)
        {
            return EngineResult<StoryScene>.Fail(
                sceneResult.Message,
                sceneResult.ErrorCode ?? EngineErrorCodes.ScenarioSceneNotFound);
        }

        return EngineResult<StoryScene>.Ok(
            ResolveForDisplay(sceneResult.Data, NicknameOf(character)));
    }

    public EngineResult<IReadOnlyList<StoryChoice>> GetAvailableChoices(
        ScenarioState state,
        StoryScene scene,
        PlayerCharacter character,
        IReadOnlyCollection<int>? ownedItemIds = null)
    {
        if (scene is null || character is null)
        {
            return InvalidChoices("A scene and a character are required to list choices.");
        }

        var nickname = NicknameOf(character);
        var available = scene.Choices
            .Where(choice => !state.SelectedChoiceIds.Contains(choice.Id))
            .Where(choice => CheckChoiceRequirements(choice, character, state, ownedItemIds).Success)
            .Select(choice => ResolveChoiceForDisplay(choice, nickname))
            .ToList();

        return EngineResult<IReadOnlyList<StoryChoice>>.Ok(available);
    }

    public EngineResult<bool> CheckChoiceRequirements(
        StoryChoice choice,
        PlayerCharacter character,
        ScenarioState state,
        IReadOnlyCollection<int>? ownedItemIds = null)
    {
        if (choice is null || character is null || state is null)
        {
            return EngineResult<bool>.Fail(
                "A choice, a character and a scenario state are required to evaluate requirements.",
                EngineErrorCodes.ScenarioInvalidState);
        }

        foreach (var requirement in choice.Requirements)
        {
            if (requirement.RaceId.HasValue && character.RaceId != requirement.RaceId.Value)
            {
                return RequirementNotMet($"Race {requirement.RaceId.Value} is required.");
            }

            if (requirement.ClassId.HasValue && character.ClassId != requirement.ClassId.Value)
            {
                return RequirementNotMet($"Class {requirement.ClassId.Value} is required.");
            }

            if (requirement.MinimumLevel.HasValue &&
                character.Level < requirement.MinimumLevel.Value)
            {
                return RequirementNotMet(
                    $"Level {requirement.MinimumLevel.Value} is required (current {character.Level}).");
            }

            if (requirement.Stat.HasValue && requirement.MinimumStat.HasValue &&
                ReadAttribute(requirement.Stat.Value, character) < requirement.MinimumStat.Value)
            {
                return RequirementNotMet(
                    $"{requirement.Stat.Value} {requirement.MinimumStat.Value} is required.");
            }

            if (requirement.RequiredQuestId.HasValue)
            {
                var hasQuest = state.Progress.QuestProgress.TryGetValue(
                    requirement.RequiredQuestId.Value,
                    out var questProgress);
                if (!hasQuest)
                {
                    return RequirementNotMet(
                        $"Quest {requirement.RequiredQuestId.Value} must be present in quest progress.");
                }

                if (requirement.MinimumQuestProgress.HasValue &&
                    questProgress < requirement.MinimumQuestProgress.Value)
                {
                    return RequirementNotMet(
                        $"Quest {requirement.RequiredQuestId.Value} progress {requirement.MinimumQuestProgress.Value} is required.");
                }
            }

            if (requirement.RequiredItemId.HasValue &&
                (ownedItemIds is null || !ownedItemIds.Contains(requirement.RequiredItemId.Value)))
            {
                return RequirementNotMet(
                    $"Item {requirement.RequiredItemId.Value} must be present in the character's inventory.");
            }

            if (requirement.RequiredFlag is not null &&
                !(state.Progress.StoryFlags.TryGetValue(requirement.RequiredFlag, out var flagValue) &&
                  flagValue == requirement.RequiredFlagValue))
            {
                return RequirementNotMet($"Story flag '{requirement.RequiredFlag}' is required.");
            }
        }

        return EngineResult<bool>.Ok(true, "All requirements are met.");
    }

    public EngineResult<ChoiceConsequence> ApplyChoiceConsequences(
        ScenarioState state,
        PlayerCharacter character,
        StoryChoice choice)
    {
        if (character is null || choice is null)
        {
            return EngineResult<ChoiceConsequence>.Fail(
                "A character and a choice are required to apply consequences.",
                EngineErrorCodes.ScenarioInvalidState);
        }

        if (state is null || state.Progress is null)
        {
            return EngineResult<ChoiceConsequence>.Fail(
                "Scenario progress is required to apply consequences.",
                EngineErrorCodes.ScenarioInvalidState);
        }

        foreach (var consequence in choice.Consequences)
        {
            ApplyConsequence(consequence, state.Progress, character);
        }

        return EngineResult<ChoiceConsequence>.Ok(
            new ChoiceConsequence(),
            "Consequences applied.");
    }

    public EngineResult<ChoiceSelectionResult> SelectChoice(
        ScenarioState state,
        StoryScene scene,
        PlayerCharacter character,
        int choiceId,
        IReadOnlyCollection<StoryScene> scenes,
        IReadOnlyCollection<Quest> quests,
        ICollection<PlayerQuest> playerQuests,
        IReadOnlyCollection<int>? ownedItemIds = null)
    {
        if (scene is null || character is null || scenes is null)
        {
            return EngineResult<ChoiceSelectionResult>.Fail(
                "A scene, a character and the scene catalog are required to select a choice.",
                EngineErrorCodes.ScenarioInvalidState);
        }

        if (state.Progress.IsCompleted)
        {
            return EngineResult<ChoiceSelectionResult>.Fail(
                "The story has already been completed.",
                EngineErrorCodes.ScenarioStoryCompleted);
        }

        var choice = scene.Choices.FirstOrDefault(c => c.Id == choiceId);
        if (choice is null)
        {
            return EngineResult<ChoiceSelectionResult>.Fail(
                $"Choice {choiceId} was not found in scene {scene.Id}.",
                EngineErrorCodes.ScenarioChoiceNotFound);
        }

        if (state.SelectedChoiceIds.Contains(choiceId))
        {
            return EngineResult<ChoiceSelectionResult>.Fail(
                $"Choice {choiceId} has already been selected in this run.",
                EngineErrorCodes.ScenarioChoiceAlreadySelected);
        }

        var requirementsResult = CheckChoiceRequirements(choice, character, state, ownedItemIds);
        if (!requirementsResult.Success)
        {
            return EngineResult<ChoiceSelectionResult>.Fail(
                requirementsResult.Message,
                EngineErrorCodes.ScenarioRequirementNotMet);
        }

        var applied = new List<ChoiceConsequence>(choice.Consequences);
        foreach (var consequence in choice.Consequences)
        {
            ApplyConsequence(consequence, state.Progress, character);
        }

        state.SelectedChoiceIds.Add(choiceId);

        if (choice.NextSceneId is not int destination)
        {
            state.Progress.IsCompleted = true;
            return EngineResult<ChoiceSelectionResult>.Ok(
                new ChoiceSelectionResult(state, null, applied),
                "The story has ended.");
        }

        var moveResult = MoveToNextSceneCore(
            state,
            scenes,
            destination,
            character,
            quests,
            playerQuests);
        if (!moveResult.Success || moveResult.Data is null)
        {
            return EngineResult<ChoiceSelectionResult>.Fail(
                moveResult.Message,
                moveResult.ErrorCode ?? EngineErrorCodes.ScenarioSceneNotFound);
        }

        var destinationScene = scenes.First(s => s.Id == destination);
        return EngineResult<ChoiceSelectionResult>.Ok(
            new ChoiceSelectionResult(moveResult.Data, destinationScene, applied),
            "Choice applied.");
    }

    public EngineResult<ScenarioState> MoveToNextScene(
        ScenarioState state,
        IReadOnlyCollection<StoryScene> scenes,
        int nextSceneId,
        PlayerCharacter character,
        IReadOnlyCollection<Quest> quests,
        ICollection<PlayerQuest> playerQuests)
    {
        if (state is null || state.Progress is null)
        {
            return InvalidState("Scenario progress is required to move scenes.");
        }

        if (state.Progress.IsCompleted)
        {
            return EngineResult<ScenarioState>.Fail(
                "The story has already been completed.",
                EngineErrorCodes.ScenarioStoryCompleted);
        }

        return MoveToNextSceneCore(state, scenes, nextSceneId, character, quests, playerQuests);
    }

    private static EngineResult<ScenarioState> MoveToNextSceneCore(
        ScenarioState state,
        IReadOnlyCollection<StoryScene> scenes,
        int nextSceneId,
        PlayerCharacter character,
        IReadOnlyCollection<Quest> quests,
        ICollection<PlayerQuest> playerQuests)
    {
        var destination = scenes?.FirstOrDefault(s => s.Id == nextSceneId);
        if (destination is null)
        {
            return EngineResult<ScenarioState>.Fail(
                $"Scene {nextSceneId} was not found in the scenario catalog.",
                EngineErrorCodes.ScenarioSceneNotFound);
        }

        state.Progress.CurrentSceneId = nextSceneId;
        state.VisitedSceneIds.Add(nextSceneId);

        if (destination.IsFinalScene)
        {
            state.Progress.IsCompleted = true;
        }

        StartQuestsForScene(state, character, destination, quests, playerQuests);

        return OkState(state, "Scene advanced.");
    }

    private static void StartQuestsForScene(
        ScenarioState state,
        PlayerCharacter character,
        StoryScene scene,
        IReadOnlyCollection<Quest> quests,
        ICollection<PlayerQuest> playerQuests)
    {
        if (quests is null || playerQuests is null)
        {
            return;
        }

        foreach (var quest in quests.Where(q => q.AssociatedSceneIds.Contains(scene.Id)))
        {
            var existing = playerQuests.FirstOrDefault(pq => pq.QuestId == quest.Id);

            if (existing is not null &&
                (existing.Status == QuestStatus.Active ||
                 existing.Status == QuestStatus.Completed ||
                 existing.Status == QuestStatus.Failed))
            {
                continue;
            }

            if (!CanStartQuest(quest, character, state, playerQuests))
            {
                continue;
            }

            var now = DateTime.UtcNow;
            if (existing is null)
            {
                playerQuests.Add(new PlayerQuest
                {
                    GameSessionId = state.Progress.GameSessionId,
                    QuestId = quest.Id,
                    Status = QuestStatus.Active,
                    CurrentObjectiveIndex = 0,
                    StartedAt = now
                });
            }
            else
            {
                existing.Status = QuestStatus.Active;
                existing.StartedAt ??= now;
            }
        }
    }

    private static bool CanStartQuest(
        Quest quest,
        PlayerCharacter character,
        ScenarioState state,
        ICollection<PlayerQuest> playerQuests)
    {
        foreach (var entry in quest.RequiredFlags)
        {
            if (!(state.Progress.StoryFlags.TryGetValue(entry.Key, out var value) &&
                  value == entry.Value))
            {
                return false;
            }
        }

        foreach (var prerequisite in quest.Prerequisites)
        {
            if (prerequisite.RequiredQuestId.HasValue)
            {
                var prereqQuest = playerQuests.FirstOrDefault(
                    pq => pq.QuestId == prerequisite.RequiredQuestId.Value);
                if (prereqQuest is null || prereqQuest.Status != prerequisite.RequiredQuestStatus)
                {
                    return false;
                }
            }

            if (prerequisite.MinimumLevel.HasValue &&
                character.Level < prerequisite.MinimumLevel.Value)
            {
                return false;
            }

            if (prerequisite.RequiredFlag is not null &&
                !(state.Progress.StoryFlags.TryGetValue(prerequisite.RequiredFlag, out var flagValue) &&
                  flagValue == prerequisite.RequiredFlagValue))
            {
                return false;
            }
        }

        return true;
    }

    private static void ApplyConsequence(
        ChoiceConsequence consequence,
        ScenarioProgress progress,
        PlayerCharacter character)
    {
        if (consequence is null)
        {
            return;
        }

        if (consequence.EXP != 0)
        {
            character.CurrentXp = Math.Max(0, character.CurrentXp + consequence.EXP);
        }

        if (consequence.AshClock != 0)
        {
            progress.AshClock = ClampNonNegative(progress.AshClock + consequence.AshClock);
        }

        if (consequence.Corruption != 0)
        {
            progress.Corruption = ClampNonNegative(progress.Corruption + consequence.Corruption);
        }

        if (consequence.WarScore != 0)
        {
            progress.WarScore = ClampNonNegative(progress.WarScore + consequence.WarScore);
        }

        foreach (var (companionId, delta) in consequence.CompanionLoyalty)
        {
            if (delta == 0)
            {
                continue;
            }

            progress.CompanionLoyalty.TryGetValue(companionId, out var current);
            progress.CompanionLoyalty[companionId] = ClampNonNegative(current + delta);
        }

        foreach (var (questId, delta) in consequence.QuestProgress)
        {
            if (delta == 0)
            {
                continue;
            }

            progress.QuestProgress.TryGetValue(questId, out var current);
            progress.QuestProgress[questId] = ClampNonNegative(current + delta);
        }

        foreach (var (flag, value) in consequence.StoryFlags)
        {
            progress.StoryFlags[flag] = value;
        }

        foreach (var locationId in consequence.NewLocationIds.Distinct())
        {
            progress.UnlockedLocationIds.Add(locationId);
        }
    }

    private static int ReadAttribute(AttributeType stat, PlayerCharacter character)
    {
        return stat switch
        {
            AttributeType.Health => character.MaxHealth,
            AttributeType.Strength => character.Strength,
            AttributeType.Dexterity => character.Dexterity,
            AttributeType.Intelligence => character.Intelligence,
            AttributeType.Charisma => character.Charisma,
            _ => 0
        };
    }

    private static int ClampNonNegative(int value)
    {
        return Math.Max(0, value);
    }

    private static EngineResult<StoryScene> FindCurrentScene(
        ScenarioState state,
        IReadOnlyCollection<StoryScene> scenes)
    {
        if (state is null || state.Progress is null)
        {
            return EngineResult<StoryScene>.Fail(
                "Scenario progress is required to read the current scene.",
                EngineErrorCodes.ScenarioInvalidState);
        }

        if (scenes?.FirstOrDefault(s => s.Id == state.Progress.CurrentSceneId) is not { } scene)
        {
            return EngineResult<StoryScene>.Fail(
                $"Scene {state.Progress.CurrentSceneId} was not found in the scenario catalog.",
                EngineErrorCodes.ScenarioSceneNotFound);
        }

        return EngineResult<StoryScene>.Ok(scene);
    }

    private static StoryScene ResolveForDisplay(StoryScene scene, string nickname)
    {
        return new StoryScene
        {
            Id = scene.Id,
            Act = scene.Act,
            Chapter = scene.Chapter,
            Title = Substitute(scene.Title, nickname),
            LocationId = scene.LocationId,
            BackgroundImage = scene.BackgroundImage,
            IsFinalScene = scene.IsFinalScene,
            Dialogues = scene.Dialogues
                .OrderBy(dialogue => dialogue.DialogueOrder)
                .Select(dialogue => new StoryDialogue
                {
                    Id = dialogue.Id,
                    Speaker = Substitute(dialogue.Speaker, nickname),
                    Text = Substitute(dialogue.Text, nickname),
                    DialogueOrder = dialogue.DialogueOrder,
                    DialogueType = dialogue.DialogueType
                })
                .ToList(),
            Choices = scene.Choices
                .Select(choice => ResolveChoiceForDisplay(choice, nickname))
                .ToList()
        };
    }

    private static StoryChoice ResolveChoiceForDisplay(StoryChoice choice, string nickname)
    {
        return new StoryChoice
        {
            Id = choice.Id,
            Text = Substitute(choice.Text, nickname),
            NextSceneId = choice.NextSceneId,
            Requirements = choice.Requirements,
            Consequences = choice.Consequences
        };
    }

    private static string Substitute(string text, string nickname)
    {
        return string.IsNullOrEmpty(text)
            ? text
            : text.Replace(UserNicknamePlaceholder, nickname);
    }

    private static string NicknameOf(PlayerCharacter character)
    {
        return character.Name ?? string.Empty;
    }

    private static EngineResult<ScenarioState> InvalidState(string message)
    {
        return EngineResult<ScenarioState>.Fail(message, EngineErrorCodes.ScenarioInvalidState);
    }

    private static EngineResult<bool> RequirementNotMet(string message)
    {
        return EngineResult<bool>.Fail(message, EngineErrorCodes.ScenarioRequirementNotMet);
    }

    private static EngineResult<IReadOnlyList<StoryChoice>> InvalidChoices(string message)
    {
        return EngineResult<IReadOnlyList<StoryChoice>>.Fail(
            message,
            EngineErrorCodes.ScenarioInvalidState);
    }

    private static EngineResult<ScenarioState> OkState(ScenarioState state, string message)
    {
        return EngineResult<ScenarioState>.Ok(state, message);
    }
}
