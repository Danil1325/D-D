using DnDGame.Domain.Engine.Common;
using DnDGame.Domain.Engine.Scenario;
using DnDGame.Domain.Entities.Characters;
using DnDGame.Domain.Entities.Game;
using DnDGame.Domain.Enums;
using Xunit;

namespace DnDGame.Tests.Engine.Scenario;

public class ScenarioEngineTests
{
    private readonly IScenarioEngine _engine = new ScenarioEngine();

    // --- StartScenario ---

    [Fact]
    public void StartScenario_SetsCurrentSceneAndMarksItVisited()
    {
        var progress = CreateProgress();
        var result = _engine.StartScenario(
            progress,
            startSceneId: 900,
            Enumerable.Range(900, 5).Select(id => CreateScene(id)).ToList(),
            CreateCharacter(),
            Array.Empty<Quest>(),
            new List<PlayerQuest>());

        Assert.True(result.Success);
        Assert.Equal(900, result.Data!.Progress.CurrentSceneId);
        Assert.False(result.Data.IsCompleted);
        Assert.Contains(900, result.Data.VisitedSceneIds);
    }

    [Fact]
    public void StartScenario_UnknownStartScene_Fails()
    {
        var result = _engine.StartScenario(
            CreateProgress(),
            startSceneId: 999,
            scenes: new List<StoryScene> { CreateScene(900) },
            CreateCharacter(),
            Array.Empty<Quest>(),
            new List<PlayerQuest>());

        Assert.False(result.Success);
        Assert.Equal(EngineErrorCodes.ScenarioSceneNotFound, result.ErrorCode);
    }

    [Fact]
    public void StartScenario_NullProgress_Fails()
    {
        var result = _engine.StartScenario(
            null!,
            startSceneId: 900,
            scenes: new List<StoryScene> { CreateScene(900) },
            CreateCharacter(),
            Array.Empty<Quest>(),
            new List<PlayerQuest>());

        Assert.False(result.Success);
        Assert.Equal(EngineErrorCodes.ScenarioInvalidState, result.ErrorCode);
    }

    [Fact]
    public void StartScenario_StartsQuestsAssociatedWithEntryScene()
    {
        var quest = CreateQuest(questId: 1, associatedSceneIds: new[] { 900 });
        var playerQuests = new List<PlayerQuest>();

        var result = _engine.StartScenario(
            CreateProgress(),
            startSceneId: 900,
            scenes: new List<StoryScene> { CreateScene(900) },
            CreateCharacter(),
            quests: new List<Quest> { quest },
            playerQuests);

        Assert.True(result.Success);
        var playerQuest = Assert.Single(playerQuests);
        Assert.Equal(quest.Id, playerQuest.QuestId);
        Assert.Equal(QuestStatus.Active, playerQuest.Status);
        Assert.NotNull(playerQuest.StartedAt);
    }

    // --- ResumeScenario ---

    [Fact]
    public void ResumeScenario_RestoresVisitedAndSelectedCollections()
    {
        var progress = CreateProgress();
        progress.CurrentSceneId = 902;
        progress.IsCompleted = false;

        var result = _engine.ResumeScenario(
            progress,
            visitedSceneIds: new[] { 900, 901, 902 },
            selectedChoiceIds: new[] { 10, 11 });

        Assert.True(result.Success);
        Assert.Equal(new[] { 900, 901, 902 }, result.Data!.VisitedSceneIds.OrderBy(id => id));
        Assert.Equal(new[] { 10, 11 }, result.Data.SelectedChoiceIds.OrderBy(id => id));
        Assert.Equal(902, result.Data.Progress.CurrentSceneId);
    }

    [Fact]
    public void ResumeScenario_NullProgress_Fails()
    {
        var result = _engine.ResumeScenario(null!, Array.Empty<int>(), Array.Empty<int>());

        Assert.False(result.Success);
        Assert.Equal(EngineErrorCodes.ScenarioInvalidState, result.ErrorCode);
    }

    // --- GetCurrentScene ---

