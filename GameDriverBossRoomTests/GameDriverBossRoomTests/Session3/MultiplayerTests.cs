using System.Diagnostics;
using gdio.common.objects;
using gdio.unity_api.v2;

namespace GameDriverBossRoomTests.Session3;

[TestFixture]
public class MultiplayerTests : GameDriverTest
{
    private Process standaloneProcess;
    private ApiClient apiP2;
    private bool isConsoleError;
    private readonly List<string> outputs = [];

    [OneTimeSetUp]
    public void ConnectP2AndInitLogging()
    {
        // Launch the standalone build and connect P2 to it
        standaloneProcess = StandaloneProcess.Launch();
        apiP2 = new ApiClient();
        apiP2.Connect("localhost", 19735);
        api.UnityLoggedMessage += OnUnityLog;
    }

    [OneTimeTearDown]
    public void DisconnectP2AndCleanupLogging()
    {
        api.UnityLoggedMessage -= OnUnityLog;
        apiP2?.Disconnect();
        // Kill the standalone build process
        standaloneProcess?.Kill();
        standaloneProcess?.Dispose();
    }

    [SetUp]
    public void SetUp()
    {
        isConsoleError = false;
        outputs.Clear();
        // Wait for both clients to finish booting past the Startup scene
        while (api.GetSceneName() == "Startup")
            api.Wait(100);
        while (apiP2.GetSceneName() == "Startup")
            apiP2.Wait(100);
    }

    [Test, Order(010)]
    public void T010_GivenBothOnMainMenu_WhenHostCreatesAndClientJoins_HostSelectionVisibleOnClient()
    {
        // P1 (host/editor): open the IP popup and host a session
        api.ClickObject(MouseButtons.LEFT, "//Test_MainMenuLocalHostBtn", 1);
        api.WaitForEmptyInput();
        api.ClickObject(MouseButtons.LEFT, "//Test_IpSessionHostBtn", 1);
        api.WaitForEmptyInput();
        // Wait until P1 reaches the character select screen
        api.WaitForObject("/*[fn:component('Unity.BossRoom.Gameplay.GameState.ClientCharSelectState')]");

        // Hide the network simulator overlay so it doesn't interfere with clicks
        api.CallMethod(
            "/*[contains(.,fn:type('Unity.Multiplayer.Tools.NetworkSimulator.Runtime.NetworkSimulator'))][0]//fn:component('Unity.BossRoom.Utils.NetworkSimulatorUIMediator')",
            "Hide");

        
        api.Wait(500);

        // P2 (client/standalone): open the IP popup, switch to join tab, and connect
        apiP2.ClickObject(MouseButtons.LEFT, "//Test_MainMenuLocalHostBtn", 1);
        apiP2.WaitForEmptyInput();
        apiP2.ClickObject(MouseButtons.LEFT,
            "//*[@name='IPPopup']/*[@name='Tab Buttons']/*[@name='JoinButton']", 1);
        apiP2.WaitForEmptyInput();
        apiP2.ClickObject(MouseButtons.LEFT,
            "//*[@name='IPPopup']/*[@name='Tabs']/*[@name='IPJoiningUI']/*[@name='Join IP Connection Button']", 1);
        apiP2.WaitForEmptyInput();
        // Wait until P2 fully transitions into the character select scene
        while (apiP2.GetSceneName() != "CharSelect")
            apiP2.Wait(500);
        // Hide the network simulator on P2 as well
        apiP2.CallMethod(
            "/*[contains(.,fn:type('Unity.Multiplayer.Tools.NetworkSimulator.Runtime.NetworkSimulator'))][0]//fn:component('Unity.BossRoom.Utils.NetworkSimulatorUIMediator')",
            "Hide");
        
        // P1 selects the first character seat
        api.ClickObject(MouseButtons.LEFT,
            "//Test_CharSelectSeat[0]//*[contains(.,fn:type('UnityEngine.UI.Button'))]", 1);
        api.WaitForEmptyInput();
        
        // Wait for P1's selection to replicate and appear in P2's UI
        apiP2.WaitForObject("//*[@name='ActiveBkgnd' and ../@name='AnimationContainer' and @activeInHierarchy='true']", 15);

        // Assert P2 sees exactly one highlighted seat (P1's selection)
        var selectedOnClient = apiP2.GetObjectFieldValue<int>(
            "//*[@name='ActiveBkgnd' and ../@name='AnimationContainer' and @activeInHierarchy='true']/fn:count()");
        Assert.That(selectedOnClient, Is.EqualTo(1));
    }

