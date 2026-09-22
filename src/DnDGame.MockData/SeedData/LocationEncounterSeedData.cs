using DnDGame.Domain.Entities.Locations;
using DnDGame.Domain.Enums;

namespace DnDGame.MockData.SeedData;

/// <summary>
/// Seeds the per-location enemy encounter pools (BACK-LOC-05), one
/// LocationEncounterDefinition per LocationId. References Enemy rows from
/// EnemySeedData by Id, including the 6 named campaign bosses (Ids 22-27) and
/// Kregg the Sundered (Id 28) added there for this feature — see EnemySeedData's
/// doc comment; their stats are placeholders, not final.
///
/// BACK-LOC-06 wires the "an NPC that became an ally is never an enemy" rule onto
/// the specific bosses named in that task, reusing the real, persisted story flags
/// from ScenarioSideQuestSeedData rather than inventing new ones:
///   - Kregg (id 28) and Hobgoblin at Oakheaven, and Grommash-Vurr at The Bone
///     Peaks, require their family's alliance route flag to be false (i.e. the
///     diplomatic route has not been opened) — see GoblinAllianceRouteFlag/
///     TrollAllianceRouteFlag. Goblin (id 4, the generic raider, distinct from
///     Kregg himself) shares the same gate.
///   - Divine Chimera at Darkstorm Keep requires the real "divine-chimera" ally
///     code (DivineChimeraAllyFlag) to be false. QuestService now persists
///     QuestOutcome.Allies into ScenarioProgress.StoryFlags on quest completion
///     (see QuestService.ApplyOutcomeEffectsAsync), so this is a live, functional
///     gate, not a dead flag.
/// </summary>
internal static class LocationEncounterSeedData
{
    /// <summary>Set true by SQ-OH-02's "banner-returned" outcome (ScenarioSideQuestSeedData.Oakheaven.cs).</summary>
    private const string GoblinAllianceRouteFlag = "goblin_alliance_route";

    /// <summary>Set true by SQ-BP's troll-duel outcome (ScenarioSideQuestSeedData.TheBonePeaks.cs).</summary>
    private const string TrollAllianceRouteFlag = "troll_alliance_route";

    /// <summary>
    /// The real QuestOutcome.Allies code set by SQ-DK-02's "restored" outcome
    /// (ScenarioSideQuestSeedData.DarkstormKeep.cs), persisted as a story flag by
    /// QuestService.ApplyOutcomeEffectsAsync.
    /// </summary>
    private const string DivineChimeraAllyFlag = "divine-chimera";

    /// <summary>Mirrors Hero's Overlook's own LocationDefinition.SpecialFlags entry (LocationSeedData.cs).</summary>
    private const string FinaleUnlockedFlag = "FinaleUnlocked";

    /// <summary>
    /// SQ-MP-02 "Nine Rings, No Champion" (quest id 1005): Arena Master Kael's
    /// Hobgoblin challenger. The quest's own QuestEnemy handles the scripted duel;
    /// this ties the arena's separate, roamable Hobgoblin random encounter to the
    /// same story window so it only appears while that questline is actually live.
    /// </summary>
    private const int ArenaHobgoblinChallengeQuestId = 1005;

    private static readonly string[] MisthavenPortSubLocations = { "arena", "archives", "port", "wrecks" };