    [Fact]
    public void GetCurrentScene_ReplacesUserNicknameAndDoesNotMutateTheStore()
    {
        var storeScene = CreateScene(
            id: 900,
            dialogues: new[]
            {
                new StoryDialogue { Speaker = "{user_nickname}", Text = "Salut {user_nickname}!" }
            },
            choices: new[]
            {
                CreateChoice(id: 10, text: "{user_nickname} incepe aventura.")
            });
        var startResult = _engine.StartScenario(
            CreateProgress(),
            startSceneId: 900,
            scenes: new List<StoryScene> { storeScene },
            CreateCharacter(),
            Array.Empty<Quest>(),
            new List<PlayerQuest>());

        var result = _engine.GetCurrentScene(
            startResult.Data!,
            new List<StoryScene> { storeScene },
            CreateCharacter(name: "Alex"));

        Assert.True(result.Success);
        var dialogue = Assert.Single(result.Data!.Dialogues);
        Assert.Equal("Alex", dialogue.Speaker);
        Assert.Equal("Salut Alex!", dialogue.Text);
        Assert.Equal("Alex incepe aventura.", Assert.Single(result.Data.Choices).Text);
        Assert.Equal("Salut {user_nickname}!", Assert.Single(storeScene.Dialogues).Text);
    }

    [Fact]
    public void GetCurrentScene_OrdersDialoguesByDialogueOrder()
    {
        var storeScene = CreateScene(
            id: 900,
            dialogues: new[]
            {
                new StoryDialogue { Id = 1, Text = "ultimul", DialogueOrder = 20 },
                new StoryDialogue { Id = 2, Text = "primul", DialogueOrder = 10 }
            });

        var startResult = _engine.StartScenario(
            CreateProgress(),
            startSceneId: 900,
            scenes: new List<StoryScene> { storeScene },
            CreateCharacter(),
            Array.Empty<Quest>(),
            new List<PlayerQuest>());

        var result = _engine.GetCurrentScene(
            startResult.Data!,
            new List<StoryScene> { storeScene },
            CreateCharacter());

        Assert.Equal(new[] { "primul", "ultimul" }, result.Data!.Dialogues.Select(d => d.Text));
    }

    [Fact]
    public void GetCurrentScene_UnknownScene_Fails()
    {
        var progress = CreateProgress();
        progress.CurrentSceneId = 999;

        var result = _engine.GetCurrentScene(
            new ScenarioState(progress),
            scenes: new List<StoryScene> { CreateScene(900) },
            character: CreateCharacter());

        Assert.False(result.Success);
        Assert.Equal(EngineErrorCodes.ScenarioSceneNotFound, result.ErrorCode);
    }

    // --- GetAvailableChoices ---

    [Fact]
    public void GetAvailableChoices_ExcludesAlreadySelectedChoice()
    {
        var scene = CreateScene(
            id: 900,
            choices: new[]
            {
                CreateChoice(id: 10, nextSceneId: 901),
                CreateChoice(id: 11, nextSceneId: 902)
            });
        var state = new ScenarioState(CreateProgress());
        state.SelectedChoiceIds.Add(10);

        var result = _engine.GetAvailableChoices(state, scene, CreateCharacter());

        Assert.True(result.Success);
        Assert.Equal(new[] { 11 }, result.Data!.Select(c => c.Id));
    }

    [Fact]
    public void GetAvailableChoices_ExcludesChoicesWithUnmetRequirements()
    {
        var scene = CreateScene(
            id: 900,
            choices: new[]
            {
                CreateChoice(id: 10, nextSceneId: 901, requirements: new[] { CreateRequirement(minimumLevel: 5) }),
                CreateChoice(id: 11, nextSceneId: 902)
            });

        var result = _engine.GetAvailableChoices(
            new ScenarioState(CreateProgress()),
            scene,
            CreateCharacter(level: 3));

        Assert.True(result.Success);
        Assert.Equal(new[] { 11 }, result.Data!.Select(c => c.Id));
    }

