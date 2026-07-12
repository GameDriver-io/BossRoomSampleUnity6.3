namespace GameDriverBossRoomTests;

/// <summary>
/// Input.Console.Gamepad domain: NOT run, and NOT just because no console/gamepad
/// hardware is available here -- this documents a real finding, distinct from
/// TouchInputTests' "untested but real" gap.
///
/// Assets/InputSystem/PlayerActions.inputactions defines Gamepad, Joystick, and XR
/// control schemes, but scanning the asset directly (every binding's "groups" field)
/// shows those schemes are wired ONLY on the "UI" action map (menu Navigate/Submit/
/// Cancel) -- there are ZERO Gamepad/Joystick/XR bindings anywhere on the "Player"
/// action map (movement, targeting, Action1-8, Skill1). Confirmed by exhaustive scan,
/// not by omission.
///
/// So this isn't "we haven't tested controller gameplay" -- as configured today,
/// controller gameplay input doesn't exist to test. That's worth surfacing to
/// design/dev as a real gap, separate from the QaaS test-coverage story: adding
/// these Input.Console.Gamepad tests later is gated on Player action map bindings
/// being added first, not just on getting a console/controller in this environment.
/// </summary>
[TestFixture]
[Category("Input.Console.Gamepad")]
[Ignore("PlayerActions.inputactions has NO Gamepad/Joystick/XR bindings on the Player " +
        "(gameplay) action map -- only Keyboard&Mouse and partial Touch. Controller " +
        "control schemes are wired for UI menu navigation only. This is a design/input-" +
        "config gap to flag, not just a missing test device.")]
public class ConsoleInputTests : GameDriverTest
{
    [Test]
    [Order(010)]
    public void T010_GivenGameplay_WhenAbilityButtonPressed_ActionFires()
    {
        // Would mirror KeyboardInputTests.T010 via a Gamepad button binding, once
        // the Player action map actually has one to press.
    }
}
