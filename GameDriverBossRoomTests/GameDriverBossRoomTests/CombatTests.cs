using gdio.common.objects;
using gdio.unity_api.v2;

namespace GameDriverBossRoomTests;

/// <summary>
/// Combat/Abilities domain: the Hero Action Bar (Assets/Prefabs/UI/Hero Action Bar.prefab),
/// which exposes the Basic Action, two Special actions, and the Emote bar toggle.
/// Button paths and the Emote panel toggle live in <see cref="BossRoomFlow"/> so the
/// HUD domain's tests reuse them (see that file for the Button0-3 naming landmine).
///
/// Scope note: this only covers what's safely groundable from source without live
/// verification -- that the action buttons wire up and become interactable once in
/// gameplay, that the Emote panel toggles correctly, and that pressing the Basic
/// Action doesn't error. It does NOT assert specific combat outcomes (damage dealt,
/// projectile hits, cooldown timing) -- those depend on live targeting/aim behavior
/// (SkillTriggerStyle.UI) that needs to be observed against a running Editor before
/// encoding as assertions.
///
/// Domain boundary: this fixture clicks the UI button directly (SkillTriggerStyle.UI)
/// to test the ability-wiring RESULT, deliberately independent of which physical
/// input device triggered it. Whether the raw platform input mechanism itself works
/// (mouse clicks, keyboard presses, touch, gamepad) is the Input.* domains' job --
/// see MouseInputTests, KeyboardInputTests, TouchInputTests, ConsoleInputTests.
///
/// COVERAGE GAP: OneTimeSetUp only plays as BossRoomFlow.Tank. Archer/Mage/Rogue
/// aren't exercised here (unlike ClassRollThroughTests, which rolls through all
/// four for CharSelect). Each class has a different Special1/Special2 kit, so this
/// fixture's action-bar/ability assertions are only verified for Tank's abilities.
/// Also observed live: the very first run of this fixture flickered once, most
/// likely HUD/action-bar load timing right after entering BossRoom (see
/// BossRoomFlow.SelectClassAndEnterGame's WaitForObject(ActionBarRoot) guard) --
/// not reproduced on a second run, not chased further per explicit instruction.
/// </summary>
[TestFixture]
[Category("Combat")]
public class CombatTests : GameDriverTest
{
    private bool sawConsoleError;
    private readonly List<string> logs = new();

    [OneTimeSetUp]
    public void EnterGameplayAsTank()
    {
        BossRoomFlow.EnsureAtMainMenu(api);
        BossRoomFlow.StartNewGame(api);
        BossRoomFlow.SelectClassAndEnterGame(api, BossRoomFlow.Tank);

        api.UnityLoggedMessage += OnUnityLog;
    }

    [OneTimeTearDown]
    public void Unhook()
    {
        if (api != null)
            api.UnityLoggedMessage -= OnUnityLog;
    }

    private void OnUnityLog(object? sender, UnityLogEventEventArgs args)
    {
        sawConsoleError |= args.type is LogType.Error or LogType.Exception or LogType.Assert;
        logs.Add($"[{args.type}] {args.condition}");
    }

    [Test]
    [Order(010)]
    public void T010_GivenGameplay_ActionButtonsAreActiveAndInteractable()
    {
        Assert.That(api.GetObjectFieldValue<bool>($"{BossRoomFlow.BasicActionButton}/@activeSelf", 30), Is.True,
            "Basic Action button was not active");
        // "interactable" is a property of the Button component, not the GameObject --
        // /@activeSelf resolves directly on the GameObject reference (that's valid),
        // but /@interactable needs an explicit component chain or the Agent can't
        // find it ("Couldn't find object with identifier").
        Assert.That(api.GetObjectFieldValue<bool>(
                $"{BossRoomFlow.BasicActionButton}/fn:component('UnityEngine.UI.Button')/@interactable", 30),
            Is.True, "Basic Action button was not interactable");

        Assert.That(api.GetObjectFieldValue<bool>($"{BossRoomFlow.Special1Button}/@activeSelf", 30), Is.True,
            "Special 1 button was not active");
        Assert.That(api.GetObjectFieldValue<bool>($"{BossRoomFlow.Special2Button}/@activeSelf", 30), Is.True,
            "Special 2 button was not active");
    }

    [Test]
    [Order(020)]
    public void T020_GivenGameplay_WhenEmoteBarClicked_PanelToggles()
    {
        var before = BossRoomFlow.IsEmotePanelActive(api);

        BossRoomFlow.OpenEmotePanel(api);
        Assert.That(BossRoomFlow.IsEmotePanelActive(api), Is.Not.EqualTo(before),
            "Emote panel did not toggle open");

        BossRoomFlow.CloseEmotePanel(api);
        Assert.That(BossRoomFlow.IsEmotePanelActive(api), Is.EqualTo(before),
            "Emote panel did not toggle back to its original state");
    }

    [Test]
    [Order(030)]
    public void T030_GivenGameplay_WhenBasicActionPressed_NoConsoleErrors()
    {
        sawConsoleError = false;
        logs.Clear();

        api.ClickObject(MouseButtons.LEFT, BossRoomFlow.BasicActionButton, 30);
        api.WaitForEmptyInput();
        api.Wait(500);

        if (sawConsoleError)
            Console.WriteLine(string.Join("\n", logs));
        Assert.That(sawConsoleError, Is.False);
    }
}