    [Fact]
    public void GetAvailableChoices_KeepsChoicesMeetingRaceRequirement()
    {
        var scene = CreateScene(
            id: 900,
            choices: new[] { CreateChoice(id: 10, nextSceneId: 901, requirements: new[] { CreateRequirement(raceId: 2) }) });

        var result = _engine.GetAvailableChoices(
            new ScenarioState(CreateProgress()),
            scene,
            CreateCharacter(raceId: 2));

        Assert.True(result.Success);
        Assert.Equal(new[] { 10 }, result.Data!.Select(c => c.Id));
    }

    [Fact]
    public void GetAvailableChoices_SubstitutesNicknameInChoiceText()
    {
        var scene = CreateScene(
            id: 900,
            choices: new[] { CreateChoice(id: 10, text: "{user_nickname} fugeste.") });

        var result = _engine.GetAvailableChoices(
            new ScenarioState(CreateProgress()),
            scene,
            CreateCharacter(name: "Alex"));

        Assert.Equal("Alex fugeste.", Assert.Single(result.Data!).Text);
    }

    [Fact]
    public void GetAvailableChoices_ReturnsEmptyWhenNothingIsSelectable()
    {
        var scene = CreateScene(
            id: 900,
            choices: new[] { CreateChoice(id: 10, nextSceneId: 901) });
        var state = new ScenarioState(CreateProgress());
        state.SelectedChoiceIds.Add(10);

        var result = _engine.GetAvailableChoices(state, scene, CreateCharacter());

        Assert.True(result.Success);
        Assert.Empty(result.Data!);
    }

    // --- CheckChoiceRequirements ---

    [Fact]
    public void CheckChoiceRequirements_NoRequirements_ReturnsTrue()
    {
        var result = _engine.CheckChoiceRequirements(
            CreateChoice(id: 10),
            CreateCharacter(),
            new ScenarioState(CreateProgress()));

        Assert.True(result.Success);
        Assert.True(result.Data);
    }

    [Fact]
    public void CheckChoiceRequirements_StrengthThresholdIsInclusive()
    {
        var choice = CreateChoice(id: 10, requirements: new[]
        {
            CreateRequirement(stat: AttributeType.Strength, minimumStat: 14)
        });

        var character = CreateCharacter();
        character.Strength = 14;
        Assert.True(_engine.CheckChoiceRequirements(choice, character, new ScenarioState(CreateProgress())).Data);

        character.Strength = 13;
        Assert.False(_engine.CheckChoiceRequirements(choice, character, new ScenarioState(CreateProgress())).Success);
    }

    [Fact]
    public void CheckChoiceRequirements_HealthUsesMaxHealthNotCurrentHealth()
    {
        var choice = CreateChoice(id: 10, requirements: new[]
        {
            CreateRequirement(stat: AttributeType.Health, minimumStat: 20)
        });

        var character = CreateCharacter();
        character.MaxHealth = 20;
        character.CurrentHealth = 1;

        Assert.True(_engine.CheckChoiceRequirements(choice, character, new ScenarioState(CreateProgress())).Data);
    }

    [Fact]
    public void CheckChoiceRequirements_MissingQuestProgress_Fails()
    {
        var choice = CreateChoice(id: 10, requirements: new[]
        {
            CreateRequirement(requiredQuestId: 5)
        });

        var result = _engine.CheckChoiceRequirements(
            choice,
            CreateCharacter(),
            new ScenarioState(CreateProgress()));

        Assert.False(result.Success);
        Assert.Equal(EngineErrorCodes.ScenarioRequirementNotMet, result.ErrorCode);
    }

    [Fact]
    public void CheckChoiceRequirements_QuestProgressThresholdIsInclusive()
    {
        var choice = CreateChoice(id: 10, requirements: new[]
        {
            CreateRequirement(requiredQuestId: 5, minimumQuestProgress: 2)
        });
        var state = new ScenarioState(CreateProgress());
        state.Progress.QuestProgress[5] = 2;

        Assert.True(_engine.CheckChoiceRequirements(choice, CreateCharacter(), state).Data);

        state.Progress.QuestProgress[5] = 1;
        Assert.False(_engine.CheckChoiceRequirements(choice, CreateCharacter(), state).Success);
    }

