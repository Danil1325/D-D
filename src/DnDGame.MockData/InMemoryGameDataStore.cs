using DnDGame.Domain.Entities.Accounts;
using DnDGame.Domain.Entities.Achievements;
using DnDGame.Domain.Entities.Characters;
using DnDGame.Domain.Entities.Cards;
using DnDGame.Domain.Entities.Classes;
using DnDGame.Domain.Entities.Enemies;
using DnDGame.Domain.Entities.Game;
using DnDGame.Domain.Entities.Locations;
using DnDGame.Domain.Entities.Portraits;
using DnDGame.Domain.Entities.Races;
using DnDGame.Domain.Entities.Skills;
using DnDGame.Domain.Entities.Talents;

namespace DnDGame.MockData;

/// <summary>
/// The in-memory "database" for the mock-data phase. One instance of this is
/// registered as a DI singleton and shared by every Mock*Repository, so data
/// written by one request (e.g. creating a character) is still there for the
/// next request — exactly like a real database would behave, just without one.
///
/// Storage here is deliberately flat and normalized (one list per entity type,
/// linked only by the same FK ints the Domain entities already carry) rather than
/// pre-linked object graphs. Mock*Repository classes are responsible for "joining"
/// related rows together before returning them — this mirrors what EF Core's
/// .Include() will do automatically once DataAccessLayer replaces this in Phase 7.
/// </summary>
public class InMemoryGameDataStore
{
    // --- Reference data (seeded once at startup) ---
    public List<Race> Races { get; } = new();
    public List<RaceTrait> RaceTraits { get; } = new();
    public List<RaceAttributeRange> RaceAttributeRanges { get; } = new();
    public List<CharacterClass> Classes { get; } = new();
    public List<ClassFeature> ClassFeatures { get; } = new();
    public List<CharacterPortrait> Portraits { get; } = new();
    public List<Talent> Talents { get; } = new();
    public List<Enemy> Enemies { get; } = new();
    public List<Location> Locations { get; } = new();
    public List<LocationDefinition> LocationDefinitions { get; } = new();
    public List<LocationEncounterDefinition> LocationEncounterDefinitions { get; } = new();
    public List<Quest> Quests { get; } = new();
    public List<StoryScene> StoryScenes { get; } = new();
    public List<Adventure> Adventures { get; } = new();
    public List<StoryNode> StoryNodes { get; } = new();
    public List<Choice> Choices { get; } = new();
    public List<Card> Cards { get; } = new();
    public List<Achievement> Achievements { get; } = new();
    public List<SkillDefinition> SkillDefinitions { get; } = new();

    // --- Runtime / gameplay data (empty until Phase 3 starts creating characters) ---
    public List<PlayerCharacter> Characters { get; } = new();
    public List<CharacterTalent> CharacterTalents { get; } = new();
    public List<GameSession> GameSessions { get; } = new();
    public List<SessionLogEntry> SessionLogEntries { get; } = new();

    // --- Runtime / gameplay data (empty until a PlayerCharacter unlocks or is given a card) ---
    public List<CardCollection> CardCollections { get; } = new();

    // --- Runtime / gameplay data (empty until a PlayerCharacter builds a deck) ---
    public List<Deck> Decks { get; } = new();

    // --- Runtime / gameplay data (empty until a battle is started) ---
    public List<Battle> Battles { get; } = new();
    public List<BattleDeck> BattleDecks { get; } = new();

    // --- Runtime / gameplay data (empty until a quest is started by the scenario
    // engine or the quest service) ---
    public List<PlayerQuest> PlayerQuests { get; } = new();
    public List<ScenarioProgress> ScenarioProgresses { get; } = new();

    // --- Runtime / gameplay data (empty until a quest completion unlocks a
    // location for a player — see QuestService.EvaluateLocationUnlocksAsync) ---
    public List<LocationProgress> LocationProgresses { get; } = new();

    // --- Runtime / gameplay data (empty until a game event advances an
    // achievement — see QuestService/BattleService/CharacterService hooks) ---
    public List<AchievementProgress> AchievementProgresses { get; } = new();

    // --- Runtime / gameplay data (empty until the first real game event is
    // counted toward an achievement — the exactly-once ledger) ---
    public List<AchievementEvent> AchievementEvents { get; } = new();

    // --- Runtime / gameplay data (empty until a PlayerCharacter unlocks a skill
    // — see SkillService.UnlockSkillForCurrentPlayerAsync) ---
    public List<CharacterSkillUnlock> CharacterSkillUnlocks { get; } = new();

    // --- Auth data (empty until someone registers). Deliberately not linked to
    // Characters — there is no Account-to-PlayerCharacter relationship yet.
    public List<Account> Accounts { get; } = new();

    // Id counters only for the runtime tables above — every reference-data row gets
    // an explicit, hardcoded Id from its seed data instead, so cross-references
    // between seed files (e.g. a Choice pointing at an Enemy) are predictable.
    private int _nextCharacterId = 1;
    private int _nextCharacterTalentId = 1;
    private int _nextGameSessionId = 1;
    private int _nextLogEntryId = 1;
    private int _nextDeckId = 1;
    private int _nextBattleId = 1;
    private int _nextBattleDeckId = 1;
    private int _nextAccountId = 1;
    private int _nextPlayerQuestId = 1;
    private int _nextScenarioProgressId = 1;

    public int GetNextCharacterId() => _nextCharacterId++;
    public int GetNextCharacterTalentId() => _nextCharacterTalentId++;
    public int GetNextGameSessionId() => _nextGameSessionId++;
    public int GetNextLogEntryId() => _nextLogEntryId++;
    public int GetNextDeckId() => _nextDeckId++;
    public int GetNextBattleId() => _nextBattleId++;
    public int GetNextBattleDeckId() => _nextBattleDeckId++;
    public int GetNextAccountId() => _nextAccountId++;
    public int GetNextPlayerQuestId() => _nextPlayerQuestId++;
    public int GetNextScenarioProgressId() => _nextScenarioProgressId++;
}
