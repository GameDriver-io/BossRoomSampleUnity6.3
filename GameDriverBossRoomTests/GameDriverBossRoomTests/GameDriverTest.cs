using gdio.unity_api.v2;
using NUnit.Framework.Interfaces;

namespace GameDriverBossRoomTests.Session1Lab2;

/// <summary>
/// Base class for all GameDriver Unreal tests.
/// Inherit from this to get automatic connect/disconnect and
/// failure screenshots with zero boilerplate.
///
/// Configuration (set as environment variables or override the properties):
///   GDIO_HOST  — game host (default: localhost)
///   GDIO_PORT  — game port (default: 19002)
/// </summary>
public abstract class GameDriverTest
{
    protected ApiClient api { get; private set; }
    
    // Override these properties in your subclass if you need
    // to hardcode values instead of using environment variables.
    protected virtual string Host =>
        Environment.GetEnvironmentVariable("GDIO_HOST") ?? "localhost";

    protected virtual int Port =>
        int.TryParse(Environment.GetEnvironmentVariable("GDIO_PORT"), out var p) ? p : 19002;

    [OneTimeSetUp]
    public virtual void GameDriverOneTimeSetUp()
    {
        api = new ApiClient();
        api.Connect(Host, Port, true, 30);
        OnConnected();
    }
    /// <summary>
    /// Called after Connect() succeeds. Override to load a level,
    /// set up input devices, or do any one-time test-fixture setup.
    /// </summary>
    protected virtual void OnConnected() { }
   
    /// <summary>
    /// Captures a screenshot on test failure and saves it to
    /// WorkDirectory/screenshots/FAIL_{testName}_{timestamp}.png
    /// This file is then picked up by the upload script and sent
    /// to the GameDriverGrid reporting dashboard.
    /// </summary>
    [TearDown]
    public void GameDriverTestTearDown()
    {
        if (TestContext.CurrentContext.Result.Outcome.Status == TestStatus.Failed)
        {
            var testName = TestContext.CurrentContext.Test.Name;
            var ts       = DateTime.Now.ToString("yyyyMMdd_HHmmss");
            var dir      = Path.Combine(TestContext.CurrentContext.WorkDirectory, "screenshots");
            Directory.CreateDirectory(dir);
            var filePath = Path.Combine(dir, $"FAIL_{testName}_{ts}.png");

            try
            {
                // storeInGameFolder=false transfers the file to this machine
                // overwriteExisting=true prevents stale screenshots from a previous run
                api?.CaptureScreenshot(filePath, false, true);
                TestContext.AddTestAttachment(filePath, "Failure screenshot");
            }
            catch
            {
                // Screenshot is best-effort — never fail a cleanup because of this
            }
        }
    }

    [OneTimeTearDown]
    public virtual void GameDriverOneTimeTearDown()
    {
        try { api?.Disconnect(); } catch { /* best-effort */ }
    }
}