    [Fact]
    public void CheckChoiceRequirements_ItemRequirementUsesOwnedItemIds()
    {
        var choice = CreateChoice(id: 10, requirements: new[] { CreateRequirement(requiredItemId: 7) });

        Assert.True(_engine.CheckChoiceRequirements(choice, CreateCharacter(), new ScenarioState(CreateProgress()), new[] { 7 }).Data);
        Assert.False(_engine.CheckChoiceRequirements(choice, CreateCharacter(), new ScenarioState(CreateProgress()), new[] { 8 }).Success);
        Assert.False(_engine.CheckChoiceRequirements(choice, CreateCharacter(), new ScenarioState(CreateProgress())).Success);
    }

    [Fact]
    public void CheckChoiceRequirements_FlagRequirementAndMissingFlagCountsAsFalse()
    {
        var choice = CreateChoice(id: 10, requirements: new[]
        {
            new ChoiceRequirement { RequiredFlag = "corrupted_path", RequiredFlagValue = true }
        });

        Assert.False(_engine.CheckChoiceRequirements(choice, CreateCharacter(), new ScenarioState(CreateProgress())).Success);

        var state = new ScenarioState(CreateProgress());
        state.Progress.StoryFlags["corrupted_path"] = false;
        Assert.False(_engine.CheckChoiceRequirements(choice, CreateCharacter(), state).Success);

        state.Progress.StoryFlags["corrupted_path"] = true;
        Assert.True(_engine.CheckChoiceRequirements(choice, CreateCharacter(), state).Data);
    }

    // --- ApplyChoiceConsequences ---

    [Fact]
    public void ApplyChoiceConsequences_UpdatesXpCountersAndDictionaries()
    {
        var choice = CreateChoice(id: 10, consequences: new[]
        {
            new ChoiceConsequence
            {
                EXP = 50,
                AshClock = 3,
                Corruption = 2,
                WarScore = 5,
                CompanionLoyalty = new Dictionary<int, int> { [1] = 2 },
                QuestProgress = new Dictionary<int, int> { [5] = 1 },
                StoryFlags = new Dictionary<string, bool> { ["gate_open"] = true }
            }
        });
        var progress = CreateProgress();
        var character = CreateCharacter(xp: 100);

        var result = _engine.ApplyChoiceConsequences(new ScenarioState(progress), character, choice);

        Assert.True(result.Success);
        Assert.Equal(150, character.CurrentXp);
        Assert.Equal(3, progress.AshClock);
        Assert.Equal(2, progress.Corruption);
        Assert.Equal(5, progress.WarScore);
        Assert.Equal(2, progress.CompanionLoyalty[1]);
        Assert.Equal(1, progress.QuestProgress[5]);
        Assert.True(progress.StoryFlags["gate_open"]);
    }

    [Fact]
    public void ApplyChoiceConsequences_ClampsTotalsAtZero()
    {
        var choice = CreateChoice(id: 10, consequences: new[]
        {
            new ChoiceConsequence
            {
                EXP = -100,
                AshClock = -5,
                Corruption = -10,
                WarScore = -2,
                CompanionLoyalty = new Dictionary<int, int> { [1] = -3 },
                QuestProgress = new Dictionary<int, int> { [5] = -7 }
            }
        });
        var progress = CreateProgress();
        progress.Corruption = 4;
        var character = CreateCharacter(xp: 10);

        var result = _engine.ApplyChoiceConsequences(
            new ScenarioState(progress),
            character,
            choice);

        Assert.True(result.Success);
        Assert.Equal(0, character.CurrentXp);
        Assert.Equal(0, progress.AshClock);
        Assert.Equal(0, progress.Corruption);
        Assert.Equal(0, progress.WarScore);
        Assert.Equal(0, progress.CompanionLoyalty[1]);
        Assert.Equal(0, progress.QuestProgress[5]);
    }

