namespace GameDriverBossRoomTests;

/// <summary>
/// Input.Mobile.Touch domain: NOT run in this environment -- there is no mobile
/// device or build available here, only the Mac Editor "PC" build.
///
/// This is a genuine coverage gap, not a hypothetical one: Assets/InputSystem/
/// PlayerActions.inputactions defines a real "Touch" control scheme with actual
/// bindings on the Player (gameplay) action map -- Target (&lt;Touchscreen&gt;/touch*/press),
/// Point (&lt;Touchscreen&gt;/touch*/position), plus debug toggles on touch3/touch4. A
/// mobile build would let these be exercised the same way MouseInputTests exercises
/// the Keyboard&amp;Mouse scheme. [Ignore] reports as "blocked" in the QaaS pipeline
/// (gdio.qaas.reporter.Capture.MapState maps TestStatus.Skipped -> "blocked"),
/// distinguishing this from a failure -- it's untested, not broken.
/// </summary>
[TestFixture]
[Category("Input.Mobile.Touch")]
[Ignore("No mobile device or build available in this environment (Mac Editor PC build only). " +
        "Real Touch bindings exist in PlayerActions.inputactions (Target/Point on touch press) -- " +
        "this is a genuine untested coverage gap, not a hypothetical placeholder.")]
public class TouchInputTests : GameDriverTest
{
    [Test]
    [Order(010)]
    public void T010_GivenGameplay_WhenGroundTapped_PlayerMoves()
    {
        // Would mirror MouseInputTests.T010 using api.Tap / TouchInput at the same
        // screen position, once a mobile build + device/simulator is available.
    }

    [Test]
    [Order(020)]
    public void T020_GivenGameplay_WhenEnemyTapped_TargetIsSet()
    {
        // Would verify the Target action's Touchscreen binding sets the player's
        // active target, mirroring the Mouse/leftButton path MouseInputTests covers.
    }
}
