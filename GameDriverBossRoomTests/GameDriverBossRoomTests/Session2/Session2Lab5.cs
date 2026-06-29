using gdio.unity_api.v2;
using NuGet.Frameworks;

namespace GameDriverBossRoomTests.Session2;


[TestFixture]
public class Session2Lab5TestFixture
{
    private ApiClient api;

    [OneTimeSetUp]
    public void ConnectApiAndLoadLevel()
    {
        api = new ApiClient();
        api.Connect("localhost", 19734, true);
        api.Wait(1000);
    }

    /*[OneTimeTearDown]
    public void DisconnectApiAndStopPlayMode()
    {
        api.StopEditorPlay();
        api.Disconnect();
    }*/



    [Test]
    public void GivenUIToolkitMenu_WhenSetPlayerName_UpdatesValue()
    {
        var playerNameHPath = 
            "ENTER_HIERARCHY_PATH_TO_PLAYER_NAME";
        api.SetObjectFieldValue($"{playerNameHPath}","text", "YOUR_NAME");
        var playerName = api.GetObjectFieldValue<string>($"{playerNameHPath}/@text");
        Assert.That(playerName, Is.EqualTo("Hero"));
    }
}
