using DnDGame.Domain.Entities.Game;
using DnDGame.Domain.Enums;
using DnDGame.MockData;

namespace DnDGame.Tests.MockData;

public class ScenarioSideQuestSeedDataTests
{
    private static Quest GetQuest(string code) =>
        MockDataBootstrapper.CreateSeededStore().Quests.Single(quest => quest.Code == code);

    [Theory]
    [InlineData("heros-overlook", "SQ-HO-", 3)]
    [InlineData("misthaven-port", "SQ-MP-", 5)]
    [InlineData("whispering-woods", "SQ-WW-", 4)]
    [InlineData("ashtonia", "SQ-AS-", 4)]
    [InlineData("oakheaven", "SQ-OH-", 4)]
    [InlineData("the-bone-peaks", "SQ-BP-", 5)]
    [InlineData("darkstorm-keep", "SQ-DK-", 4)]
    public void RegionalCatalogues_ContainAllSourceQuestsAndResolveLocationLinks(
        string slug, string prefix, int count)
    {
        var store = MockDataBootstrapper.CreateSeededStore();
        var location = store.Locations.Single(location => location.Slug == slug);
        var quests = store.Quests.Where(quest => quest.Code.StartsWith(prefix)).ToList();

        Assert.Equal(count, quests.Count);
        Assert.Equal(count, location.AvailableSideQuestIds.Count);
        Assert.Equal(count, location.AvailableSideQuestIds.Distinct().Count());
        foreach (var quest in quests)
        {
            Assert.Equal(location.Id, quest.LocationId);
            Assert.Contains(quest.Id, location.AvailableSideQuestIds);
            Assert.DoesNotContain(quest.Id, location.AvailableMainQuestIds);
        }
    }

    [Fact]
    public void SideQuests_AreOptionalWithoutBecomingMainQuestPrerequisites()
    {
        var store = MockDataBootstrapper.CreateSeededStore();
        var sideQuests = store.Quests.Where(quest => quest.QuestType == QuestType.Side).ToList();
        var sideIds = sideQuests.Select(quest => quest.Id).ToHashSet();
        Assert.Equal(29, sideQuests.Count);
        Assert.Equal(44, store.Quests.Count);
        Assert.Equal(44, store.Quests.Select(quest => quest.Id).Distinct().Count());
        Assert.Equal(44, store.Quests.Select(quest => quest.Code).Distinct().Count());

        foreach (var quest in sideQuests)
        {
            Assert.True(quest.IsOptional);
            Assert.Null(quest.NextQuestId);
            Assert.NotEmpty(quest.QuestGiver);
            Assert.NotEmpty(quest.Description);
            Assert.NotEmpty(quest.Objectives);
            Assert.NotEmpty(quest.Enemies);
            Assert.NotEmpty(quest.AdditionalRewards);
            Assert.InRange(quest.RecommendedLevel, 1, 10);
            Assert.Contains("not specified", quest.ScenarioNotes);
        }
        foreach (var quest in store.Quests.Where(quest => quest.QuestType == QuestType.Main))
        {
            Assert.False(quest.IsOptional);
            Assert.DoesNotContain(quest.Prerequisites,
                prerequisite => prerequisite.RequiredQuestId is int id && sideIds.Contains(id));
        }
    }

    [Theory]
    [InlineData("SQ-HO-01", 80)]
    [InlineData("SQ-HO-02", 100)]
    [InlineData("SQ-HO-03", 90)]
    [InlineData("SQ-MP-01", 110)]
    [InlineData("SQ-MP-02", 100)]
    [InlineData("SQ-MP-03", 130)]
    [InlineData("SQ-MP-04", 90)]
    [InlineData("SQ-MP-05", 150)]
    [InlineData("SQ-WW-01", 120)]
    [InlineData("SQ-WW-02", 100)]
    [InlineData("SQ-WW-03", 140)]
    [InlineData("SQ-WW-04", 170)]
    [InlineData("SQ-AS-01", 130)]
    [InlineData("SQ-AS-02", 120)]
    [InlineData("SQ-AS-03", 150)]
    [InlineData("SQ-AS-04", 170)]
    [InlineData("SQ-OH-01", 110)]
    [InlineData("SQ-OH-02", 130)]
    [InlineData("SQ-OH-03", 150)]
    [InlineData("SQ-OH-04", 100)]
    [InlineData("SQ-BP-01", 140)]
    [InlineData("SQ-BP-02", 160)]
    [InlineData("SQ-BP-03", 180)]
    [InlineData("SQ-BP-04", 170)]
    [InlineData("SQ-BP-05", 190)]
    [InlineData("SQ-DK-01", 190)]
    [InlineData("SQ-DK-02", 220)]
    [InlineData("SQ-DK-03", 210)]
    [InlineData("SQ-DK-04", 230)]
    public void ExperienceRewards_MatchTheSource(string code, int experience)
    {
        var quest = GetQuest(code);
        Assert.Equal(experience, quest.ExperienceReward);
        Assert.Equal(experience, Assert.Single(quest.Rewards).Experience);
        Assert.All(quest.Outcomes.Where(outcome => outcome.Status == QuestStatus.Completed),
            outcome => Assert.Equal(100, outcome.ExperienceRewardPercentage));
    }

