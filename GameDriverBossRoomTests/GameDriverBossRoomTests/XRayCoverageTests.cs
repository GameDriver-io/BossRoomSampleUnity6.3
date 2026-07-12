using gdio.common.objects;
using gdio.qaas.coverage;
using gdio.qaas.coverage.analysis;
using gdio.qaas.coverage.recording;

namespace GameDriverBossRoomTests;

/// <summary>
/// Coverage X-Ray demo (gdio.qaas.coverage): exercises ONE seat at Character Select through a
/// <see cref="RecordingApiClient"/> -- a drop-in composition wrapper over the same ApiClient this
/// whole suite already uses, which logs every node-resolving call it makes -- then snapshots which
/// interactive nodes were touched vs missed into a self-contained HTML overlay + JSON report.
///
/// Deliberately NOT built on <see cref="GameDriverTest"/>: that base class's `api` field is a plain
/// ApiClient, and RecordingApiClient isn't a subclass of it (composition, not inheritance -- see its
/// own doc comment), so this fixture owns its connection lifecycle directly.
///
/// Navigation to CharSelect reuses BossRoomFlow.StartNewGame via <see cref="RecordingApiClient.Inner"/>
/// (the real, unwrapped client) -- getting there isn't the interesting part, and BossRoomFlow's own
/// static helpers are typed to plain ApiClient, not this wrapper. Only the ONE seat click below goes
/// through the wrapper itself, so only it counts as "covered". Run this fixture alone for the clean
/// demo: the other 7 seats and the rest of the CharSelect UI light up uncovered in the overlay.
///
/// This is a port of an earlier, abandoned spike (a separate net472 demo suite, `Boss_Room_Demo_Tests`,
/// stuck on a New Input System input bug that this suite's own gotchas -- right-click attack, no
/// NavAgentMoveToPoint, fully-qualified Sessions UI paths -- already solve) -- rebuilt here on top of
/// this suite's proven-working navigation instead of that abandoned one's.
/// </summary>
[TestFixture]
[Category("CoverageXRay")]
public class XRayCoverageTests
{
    private RecordingApiClient api = null!;

    private const string TankSeatClickTarget =
        "/Untagged[@name='CharacterSelectCanvas']/Untagged[@name='PlayerSeats']" +
        "/Untagged[@name='PlayerSeat (0)']/Untagged[@name='AnimationContainer']/Untagged[@name='ClickInteract']";

    [OneTimeSetUp]
    public void Connect()
    {
        api = new RecordingApiClient();
        api.Connect(GameDriverTest.Host, GameDriverTest.Port, true, 30);
    }

    [OneTimeTearDown]
    public void Disconnect()
    {
        try
        {
            BossRoomFlow.EnsureAtMainMenu(api.Inner);
            api.StopEditorPlay();
        }
        catch { /* best-effort, mirrors GameDriverConnection */ }
        try { api?.Disconnect(); } catch { /* best-effort */ }
    }

    [Test]
    [Order(010)]
    public void T010_CharSelect_OneSeatClicked_XRaySnapshot()
    {
        BossRoomFlow.StartNewGame(api.Inner);

        // The one interaction this snapshot measures -- through the recording wrapper, not .Inner,
        // so it lands in the covered set. Same click target BossRoomFlow.SelectSeat uses internally.
        api.MouseMoveToObject(TankSeatClickTarget, 30);
        api.Wait(500);
        api.ClickObject(MouseButtons.LEFT, TankSeatClickTarget, 30);
        api.WaitForEmptyInput();
        api.Wait(500);

        CoverageReport? report = XRay.Capture(api, "charselect");

        Assert.That(report, Is.Not.Null, "X-Ray snapshot did not produce a report.");
        TestContext.WriteLine($"[XRay] {report!.CoveredCount}/{report.CoverableCount} interactive nodes covered, " +
                              $"{report.UncoveredCount} uncovered. Overlay: {report.HtmlFile}");
    }
}
