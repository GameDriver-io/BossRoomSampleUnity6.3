using gdio.unity_api.v2;

namespace GameDriverBossRoomTests;

[SetUpFixture]
public class GameDriverConnection
{
    [OneTimeTearDown]
    public void AfterAllFixtures()
    {
        var api = new ApiClient();
        try
        {
            // we're connecting just to call StopEditorPlay
            api.Connect(GameDriverTest.Host, GameDriverTest.Port, true);
            api.StopEditorPlay();
            api?.Disconnect();
        }
        catch (Exception) { /* best-effort */ }
    }
}