    [Fact]
    public void FailureOutcomes_OnlyGrantPartialExperienceForMeaningfulContinuation()
    {
        var quests = MockDataBootstrapper.CreateSeededStore().Quests.Where(quest => quest.IsOptional);
        foreach (var quest in quests)
        {
            var failure = Assert.Single(quest.Outcomes, outcome => outcome.Code == "failure");
            var meaningful = Assert.Single(quest.Outcomes, outcome => outcome.Code == "meaningful-failure");
            Assert.Equal(0, failure.ExperienceRewardPercentage);
            Assert.Equal(40, meaningful.ExperienceRewardPercentage);
            var condition = Assert.Single(meaningful.RequiredFlags);
            Assert.True(condition.Value);
            Assert.False(failure.RequiredFlags[condition.Key]);
            foreach (var outcome in new[] { failure, meaningful })
            {
                Assert.Equal(QuestStatus.Failed, outcome.Status);
                Assert.Empty(outcome.Items);
                Assert.Empty(outcome.Allies);
                Assert.Empty(outcome.CompanionLoyaltyChanges);
                Assert.Equal(0, outcome.WarScoreChange);
                Assert.Equal(0, outcome.AshClockChange);
                Assert.Equal(0, outcome.CorruptionChange);
                var completedFlag = Assert.Single(quest.ResultFlags, flag => flag.Key.EndsWith("_completed"));
                Assert.False(outcome.ResultFlags[completedFlag.Key]);
                Assert.True(outcome.ResultFlags.Single(flag => flag.Key.EndsWith("_failed")).Value);
            }
        }
    }

    [Fact]
    public void EnemyTemplates_ResolveWithoutFabricatedIdsForNarrativeOpponents()
    {
        var store = MockDataBootstrapper.CreateSeededStore();
        var enemies = store.Quests.Where(quest => quest.IsOptional).SelectMany(quest => quest.Enemies).ToList();
        foreach (var enemy in enemies)
        {
            Assert.NotEmpty(enemy.Name);
            Assert.NotEmpty(enemy.Code);
            if (enemy.EnemyId is int id)
                Assert.Contains(store.Enemies, template => template.Id == id);
            else
                Assert.Contains(enemy.Name, new[] { "Animated books", "Guild enforcers", "Guild guards", "Hunters", "Possessed guards", "Fellowship echoes" });
            if (enemy.Count.HasValue)
                Assert.True(enemy.Count.Value > 0);
        }

        var phantom = GetQuest("SQ-HO-01").Enemies.Single(enemy => enemy.Name == "Phantom");
        Assert.Equal(3, phantom.Count);
        var wraith = GetQuest("SQ-HO-01").Enemies.Single(enemy => enemy.Name == "Wraith");
        Assert.True(wraith.IsOptional);
        Assert.True(wraith.RequiredFlags["inscription_read_aloud"]);
        var slime = GetQuest("SQ-OH-01").Enemies.Single(enemy => enemy.Name == "King Slime");
        Assert.Equal(1, slime.Count);
        Assert.Equal(4, slime.MinimumAshClock);
        Assert.Null(GetQuest("SQ-WW-03").Enemies.Single().Count);
    }

    [Fact]
    public void NonCombatSolutions_AreRetainedInsteadOfMandatoryKills()
    {
        foreach (var code in new[] { "SQ-MP-04", "SQ-WW-04", "SQ-BP-01", "SQ-DK-02", "SQ-DK-03" })
        {
            var quest = GetQuest(code);
            Assert.DoesNotContain(quest.Objectives, objective => objective.ObjectiveType == ObjectiveType.DefeatEnemies);
            Assert.Contains(quest.Enemies, enemy => enemy.IsOptional);
        }
    }

