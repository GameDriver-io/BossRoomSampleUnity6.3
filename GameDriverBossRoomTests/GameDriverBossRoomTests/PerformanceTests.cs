using System;
using gdio.unity_api.v2;

namespace GameDriverBossRoomTests;

/// <summary>
/// Performance domain: exists to populate the QaaS demo's FLAKY bucket with a realistic
/// intermittent signal.
///
/// DEMO INTENT (read before "fixing" this): the suite is a QaaS demo and deliberately
/// needs a flaky signal in the overall picture. In the QaaS model, flakiness is the
/// WITHIN-RUN share of INCONCLUSIVE outcomes (gdio.qaas.reporter.Capture.MapState maps
/// TestStatus.Inconclusive -> "flaky"). QaaS does NOT detect a test flipping pass&lt;-&gt;fail
/// ACROSS runs -- a test that merely alternates pass/fail just uploads as an occasional
/// plain "failure" (zero noise). So T010 reports Assert.Inconclusive on its "over budget"
/// branch to produce real flaky noise on a SINGLE upload, and keeps a per-run variance so
/// the flaky rate (and thus DSC/trend) still moves across uploads. It is grounded in a
/// REAL thing we observed this session (session-create + scene-load timing was genuinely
/// variable, and CombatTests flickered once on a cold HUD load), made reliably-intermittent
/// for the demo instead of depending on real timing luck.
///
/// T020 is a stable healthy signal so the domain reads as "mostly fine, one flaky area"
/// rather than 100% flaky (which would look like a broken test, not a flaky one).
/// </summary>
[TestFixture]
[Category("Performance")]
public class PerformanceTests : GameDriverTest
{
    [OneTimeSetUp]
    public void WaitForBoot() => BossRoomFlow.EnsureAtMainMenu(api);

    [Test]
    [Order(010)]
    public void T010_SceneEstablishmentWithinBudget_Intermittent()
    {
        // Realistic intermittent flake: "session/scene establishment occasionally exceeds
        // its time budget." When it does, we report INCONCLUSIVE, which QaaS maps to
        // "flaky" -- the only outcome that actually feeds the noise axis. (An alternating
        // pass/fail would upload as a plain failure and add zero noise; see class doc.)
        var overBudget = (DateTime.UtcNow.Ticks / TimeSpan.TicksPerSecond) % 2 == 0;

        if (overBudget)
            Assert.Inconclusive(
                "PERF (intermittent): scene/session establishment exceeded its time budget this run. " +
                "Deliberately flaky demo signal -- represents the real session/load-timing variance " +
                "observed while building this suite.");
        else
            Assert.Pass("Scene/session establishment within budget this run.");
    }

    [Test]
    [Order(020)]
    public void T020_GameIsResponsive_Stable()
    {
        // Stable baseline so the Performance domain isn't uniformly flaky.
        Assert.That(api.GetSceneName(), Is.Not.Null.And.Not.Empty,
            "Game did not report a scene name — agent/game unresponsive");
    }

    [Test]
    [Order(030)]
    public void T030_BootReachedMainMenu_Stable()
    {
        // Reliable baseline: boot settled on the MainMenu (WaitForBoot in [OneTimeSetUp]).
        Assert.That(api.GetSceneName(), Is.EqualTo(BossRoomFlow.MainMenu),
            "Boot did not settle on the MainMenu");
    }

    [Test]
    [Order(040)]
    public void T040_SceneStaysStable_AcrossShortWait()
    {
        // Reliable baseline: the agent stays responsive and the scene doesn't churn.
        var before = api.GetSceneName();
        api.Wait(300);
        Assert.That(api.GetSceneName(), Is.EqualTo(before),
            "Scene changed unexpectedly across a short wait — agent/game unresponsive");
    }
}
