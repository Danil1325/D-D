using System.Text.RegularExpressions;
using DnDGame.Domain.Entities.Game;
using DnDGame.Domain.Enums;

namespace DnDGame.MockData.SeedData;

/// <summary>
/// Story scenes (dialogue + choices) from The_Crown_of_Ash_Interactive_Scenario.docx.
/// See docs/TheCrownOfAshStorySceneSeed.md for source mapping and technical conventions.
///
/// Scene ids up to 899 are the ones created by the original main-quest scene reference
/// catalog; they are reproduced here WITH content. 900+ are continuation/split scenes
/// introduced so that every authored choice and outcome has a scene to route to.
/// The "Continue" choices on linear scenes are a UI affordance: the data model routes
/// the player forward with a choice, and they do not alter the scenario text.
/// </summary>
internal partial class ScenarioStorySceneSeedData
{
    private static readonly Regex SentenceBoundary = new(@"(?<=[.!?])\s+", RegexOptions.Compiled);

    private const int MaxDialogueLength = 350;

    private InMemoryGameDataStore _store = null!;
    private int _dialogueId;
    private int _choiceId;

    public void Seed(InMemoryGameDataStore store)
    {
        _store = store;
        _dialogueId = 90_001;
        _choiceId = 90_001;

        SeedPrologue();
        SeedAct1();
        SeedAct2();
        SeedAct3();
        SeedAct4();
        SeedAct5();
        SeedAct6();
        SeedEpilogues();
    }

    private void SeedPrologue()
    {
        // Act 0 is an unnumbered framing section (How to Use / Prologue / Awakening).
        var s900 = Scene(900, 0, 0, "How to Use This Script", 3);
        Say(s900, 5);
        Say(s900, 6);
        ContinueTo(s900, 901);

        var s901 = Scene(901, 0, 0, "Prologue - The Slow Lock", 3);
        Say(s901, 31);
        Say(s901, 32);
        Say(s901, 33);
        Say(s901, 34);
        Say(s901, 35);
        ContinueTo(s901, 902);

        var s902 = Scene(902, 0, 0, "Character Awakening", 3);
        Say(s902, 37);
        Say(s902, 38);
        Say(s902, 39);
        ContinueTo(s902, 903);

        var s903 = Scene(903, 0, 0, "Racial Memory and Calling", 3);
        Narrate(s903, 42);
        Narrate(s903, 43);
        Narrate(s903, 44);
        Narrate(s903, 45);
        Narrate(s903, 48);
        Narrate(s903, 49);
        Narrate(s903, 50);
        Narrate(s903, 51);
        ContinueTo(s903, 1001);
    }

