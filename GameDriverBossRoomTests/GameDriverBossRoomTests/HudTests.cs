using gdio.common.objects;
using gdio.unity_api.v2;

namespace GameDriverBossRoomTests;

/// <summary>
/// HUD domain: the Party HUD (Assets/Prefabs/UI/PartyHUD.prefab) and the Hero Emote
/// Bar's own button behavior (the Action Bar's emote *toggle* is covered by Combat).
///
/// Grounded in PartyHUD.cs: SetUIFromSlotData sets slot 0's (the local hero's) health
/// slider maxValue = CharacterClass.BaseHP.Value and value = current HitPoints, and
/// the name label from NetworkNameState. Object names ("Hero HP Slider", "Hero Name
/// (TMP)") are plain, uniquely-serialized names in the prefab -- not the shared-button-
/// template overrides that caused ambiguity elsewhere, so no scoping landmine here.
///
/// Health is asserted as value == maxValue (full at fresh spawn) rather than a
/// hardcoded number, so this test doesn't need to hardcode a specific class's BaseHP.
///
/// COVERAGE GAP: OneTimeSetUp only plays as BossRoomFlow.Tank -- Archer/Mage/Rogue's
/// HP sliders and portraits aren't independently verified. The value==maxValue
/// assertion is class-agnostic by design, but that only proves it works for
/// whichever class actually ran.
/// </summary>
[TestFixture]
[Category("HUD")]
public class HudTests : GameDriverTest
{
    // "value"/"maxValue" are Slider component properties, not GameObject properties --
    // the bare name query resolves to the GameObject, so the Slider component must be
    // chained in explicitly or the Agent can't find the field ("Couldn't find object
    // with identifier"). Same class of bug CombatTests hit on Button's "interactable".
    private const string HeroHpSlider =
        "//*[@name='Hero HP Slider']/fn:component('UnityEngine.UI.Slider')";
    private const string HeroNameText =
        "//*[@name='Hero Name (TMP)']/@gameObject/fn:component('TMPro.TextMeshProUGUI')/@text";

    [OneTimeSetUp]
    public void EnterGameplayAsTank()
    {
        BossRoomFlow.EnsureAtMainMenu(api);
        BossRoomFlow.StartNewGame(api);
        BossRoomFlow.SelectClassAndEnterGame(api, BossRoomFlow.Tank);
    }

    [Test]
    [Order(010)]
    public void T010_GivenFreshSpawn_HeroHealthSliderShowsFullHealth()
    {
        var value = api.GetObjectFieldValue<float>($"{HeroHpSlider}/@value", 30);
        var maxValue = api.GetObjectFieldValue<float>($"{HeroHpSlider}/@maxValue", 30);

        Assert.That(maxValue, Is.GreaterThan(0f), "Hero HP slider maxValue was not set from CharacterClass.BaseHP");
        Assert.That(value, Is.EqualTo(maxValue),
            "Hero HP slider did not show full health on fresh spawn");
    }

    [Test]
    [Order(020)]
    public void T020_GivenFreshSpawn_HeroNameLabelIsSet()
    {
        var name = api.GetObjectFieldValue<string>(HeroNameText, 30);
        Assert.That(name, Is.Not.Null.And.Not.Empty,
            "Hero name label was empty; NetworkNameState.Name should be set at spawn");
    }

    [Test]
    [Order(030)]
    public void T030_GivenEmotePanelOpen_WhenEmoteButtonClicked_PanelCloses()
    {
        BossRoomFlow.OpenEmotePanel(api);
        Assert.That(BossRoomFlow.IsEmotePanelActive(api), Is.True,
            "Precondition failed: Emote panel did not open");

        // HeroEmoteBar.OnButtonClicked fires the emote action then closes its own
        // panel (gameObject.SetActive(false)) regardless of which of the 4 emotes.
        api.ClickObject(MouseButtons.LEFT, BossRoomFlow.EmoteButton0, 30);
        api.WaitForEmptyInput();
        api.Wait(300);

        Assert.That(BossRoomFlow.IsEmotePanelActive(api), Is.False,
            "Emote panel did not close itself after an emote button was clicked");
    }
}
