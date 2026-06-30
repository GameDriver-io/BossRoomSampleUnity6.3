using gdio.unity_api.v2;

namespace GameDriverBossRoomTests;

[SetUpFixture]
public class GameDriverConnection
{
    private readonly ApiClient api = new();
    
    [OneTimeSetUp]
    public void BeforeAllFixtures()
    {
        api.Connect(GameDriverTest.Host, GameDriverTest.Port, true, 30);
    }

    [OneTimeTearDown]
    public void AfterAllFixtures()
    {
        try
        {
            api.StopEditorPlay();
            api?.Disconnect();
        } catch { /* best-effort */ }
    }
}