    [Test, Order(020)]
    public void T020_GivenBothInLobby_WhenClientSelectsCharacter_BothSelectionsVisibleOnHost()
    {
        // P2 selects the second character seat
        apiP2.ClickObject(MouseButtons.LEFT,
            "//*[@name='CharacterSelectCanvas']//*[@name='PlayerSeats']//*[@name='PlayerSeat (1)']//*[@name='AnimationContainer']//*[@name='ClickInteract']", 30);
        apiP2.WaitForEmptyInput();
        // Give P2's selection time to replicate to P1
        api.Wait(500);

        // Assert P1 now sees two highlighted seats (P1's and P2's selections)
        var selectedOnHost = api.GetObjectFieldValue<int>(
            "//*[@name='ActiveBkgnd' and ../@name='AnimationContainer' and @activeInHierarchy='true']/fn:count()");
        Assert.That(selectedOnHost, Is.EqualTo(2));
    }

    [Test, Order(030)]
    public void T030_GivenBothInLobby_WhenBothReadyUp_EntersBossRoomWithNoErrors()
    {
        // Both players ready up via the ClientCharSelectState component
        api.CallMethod("/*/fn:component('Unity.BossRoom.Gameplay.GameState.ClientCharSelectState')", "OnPlayerClickedReady");
        apiP2.CallMethod("/*/fn:component('Unity.BossRoom.Gameplay.GameState.ClientCharSelectState')", "OnPlayerClickedReady");

        // Wait for both players to transition into the BossRoom gameplay scene
        while (api.GetSceneName() != "BossRoom")
            api.Wait(500);
        while (apiP2.GetSceneName() != "BossRoom")
            apiP2.Wait(500);

        // Wait for both loading screens to fade out
        api.WaitForObjectValue("/*[@name='LoadingScreen']/fn:component('UnityEngine.CanvasGroup')", "alpha", 0f);
        apiP2.WaitForObjectValue("/*[@name='LoadingScreen']/fn:component('UnityEngine.CanvasGroup')", "alpha", 0f);

        // Dismiss the cheats and how-to-play popups on both clients
        // CallMethod used instead of SetObjectFieldValue("active") — obsolete property, stripped in standalone
        api.CallMethod("//*[@name='CheatsPopupPanel']", "SetActive", [false]);
        api.CallMethod("//*[@name='HowToPlayPopupPanel']", "SetActive", [false]);
        apiP2.CallMethod("//*[@name='CheatsPopupPanel']", "SetActive", [false]);
        apiP2.CallMethod("//*[@name='HowToPlayPopupPanel']", "SetActive", [false]);

        api.Wait(1000);

        // Assert no errors were logged on P1 during the transition
        Console.WriteLine(string.Join("\n", outputs));
        Assert.That(isConsoleError, Is.False);
    }

    [Test, Order(040)]
    public void T040_GivenBothInGame_WhenHostMoves_MovementReflectedOnClient()
    {
        // Get P1's player screen position for clicking, and record start position as seen by P2
        var hostScreenPos = api.GetObjectPosition("/*[@name='PlayerAvatar0']", CoordinateConversion.WorldToScreenPoint);
        var hostStartPos = apiP2.GetObjectPosition("/*[@name='PlayerAvatar0']");

        // Click to the right of P1's character to trigger movement
        var toTheRight = new Vector2(hostScreenPos.x + 200, hostScreenPos.y);
        api.Click(toTheRight, MouseButtons.LEFT, 10);

        // Wait for movement to occur and replicate to P2
        api.Wait(1000);

        // Read P1's new position as seen by P2 and assert it has moved
        var hostEndPos = apiP2.GetObjectPosition("/*[@name='PlayerAvatar0']");
        var distanceTravelled = Vector3.Distance(hostEndPos, hostStartPos);

        Assert.That(distanceTravelled, Is.GreaterThan(0.5f));
    }

    private void OnUnityLog(object? sender, UnityLogEventEventArgs args)
    {
        isConsoleError |= args.type is LogType.Error or LogType.Exception or LogType.Assert;
        outputs.Add($"[{args.type}] {args.condition}");
    }
}