    // --- SelectChoice ---

    [Fact]
    public void SelectChoice_UnknownChoice_Fails()
    {
        var scene = CreateScene(
            id: 900,
            choices: new[] { CreateChoice(id: 10, nextSceneId: 901) });

        var result = _engine.SelectChoice(
            new ScenarioState(CreateProgress()),
            scene,
            CreateCharacter(),
            choiceId: 999,
            scenes: new List<StoryScene> { scene, CreateScene(901) },
            quests: Array.Empty<Quest>(),
            playerQuests: new List<PlayerQuest>());

        Assert.False(result.Success);
        Assert.Equal(EngineErrorCodes.ScenarioChoiceNotFound, result.ErrorCode);
    }

    [Fact]
    public void SelectChoice_SameChoiceTwice_FailsOnSecondSelection()
    {
        var scene = CreateScene(
            id: 900,
            choices: new[] { CreateChoice(id: 10, nextSceneId: 901) });
        var scenes = new List<StoryScene> { scene, CreateScene(901) };
        var state = new ScenarioState(CreateProgress());
        var quests = new List<Quest>();
        var playerQuests = new List<PlayerQuest>();

        var first = _engine.SelectChoice(state, scene, CreateCharacter(), 10, scenes, quests, playerQuests);
        var second = _engine.SelectChoice(state, scene, CreateCharacter(), 10, scenes, quests, playerQuests);

        Assert.True(first.Success);
        Assert.False(second.Success);
        Assert.Equal(EngineErrorCodes.ScenarioChoiceAlreadySelected, second.ErrorCode);
    }

    [Fact]
    public void SelectChoice_UnmetRequirement_FailsWithoutApplying()
    {
        var scene = CreateScene(
            id: 900,
            choices: new[] { CreateChoice(id: 10, nextSceneId: 901, requirements: new[] { CreateRequirement(minimumLevel: 5) }) });
        var state = new ScenarioState(CreateProgress());
        state.Progress.CurrentSceneId = 900;

        var result = _engine.SelectChoice(
            state,
            scene,
            CreateCharacter(level: 2),
            choiceId: 10,
            scenes: new List<StoryScene> { scene, CreateScene(901) },
            quests: Array.Empty<Quest>(),
            playerQuests: new List<PlayerQuest>());

        Assert.False(result.Success);
        Assert.Equal(EngineErrorCodes.ScenarioRequirementNotMet, result.ErrorCode);
        Assert.Empty(state.SelectedChoiceIds);
        Assert.Equal(900, state.Progress.CurrentSceneId);
    }

    [Fact]
    public void SelectChoice_MovesToNextSceneAndStartsItsQuests()
    {
        var scene = CreateScene(id: 900, choices: new[] { CreateChoice(id: 10, nextSceneId: 901) });
        var destination = CreateScene(901);
        var quest = CreateQuest(questId: 1, associatedSceneIds: new[] { 901 });
        var playerQuests = new List<PlayerQuest>();
        var state = new ScenarioState(CreateProgress());

        var result = _engine.SelectChoice(
            state,
            scene,
            CreateCharacter(),
            choiceId: 10,
            scenes: new List<StoryScene> { scene, destination },
            quests: new List<Quest> { quest },
            playerQuests);

        Assert.True(result.Success);
        Assert.Equal(901, result.Data!.State.Progress.CurrentSceneId);
        Assert.Contains(901, state.VisitedSceneIds);
        Assert.Contains(10, state.SelectedChoiceIds);
        Assert.Equal(901, result.Data.NextScene!.Id);
        Assert.False(result.Data.StoryEnded);

        var playerQuest = Assert.Single(playerQuests);
        Assert.Equal(quest.Id, playerQuest.QuestId);
        Assert.Equal(QuestStatus.Active, playerQuest.Status);
        Assert.NotNull(playerQuest.StartedAt);
    }

