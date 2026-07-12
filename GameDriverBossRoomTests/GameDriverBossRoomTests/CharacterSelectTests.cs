using gdio.common.objects;
using gdio.unity_api.v2;

namespace GameDriverBossRoomTests;

/// <summary>
/// Reusable "start a new game" flow test. Creates a fresh session and confirms we
/// land in CharSelect.
/// The heavy lifting lives in <see cref="BossRoomFlow"/> so other fixtures reuse it.
/// </summary>
[TestFixture]
[Category("Session")]
public class StartNewGameTests : GameDriverTest
{
    [OneTimeSetUp]
    public void WaitForBoot() => BossRoomFlow.EnsureAtMainMenu(api);

    [Test]
    [Order(010)]
    public void T010_GivenMainMenu_WhenCreatingNewSession_ReachesCharSelect()
    {
        BossRoomFlow.StartNewGame(api);
        Assert.That(api.GetSceneName(), Is.EqualTo(BossRoomFlow.CharSelect));
    }
}

/// <summary>
/// Rolls through every hero class from CharSelect into gameplay, asserting the
/// character-select UI at each key point:
///   - the clicked seat panel highlights,
///   - the ClassInfoBox reveals its details, and
///   - the info-box class label matches the expected class (text on screen).
/// Then locks in with Ready, clears the debug Cheats and How-To-Play popups, and
/// confirms we entered the BossRoom gameplay scene.
///
/// Each [TestCase] is fully self-contained: it starts a new game, plays the class in,
/// and the [SetUp] guard returns us to the MainMenu so the next class starts clean.
/// </summary>
[TestFixture]
[Category("CharSelect")]
public class ClassRollThroughTests : GameDriverTest
{
    [SetUp]
    public void ReturnToMenu() => BossRoomFlow.EnsureAtMainMenu(api);

    // seatIndex -> expected DisplayedName, from AvatarConfiguration in CharSelectState.prefab.
    // "the tank" is seat 0; the other classes follow at the first seat of each boy/girl pair.
    [TestCase(0, "TANK")]
    [TestCase(2, "ARCHER")]
    [TestCase(4, "MAGE")]
    [TestCase(6, "ROGUE")]
    public void SelectClass_HighlightsSeat_MatchesInfoBox_EntersGame(int seatIndex, string expectedClass)
    {
        // --- Arrange: fresh game, sitting in CharSelect ---
        BossRoomFlow.StartNewGame(api);

        // --- Act: pick this class's seat ---
        BossRoomFlow.SelectSeat(api, seatIndex);

        // --- Assert: CharSelect UI reflects the selection ---
        Assert.That(BossRoomFlow.IsSeatHighlighted(api, seatIndex), Is.True,
            $"Seat {seatIndex} ({expectedClass}) did not show its selection highlight");

        Assert.That(BossRoomFlow.IsInfoBoxShowingClass(api), Is.True,
            "ClassInfoBox did not reveal its details after selecting a seat");

        Assert.That(BossRoomFlow.GetInfoBoxClassText(api), Is.EqualTo(expectedClass),
            $"ClassInfoBox label did not match the selected class for seat {seatIndex}");

        // --- Act: lock in, clear debug cheats + how-to-play, enter gameplay ---
        BossRoomFlow.SelectClassAndEnterGame(api, seatIndex);

        // --- Assert: we made it into the BossRoom gameplay scene ---
        Assert.That(api.GetSceneName(), Is.EqualTo(BossRoomFlow.BossRoom),
            $"Did not enter the BossRoom scene for seat {seatIndex} ({expectedClass})");
    }
}