    private void SeedAct1()
    {
        var s1001 = Scene(1001, 1, 1, "Ash at Hero's Overlook", 3);
        Say(s1001, 54);
        Say(s1001, 55);
        Say(s1001, 56);
        Say(s1001, 57);
        Say(s1001, 58);
        ContinueTo(s1001, 1002);

        var s1002 = Scene(1002, 1, 1, "Ash at Hero's Overlook", 3);
        Say(s1002, 59);
        Say(s1002, 60);
        Say(s1002, 61);
        Say(s1002, 62);
        Choice(s1002, 64, 1205);
        Choice(s1002, 65, 1003);
        Choice(s1002, 66, 1004);
        Choice(s1002, 67, 1205);

        var s1003 = Scene(1003, 1, 1, "Ash at Hero's Overlook", 3);
        Narrate(s1003, 68);
        ContinueTo(s1003, 1205);

        var s1004 = Scene(1004, 1, 1, "Ash at Hero's Overlook", 3);
        Narrate(s1004, 69);
        ContinueTo(s1004, 1205);

        var s1205 = Scene(1205, 1, 2, "The Road That Is Yours", 4);
        Say(s1205, 71);
        Say(s1205, 72);
        Choice(s1205, 74, 1201);
        Choice(s1205, 85, 1202);
        Choice(s1205, 96, 1203);
        Choice(s1205, 107, 1204);

        var s1201 = Scene(1201, 1, 2, "The Wood That Remembers", 7);
        Say(s1201, 75);
        Say(s1201, 76);
        Say(s1201, 77);
        Say(s1201, 78);
        ContinueTo(s1201, 1211);

        var s1211 = Scene(1211, 1, 2, "The Wood That Remembers", 7);
        Say(s1211, 79);
        Choice(s1211, 81, 1212);
        Choice(s1211, 82, 1212);
        Choice(s1211, 83, 1212);

        var s1212 = Scene(1212, 1, 2, "The Wood That Remembers", 7);
        Narrate(s1212, 84);
        ContinueTo(s1212, 2003);

        var s1202 = Scene(1202, 1, 2, "Ash Beneath Ashtonia", 1);
        Say(s1202, 86);
        Say(s1202, 87);
        Say(s1202, 88);
        Say(s1202, 89);
        ContinueTo(s1202, 1221);

        var s1221 = Scene(1221, 1, 2, "Ash Beneath Ashtonia", 1);
        Say(s1221, 90);
        Choice(s1221, 92, 1222);
        Choice(s1221, 93, 1222);
        Choice(s1221, 94, 1222);

        var s1222 = Scene(1222, 1, 2, "Ash Beneath Ashtonia", 1);
        Narrate(s1222, 95);
        ContinueTo(s1222, 2003);

        var s1203 = Scene(1203, 1, 2, "The Lighthouse Guild", 4);
        Say(s1203, 97);
        Say(s1203, 98);
        Say(s1203, 99);
        Say(s1203, 100);
        Say(s1203, 101);
        ContinueTo(s1203, 1231);

        var s1231 = Scene(1231, 1, 2, "The Lighthouse Guild", 4);
        Choice(s1231, 103, 1232);
        Choice(s1231, 104, 1232);
        Choice(s1231, 105, 1232);

        var s1232 = Scene(1232, 1, 2, "The Lighthouse Guild", 4);
        Narrate(s1232, 106);
        ContinueTo(s1232, 2003);

        var s1204 = Scene(1204, 1, 2, "The Silence of Karag-Dur", 6);
        Say(s1204, 108);
        Say(s1204, 109);
        Say(s1204, 110);
        Say(s1204, 111);
        Say(s1204, 112);
        ContinueTo(s1204, 1241);

        var s1241 = Scene(1241, 1, 2, "The Silence of Karag-Dur", 6);
        Choice(s1241, 114, 1242);
        Choice(s1241, 115, 1242);
        Choice(s1241, 116, 1242);

        var s1242 = Scene(1242, 1, 2, "The Silence of Karag-Dur", 6);
        Narrate(s1242, 117);
        ContinueTo(s1242, 2003);
    }

    private void SeedAct2()
    {
        var s2003 = Scene(2003, 2, 3, "The Lighthouse with No Sea-Light", 4);
        Say(s2003, 120);
        Say(s2003, 121);
        Say(s2003, 122);
        Say(s2003, 123);
        ContinueTo(s2003, 2013);

        var s2013 = Scene(2013, 2, 3, "The Lighthouse with No Sea-Light", 4);
        Say(s2013, 124);
        Say(s2013, 125);
        Say(s2013, 126);
        ContinueTo(s2013, 2023);

        var s2023 = Scene(2023, 2, 3, "The Lighthouse with No Sea-Light", 4);
        Narrate(s2023, 127);
        Choice(s2023, 129, 2033);
        Choice(s2023, 130, 2033);
        Choice(s2023, 131, 2033);
        Choice(s2023, 132, 2033);

        var s2033 = Scene(2033, 2, 3, "The Lighthouse with No Sea-Light", 4);
        Narrate(s2033, 133);
        Narrate(s2033, 134);
        ContinueTo(s2033, 2004);

        var s2004 = Scene(2004, 2, 4, "Companions in the Lantern Hall", 4);
        Say(s2004, 136);
        Say(s2004, 137);
        Say(s2004, 138);
        Say(s2004, 139);
        ContinueTo(s2004, 2014);

        var s2014 = Scene(2014, 2, 4, "Companions in the Lantern Hall", 4);
        Choice(s2014, 141, 2005);
        Choice(s2014, 142, 2005);
        Choice(s2014, 143, 2005);

        var s2005 = Scene(2005, 2, 5, "The Missing Reports", 4);
        Say(s2005, 145);
        Say(s2005, 146);
        Say(s2005, 147);
        Say(s2005, 148);
        Say(s2005, 149);
        ContinueTo(s2005, 2015);

        var s2015 = Scene(2015, 2, 5, "The Missing Reports", 4);
        Choice(s2015, 151, 3006);
        Choice(s2015, 152, 3006);
        Choice(s2015, 153, 3006);
    }

