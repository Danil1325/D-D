using DnDGame.Domain.Entities.Game;
using DnDGame.Domain.Enums;

namespace DnDGame.MockData.SeedData;

/// <summary>
/// All 29 optional quests from The_Crown_of_Ash_Interactive_Scenario.docx.
/// The regional partial files preserve source descriptions, encounters and EXP.
/// See docs/TheCrownOfAshSideQuestSeed.md for inferred balance values and outcome semantics.
/// </summary>
internal static partial class ScenarioSideQuestSeedData
{
    public static void Seed(InMemoryGameDataStore store)
    {
        SeedHerosOverlook(store);
        SeedMisthavenPort(store);
        SeedWhisperingWoods(store);
        SeedAshtonia(store);
        SeedOakheaven(store);
        SeedTheBonePeaks(store);
        SeedDarkstormKeep(store);
    }

    private static void AddQuest(
        InMemoryGameDataStore store, int id, string code, string title, string giver,
        string description, int level, string locationSlug, string encounters,
        int experience, string additionalRewards, QuestObjective[] objectives,
        QuestEnemy[] enemies, QuestOutcome[] successfulOutcomes,
        Dictionary<string, bool>? requiredFlags = null)
    {
        var location = store.Locations.Single(location => location.Slug == locationSlug);
        var flagPrefix = code.ToLowerInvariant().Replace('-', '_');
        var completionFlags = Flags(($"{flagPrefix}_completed", true), ($"{flagPrefix}_failed", false),
            ($"{flagPrefix}_unresolved", false));

        for (var index = 0; index < objectives.Length; index++)
            objectives[index].Id = id * 100 + index + 1;

        foreach (var outcome in successfulOutcomes)
        {
            foreach (var flag in completionFlags)
                outcome.ResultFlags.Add(flag.Key, flag.Value);
        }

        var outcomes = successfulOutcomes.ToList();
        // The source prescribes 40% EXP only when a failed objective moves the
        // story forward meaningfully. It prescribes no fixed failure penalties.
        foreach (var meaningfulFailure in new[] { false, true })
        {
            outcomes.Add(new QuestOutcome
            {
                Code = meaningfulFailure ? "meaningful-failure" : "failure",
                Description = meaningfulFailure
                    ? "The quest fails, but the story continues with a meaningful consequence."
                    : "The quest fails without a meaningful continuation reward.",
                Status = QuestStatus.Failed,
                ExperienceRewardPercentage = meaningfulFailure ? 40 : 0,
                RequiredFlags = Flags(($"{flagPrefix}_meaningful_failure", meaningfulFailure)),
                ResultFlags = Flags(($"{flagPrefix}_completed", false), ($"{flagPrefix}_failed", true),
                    ($"{flagPrefix}_unresolved", true))
            });
        }

        var quest = new Quest
        {
            Id = id,
            Code = code,
            Title = title,
            QuestGiver = giver,
            Description = description,
            QuestType = QuestType.Side,
            LocationId = location.Id,
            RecommendedLevel = level,
            RecommendedMaximumLevel = level,
            Objectives = objectives.ToList(),
            Enemies = enemies.ToList(),
            EncounterDescription = encounters,
            Rewards = new List<QuestReward> { new() { Experience = experience } },
            RewardDescription = additionalRewards,
            AdditionalRewards = additionalRewards.Split(';', StringSplitOptions.TrimEntries | StringSplitOptions.RemoveEmptyEntries).ToList(),
            RequiredFlags = requiredFlags ?? new(),
            ResultFlags = completionFlags,
            Outcomes = outcomes,
            NextQuestId = null,
            ScenarioNotes = "Recommended level is initial balancing, not specified per side quest in the source. " +
                "Select exactly one outcome; apply EXP once using its percentage. Optional combat and non-combat resolution share quest EXP. " +
                "Failure has no invented numeric penalty; unresolved regional quests may affect later story logic."
        };
        foreach (var flag in quest.RequiredFlags)
            quest.Prerequisites.Add(new QuestPrerequisite { RequiredFlag = flag.Key, RequiredFlagValue = flag.Value });

        if (code == "SQ-WW-04")
            quest.ScenarioNotes += " The freed-scout entry flag is inferred from the named quest giver and SQ-WW-01.";
        if (code == "SQ-AS-03")
            quest.ScenarioNotes += " The document leaves the corruption amount unspecified; null deliberately preserves that uncertainty.";
        if (code == "SQ-OH-03")
            quest.ScenarioNotes += " Safe rest prevents future Ash Clock increments; it does not subtract past Ash Clock.";

        store.Quests.Add(quest);
        location.AvailableSideQuestIds.Add(quest.Id);
    }