    [Fact]
    public void SelectChoice_QuestStartRespectsRequiredFlags()
    {
        var scene = CreateScene(id: 900, choices: new[] { CreateChoice(id: 10, nextSceneId: 901) });
        var destination = CreateScene(901);
        var quest = CreateQuest(questId: 1, associatedSceneIds: new[] { 901 });
        quest.RequiredFlags["elven_path"] = true;
        var scenes = new List<StoryScene> { scene, destination };

        var lockedQuests = new List<PlayerQuest>();
        var lockedState = new ScenarioState(CreateProgress());
        var lockedResult = _engine.SelectChoice(
            lockedState,
            scene,
            CreateCharacter(),
            choiceId: 10,
            scenes,
            quests: new List<Quest> { quest },
            lockedQuests);

        Assert.True(lockedResult.Success);
        Assert.Empty(lockedQuests);

        var unlockedQuests = new List<PlayerQuest>();
        var unlockedState = new ScenarioState(CreateProgress());
        unlockedState.Progress.StoryFlags["elven_path"] = true;
        var unlockedResult = _engine.SelectChoice(
            unlockedState,
            scene,
            CreateCharacter(),
            choiceId: 10,
            scenes,
            quests: new List<Quest> { quest },
            unlockedQuests);

        Assert.True(unlockedResult.Success);
        Assert.Single(unlockedQuests);
    }

    [Fact]
    public void SelectChoice_ReachingFinalScene_CompletesTheRun()
    {
        var scene = CreateScene(id: 900, choices: new[] { CreateChoice(id: 10, nextSceneId: 6108) });
        var finalScene = CreateScene(6108, isFinalScene: true);
        var state = new ScenarioState(CreateProgress());

        var result = _engine.SelectChoice(
            state,
            scene,
            CreateCharacter(),
            choiceId: 10,
            scenes: new List<StoryScene> { scene, finalScene },
            quests: Array.Empty<Quest>(),
            playerQuests: new List<PlayerQuest>());

        Assert.True(result.Success);
        Assert.True(result.Data!.StoryEnded);
        Assert.True(state.IsCompleted);
        Assert.Equal(6108, result.Data.NextScene!.Id);
    }

    [Fact]
    public void SelectChoice_ChoiceWithoutNextScene_EndsTheRun()
    {
        var scene = CreateScene(id: 900, choices: new[] { CreateChoice(id: 10, nextSceneId: null) });
        var state = new ScenarioState(CreateProgress());

        var result = _engine.SelectChoice(
            state,
            scene,
            CreateCharacter(),
            choiceId: 10,
            scenes: new List<StoryScene> { scene },
            quests: Array.Empty<Quest>(),
            playerQuests: new List<PlayerQuest>());

        Assert.True(result.Success);
        Assert.Null(result.Data!.NextScene);
        Assert.True(result.Data.StoryEnded);
        Assert.True(state.IsCompleted);
    }

    [Fact]
    public void SelectChoice_CompletedRun_Fails()
    {
        var scene = CreateScene(id: 900, choices: new[] { CreateChoice(id: 10, nextSceneId: 901) });
        var state = new ScenarioState(CreateProgress());
        state.Progress.IsCompleted = true;

        var result = _engine.SelectChoice(
            state,
            scene,
            CreateCharacter(),
            choiceId: 10,
            scenes: new List<StoryScene> { scene, CreateScene(901) },
            quests: Array.Empty<Quest>(),
            playerQuests: new List<PlayerQuest>());

        Assert.False(result.Success);
        Assert.Equal(EngineErrorCodes.ScenarioStoryCompleted, result.ErrorCode);
    }

    // --- MoveToNextScene ---

    [Fact]
    public void MoveToNextScene_MovesAndMarksVisited()
    {
        var state = new ScenarioState(CreateProgress());
        state.Progress.CurrentSceneId = 900;

        var result = _engine.MoveToNextScene(
            state,
            scenes: new List<StoryScene> { CreateScene(900), CreateScene(901) },
            nextSceneId: 901,
            CreateCharacter(),
            quests: Array.Empty<Quest>(),
            playerQuests: new List<PlayerQuest>());

        Assert.True(result.Success);
        Assert.Equal(901, result.Data!.Progress.CurrentSceneId);
        Assert.Contains(901, result.Data.VisitedSceneIds);
    }