    private void SeedAct3()
    {
        // Source reference scene only: "The Silent Grain Road" has no authored dialogue.
        Scene(3005, 3, 0, "The Silent Grain Road", 5);

        var s3006 = Scene(3006, 3, 6, "A Town Being Consumed", 5);
        Say(s3006, 156);
        Say(s3006, 157);
        Say(s3006, 158);
        Say(s3006, 159);
        Say(s3006, 160);
        Say(s3006, 161);
        ContinueTo(s3006, 3016);

        var s3016 = Scene(3016, 3, 6, "A Town Being Consumed", 5);
        Choice(s3016, 162, 3026);
        Choice(s3016, 168, 3036);
        Choice(s3016, 174, 3046);
        Choice(s3016, 180, 3056);

        var s3026 = Scene(3026, 3, 6, "The Mage Solution - Burn the Sigils", 5);
        Say(s3026, 163);
        Say(s3026, 164);
        ContinueTo(s3026, 3027);

        var s3027 = Scene(3027, 3, 6, "The Mage Solution - Burn the Sigils", 5);
        Choice(s3027, 166, 3007);
        Choice(s3027, 167, 3007);

        var s3036 = Scene(3036, 3, 6, "The Warrior Solution - Hold the Bridge", 5);
        Say(s3036, 169);
        Say(s3036, 170);
        ContinueTo(s3036, 3037);

        var s3037 = Scene(3037, 3, 6, "The Warrior Solution - Hold the Bridge", 5);
        Choice(s3037, 172, 3007);
        Choice(s3037, 173, 3007);

        var s3046 = Scene(3046, 3, 6, "The Bard Solution - Two Unlikely Armies", 5);
        Say(s3046, 175);
        Say(s3046, 176);
        ContinueTo(s3046, 3047);

        var s3047 = Scene(3047, 3, 6, "The Bard Solution - Two Unlikely Armies", 5);
        Choice(s3047, 178, 3007);
        Choice(s3047, 179, 3007);

        var s3056 = Scene(3056, 3, 6, "The Healer Solution - Three Nights of Names", 5);
        Say(s3056, 181);
        Say(s3056, 182);
        Say(s3056, 183);
        ContinueTo(s3056, 3057);

        var s3057 = Scene(3057, 3, 6, "The Healer Solution - Three Nights of Names", 5);
        Choice(s3057, 185, 3007);
        Choice(s3057, 186, 3007);

        var s3007 = Scene(3007, 3, 7, "The Voice Behind the Face", 5);
        Say(s3007, 188);
        Say(s3007, 189);
        Say(s3007, 190);
        Say(s3007, 191);
        ContinueTo(s3007, 3017);

        var s3017 = Scene(3017, 3, 7, "The Voice Behind the Face", 5);
        Say(s3017, 192);
        Say(s3017, 193);
        ContinueTo(s3017, 4100);
    }

