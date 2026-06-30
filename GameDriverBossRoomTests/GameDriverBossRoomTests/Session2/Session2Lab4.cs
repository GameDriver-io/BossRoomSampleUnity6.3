#if false
using gdio.unity_api.v2;

namespace GameDriverBossRoomTests.Session2;

[TestFixture]
public class Session2Lab4TestFixture
{
    private ApiClient api;

    [OneTimeSetUp]
    public void ConnectApiAndLoadLevel()
    {
        api = new ApiClient();
        api.Connect("localhost", 19734, true);
        api.Wait(1000);

        Session2Helpers.LoadBossRoomLevel(api);
    }

    /*[OneTimeTearDown]
    public void DisconnectApiAndStopPlayMode()
    {
        api.StopEditorPlay();
        api.Disconnect();
    }*/


    [Test]
    public void GivenLoadedLevel_WhenGetPlayerHealth_GreaterThanZero()
    {
        //var hp = api.GetObjectFieldValue<int>("ENTER_HIERARCHY_PATH");
        //Assert.That(hp, Is.EqualTo(1234));
    }
}
#endif