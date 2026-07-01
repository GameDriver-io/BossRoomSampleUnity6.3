using gdio.unity_api.v2;
using NUnit.Framework.Interfaces;

namespace GameDriverBossRoomTests;

/// <summary>
/// Base class for all GameDriver tests.
/// Inherit from this to get automatic connect/disconnect
///
/// Configuration (set as environment variables or override the properties):
///   GDIO_HOST  — game host (default: localhost)
///   GDIO_PORT  — game port (default: 19734)
/// </summary>
public abstract class GameDriverTest
{
    protected ApiClient api { get; private set; }
    
    // Override these properties in your subclass if you need
    // to hardcode values instead of using environment variables.
    public static string Host =>
        Environment.GetEnvironmentVariable("GDIO_HOST") ?? "localhost";

    public static int Port =>
        int.TryParse(Environment.GetEnvironmentVariable("GDIO_PORT"), out var p) ? p : 19734;

    [OneTimeSetUp]
    public virtual void GameDriverOneTimeSetUp()
    {
        api = new ApiClient();
        api.Connect(Host, Port, true, 30);
    }
   
    [OneTimeTearDown]
    public virtual void GameDriverOneTimeTearDown()
    {
        try
        {
            api?.Disconnect();
        } catch { /* best-effort */ }
    }
}