    private void SeedAct4()
    {
        var s4100 = Scene(4100, 4, 0, "Act IV - The Gathering", 4);
        Say(s4100, 195);
        ContinueTo(s4100, 4008);

        var s4008 = Scene(4008, 4, 8, "Ashtonia's Sealed Chamber", 1);
        Say(s4008, 197);
        Say(s4008, 198);
        Say(s4008, 199);
        Say(s4008, 200);
        Say(s4008, 201);
        ContinueTo(s4008, 4018);

        var s4018 = Scene(4018, 4, 8, "Ashtonia's Sealed Chamber", 1);
        Choice(s4018, 203, 4028);
        Choice(s4018, 204, 4028);
        Choice(s4018, 205, 4028);
        Choice(s4018, 206, 4028);

        var s4028 = Scene(4028, 4, 8, "Ashtonia's Sealed Chamber", 1);
        Narrate(s4028, 207);
        ContinueTo(s4028, 4009);

        var s4009 = Scene(4009, 4, 9, "The Heart-Tree's Fifth Shadow", 7);
        Say(s4009, 209);
        Say(s4009, 210);
        Say(s4009, 211);
        Say(s4009, 212);
        Say(s4009, 213);
        ContinueTo(s4009, 4019);

        var s4019 = Scene(4019, 4, 9, "The Heart-Tree's Fifth Shadow", 7);
        Choice(s4019, 215, 4029);
        Choice(s4019, 216, 4029);
        Choice(s4019, 217, 4029);
        Choice(s4019, 218, 4029);

        var s4029 = Scene(4029, 4, 9, "The Heart-Tree's Fifth Shadow", 7);
        Narrate(s4029, 219);
        ContinueTo(s4029, 4010);

        var s4010 = Scene(4010, 4, 10, "Karag-Dur, Where Nothing Lives", 6);
        Say(s4010, 221);
        Say(s4010, 222);
        Say(s4010, 223);
        Say(s4010, 224);
        Say(s4010, 225);
        ContinueTo(s4010, 4020);

        var s4020 = Scene(4020, 4, 10, "Karag-Dur, Where Nothing Lives", 6);
        Narrate(s4020, 226);
        Choice(s4020, 228, 4030);
        Choice(s4020, 229, 4030);
        Choice(s4020, 230, 4030);
        Choice(s4020, 231, 4030);

        var s4030 = Scene(4030, 4, 10, "Karag-Dur, Where Nothing Lives", 6);
        Narrate(s4030, 232);
        ContinueTo(s4030, 4111);

        // Chapter 11 has three variants because the last fragment's location is not fixed.
        var s4111 = Scene(4111, 4, 11, "The Traitor's Errand", 1);
        Say(s4111, 234);
        Say(s4111, 235);
        Say(s4111, 236);
        Say(s4111, 237);
        Say(s4111, 238);
        ContinueTo(s4111, 4114);

        var s4112 = Scene(4112, 4, 11, "The Traitor's Errand", 7);
        Say(s4112, 234);
        Say(s4112, 235);
        Say(s4112, 236);
        Say(s4112, 237);
        Say(s4112, 238);
        ContinueTo(s4112, 4115);

        var s4113 = Scene(4113, 4, 11, "The Traitor's Errand", 6);
        Say(s4113, 234);
        Say(s4113, 235);
        Say(s4113, 236);
        Say(s4113, 237);
        Say(s4113, 238);
        ContinueTo(s4113, 4116);

        var s4114 = Scene(4114, 4, 11, "The Traitor's Errand", 1);
        Say(s4114, 239);
        ContinueTo(s4114, 5101);

        var s4115 = Scene(4115, 4, 11, "The Traitor's Errand", 7);
        Say(s4115, 239);
        ContinueTo(s4115, 5101);

        var s4116 = Scene(4116, 4, 11, "The Traitor's Errand", 6);
        Say(s4116, 239);
        ContinueTo(s4116, 5101);
    }

    private void SeedAct5()
    {
        var s5101 = Scene(5101, 5, 0, "The Gate of Rust", 2);
        Say(s5101, 243);
        Choice(s5101, 245, 5102);
        Choice(s5101, 246, 5102);
        Choice(s5101, 247, 5102);
        Choice(s5101, 248, 5102);

        var s5102 = Scene(5102, 5, 0, "The Gate of Names", 2);
        Say(s5102, 250);
        Choice(s5102, 252, 5103);
        Choice(s5102, 253, 5103);
        Choice(s5102, 254, 5103);
        Choice(s5102, 255, 5103);

        var s5103 = Scene(5103, 5, 0, "The Gate of Weight", 2);
        Say(s5103, 257);
        Choice(s5103, 259, 5104);
        Choice(s5103, 260, 5104);
        Choice(s5103, 261, 5104);
        Choice(s5103, 262, 5104);

        var s5104 = Scene(5104, 5, 0, "The Gate of Reach", 2);
        Say(s5104, 264);
        Choice(s5104, 266, 5105);
        Choice(s5104, 267, 5105);
        Choice(s5104, 268, 5105);
        Choice(s5104, 269, 5105);

        var s5105 = Scene(5105, 5, 0, "The Gate of Sorrow", 2);
        Say(s5105, 271);
        Choice(s5105, 273, 5106);
        Choice(s5105, 274, 5106);
        Choice(s5105, 275, 5106);
        Choice(s5105, 276, 5106);

        var s5106 = Scene(5106, 5, 0, "The Gate of Teeth", 2);
        Say(s5106, 278);
        Choice(s5106, 280, 5107);
        Choice(s5106, 281, 5107);
        Choice(s5106, 282, 5107);
        Choice(s5106, 283, 5107);

        var s5107 = Scene(5107, 5, 0, "The Gate of Fire", 2);
        Say(s5107, 285);
        Choice(s5107, 287, 5108);
        Choice(s5107, 288, 5108);
        Choice(s5107, 289, 5108);
        Choice(s5107, 290, 5108);

        var s5108 = Scene(5108, 5, 0, "The Gate of Stone", 2);
        Say(s5108, 292);
        Choice(s5108, 294, 5109);
        Choice(s5108, 295, 5109);
        Choice(s5108, 296, 5109);
        Choice(s5108, 297, 5109);

        var s5109 = Scene(5109, 5, 0, "The Fellowship Vault", 2);
        Say(s5109, 299);
        Narrate(s5109, 301);
        ContinueTo(s5109, 5012);

        var s5012 = Scene(5012, 5, 12, "The Fellowship Vault", 2);
        Say(s5012, 303);
        Say(s5012, 304);
        Say(s5012, 305);
        Say(s5012, 306);
        Say(s5012, 307);
        Say(s5012, 308);
        Say(s5012, 309);
        ContinueTo(s5012, 5014);

        var s5014 = Scene(5014, 5, 12, "The Fellowship Vault", 2);
        Choice(s5014, 311, 5013);
        Choice(s5014, 312, 5013);
        Choice(s5014, 313, 5013);
        Choice(s5014, 314, 5013);

        var s5013 = Scene(5013, 5, 13, "The Throne and the Offer", 2);
        Say(s5013, 316);
        Say(s5013, 317);
        Say(s5013, 318);
        Say(s5013, 319);
        Say(s5013, 320);
        Say(s5013, 321);
        Say(s5013, 322);
        ContinueTo(s5013, 6014);
    }