    [Fact]
    public void DivineChimera_OutcomesDoNotGrantBothAllyAndMaterial()
    {
        var quest = GetQuest("SQ-DK-02");
        var restored = quest.Outcomes.Single(outcome => outcome.Code == "restored");
        var defeated = quest.Outcomes.Single(outcome => outcome.Code == "defeated");

        Assert.True(restored.RequiredFlags["divine_chimera_restored"]);
        Assert.False(defeated.RequiredFlags["divine_chimera_restored"]);
        Assert.True(restored.Allies["divine-chimera"]);
        Assert.Empty(restored.Items);
        Assert.Equal(1, defeated.Items["legendary-material"]);
        Assert.Empty(defeated.Allies);
    }

    [Theory]
    [InlineData("SQ-AS-02", "drum-preserved", "drum-not-preserved")]
    [InlineData("SQ-DK-03", "truce", "no-truce")]
    public void WarScoreBonus_RequiresTheSpecifiedResolution(string code, string positiveCode, string otherCode)
    {
        var quest = GetQuest(code);
        var positive = quest.Outcomes.Single(outcome => outcome.Code == positiveCode);
        var other = quest.Outcomes.Single(outcome => outcome.Code == otherCode);
        Assert.Equal(1, positive.WarScoreChange);
        Assert.Equal(0, other.WarScoreChange);
        var condition = Assert.Single(positive.RequiredFlags);
        Assert.True(condition.Value);
        Assert.False(other.RequiredFlags[condition.Key]);
    }

    [Fact]
    public void CorruptionAndSafeRest_PreserveSourceSemantics()
    {
        foreach (var code in new[] { "SQ-WW-03", "SQ-BP-03" })
            Assert.Equal(-1, GetQuest(code).Outcomes.Single(outcome => outcome.Status == QuestStatus.Completed).CorruptionChange);

        Assert.All(GetQuest("SQ-AS-03").Outcomes.Where(outcome => outcome.Status == QuestStatus.Completed),
            outcome => Assert.Null(outcome.CorruptionChange));

        var safeRest = GetQuest("SQ-OH-03").Outcomes.Single(outcome => outcome.Status == QuestStatus.Completed);
        Assert.True(safeRest.ResultFlags["oakheaven_rest_without_ash_clock"]);
        Assert.Equal(0, safeRest.AshClockChange);
    }

    [Theory]
    [InlineData("SQ-HO-03", "chosen-companion", 1)]
    [InlineData("SQ-MP-01", "odessa-vray", 1)]
    [InlineData("SQ-MP-03", "lira-sunstroke", 1)]
    [InlineData("SQ-MP-05", "odessa-vray", 2)]
    [InlineData("SQ-WW-04", "silvaneth", 1)]
    [InlineData("SQ-AS-01", "mother-sereh", 1)]
    [InlineData("SQ-OH-02", "kregg", 1)]
    [InlineData("SQ-BP-01", "ser-halbrecht", 1)]
    [InlineData("SQ-BP-05", "bram-ironjaw", 1)]
    public void LoyaltyChanges_MatchNamedCompanionRewards(string code, string companion, int amount)
    {
        Assert.All(GetQuest(code).Outcomes.Where(outcome => outcome.Status == QuestStatus.Completed),
            outcome => Assert.Equal(amount, outcome.CompanionLoyaltyChanges[companion]));
    }

    [Fact]
    public void AllianceRoutes_DoNotPrematurelyRecruitFinaleArmies()
    {
        var troll = GetQuest("SQ-BP-02").Outcomes.Single(outcome => outcome.Status == QuestStatus.Completed);
        Assert.True(troll.ResultFlags["troll_alliance_route"]);
        Assert.Empty(troll.Allies);
        var goblin = GetQuest("SQ-OH-02").Outcomes.Single(outcome => outcome.Status == QuestStatus.Completed);
        Assert.True(goblin.ResultFlags["goblin_alliance_route"]);
        Assert.Empty(goblin.Allies);
        var fleet = GetQuest("SQ-MP-05").Outcomes.Single(outcome => outcome.Status == QuestStatus.Completed);
        Assert.True(fleet.Allies["odessa-fleet"]);
        Assert.Equal(0, fleet.WarScoreChange);
    }

[Theory]
    [InlineData("SQ-HO-02", "healing-supply", 2)]
    [InlineData("SQ-HO-03", "memorial-charm", 1)]
    [InlineData("SQ-MP-03", "arcane-item", 1)]
    [InlineData("SQ-AS-01", "fire-resistance-supply", 1)]
    [InlineData("SQ-AS-04", "sealing-ember", 1)]
    [InlineData("SQ-BP-05", "sonic-protection-item", 1)]
    [InlineData("SQ-DK-02", "legendary-material", 1)]
    public void ItemRewards_UseSourceQuantities(string code, string item, int quantity)
    {
        var itemOutcomes = GetQuest(code).Outcomes
            .Where(outcome => outcome.Status == QuestStatus.Completed && outcome.Items.ContainsKey(item))
            .ToList();
        Assert.NotEmpty(itemOutcomes);
        Assert.All(itemOutcomes, outcome => Assert.Equal(quantity, outcome.Items[item]));
        Assert.DoesNotContain(itemOutcomes, outcome => outcome.Items.Count != 1);
    }

