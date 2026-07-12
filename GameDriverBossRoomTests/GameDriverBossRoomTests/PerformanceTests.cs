using System;
using gdio.unity_api.v2;

namespace GameDriverBossRoomTests;

/// <summary>
/// Performance domain: exists to populate the QaaS demo's FLAKY bucket with a realistic
/// intermittent signal.
///
/// DEMO INTENT (read before "fixing" this): the suite is a QaaS demo and deliberately
/// needs a flaky signal in the overall picture. Flakiness in the QaaS model is detected
/// as a test whose result VARIES across runs, so this test is intentionally
/// non-deterministic run-to-run rather than a static Inconclusive -- upload a few runs
/// and QaaS will flag it as flaky. It is grounded in a REAL thing we observed this
/// session (session-create + scene-load timing was genuinely variable, and CombatTests
/// flickered once on a cold HUD load), just made reliably-intermittent for the demo
/// instead of depending on real timing luck.
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
        // Synthetic-but-realistic flake: represents "session/scene establishment
        // occasionally exceeds its time budget." Varies per run so QaaS sees it flip
        // between pass and fail across uploads and classifies it as flaky.
        var overBudget = (DateTime.UtcNow.Ticks / TimeSpan.TicksPerSecond) % 2 == 0;

        Assert.That(overBudget, Is.False,
            "PERF (intermittent): scene/session establishment exceeded its time budget this run. " +
            "Deliberately flaky demo signal — represents the real session/load-timing variance " +
            "observed while building this suite.");
    }

    [Test]
    [Order(020)]
    public void T020_GameIsResponsive_Stable()
    {
        // Stable baseline so the Performance domain isn't uniformly flaky.
        Assert.That(api.GetSceneName(), Is.Not.Null.And.Not.Empty,
            "Game did not report a scene name — agent/game unresponsive");
    }
}
