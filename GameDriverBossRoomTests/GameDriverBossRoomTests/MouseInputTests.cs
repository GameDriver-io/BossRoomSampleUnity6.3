using gdio.common.objects;
using gdio.unity_api.v2;

namespace GameDriverBossRoomTests;

/// <summary>
/// Input.PC.Mouse domain: verifies the raw mouse input MECHANISM reaches the game
/// on this platform, as distinct from testing combat/gameplay RESULTS.
///
/// This is a deliberate split from fixtures like MobDamageTests, which use
/// api.NavAgentMoveToPoint to move the player -- that bypasses real input entirely
/// on purpose, because that fixture's job is to verify combat *outcomes* (damage
/// registering) regardless of how the player got there. This fixture's job is the
/// opposite: verify the actual Mouse device input, simulated at the OS/Input System
/// level (api.Click at a screen position), produces the effect Boss Room's own
/// ClientInputSender expects.
///
/// Traced from Assets/InputSystem/PlayerActions.inputactions: the "Player" action
/// map's Point action is bound to &lt;Mouse&gt;/position, and ClientInputSender.FixedUpdate
/// raycasts from that screen position against the ground layer to move the local
/// player (Unity.BossRoom.Gameplay.UserInput.ClientInputSender). "Target" is bound to
/// Mouse/leftButton.
///
/// Platform note: this Mac Editor session only exercises the "Keyboard&amp;Mouse"
/// control scheme. The .inputactions asset also defines Touch (see TouchInputTests,
/// which has real bindings but no device here) and Gamepad/Joystick/XR control
/// schemes -- but those are only wired for UI menu navigation; there are NO
/// Gamepad/Joystick/XR bindings anywhere in the Player (gameplay) action map at all
/// (confirmed by scanning the asset directly, not by omission). See ConsoleInputTests
/// for that finding.
///
/// COVERAGE GAP: OneTimeSetUp only plays as BossRoomFlow.Tank. Lower risk than the
/// combat-domain fixtures -- movement is class-agnostic ClientInputSender/NavMesh
/// logic, not per-class ability data -- but still unverified for Archer/Mage/Rogue.
/// </summary>
[TestFixture]
[Category("Input.PC.Mouse")]
public class MouseInputTests : GameDriverTest
{
    [OneTimeSetUp]
    public void EnterGameplayAsTank()
    {
        BossRoomFlow.EnsureAtMainMenu(api);
        BossRoomFlow.StartNewGame(api);
        BossRoomFlow.SelectClassAndEnterGame(api, BossRoomFlow.Tank);
    }

    [Test]
    [Order(010)]
    public void T010_GivenGameplay_WhenGroundClicked_PlayerMoves()
    {
        var startPos = api.GetObjectPosition(BossRoomFlow.LocalPlayerHPath, CoordinateConversion.None, "", 30);
        var screenPos = api.GetObjectPosition(
            BossRoomFlow.LocalPlayerHPath, CoordinateConversion.WorldToScreenPoint, "", 30);

        // A real mouse click, at a real screen coordinate -- goes through the same
        // Point/Target Input Actions Boss Room reads, not an API movement shortcut.
        var clickTarget = new Vector2(screenPos.x + 150, screenPos.y);
        api.Click(clickTarget, MouseButtons.LEFT, 10, "", 30);
        api.Wait(1000);

        var endPos = api.GetObjectPosition(BossRoomFlow.LocalPlayerHPath, CoordinateConversion.None, "", 30);
        var dx = endPos.x - startPos.x;
        var dy = endPos.y - startPos.y;
        var dz = endPos.z - startPos.z;
        var distanceMoved = Math.Sqrt(dx * dx + dy * dy + dz * dz);

        Assert.That(distanceMoved, Is.GreaterThan(0.25),
            "Clicking the ground did not move the player -- the Point/Target mouse " +
            "input may not be reaching ClientInputSender on this platform");
    }
}