    private static QuestObjective Objective(
        string description, ObjectiveType type, string targetCode, int requiredAmount = 1) => new()
    {
        Description = description,
        ObjectiveType = type,
        TargetCode = targetCode,
        RequiredAmount = requiredAmount
    };

    private static QuestOutcome Outcome(
        string code, string description, Dictionary<string, bool>? requiredFlags = null,
        Dictionary<string, bool>? resultFlags = null, Dictionary<string, int>? loyalty = null,
        Dictionary<string, int>? counters = null, Dictionary<string, int>? items = null,
        Dictionary<string, bool>? allies = null, int warScore = 0, int? corruption = 0) => new()
    {
        Code = code,
        Description = description,
        RequiredFlags = requiredFlags ?? new(),
        ResultFlags = resultFlags ?? new(),
        CompanionLoyaltyChanges = loyalty ?? new(),
        CounterChanges = counters ?? new(),
        Items = items ?? new(),
        Allies = allies ?? new(),
        WarScoreChange = warScore,
        CorruptionChange = corruption
    };

    private static QuestEnemy Enemy(
        InMemoryGameDataStore store, string name, int? count = null, bool optional = false,
        string? requiredFlag = null, int? minimumAshClock = null)
    {
        // Named variants reuse the matching combat template. Narrative-only
        // opponents have no invented combat stats or fake enemy IDs.
        (EnemyFamily Family, EnemyTier Tier)? template = name switch
        {
            "Phantom" or "Drowned Phantoms" => (EnemyFamily.Wraith, EnemyTier.Base),
            "Wraith" => (EnemyFamily.Wraith, EnemyTier.Evolved),
            "Dread Wraith" => (EnemyFamily.Wraith, EnemyTier.Elite),
            "Slime" => (EnemyFamily.Slime, EnemyTier.Base),
            "Great Slime" => (EnemyFamily.Slime, EnemyTier.Evolved),
            "King Slime" => (EnemyFamily.Slime, EnemyTier.Elite),
            "Skeleton" => (EnemyFamily.Skeleton, EnemyTier.Base),
            "Skeleton Knight" => (EnemyFamily.Skeleton, EnemyTier.Evolved),
            "Hobgoblin" or "Possessed Hobgoblins" => (EnemyFamily.Goblin, EnemyTier.Evolved),
            "Demon" => (EnemyFamily.Demon, EnemyTier.Base),
            "Greater Demon" or "Greater Demon smith" or "Greater Demon jailer" or "Greater Demon ritualist" or "Karnyx"
                => (EnemyFamily.Demon, EnemyTier.Evolved),
            "Chimera" => (EnemyFamily.Chimera, EnemyTier.Base),
            "Great Chimera" or "Great Chimera aspects" => (EnemyFamily.Chimera, EnemyTier.Evolved),
            "Divine Chimera" => (EnemyFamily.Chimera, EnemyTier.Elite),
            "Troll" => (EnemyFamily.Troll, EnemyTier.Base),
            "Warrior Troll" => (EnemyFamily.Troll, EnemyTier.Evolved),
            _ => null
        };
        return new QuestEnemy
        {
            Name = name,
            Code = name.ToLowerInvariant().Replace(' ', '-'),
            EnemyId = template is { } type
                ? store.Enemies.Single(enemy => enemy.Family == type.Family && enemy.Tier == type.Tier).Id
                : null,
            Count = count,
            IsOptional = optional,
            MinimumAshClock = minimumAshClock,
            RequiredFlags = requiredFlag is null ? new() : Flags((requiredFlag, true))
        };
    }

    private static Dictionary<string, bool> Flags(params (string Code, bool Value)[] flags) =>
        flags.ToDictionary(flag => flag.Code, flag => flag.Value);

    private static Dictionary<string, int> Values(params (string Code, int Value)[] values) =>
        values.ToDictionary(value => value.Code, value => value.Value);
}