    [Fact]
    public void SourceCountedRewards_AreStructuredCounterChanges()
    {
        var trust = GetQuest("SQ-HO-01").Outcomes.Single(outcome => outcome.Code == "trust-aldwyn");
        Assert.Equal(1, trust.CounterChanges["trust_aldwyn"]);
        var suspicion = GetQuest("SQ-HO-01").Outcomes.Single(outcome => outcome.Code == "suspect-aldwyn");
        Assert.Equal(1, suspicion.CounterChanges["suspicion"]);
        Assert.Equal(1, GetQuest("SQ-OH-01").Outcomes.Single(outcome => outcome.Status == QuestStatus.Completed).CounterChanges["militia_health"]);
        Assert.Equal(2, GetQuest("SQ-DK-01").Outcomes.Single(outcome => outcome.Status == QuestStatus.Completed).CounterChanges["final_defense_survivors"]);
    }

    [Fact]
    public void AlternativeChoiceOutcomes_AreMutuallyExclusiveSplits()
    {
        var mp02 = GetQuest("SQ-MP-02");
        var recruited = mp02.Outcomes.Single(outcome => outcome.Code == "recruited");
        var notRecruited = mp02.Outcomes.Single(outcome => outcome.Code == "not-recruited");
        Assert.True(recruited.RequiredFlags["hobgoblin_recruited"]);
        Assert.False(notRecruited.RequiredFlags["hobgoblin_recruited"]);
        Assert.True(recruited.Allies["hobgoblin-scout"]);
        Assert.Empty(notRecruited.Allies);

        var ww02 = GetQuest("SQ-WW-02");
        var protectedOutcome = ww02.Outcomes.Single(outcome => outcome.Code == "pip-protected");
        var unprotected = ww02.Outcomes.Single(outcome => outcome.Code == "pip-unprotected");
        Assert.True(protectedOutcome.RequiredFlags["pip_protected"]);
        Assert.False(unprotected.RequiredFlags["pip_protected"]);
        Assert.True(protectedOutcome.ResultFlags["pip_merchant_available"]);
        Assert.False(unprotected.ResultFlags.ContainsKey("pip_merchant_available"));

        var as03 = GetQuest("SQ-AS-03");
        Assert.True(as03.Outcomes.Single(outcome => outcome.Code == "recruit").Allies["irix-informant"]);
        Assert.All(as03.Outcomes.Where(outcome => outcome.Status == QuestStatus.Completed),
            outcome => Assert.False(outcome.Allies.ContainsKey("irix-informant") && outcome.Code != "recruit"));
    }

    [Fact]
    public void ScoutQuestDependency_IsProducedByTheRescueOutcome()
    {
        var rescue = GetQuest("SQ-WW-01").Outcomes.Single(outcome => outcome.Status == QuestStatus.Completed);
        var followup = GetQuest("SQ-WW-04");
        Assert.True(rescue.ResultFlags["elf_scout_freed"]);
        Assert.True(followup.RequiredFlags["elf_scout_freed"]);
        Assert.Equal("elf_scout_freed", Assert.Single(followup.Prerequisites).RequiredFlag);
    }

    [Fact]
    public void MutableOutcomeData_IsNotSharedBetweenStoresOrBranches()
    {
        var first = GetQuest("SQ-DK-02");
        var second = GetQuest("SQ-DK-02");
        first.Outcomes.Single(outcome => outcome.Code == "restored").Allies.Clear();
        Assert.NotEmpty(second.Outcomes.Single(outcome => outcome.Code == "restored").Allies);
        first.Outcomes.Single(outcome => outcome.Code == "failure").ResultFlags.Clear();
        Assert.NotEmpty(first.Outcomes.Single(outcome => outcome.Code == "meaningful-failure").ResultFlags);
    }
}