    [Fact]
    public void MoveToNextScene_CompletedRun_Fails()
    {
        var state = new ScenarioState(CreateProgress());
        state.Progress.IsCompleted = true;

        var result = _engine.MoveToNextScene(
            state,
            scenes: new List<StoryScene> { CreateScene(901) },
            nextSceneId: 901,
            CreateCharacter(),
            quests: Array.Empty<Quest>(),
            playerQuests: new List<PlayerQuest>());

        Assert.False(result.Success);
        Assert.Equal(EngineErrorCodes.ScenarioStoryCompleted, result.ErrorCode);
    }

    [Fact]
    public void MoveToNextScene_UnknownDestination_Fails()
    {
        var result = _engine.MoveToNextScene(
            new ScenarioState(CreateProgress()),
            scenes: new List<StoryScene> { CreateScene(900) },
            nextSceneId: 999,
            CreateCharacter(),
            quests: Array.Empty<Quest>(),
            playerQuests: new List<PlayerQuest>());

        Assert.False(result.Success);
        Assert.Equal(EngineErrorCodes.ScenarioSceneNotFound, result.ErrorCode);
    }

    // --- Helpers ---

    private static ScenarioProgress CreateProgress(int gameSessionId = 1)
    {
        return new ScenarioProgress { GameSessionId = gameSessionId };
    }

    private static PlayerCharacter CreateCharacter(
        string name = "Alex",
        int raceId = 1,
        int classId = 1,
        int level = 3,
        int xp = 0)
    {
        return new PlayerCharacter
        {
            Name = name,
            RaceId = raceId,
            ClassId = classId,
            Level = level,
            CurrentXp = xp,
            MaxHealth = 10,
            CurrentHealth = 10,
            Strength = 10,
            Dexterity = 10,
            Intelligence = 10,
            Charisma = 10
        };
    }

    private static StoryScene CreateScene(
        int id,
        int act = 1,
        int chapter = 1,
        string title = "",
        bool isFinalScene = false,
        ICollection<StoryDialogue>? dialogues = null,
        ICollection<StoryChoice>? choices = null)
    {
        return new StoryScene
        {
            Id = id,
            Act = act,
            Chapter = chapter,
            Title = title,
            IsFinalScene = isFinalScene,
            Dialogues = dialogues ?? new List<StoryDialogue>(),
            Choices = choices ?? new List<StoryChoice>()
        };
    }

    private static StoryChoice CreateChoice(
        int id,
        string text = "Continua",
        int? nextSceneId = null,
        ICollection<ChoiceRequirement>? requirements = null,
        ICollection<ChoiceConsequence>? consequences = null)
    {
        return new StoryChoice
        {
            Id = id,
            Text = text,
            NextSceneId = nextSceneId,
            Requirements = requirements ?? new List<ChoiceRequirement>(),
            Consequences = consequences ?? new List<ChoiceConsequence>()
        };
    }

    private static ChoiceRequirement CreateRequirement(
        int? raceId = null,
        int? classId = null,
        int? minimumLevel = null,
        AttributeType? stat = null,
        int? minimumStat = null,
        int? requiredQuestId = null,
        int? minimumQuestProgress = null,
        int? requiredItemId = null)
    {
        return new ChoiceRequirement
        {
            RaceId = raceId,
            ClassId = classId,
            MinimumLevel = minimumLevel,
            Stat = stat,
            MinimumStat = minimumStat,
            RequiredQuestId = requiredQuestId,
            MinimumQuestProgress = minimumQuestProgress,
            RequiredItemId = requiredItemId
        };
    }

    private static Quest CreateQuest(int questId, int[] associatedSceneIds)
    {
        return new Quest
        {
            Id = questId,
            Code = $"Q-{questId}",
            AssociatedSceneIds = associatedSceneIds
        };
    }
}