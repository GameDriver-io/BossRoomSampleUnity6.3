using System.Collections;
using gdio.common.objects;

namespace GameDriverBossRoomTests.Session3;

class CharactersEnumerator : IEnumerable<int>
{
    public IEnumerator<int> GetEnumerator() => Enumerable.Range(0, 8).GetEnumerator();
    IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
}

[TestFixtureSource(typeof(CharactersEnumerator))]
public class CharacterSmokeTests : GameDriverTest
{
    private LogTracker logs = new();
    private int charIndex;

    public CharacterSmokeTests(int index)
    {
        charIndex = index;
    }

    [OneTimeSetUp]
    public void OneTimeSetUp()
    {
        logs.OnOneTimeSetUp(api);
        
        api.WaitForObjectValue("/*[@name='LoadingScreen']/fn:component('UnityEngine.CanvasGroup')", "alpha", 0f);
        
        api.Wait(500);

        api.CallMethod("/*/fn:component('Unity.BossRoom.Gameplay.GameState.ClientMainMenuState')",
            "OnDirectIPClicked");
        
        api.ClickObject(MouseButtons.LEFT, "//Test_IpSessionHostBtn", 1);
        api.WaitForEmptyInput();
        api.WaitForObject("/*[fn:component(\"Unity.BossRoom.Gameplay.GameState.ClientCharSelectState\")]");
                
        api.WaitForObjectValue("/*[@name='LoadingScreen']/fn:component('UnityEngine.CanvasGroup')", "alpha", 0f);
        
        // hide the network simulator, it's annoying
        api.CallMethod(
            "/*[contains(.,fn:type('Unity.Multiplayer.Tools.NetworkSimulator.Runtime.NetworkSimulator'))][0]//fn:component('Unity.BossRoom.Utils.NetworkSimulatorUIMediator')",
            "Hide");
    }

    [SetUp]
    public void SetUp()
    {
        logs.OnSetUp();
    }

    [OneTimeTearDown]
    public void OneTimeTearDown()
    {
        logs.OnOneTimeTearDown(api);
    }

    [Test, Order(010)]
    public void T010_GivenLobby_SelectCharacter()
    {
        api.ClickObject(MouseButtons.LEFT, 
            $"//Test_CharSelectSeat[{charIndex}]//*[contains(.,fn:type('UnityEngine.UI.Button'))]", 1);
        api.WaitForEmptyInput();
        
        api.Wait(1000);
        
        logs.AssertThatClearOfErrors();
    }

    [Test, Order(020)]
    public void T020_GivenSelectedCharInLobby_WhenStartingGame_EntersGameplayWithNoErrors()
    {
        api.CallMethod("/*/fn:component('Unity.BossRoom.Gameplay.GameState.ClientCharSelectState')", "OnPlayerClickedReady");
        
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
        
        logs.AssertThatClearOfErrors();
    }

    [Test, Order(030)]
    public void T030_GivenIdleGame_WhenReturningToMenu_ReturnsWithNoErrors()
    {
        api.CallMethod(
            "/*[fn:component('Unity.BossRoom.Gameplay.UI.UISettingsCanvas')][0]//fn:component('Unity.BossRoom.Gameplay.UI.UIQuitPanel')",
            "Quit");
        
        while (api.GetSceneName() == "BossRoom")
        {
            api.Wait(500);
        }
        
        api.WaitForObjectValue("/*[@name='LoadingScreen']/fn:component('UnityEngine.CanvasGroup')", "alpha", 0f);
        
        api.Wait(1000);
        
        logs.AssertThatClearOfErrors();
    }
}