    public static void Seed(InMemoryGameDataStore store)
    {
        store.LocationEncounterDefinitions.AddRange(new[]
        {
            new LocationEncounterDefinition
            {
                Id = 1,
                LocationId = LocationId.Ashtonia,
                Enemies = new List<EncounterEnemyDefinition>
                {
                    new() { EnemyId = 16, Tier = EncounterTier.Normal }, // Demon
                    new() { EnemyId = 13, Tier = EncounterTier.Normal }, // Chimera
                    new() { EnemyId = 7, Tier = EncounterTier.Normal },  // Slime
                    new() { EnemyId = 17, Tier = EncounterTier.Elite },  // Greater Demon
                    new() { EnemyId = 14, Tier = EncounterTier.Elite },  // Great Chimera
                    new() { EnemyId = 24, Tier = EncounterTier.Boss }    // Karnyx
                }
            },

            new LocationEncounterDefinition
            {
                Id = 2,
                LocationId = LocationId.DarkstormKeep,
                Enemies = new List<EncounterEnemyDefinition>
                {
                    new() { EnemyId = 2, Tier = EncounterTier.Normal },  // Skeleton Knight
                    new() { EnemyId = 19, Tier = EncounterTier.Normal }, // Phantom
                    new() { EnemyId = 20, Tier = EncounterTier.Normal }, // Wraith
                    new() { EnemyId = 11, Tier = EncounterTier.Normal }, // Warrior Troll
                    new() { EnemyId = 16, Tier = EncounterTier.Normal }, // Demon
                    new() { EnemyId = 21, Tier = EncounterTier.Elite },  // Dread Wraith
                    new() { EnemyId = 9, Tier = EncounterTier.Elite },   // King Slime
                    new() { EnemyId = 17, Tier = EncounterTier.Elite },  // Greater Demon
                    new() { EnemyId = 14, Tier = EncounterTier.Elite },  // Great Chimera
                    new()
                    {
                        EnemyId = 15, Tier = EncounterTier.Boss, // Divine Chimera
                        Availability = new EncounterAvailabilityRequirement { RequiredStoryFlags = new() { [DivineChimeraAllyFlag] = false } }
                    },
                    new() { EnemyId = 24, Tier = EncounterTier.Boss },   // Karnyx
                    new() { EnemyId = 22, Tier = EncounterTier.Boss },   // The Herald
                    new() { EnemyId = 23, Tier = EncounterTier.Boss }    // Vharruk
                }
            },

            new LocationEncounterDefinition
            {
                Id = 3,
                LocationId = LocationId.HerosOverlook,
                Enemies = new List<EncounterEnemyDefinition>
                {
                    new() { EnemyId = 19, Tier = EncounterTier.Normal }, // Phantom
                    new() { EnemyId = 1, Tier = EncounterTier.Normal },  // Skeleton
                    new() { EnemyId = 4, Tier = EncounterTier.Normal },  // Goblin
                    new() { EnemyId = 20, Tier = EncounterTier.Elite },  // Wraith
                    new() { EnemyId = 2, Tier = EncounterTier.Elite },   // Skeleton Knight
                    new()
                    {
                        // Final bosses do not appear at Hero's Overlook during the prologue —
                        // gated on the same FinaleUnlocked flag this location's own
                        // LocationDefinition.SpecialFlags already exposes (LocationSeedData.cs).
                        EnemyId = 22, Tier = EncounterTier.Boss, // The Herald
                        Availability = new EncounterAvailabilityRequirement { RequiredStoryFlags = new() { [FinaleUnlockedFlag] = true } }
                    },
                    new()
                    {
                        EnemyId = 23, Tier = EncounterTier.Boss, // Vharruk
                        Availability = new EncounterAvailabilityRequirement { RequiredStoryFlags = new() { [FinaleUnlockedFlag] = true } }
                    }
                }
            },

            // No mandatory boss; encounters restricted to arena/archives/port/wrecks.
            new LocationEncounterDefinition
            {
                Id = 4,
                LocationId = LocationId.MisthavenPort,
                Enemies = new List<EncounterEnemyDefinition>
                {
                    new() { EnemyId = 7, Tier = EncounterTier.Normal, Availability = MisthavenPortAvailability() },  // Slime
                    new() { EnemyId = 19, Tier = EncounterTier.Normal, Availability = MisthavenPortAvailability() }, // Phantom
                    new()
                    {
                        EnemyId = 5, Tier = EncounterTier.Normal, // Hobgoblin (Arena Master Kael's challenger)
                        Availability = new EncounterAvailabilityRequirement
                        {
                            RequiredSubLocations = MisthavenPortSubLocations.ToList(),
                            RequiredActiveQuestId = ArenaHobgoblinChallengeQuestId
                        }
                    },
                    new() { EnemyId = 8, Tier = EncounterTier.Elite, Availability = MisthavenPortAvailability() },   // Great Slime
                    new() { EnemyId = 20, Tier = EncounterTier.Elite, Availability = MisthavenPortAvailability() },  // Wraith
                    new() { EnemyId = 14, Tier = EncounterTier.Elite, Availability = MisthavenPortAvailability() }   // Great Chimera
                }
            },

            // Goblin/Hobgoblin only appear once the local alliance has failed.
            new LocationEncounterDefinition
            {
                Id = 5,
                LocationId = LocationId.Oakheaven,
                Enemies = new List<EncounterEnemyDefinition>
                {
                    new() { EnemyId = 16, Tier = EncounterTier.Normal }, // Demon
                    new() { EnemyId = 7, Tier = EncounterTier.Normal },  // Slime
                    new()
                    {
                        EnemyId = 4, Tier = EncounterTier.Normal, // Goblin
                        Availability = new EncounterAvailabilityRequirement { RequiredStoryFlags = new() { [GoblinAllianceRouteFlag] = false } }
                    },
                    new()
                    {
                        EnemyId = 5, Tier = EncounterTier.Normal, // Hobgoblin
                        Availability = new EncounterAvailabilityRequirement { RequiredStoryFlags = new() { [GoblinAllianceRouteFlag] = false } }
                    },
                    new()
                    {
                        EnemyId = 28, Tier = EncounterTier.Normal, // Kregg the Sundered
                        Availability = new EncounterAvailabilityRequirement { RequiredStoryFlags = new() { [GoblinAllianceRouteFlag] = false } }
                    },
                    new() { EnemyId = 17, Tier = EncounterTier.Elite }, // Greater Demon
                    new() { EnemyId = 8, Tier = EncounterTier.Elite },  // Great Slime
                    new() { EnemyId = 20, Tier = EncounterTier.Elite }, // Wraith
                    new() { EnemyId = 27, Tier = EncounterTier.Boss },  // Greater Demon Mayor
                    new()
                    {
                        // King Slime is an optional boss; reuses the same Ash Clock 4+ threshold
                        // already established for this exact enemy at Oakheaven in
                        // ScenarioSideQuestSeedData.Oakheaven.cs ("Slimes and one King Slime if Ash Clock is 4+").
                        EnemyId = 9, Tier = EncounterTier.Boss,
                        Availability = new EncounterAvailabilityRequirement { MinimumAshClock = 4 }
                    }
                }
            },

            new LocationEncounterDefinition
            {
                Id = 6,
                LocationId = LocationId.TheBonePeaks,
                Enemies = new List<EncounterEnemyDefinition>
                {
                    new() { EnemyId = 1, Tier = EncounterTier.Normal },  // Skeleton
                    new() { EnemyId = 10, Tier = EncounterTier.Normal }, // Troll
                    new() { EnemyId = 7, Tier = EncounterTier.Normal },  // Slime
                    new() { EnemyId = 2, Tier = EncounterTier.Elite },   // Skeleton Knight
                    new() { EnemyId = 11, Tier = EncounterTier.Elite },  // Warrior Troll
                    new() { EnemyId = 8, Tier = EncounterTier.Elite },   // Great Slime
                    new() { EnemyId = 14, Tier = EncounterTier.Elite },  // Great Chimera
                    new() { EnemyId = 25, Tier = EncounterTier.Boss },   // Nerath-Dur the Lich
                    new()
                    {
                        EnemyId = 26, Tier = EncounterTier.Boss, // Grommash-Vurr the Troll King
                        Availability = new EncounterAvailabilityRequirement { RequiredStoryFlags = new() { [TrollAllianceRouteFlag] = false } }
                    }
                }
            },

            new LocationEncounterDefinition
            {
                Id = 7,
                LocationId = LocationId.WhisperingWoods,
                Enemies = new List<EncounterEnemyDefinition>
                {
                    new() { EnemyId = 4, Tier = EncounterTier.Normal },  // Goblin
                    new() { EnemyId = 7, Tier = EncounterTier.Normal },  // Slime
                    new() { EnemyId = 19, Tier = EncounterTier.Normal }, // Phantom
                    new() { EnemyId = 5, Tier = EncounterTier.Elite },   // Hobgoblin
                    new() { EnemyId = 8, Tier = EncounterTier.Elite },   // Great Slime
                    new() { EnemyId = 20, Tier = EncounterTier.Elite },  // Wraith
                    new() { EnemyId = 21, Tier = EncounterTier.Boss }    // Dread Wraith
                }
            }
        });
    }

    private static EncounterAvailabilityRequirement MisthavenPortAvailability() =>
        new() { RequiredSubLocations = MisthavenPortSubLocations.ToList() };
}
