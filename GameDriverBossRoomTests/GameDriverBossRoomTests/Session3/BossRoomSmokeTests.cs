using gdio.common.objects;

namespace GameDriverBossRoomTests.Session3;

[TestFixture]
public class BossRoomSmokeTests : GameDriverTest
{
    private bool isConsoleError;
    private readonly List<string> outputs = new();
    private void AssertThatLogsHaveNoErrors()
    {
        Console.WriteLine(string.Join("\n", outputs));
        Assert.That(isConsoleError, Is.False);
    }
    
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
        
        AssertThatLogsHaveNoErrors();
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
        
        AssertThatLogsHaveNoErrors();
    }

    [Test, Order(030)]
    public void T030_GivenLobby_WhenSelectFirstCharacter_NoErrors()
    {
        // hide the network simulator, it's annoying
        api.CallMethod(
            "/*[contains(.,fn:type('Unity.Multiplayer.Tools.NetworkSimulator.Runtime.NetworkSimulator'))][0]//fn:component('Unity.BossRoom.Utils.NetworkSimulatorUIMediator')",
            "Hide");

        api.ClickObject(MouseButtons.LEFT, "//Test_CharSelectSeat[0]//*[contains(.,fn:type('UnityEngine.UI.Button'))]", 1);
        api.WaitForEmptyInput();
        
        api.Wait(1000);
        
        AssertThatLogsHaveNoErrors();
    }

    [Test, Order(040)]
    public void T040_GivenSelectedCharInLobby_WhenStartingGame_EntersGameplayWithNoErrors()
    {
        api.ClickObject(MouseButtons.LEFT, "//Test_Next", 1);
        api.WaitForEmptyInput();

        while (api.GetObjectFieldValue<float>("/*[@name='LoadingScreen']/fn:component('UnityEngine.CanvasGroup')",
                   "alpha", 1)
               < 0.5f)
        {
            api.Wait(200);
        }
        
        while (api.GetSceneName() != "BossRoom")
        {
            api.Wait(500);
        }
        
        api.WaitForObjectValue("/*[@name='LoadingScreen']/fn:component('UnityEngine.CanvasGroup')", "alpha", 0f);
        
        // hide the cheats panel
        api.SetObjectFieldValue("//*[@name='CheatsPopupPanel']", "active", false);
        // also the how to play. these are both how the close buttons work directly.
        api.SetObjectFieldValue("//*[@name='HowToPlayPopupPanel']", "active", false);
        
        api.Wait(1000);
        
        AssertThatLogsHaveNoErrors();
    }

    [Test, Order(050)]
    public void T050_GivenIdleGame_WhenClickingToMove_CharacterPositionChanges()
    {
        var playerScreenPos = api.GetObjectPosition("//Player", CoordinateConversion.WorldToScreenPoint);
        var playerStartPos = api.GetObjectPosition("//Player");

        Vector2 toTheRight = new Vector2(playerScreenPos.x+200, playerScreenPos.y);
        api.Click(toTheRight, MouseButtons.LEFT, 10);
        
        api.Wait(500);
        
        var playerNewPosition = api.GetObjectPosition("//Player");
        float distanceTravelled = Vector3.Distance(playerNewPosition, playerStartPos);
        
        Assert.That(distanceTravelled, Is.GreaterThan(0.25f));
    }
    
    private void OnUnityLog(object? sender, UnityLogEventEventArgs args)
    {
        isConsoleError |= args.type is LogType.Error or LogType.Exception or LogType.Assert;
        var msg = $"[{args.type}] {args.condition}";
        outputs.Add(msg);
    }
}