    private void SeedAct6()
    {
        var s6014 = Scene(6014, 6, 14, "The Road Closing Behind You", 3);
        Say(s6014, 325);
        Say(s6014, 326);
        Say(s6014, 327);
        Say(s6014, 328);
        Say(s6014, 354);
        ContinueTo(s6014, 6015);

        var s6015 = Scene(6015, 6, 15, "Phase One: The Herald", 3);
        Say(s6015, 356);
        Say(s6015, 357);
        Say(s6015, 358);
        Say(s6015, 359);
        Say(s6015, 360);
        Say(s6015, 361);
        ContinueTo(s6015, 6016);

        var s6016 = Scene(6016, 6, 16, "Phase Two: The Contract", 3);
        Say(s6016, 362);
        Say(s6016, 363);
        Say(s6016, 364);
        Choice(s6016, 366, 6017);
        Choice(s6016, 367, 6017);
        Choice(s6016, 368, 6017);
        Choice(s6016, 369, 6017);
        Choice(s6016, 370, 6017);
        Choice(s6016, 371, 6017);

        var s6017 = Scene(6017, 6, 17, "Phase Three: The Ash", 3);
        Say(s6017, 373);
        Say(s6017, 374);
        Say(s6017, 375);
        Say(s6017, 376);
        Say(s6017, 377);
Choice(s6017, 366, 6101);
        Choice(s6017, 367, 6102);
        Choice(s6017, 368, 6103);
        Choice(s6017, 369, 6104);
        Choice(s6017, 370, 6106);
        Choice(s6017, 371, 6105);
    }

    private void SeedEpilogues()
    {
        var s6101 = Scene(6101, 6, 18, "Ending I - The Crown Broken", 3);
        Say(s6101, 380);
        Say(s6101, 381);
        ContinueTo(s6101, 6108);

        var s6102 = Scene(6102, 6, 18, "Ending II - The Crown Worn", 2);
        Say(s6102, 383);
        Say(s6102, 384);
        ContinueTo(s6102, 6108);

        var s6103 = Scene(6103, 6, 18, "Ending III - The Crown Sung", 3);
        Say(s6103, 386);
        Say(s6103, 387);
        ContinueTo(s6103, 6108);

        var s6104 = Scene(6104, 6, 18, "Ending IV - The Crown Reforged", 6);
        Say(s6104, 389);
        Say(s6104, 390);
        ContinueTo(s6104, 6108);

        var s6105 = Scene(6105, 6, 18, "Ending V - The Crown Given", 3);
        Say(s6105, 392);
        Say(s6105, 393);
        ContinueTo(s6105, 6108);

        var s6106 = Scene(6106, 6, 18, "Ending VI - The Crown Carried", 3);
        Say(s6106, 395);
        Say(s6106, 396);
        ContinueTo(s6106, 6108);

        // No inbound choice routes here: "The Ash Falls" is triggered by the engine
        // when the Ash Clock reaches 10, per the source requirement.
        var s6107 = Scene(6107, 6, 18, "Ending VII - The Ash Falls", 3);
        Say(s6107, 398);
        Say(s6107, 399);
        ContinueTo(s6107, 6108);

        var s6108 = Scene(6108, 6, 18, "Final Image", 3);
        Say(s6108, 401);
        Say(s6108, 402);
        s6108.IsFinalScene = true;
    }

