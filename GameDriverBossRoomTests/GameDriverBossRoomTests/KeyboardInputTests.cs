using gdio.unity_api;
using gdio.unity_api.v2;

namespace GameDriverBossRoomTests;

/// <summary>
/// Input.PC.Keyboard domain: verifies a real keyboard keypress reaches the game,
/// as distinct from testing what that keypress triggers in gameplay terms.
///
/// Traced from Assets/InputSystem/PlayerActions.inputactions: "ToggleCheats" is
/// bound to &lt;Keyboard&gt;/slash, wired to DebugCheatsManager's cheats panel toggle.
/// This is a clean, deterministic, side-effect-free keyboard binding to verify the
/// physical key actually reaches the input system on this platform -- unlike
/// Action1-8 (bound to number keys 1-8), which also trigger real combat actions and
/// would blur into a Combat-domain result test rather than a pure input check.
/// </summary>
[TestFixture]
[Category("Input.PC.Keyboard")]
public class KeyboardInputTests : GameDriverTest
{
    private const string DebugCheatsManager =
        "//*[fn:component('Unity.BossRoom.DebugCheats.DebugCheatsManager')]" +
        "/fn:component('Unity.BossRoom.DebugCheats.DebugCheatsManager')";
    private const string CheatsPanelActive = $"{DebugCheatsManager}/@m_DebugCheatsPanel/@activeSelf";

    [OneTimeSetUp]
    public void EnterGameplayAsTank()
    {
        BossRoomFlow.EnsureAtMainMenu(api);
        BossRoomFlow.StartNewGame(api);
        BossRoomFlow.SelectClassAndEnterGame(api, BossRoomFlow.Tank);
    }

    [Test]
    [Order(010)]
    public void T010_GivenGameplay_WhenSlashKeyPressed_CheatsPanelToggles()
    {
        var before = api.GetObjectFieldValue<bool>(CheatsPanelActive, 30);

        api.KeyPress(new[] { KeyCode.Slash }, 10);
        api.Wait(300);

        var after = api.GetObjectFieldValue<bool>(CheatsPanelActive, 30);
        Assert.That(after, Is.Not.EqualTo(before),
            "Pressing '/' did not toggle the Debug Cheats panel -- the ToggleCheats " +
            "keyboard binding may not be reaching DebugCheatsManager on this platform");

        // Restore so this fixture leaves the game state as it found it.
        api.KeyPress(new[] { KeyCode.Slash }, 10);
        api.Wait(300);
    }

    [Test]
    [Order(020)]
    public void T020_GivenGameplay_CheatsToggleTargetIsResolvable()
    {
        // Reliable companion to the keypress check: the DebugCheatsManager the '/' key
        // toggles is resolvable in gameplay. Reuses the session and a proven HPath; no
        // dependence on keypress timing.
        Assert.That(api.WaitForObject(DebugCheatsManager, 30), Is.True,
            "DebugCheatsManager (the keyboard toggle's target) did not resolve in gameplay");
    }
}
