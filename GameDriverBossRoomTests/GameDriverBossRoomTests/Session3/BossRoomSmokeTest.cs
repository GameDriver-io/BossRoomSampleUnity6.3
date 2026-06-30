using gdio.common.objects;

namespace GameDriverBossRoomTests.Session3;

[TestFixture]
public class BossRoomSmokeTest : GameDriverTest
{
    private bool isConsoleError;
    private readonly List<string> outputs = new();
    
    [OneTimeSetUp]
    public void InitLogging()
    {
        api.UnityLoggedMessage += OnUnityLog;
    }

    [OneTimeTearDown]
    public void CleanupLogging()
    {
        api.UnityLoggedMessage -= OnUnityLog;
    }

    [SetUp]
    public void SetUp()
    {
        isConsoleError = false;
        outputs.Clear();
        while(api.GetSceneName() == "Startup")
        {
            api.Wait(100);
        }
    }

    [Test, Order(010)]
    public void T010_GivenBootedToMenu_WhenCheckLogs_NoErrors()
    {
        api.Wait(500);
        Console.WriteLine(string.Join("\n", outputs));
        Assert.That(isConsoleError, Is.False);
    }

    [Test, Order(020)]
    public void T020_GivenMainMenu_WhenEnterLobbyViaDirectIPHost_NoErrors()
    {
        api.ClickObject(MouseButtons.LEFT, "//Test_MainMenuLocalHostBtn", 1);
        api.WaitForEmptyInput();
        
        api.ClickObject(MouseButtons.LEFT, "//Test_IpSessionHostBtn", 1);
        api.WaitForEmptyInput();
        api.WaitForObject("/*[fn:component(\"Unity.BossRoom.Gameplay.GameState.ClientCharSelectState\")]");
        
        api.Wait(1000);
        
        Console.WriteLine(string.Join("\n", outputs));
        Assert.That(isConsoleError, Is.False);
    }

    [Test, Order(030)]
    public void T030_GivenLobby_WhenSelectFirstCharacter_NoErrors()
    {
        // hide the network simulator, it's annoying
        api.CallMethod(
            "/*[contains(.,fn:type('Unity.Multiplayer.Tools.NetworkSimulator.Runtime.NetworkSimulator'))][0]//fn:component('Unity.BossRoom.Utils.NetworkSimulatorUIMediator')",
            "Hide");

        api.ClickObject(MouseButtons.LEFT, "//Test_CharSelectSeat[0]//*[contains(.,fn:type('UnityEngine.UI.Button'))]", 30);
        api.WaitForEmptyInput();
        
        Console.WriteLine(string.Join("\n", outputs));
        Assert.That(isConsoleError, Is.False);
    }

    private void OnUnityLog(object? sender, UnityLogEventEventArgs args)
    {
        isConsoleError |= args.type is LogType.Error or LogType.Exception or LogType.Assert;
        var msg = $"[{args.type}] {args.condition}";
        outputs.Add(msg);
    }
}
