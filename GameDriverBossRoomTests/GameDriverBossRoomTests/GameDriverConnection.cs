using gdio.unity_api.v2;

// QaaS results reporter, step 1 of 2: captures each test's outcome/duration/category
// as it runs. No-ops silently if QAAS_API_KEY is unset. See gdio.qaas.reporter README.
//
// Category convention: the reporter takes the FIRST [Category] on a test as its
// "domain" for QaaS metrics. Put exactly one [Category("...")] on each [TestFixture]
// (not on individual [Test]/[TestCase] methods) naming the functional area it
// exercises, e.g. [Category("CharSelect")]. This keeps domain unambiguous per fixture
// instead of depending on NUnit's class/method category ordering.
[assembly: gdio.qaas.reporter.GdioQaasCapture]

namespace GameDriverBossRoomTests;

/// <summary>
/// Runs once for the whole assembly. After every fixture has finished, uploads the
/// collected QaaS results, then connects briefly to leave any active network
/// session cleanly before stopping Editor play mode.
/// </summary>
[SetUpFixture]
public class GameDriverConnection
{
    [OneTimeTearDown]
    public void AfterAllFixtures()
    {
        // QaaS results reporter, step 2 of 2: upload once, at the very end of the run.
        gdio.qaas.reporter.Reporter.UploadCollected();

        var api = new ApiClient();
        try
        {
            api.Connect(GameDriverTest.Host, GameDriverTest.Port, true);

            // If the run's last fixture left the player actively hosting a session
            // (e.g. mid-BossRoom), StopEditorPlay() alone force-kills NetworkManager
            // instead of letting it shut down via the normal Quit path -- producing
            // "NetworkManagerSession.StopAsync: ... timed out" and "Destroy may not
            // be called from edit mode!" console errors as objects get torn down
            // mid Play-Mode-exit. EnsureAtMainMenu quits gracefully first.
            BossRoomFlow.EnsureAtMainMenu(api);

            api.StopEditorPlay();
            api.Disconnect();
        }
        catch (Exception) { /* best-effort */ }
    }
}
