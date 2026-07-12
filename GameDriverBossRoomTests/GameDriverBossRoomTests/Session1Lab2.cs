using gdio.common.objects;
using gdio.unity_api.v2;

namespace GameDriverBossRoomTests.Session1Lab2;

/// <summary>
/// Session 1, Lab 2 — boot smoke test.
///
/// Drives the actual Boss Room game: connects to the running Unity Editor (autoplay),
/// waits for the game to boot out of the "Startup" scene, confirms it lands on the
/// "MainMenu", and verifies nothing logged an error/exception on the way up.
///
/// Real Boss Room scene flow: Startup -> MainMenu -> CharSelect -> BossRoom -> PostGame.
/// This is the clean foundation the QaaS wrapper will extend.
/// </summary>
[TestFixture]
[Category("Boot")]
public class Session1Lab2_TestFixture : GameDriverTest
{
    private const string BootScene = "Startup";
    private const string MainMenuScene = "MainMenu";

    private bool sawConsoleError;
    private readonly List<string> logs = new();

    /// <summary>
    /// Runs after the base class has connected (NUnit runs base [OneTimeSetUp] first).
    /// Subscribes to the Unity log stream, then drives boot past the Startup scene.
    /// </summary>
    [OneTimeSetUp]
    public void BootGame()
    {
        api.UnityLoggedMessage += OnUnityLog;

        // Drive the real boot sequence: wait until we leave the Startup bootstrap scene.
        var waited = 0;
        while (api.GetSceneName() == BootScene && waited < 30_000)
        {
            api.Wait(200);
            waited += 200;
        }
    }

    private void OnUnityLog(object? sender, UnityLogEventEventArgs args)
    {
        sawConsoleError |= args.type is LogType.Error or LogType.Exception or LogType.Assert;
        logs.Add($"[{args.type}] {args.condition}");
    }

    [Test]
    [Order(010)]
    public void T010_GivenLaunch_WhenBooted_LeavesStartupScene()
    {
        Assert.That(api.GetSceneName(), Is.Not.EqualTo(BootScene));
    }

    [Test]
    [Order(020)]
    public void T020_GivenBoot_WhenComplete_ReachesMainMenu()
    {
        Assert.That(api.GetSceneName(), Is.EqualTo(MainMenuScene));
    }

    [Test]
    [Order(030)]
    public void T030_GivenBoot_WhenComplete_NoConsoleErrors()
    {
        if (sawConsoleError)
            Console.WriteLine(string.Join("\n", logs));
        Assert.That(sawConsoleError, Is.False);
    }

    [OneTimeTearDown]
    public void UnhookLog()
    {
        if (api != null)
            api.UnityLoggedMessage -= OnUnityLog;
    }
}
