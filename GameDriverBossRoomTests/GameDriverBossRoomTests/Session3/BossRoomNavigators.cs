using System.Diagnostics;
using gdio.common.objects;
using gdio.unity_api.v2;

namespace GameDriverBossRoomTests.Session3;

public static class BossRoomNavigators
{
    public static void WaitUntilBooted(ApiClient api) => WaitUntilLeftScene(api, "Startup");

    public static void WaitUntilLeftScene(ApiClient api, string sceneToLeave)
    {
        Assert.That(api, Is.Not.Null);
        Assert.That(sceneToLeave, Is.Not.Null);
        Assert.That(sceneToLeave, Is.Not.Empty);

        while (api.GetSceneName() == sceneToLeave)
        {
            api.Wait(200);
        }
        
        // and for loading screen to be gone
        api.WaitForObjectValue("/Test_LoadingScreen/fn:component('UnityEngine.CanvasGroup')", 
            "alpha", 
            0f);
    }
    
    public static void WaitUntilLoadedScene(ApiClient api, string sceneName)
    {
        Assert.That(api, Is.Not.Null);
        Assert.That(sceneName, Is.Not.Null);
        Assert.That(sceneName, Is.Not.Empty);

        while (api.GetSceneName() != sceneName)
        {
            api.Wait(200);
        }
        // and for loading screen to be gone
        api.WaitForObjectValue("/Test_LoadingScreen/fn:component('UnityEngine.CanvasGroup')", 
            "alpha", 
            0f);
    }
    
    public static void HostLocalIp(ApiClient api)
    {
        Assert.That(api, Is.Not.Null);

        api.ClickObject(MouseButtons.LEFT, "//Test_MainMenuLocalHostBtn", 1);
        api.WaitForEmptyInput();
        api.ClickObject(MouseButtons.LEFT, "//Test_IpSessionHostBtn", 1);
        api.WaitForEmptyInput();
        
        WaitUntilLoadedScene(api, "CharSelect");

        // Hide the network simulator overlay so it doesn't interfere with clicks
        api.CallMethod(
            "/*[contains(.,fn:type('Unity.Multiplayer.Tools.NetworkSimulator.Runtime.NetworkSimulator'))][0]//fn:component('Unity.BossRoom.Utils.NetworkSimulatorUIMediator')",
            "Hide");
    }

    public static void JoinLocalIp(ApiClient api)
    {
        Assert.That(api, Is.Not.Null);
        
        api.ClickObject(MouseButtons.LEFT, "//Test_MainMenuLocalHostBtn", 1);
        api.WaitForEmptyInput();
        api.ClickObject(MouseButtons.LEFT,
            "//*[@name='IPPopup']/*[@name='Tab Buttons']/*[@name='JoinButton']", 1);
        api.WaitForEmptyInput();
        api.ClickObject(MouseButtons.LEFT,
            "//*[@name='IPPopup']/*[@name='Tabs']/*[@name='IPJoiningUI']/*[@name='Join IP Connection Button']", 1);
        api.WaitForEmptyInput();
        
        WaitUntilLoadedScene(api, "CharSelect");
        
        // Hide the network simulator overlay so it doesn't interfere with clicks
        api.CallMethod(
            "/*[contains(.,fn:type('Unity.Multiplayer.Tools.NetworkSimulator.Runtime.NetworkSimulator'))][0]//fn:component('Unity.BossRoom.Utils.NetworkSimulatorUIMediator')",
            "Hide");
    }

    public static void SelectCharAndReady(ApiClient api, int charIndex)
    {
        Assert.That(api, Is.Not.Null);
        Assert.That(charIndex, Is.GreaterThanOrEqualTo(0));
        
        api.ClickObject(MouseButtons.LEFT,
            $"//Test_CharSelectSeat[{charIndex}]//*[contains(.,fn:type('UnityEngine.UI.Button'))]", 
            1);
        api.WaitForEmptyInput();
        api.Wait(200);
        
        api.CallMethod("/*/fn:component('Unity.BossRoom.Gameplay.GameState.ClientCharSelectState')", 
            "OnPlayerClickedReady");
    }

    public static void WaitForIdleGameplay(ApiClient api)
    {
        Assert.That(api, Is.Not.Null);
        
        WaitUntilLoadedScene(api, "BossRoom");
        api.Wait(500);
        // hide the cheats panel
        api.CallMethod("//*[@name='CheatsPopupPanel']", "SetActive", [false]);

        // also the how to play. these are both how the close buttons work directly.
        api.CallMethod("//*[@name='HowToPlayPopupPanel']", "SetActive", [false]);

        api.Wait(200);
    }

    public static readonly string LocalPlayerHPath =
        "//Player/fn:component('Unity.Netcode.NetworkObject')[@IsOwner='true']/@gameObject";
    public static readonly string RemotePlayerHPath =
        "//Player/fn:component('Unity.Netcode.NetworkObject')[@IsOwner='false']/@gameObject";

    public static void MoveLocalPlayer(ApiClient api)
    {
        Assert.That(api, Is.Not.Null);
        
        var playerStartScreenPosition = 
            api.GetObjectPosition(LocalPlayerHPath, CoordinateConversion.WorldToScreenPoint);
        
        var screenDestination =new Vector2(playerStartScreenPosition.x, playerStartScreenPosition.y+100); 
        api.Click(screenDestination, MouseButtons.LEFT,10);
        api.WaitForEmptyInput();
        
        api.Wait(1000);
    }

    public static Vector3 GetLocalPlayerWorldPosition(ApiClient api) => api.GetObjectPosition(LocalPlayerHPath);
    public static Vector3 GetRemotePlayerWorldPosition(ApiClient api) => api.GetObjectPosition(RemotePlayerHPath);

    public static void WaitForAllPlayersJoined(ApiClient server, int expectedPlayerCount)
    {
        Assert.That(server, Is.Not.Null);
        Assert.That(expectedPlayerCount, Is.GreaterThanOrEqualTo(1));
        
        // todo: can I count /Player from a hpath? 
        server.WaitForObject($"/Player[{expectedPlayerCount - 1}]");
    }
}
