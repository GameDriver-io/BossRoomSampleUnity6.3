using System.IO.Pipes;
using gdio.common.objects;
using gdio.unity_api.v2;

namespace GameDriverBossRoomTests.Session2;

public static class Session2Helpers
{
    private const string UICanvas = "/Untagged[@name='UI Canvas']";
    private const string IPStartButton = $"{UICanvas}/Untagged[@name='IP Start Button']";
    private const string IPPopup = $"{UICanvas}/Untagged[@name='IPPopup']";
    private const string HostIPConnectionButton =
        $"{IPPopup}/Untagged[@name='Tabs']/Untagged[@name='IPHostingUI']/Untagged[@name='Host IP Connection Button']";

    private const string CharacterSelectCanvas = "/Untagged[@name='CharacterSelectCanvas']";
    private const string PlayerSeats = $"{CharacterSelectCanvas}/Untagged[@name='PlayerSeats']";
    private const string CurrentClassText =
        $"{CharacterSelectCanvas}/Untagged[@name='ClassInfoBox']/Untagged[@name='HideAllTheseWhenNoClass']" +
        "/Untagged[@name='CurrentClass (TMP)']/@gameObject/fn:component('TMPro.TextMeshProUGUI')/@text";

    private const string ReadyButton =
        "/*[@name='CharacterSelectCanvas']/*[@name='ClassInfoBox']/*[@name='DecorativeFrame']/*[@name='Ready Btn']";

    private const string NetworkSimulatorCancelButton =
        "/Untagged[@name='NetworkSimulator']/Untagged[@name='NetworkSimulatorUICanvas']" +
        "/Untagged[@name='NetworkSimulatorPopupPanel']/Untagged[@name='Cancel Button']";

    public static void ClickIPStartButton(ApiClient api)
    {
        Assert.That(api.GetSceneName(), Is.EqualTo("MainMenu"),
            "Expected to be on the MainMenu scene before clicking IP Start Button");

        api.ClickObject(MouseButtons.LEFT,IPStartButton, 30);
        api.WaitForEmptyInput();

        var popupActive = api.GetObjectFieldValue<bool>($"{IPPopup}/@activeSelf", 30);
        Assert.That(popupActive, Is.True, "IPPopup did not open after clicking IP Start Button");
    }

    public static void ClickHostIPConnectionButton(ApiClient api)
    {
        api.ClickObject(MouseButtons.LEFT,HostIPConnectionButton, 30);
        api.WaitForEmptyInput();
        
        string sceneName = "";
        for (int i = 0; i < 30; i++)
        {
            sceneName = api.GetSceneName();
            if (sceneName == "CharSelect") break;
            api.Wait(1000);
        }
        Assert.That(sceneName, Is.EqualTo("CharSelect"),
            "CharSelect scene did not load after clicking Host Ip");
    }

    private const string CheatsCancelButton =
        "/*[@name='BossRoomHudCanvas']/*[@name='CheatsPopupPanel']/*[@name='Cancel Button']";

    private const string HowToPlayConfirmButton =
        "/*[@name='BossRoomHudCanvas']/*[@name='HowToPlayPopupPanel']/*[@name='Confirmation Button']";

    public static void ClickReadyButton(ApiClient api)
    {
        api.Wait(500);
        api.ClickObject(MouseButtons.LEFT,ReadyButton, 30);
        api.WaitForEmptyInput();

        string sceneName = "";
        for (int i = 0; i < 30; i++)
        {
            sceneName = api.GetSceneName();
            if (sceneName == "BossRoom") break;
            api.Wait(1000);
        }
        Assert.That(sceneName, Is.EqualTo("BossRoom"),
            "BossRoom scene did not load after clicking Ready");
    }

    public static void DismissNetworkSimulatorPopup(ApiClient api)
    {
        api.ClickObject(MouseButtons.LEFT,NetworkSimulatorCancelButton, 30);
        api.WaitForEmptyInput();
    }

    public static void DismissCheatsPopup(ApiClient api)
    {
        api.ClickObject(MouseButtons.LEFT,CheatsCancelButton, 30);
        api.WaitForEmptyInput();
    }

    public static void DismissHowToPlayPopup(ApiClient api)
    {
        api.ClickObject(MouseButtons.LEFT,HowToPlayConfirmButton, 30);
        api.WaitForEmptyInput();
    }

    /// <summary>
    /// Selects a hero class by its zero-based position from the left (0 = first, 1 = second, etc.).
    /// Returns the class name shown in the ClassInfoBox after selection.
    /// </summary>
    public static string SelectHeroByIndex(ApiClient api, int index)
    {
        Assert.That(api.GetSceneName(), Is.EqualTo("CharSelect"),
            "Expected to be on the CharSelect scene before selecting a hero");

        string seatPath = $"{PlayerSeats}/Untagged[@name='PlayerSeat ({index})']/Untagged[@name='AnimationContainer']/Untagged[@name='ClickInteract']";
        api.ClickObject(MouseButtons.LEFT, seatPath, 30);
        api.WaitForEmptyInput();

        string className = api.GetObjectFieldValue<string>(CurrentClassText, 30);
        Assert.That(className, Is.Not.Null.And.Not.Empty,
            $"No class was selected after clicking PlayerSeat ({index})");

        return className;
    }

    public static void WaitForLoadingScreenComplete(ApiClient api)
    {
        bool done = api.WaitForObjectValue(
            "/*[@name='LoadingScreen']/fn:component('UnityEngine.CanvasGroup')",
            "alpha", 0f, true, 30);
        Assert.That(done, Is.True,
            "Loading screen did not finish (CanvasGroup alpha never reached 0)");
    }

    public static void LoadBossRoomLevel(ApiClient api)
    {
        ClickIPStartButton(api);
        ClickHostIPConnectionButton(api);
        DismissNetworkSimulatorPopup(api);
        SelectHeroByIndex(api, 1);
        ClickReadyButton(api);
        WaitForLoadingScreenComplete(api);
        DismissCheatsPopup(api);
        DismissHowToPlayPopup(api);
    }
}
