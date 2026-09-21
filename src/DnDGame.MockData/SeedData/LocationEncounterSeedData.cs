using DnDGame.Domain.Entities.Locations;
using DnDGame.Domain.Enums;

namespace DnDGame.MockData.SeedData;

/// <summary>
/// Seeds the per-location enemy encounter pools (BACK-LOC-05), one
/// LocationEncounterDefinition per LocationId. References Enemy rows from
/// EnemySeedData by Id, including the 6 named campaign bosses (Ids 22-27) added
/// there as placeholders for this feature — see EnemySeedData's doc comment; their
/// stats are not final.
/// </summary>
internal static class LocationEncounterSeedData
{
    private const string OakheavenAllianceFailedFlag = "OakheavenAllianceFailed";
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
                    new() { EnemyId = 15, Tier = EncounterTier.Boss },   // Divine Chimera
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
                    new() { EnemyId = 22, Tier = EncounterTier.Boss },   // The Herald
                    new() { EnemyId = 23, Tier = EncounterTier.Boss }    // Vharruk
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
                    new() { EnemyId = 5, Tier = EncounterTier.Normal, Availability = MisthavenPortAvailability() },  // Hobgoblin
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
                        Availability = new EncounterAvailabilityRequirement { RequiredFlag = OakheavenAllianceFailedFlag, RequiredFlagValue = true }
                    },
                    new()
                    {
                        EnemyId = 5, Tier = EncounterTier.Normal, // Hobgoblin
                        Availability = new EncounterAvailabilityRequirement { RequiredFlag = OakheavenAllianceFailedFlag, RequiredFlagValue = true }
                    },
                    new() { EnemyId = 17, Tier = EncounterTier.Elite }, // Greater Demon
                    new() { EnemyId = 8, Tier = EncounterTier.Elite },  // Great Slime
                    new() { EnemyId = 20, Tier = EncounterTier.Elite }, // Wraith
                    new() { EnemyId = 27, Tier = EncounterTier.Boss },  // Greater Demon Mayor
                    new() { EnemyId = 9, Tier = EncounterTier.Boss }    // King Slime (optional boss)
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
                    new() { EnemyId = 26, Tier = EncounterTier.Boss }    // Grommash-Vurr the Troll King
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