    private StoryScene Scene(int id, int act, int chapter, string title, int locationId)
    {
        var location = _store.Locations.Single(location => location.Id == locationId);
        var scene = new StoryScene
        {
            Id = id,
            Act = act,
            Chapter = chapter,
            Title = title,
            LocationId = locationId,
            BackgroundImage = location.BackgroundImage
        };
        _store.StoryScenes.Add(scene);
        return scene;
    }

    /// <summary>Adds an authored dialogue line, mapping its speaker label to the dialogue type.</summary>
    private void Say(StoryScene scene, int paraIndex)
    {
        var content = P[paraIndex];
        var (type, speaker) = ResolveLabel(content, out content);
        foreach (var chunk in Wrap(content, MaxDialogueLength))
            AddDialogue(scene, type, speaker, chunk);
    }

    /// <summary>Adds authored text as narration (result lines, headings, racial/class calls).</summary>
    private void Narrate(StoryScene scene, int paraIndex)
    {
        foreach (var chunk in Wrap(P[paraIndex], MaxDialogueLength))
            AddDialogue(scene, DialogueType.Narration, string.Empty, chunk);
    }

    private (DialogueType Type, string Speaker) ResolveLabel(string value, out string content)
    {
        if (value.StartsWith("Author: ", StringComparison.Ordinal))
        {
            content = value["Author: ".Length..];
            return (DialogueType.Narration, string.Empty);
        }

        if (value.StartsWith("{user_nickname}: ", StringComparison.Ordinal))
        {
            content = value["{user_nickname}: ".Length..];
            return (DialogueType.Player, "{user_nickname}");
        }

        if (value.StartsWith("GAME: ", StringComparison.Ordinal))
        {
            content = value["GAME: ".Length..];
            return (DialogueType.System, "GAME");
        }

        var separator = value.IndexOf(": ", StringComparison.Ordinal);
        if (separator > 0)
        {
            content = value[(separator + 2)..];
            return (DialogueType.NPC, value[..separator]);
        }

        content = value;
        return (DialogueType.Narration, string.Empty);
    }

    private void AddDialogue(StoryScene scene, DialogueType type, string speaker, string text)
    {
        scene.Dialogues.Add(new StoryDialogue
        {
            Id = _dialogueId++,
            Speaker = speaker,
            Text = text,
            DialogueOrder = scene.Dialogues.Count + 1,
            DialogueType = type
        });
    }

    private void Choice(StoryScene scene, int textParaIndex, int nextSceneId)
    {
        // Every authored choice keeps its bracketed guidance verbatim in the visible text;
        // Requirements/Consequences are intentionally left to the game engine.
        scene.Choices.Add(new StoryChoice
        {
            Id = _choiceId++,
            Text = P[textParaIndex],
            NextSceneId = nextSceneId
        });
    }

    /// <summary>
    /// Linear transitions still route through a single "Continue" choice so every
    /// non-final scene has an unambiguous connected flow. This is a UI affordance
    /// only; it is not authored scenario text.
    /// </summary>
    private void ContinueTo(StoryScene scene, int nextSceneId)
    {
        scene.Choices.Add(new StoryChoice
        {
            Id = _choiceId++,
            Text = "Continue",
            NextSceneId = nextSceneId
        });
    }

    private static List<string> Wrap(string text, int maxLength)
    {
        if (text.Length <= maxLength)
            return new List<string> { text };

        var result = new List<string>();
        var current = string.Empty;
        foreach (var sentence in SentenceBoundary.Split(text))
        {
            var candidate = current.Length == 0 ? sentence : current + " " + sentence;
            if (candidate.Length <= maxLength)
            {
                current = candidate;
                continue;
            }

            if (current.Length > 0)
            {
                result.Add(current);
                current = sentence;
                continue;
            }

            result.Add(text[..maxLength]);
            current = text[maxLength..];
        }

        if (current.Length > 0)
            result.Add(current);
        return result;
    }
}

