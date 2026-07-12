using gdio.common.objects;
using gdio.unity_api.v2;

namespace GameDriverBossRoomTests;

/// <summary>
/// Reusable Boss Room navigation flow, driven purely through the game UI.
///
/// HierarchyPaths and the click sequence are lifted from the verified Session 2 lab
/// helpers in the sibling GameDriverBossRoomTests project (same Unity 6.3 Boss Room build),
/// so every path here has been exercised against a running game.
///
/// Scene flow: MainMenu -> (create session) -> CharSelect -> (ready) -> BossRoom.
/// </summary>
public static class BossRoomFlow
{
    // ---- Scene names -------------------------------------------------------
    public const string MainMenu = "MainMenu";
    public const string CharSelect = "CharSelect";
    public const string BossRoom = "BossRoom";

    // ---- MainMenu (Unity Sessions flow) ------------------------------------
    // This build uses Unity's Sessions UI (Create Session -> Create), not the old
    // direct-IP popup.
    //   - "Session Start Button" (label reads "Start with Lobby") opens the
    //     Sessions UI -- ClientMainMenuState.OnStartClicked() calls
    //     SessionUIMediator.ToggleJoinSessionUI(), so it opens on the JOIN tab.
    //   - Switching to the Create tab: click "CreateButton", fully scoped under
    //     UI Canvas/SessionPopup/Tab Buttons. A bare "//*[@name='CreateButton']"
    //     query is ambiguous (that literal name isn't unique in the wider scene),
    //     so the fully-qualified path removes that risk. (Earlier attempts tried
    //     CallMethod(SessionUIMediator, "ToggleCreateSessionUI") to dodge a suspected
    //     raycast-blocker issue on the click -- that didn't fully resolve an
    //     intermittent failure either, so simplified back to a real, precisely-scoped
    //     click per direct hierarchy inspection.)
    //   - SessionCreationUI.Show()/Hide() only toggle its CanvasGroup's alpha and
    //     blocksRaycasts -- NOT gameObject.SetActive -- so activeInHierarchy is
    //     ALWAYS true regardless of which tab is selected and is not a meaningful
    //     readiness signal. Checking the CanvasGroup's own alpha is.
    //   - The panel's actual confirm button is named "Create Session Button",
    //     fully scoped under UI Canvas/SessionPopup/Tabs/SessionCreationUI (same
    //     reasoning as CreateButton -- avoid a bare, potentially-ambiguous name
    //     query). Its MonoBehaviour's OnClick fires SessionCreationUI.OnCreateClick
    //     (matched by script GUID, since the serialized listener's cached type
    //     name is stale).
    // The menu signs in anonymously on load; the LoadingSpinner hides once
    // services are ready, and the session panel can't be created before then.
    private const string SessionStartButton = "//*[@name='Session Start Button']";
    private const string SignInSpinner = "//*[@name='LoadingSpinner']";

    private const string CreateTabButton =
        "/*[@name='UI Canvas']/*[@name='SessionPopup']/*[@name='Tab Buttons']/*[@name='CreateButton']";
    private const string CreateSessionButton =
        "/*[@name='UI Canvas']/*[@name='SessionPopup']/*[@name='Tabs']" +
        "/*[@name='SessionCreationUI']/*[@name='Create Session Button']";
    private const string SessionCreationUIComponent =
        "//*[fn:component('Unity.BossRoom.Gameplay.UI.SessionCreationUI')]" +
        "/fn:component('Unity.BossRoom.Gameplay.UI.SessionCreationUI')";
    private const string SessionCreationUICanvasGroup = $"{SessionCreationUIComponent}/@m_CanvasGroup";

    // ---- CharSelect --------------------------------------------------------
    private const string CharacterSelectCanvas = "/Untagged[@name='CharacterSelectCanvas']";
    private const string PlayerSeats = $"{CharacterSelectCanvas}/Untagged[@name='PlayerSeats']";
    private const string ClassInfoBox = $"{CharacterSelectCanvas}/Untagged[@name='ClassInfoBox']";
    private const string ClassInfoBoxDetails = $"{ClassInfoBox}/Untagged[@name='HideAllTheseWhenNoClass']";
    private const string CurrentClassText =
        $"{ClassInfoBoxDetails}/Untagged[@name='CurrentClass (TMP)']" +
        "/@gameObject/fn:component('TMPro.TextMeshProUGUI')/@text";
    private const string ReadyButton =
        $"{ClassInfoBox}/Untagged[@name='DecorativeFrame']/Untagged[@name='Ready Btn']";

    // ---- BossRoom HUD popups ----------------------------------------------
    private const string BossRoomHudCanvas = "/*[@name='BossRoomHudCanvas']";
    private const string CheatsPopupPanel = $"{BossRoomHudCanvas}/*[@name='CheatsPopupPanel']";
    private const string CheatsCancelButton = $"{CheatsPopupPanel}/*[@name='Cancel Button']";
    private const string HowToPlayPopupPanel = $"{BossRoomHudCanvas}/*[@name='HowToPlayPopupPanel']";
    private const string HowToPlayConfirmButton = $"{HowToPlayPopupPanel}/*[@name='Confirmation Button']";

    private const string LoadingScreen = "/*[@name='LoadingScreen']/fn:component('UnityEngine.CanvasGroup')";

    // ---- Settings panel (Audio + Graphics domains) -------------------------
    // Anchored by COMPONENT TYPE, not GameObject name: SettingsPanelCanvas.prefab
    // has two *different* objects both literally named "Settings Button" -- the
    // real gear icon, and (via an unrenamed copy-pasted button template) the
    // quality-cycle button nested inside the panel. Name-based queries here are
    // ambiguous; fn:component sidesteps it and is robust to future renames.
    public const string SettingsCanvas =
        "//*[fn:component('Unity.BossRoom.Gameplay.UI.UISettingsCanvas')]" +
        "/fn:component('Unity.BossRoom.Gameplay.UI.UISettingsCanvas')";
    private const string SettingsPanelActive = $"{SettingsCanvas}/@m_SettingsPanelRoot/@activeSelf";

    // ---- Hero Action Bar / Emote Bar (Combat + HUD domains) ----------------
    // LANDMINE: "Hero Action Bar.prefab" AND "Hero Emote Bar.prefab" *each* have
    // their own child buttons literally named Button0/Button1/Button2/Button3.
    // Both are instantiated simultaneously in the BossRoom HUD, so a bare
    // "//*[@name='Button0']" query is ambiguous between "basic action" and
    // "first emote". Every button path below is scoped under its own root name.
    // Action Bar button -> field mapping confirmed via HeroActionBar's own
    // serialized field bindings (fileID cross-reference, not guessed):
    //   m_BasicActionButton -> Button0, m_SpecialAction1Button -> Button1,
    //   m_SpecialAction2Button -> Button2, m_EmoteBarButton -> Button3.
    public const string ActionBarRoot = "//*[@name='Hero Action Bar']";
    public const string BasicActionButton = $"{ActionBarRoot}//*[@name='Button0']";
    public const string Special1Button = $"{ActionBarRoot}//*[@name='Button1']";
    public const string Special2Button = $"{ActionBarRoot}//*[@name='Button2']";
    public const string EmoteBarToggleButton = $"{ActionBarRoot}//*[@name='Button3']";

    public const string EmoteBarRoot = "//*[@name='Hero Emote Bar']";
    public const string EmoteButton0 = $"{EmoteBarRoot}//*[@name='Button0']";

    private const string HeroActionBarComponent =
        "//*[fn:component('Unity.BossRoom.Gameplay.UI.HeroActionBar')]" +
        "/fn:component('Unity.BossRoom.Gameplay.UI.HeroActionBar')";
    private const string EmotePanelActive = $"{HeroActionBarComponent}/@m_EmotePanel/@activeSelf";

    public static void OpenEmotePanel(ApiClient api)
    {
        if (!api.GetObjectFieldValue<bool>(EmotePanelActive, 30))
            api.ClickObject(MouseButtons.LEFT, EmoteBarToggleButton, 30);
        api.WaitForEmptyInput();
        api.Wait(200);
    }

    public static void CloseEmotePanel(ApiClient api)
    {
        if (api.GetObjectFieldValue<bool>(EmotePanelActive, 30))
            api.ClickObject(MouseButtons.LEFT, EmoteBarToggleButton, 30);
        api.WaitForEmptyInput();
        api.Wait(200);
    }

    public static bool IsEmotePanelActive(ApiClient api) =>
        api.GetObjectFieldValue<bool>(EmotePanelActive, 30);

    // ---- NPC scanning + direct kill (GameOutcome + Combat.MobDamage domains) ---
    // Enumerates ServerCharacter NPCs and can kill them by zeroing
    // NetworkHealthState.HitPoints.Value directly -- deliberately NOT via
    // DebugCheatsManager. Cheats are compiled OUT of release builds entirely
    // (#if UNITY_EDITOR || DEVELOPMENT_BUILD in DebugCheatsManager.cs), so
    // automation that depends on a cheat menu existing would silently do nothing
    // against a real build, and depending on cheats at all risks one shipping
    // reachable in the wild. Setting the public NetworkVariable directly works
    // against any build the Agent can attach to, cheat menu or not.
    private const string ServerCharacterComponent =
        "fn:component('Unity.BossRoom.Gameplay.GameplayObjects.Character.ServerCharacter')";
    private const string NetworkHealthStateComponent =
        "fn:component('Unity.BossRoom.Gameplay.GameplayObjects.NetworkHealthState')";
    private const string ServerCharacterQuery = $"//*[{ServerCharacterComponent}]";

    // Verified pattern from the sibling GameDriverBossRoomTests project (same Boss
    // Room build): the local player's owned NetworkObject, tagged "Player".
    public const string LocalPlayerHPath =
        "//Player/fn:component('Unity.Netcode.NetworkObject')[@IsOwner='true']/@gameObject";

    // IMPORTANT: ServerCharacter.CharacterType and .LifeState are both custom,
    // game-defined enums (CharacterTypeEnum / Unity.BossRoom.Gameplay.GameplayObjects.
    // LifeState). Confirmed live: reading either freezes the Agent with
    // "Serialization failure ... Implement gdio.plugin.serializer.ICustomSerializer" --
    // this fails Agent-side during wire encoding, before any client-requested type
    // (int, string, whatever) even comes into play, so there's no valueType trick
    // around it. Adding a custom serializer would mean modifying Boss Room's own
    // code, out of scope here. So neither property is ever read remotely -- NPCs are
    // identified via ServerCharacter.IsNpc (bool) and "alive" via HitPoints > 0 (int),
    // both plain primitives with no serialization issue.
    //
    // IsNpc alone doesn't distinguish Imp vs ImpBoss vs VandalImp, but every fixture
    // that calls FindAliveNpcs() deliberately only ever has ONE kind of NPC alive at
    // a time (MobDamageTests calls SpawnEnemy only; GameOutcomeTests calls SpawnBoss
    // only, never both in the same run), so the ambiguity doesn't arise in practice.
    // If a future test spawns both together, this filter needs to name-match instead
    // (e.g. GameObject name "Imp(Clone)" vs "ImpBoss(Clone)") once verified live.

    /// <summary>HierarchyPaths of every alive (HitPoints > 0) NPC ServerCharacter.</summary>
    public static List<string> FindAliveNpcs(ApiClient api)
    {
        var matches = new List<string>();
        var candidates = api.GetObjectList(ServerCharacterQuery, true, 30);
        if (candidates == null)
            return matches;

        foreach (var candidate in candidates)
        {
            var isNpc = api.GetObjectFieldValue<bool>(
                $"{candidate.HierarchyPath}/{ServerCharacterComponent}/@IsNpc", 10);
            if (!isNpc)
                continue;

            var hitPoints = api.GetObjectFieldValue<int>(
                $"{candidate.HierarchyPath}/{ServerCharacterComponent}/@HitPoints", 10);
            if (hitPoints <= 0)
                continue;

            matches.Add(candidate.HierarchyPath);
        }

        return matches;
    }

    /// <summary>
    /// HierarchyPath of the alive ImpBoss NPC, or null. Distinguishes the boss from
    /// regular trash mobs (Imp/VandalImp) by GameObject name -- CharacterType is a
    /// custom enum and can't be read remotely (see the comment above), and trash-mob
    /// EnemyPortals near the BossRoom entrance spawn real Imps immediately on scene
    /// entry (confirmed live), so "any alive NPC" no longer implies "the boss".
    /// </summary>
    public static string? FindAliveBoss(ApiClient api)
    {
        var candidates = api.GetObjectList(ServerCharacterQuery, true, 30);
        if (candidates == null)
            return null;

        foreach (var candidate in candidates)
        {
            if (!candidate.Name.Contains("ImpBoss"))
                continue;

            var hitPoints = api.GetObjectFieldValue<int>(
                $"{candidate.HierarchyPath}/{ServerCharacterComponent}/@HitPoints", 10);
            if (hitPoints > 0)
                return candidate.HierarchyPath;
        }

        return null;
    }

    public static bool IsBossAlive(ApiClient api) => FindAliveBoss(api) != null;

    /// <summary>Kills a single ServerCharacter by zeroing its HitPoints directly.</summary>
    public static void KillDirectly(ApiClient api, string serverCharacterHPath) =>
        api.SetObjectFieldValue($"{serverCharacterHPath}/{NetworkHealthStateComponent}/@HitPoints", "Value", 0);

    // ---- Return-to-menu -----------------------------------------------------
    // Driven via real clicks (not CallMethod) -- confirmed live. "Quit Button" is
    // the top-bar icon that opens QuitPanel; the actual confirm action inside the
    // panel is "Confirm Button" (scoped under QuitPanel: that name isn't globally
    // unique -- PopupPanel.prefab and IPPopup.prefab both reuse it too).
    private const string QuitButton = "/*[@name='SettingsPanelCanvas']/*[@name='Quit Button']";
    private const string QuitConfirmButton =
        "/*[@name='SettingsPanelCanvas']/*[@name='QuitPanel']/*[@name='Confirm Button']";

    // GameOutcomeTests wins a match and lands on PostGame; EnsureAtMainMenu must be
    // able to recover from there too, or any fixture run after a win-condition test
    // hangs waiting for MainMenu that never arrives on its own (PostGameUI only
    // returns to MainMenu on an explicit button click -- nothing automatic).
    public const string PostGameUIComponent =
        "//*[fn:component('Unity.BossRoom.Gameplay.UI.PostGameUI')]" +
        "/fn:component('Unity.BossRoom.Gameplay.UI.PostGameUI')";

    /// <summary>
    /// Authoritative seat -> class map, taken from AvatarConfiguration in
    /// Assets/Prefabs/State/CharSelectState.prefab. Seats come in boy/girl pairs
    /// per class; the lower even index is the "first" seat of each class.
    /// </summary>
    public sealed record HeroClass(string Name, int SeatIndex, string DisplayedName)
    {
        public override string ToString() => Name;
    }

    public static readonly HeroClass Tank = new("Tank", 0, "TANK");
    public static readonly HeroClass Archer = new("Archer", 2, "ARCHER");
    public static readonly HeroClass Mage = new("Mage", 4, "MAGE");
    public static readonly HeroClass Rogue = new("Rogue", 6, "ROGUE");

    // =======================================================================
    //  High-level flow
    // =======================================================================

    /// <summary>
    /// From the MainMenu, creates a new game session and lands on CharSelect.
    /// Reusable entry point for any test that needs to be sitting in CharSelect.
    /// </summary>
    public static void StartNewGame(ApiClient api)
    {
        EnsureAtMainMenu(api);
        WaitForSignIn(api);

        // Open the Sessions UI (opens on the Join tab).
        api.ClickObject(MouseButtons.LEFT, SessionStartButton, 30);
        api.WaitForEmptyInput();

        // Switch to the Create tab -- fully-qualified path, see the class-level comment.
        api.ClickObject(MouseButtons.LEFT, CreateTabButton, 30);
        api.WaitForEmptyInput();
        api.WaitForObjectValue(SessionCreationUICanvasGroup, "alpha", 1f, true, 10);

        // Confirm creation. SessionCreationUI falls back to a default
        // auto-generated name when the name field is empty, so nothing needs typing.
        api.ClickObject(MouseButtons.LEFT, CreateSessionButton, 30);
        api.WaitForEmptyInput();

        // The room takes a few seconds to load after session creation, behind the
        // same loading screen every network scene transition uses. Wait for the
        // screen to actually clear AND for a definitive CharSelect element to exist
        // (PlayerSeats) rather than guessing a fixed delay -- a fixed Wait() here is
        // exactly what caused ClassRollThroughTests to click seats before the scene
        // was actually interactive.
        WaitForScene(api, CharSelect);
        WaitForLoadingScreenComplete(api);
        api.WaitForObject(PlayerSeats, 30);
    }

    /// <summary>
    /// Full single-class run: select the seat, lock in via Ready, wait for the
    /// BossRoom scene, and clear the Cheats and How-To-Play popups. Leaves the
    /// player idle in gameplay. Assumes StartNewGame has already reached CharSelect.
    /// </summary>
    public static void SelectClassAndEnterGame(ApiClient api, HeroClass hero) =>
        SelectClassAndEnterGame(api, hero.SeatIndex);

    /// <summary>Seat-index overload (for [TestCase]-driven tests using primitives).</summary>
    public static void SelectClassAndEnterGame(ApiClient api, int seatIndex)
    {
        SelectSeat(api, seatIndex);
        ClickReady(api);
        WaitForScene(api, BossRoom);
        WaitForLoadingScreenComplete(api);
        // Same reasoning as StartNewGame's CharSelect wait: the loading screen
        // clearing doesn't guarantee the HUD has spawned yet. Wait for the actual
        // Action Bar to exist before handing control back to the caller.
        api.WaitForObject(ActionBarRoot, 30);
        ClearCheatsPopup(api);
        ClearHowToPlayPopup(api);
    }

    // =======================================================================
    //  MainMenu steps
    // =======================================================================

    /// <summary>
    /// Waits for the MainMenu's automatic anonymous sign-in to finish (the
    /// LoadingSpinner hides) so a session can be created. Tolerant: if the spinner
    /// object can't be read, assumes services are ready and proceeds.
    /// </summary>
    private static void WaitForSignIn(ApiClient api, int maxWaitMs = 20_000)
    {
        Assert.That(api.GetSceneName(), Is.EqualTo(MainMenu),
            "Expected MainMenu before creating a session");

        var waited = 0;
        while (waited < maxWaitMs)
        {
            try
            {
                if (!api.GetObjectFieldValue<bool>($"{SignInSpinner}/@activeSelf", 5))
                    return;
            }
            catch
            {
                return; // spinner gone / not found -> treat as signed in
            }
            api.Wait(500);
            waited += 500;
        }
    }

    // =======================================================================
    //  CharSelect steps
    // =======================================================================

    private static string SeatOutlinePath(int seatIndex) =>
        $"{PlayerSeats}/Untagged[@name='PlayerSeat ({seatIndex})']" +
        "//Untagged[@name='Outline (when selected)']";

    /// <summary>Clicks a seat by its zero-based left-to-right index.</summary>
    public static void SelectSeat(ApiClient api, int seatIndex)
    {
        Assert.That(api.GetSceneName(), Is.EqualTo(CharSelect),
            "Expected CharSelect before selecting a seat");

        string clickTarget =
            $"{PlayerSeats}/Untagged[@name='PlayerSeat ({seatIndex})']" +
            "/Untagged[@name='AnimationContainer']/Untagged[@name='ClickInteract']";
        api.ClickObject(MouseButtons.LEFT, clickTarget, 30);
        api.WaitForEmptyInput();

        // Wait for the seat's own selection highlight to actually register instead
        // of guessing a fixed delay -- callers (e.g. ClassRollThroughTests) assert
        // IsSeatHighlighted immediately after this returns.
        api.WaitForObjectValue(SeatOutlinePath(seatIndex), "activeInHierarchy", true, true, 10);
    }

    /// <summary>The class name currently shown in the ClassInfoBox (e.g. "TANK").</summary>
    public static string GetInfoBoxClassText(ApiClient api) =>
        api.GetObjectFieldValue<string>(CurrentClassText, 30);

    /// <summary>True once the ClassInfoBox details are shown (i.e. a class is selected).</summary>
    public static bool IsInfoBoxShowingClass(ApiClient api) =>
        api.GetObjectFieldValue<bool>($"{ClassInfoBoxDetails}/@activeSelf", 30);

    /// <summary>
    /// True when the given seat is drawing its selection highlight
    /// ("Outline (when selected)" object active in the seat prefab).
    /// </summary>
    public static bool IsSeatHighlighted(ApiClient api, int seatIndex) =>
        api.GetObjectFieldValue<bool>($"{SeatOutlinePath(seatIndex)}/@activeInHierarchy", 30);

    public static void ClickReady(ApiClient api)
    {
        api.Wait(300);
        api.ClickObject(MouseButtons.LEFT, ReadyButton, 30);
        api.WaitForEmptyInput();
    }

    // =======================================================================
    //  BossRoom popups
    // =======================================================================

    public static void ClearCheatsPopup(ApiClient api)
    {
        api.ClickObject(MouseButtons.LEFT, CheatsCancelButton, 30);
        api.WaitForEmptyInput();
    }

    public static void ClearHowToPlayPopup(ApiClient api)
    {
        api.ClickObject(MouseButtons.LEFT, HowToPlayConfirmButton, 30);
        api.WaitForEmptyInput();
    }

    // =======================================================================
    //  Settings panel (Audio + Graphics domains)
    // =======================================================================

    public static void OpenSettingsPanel(ApiClient api)
    {
        if (!api.GetObjectFieldValue<bool>(SettingsPanelActive, 30))
            api.CallMethod(SettingsCanvas, "OnClickSettingsButton", null);
        api.Wait(200);
    }

    public static void CloseSettingsPanel(ApiClient api)
    {
        if (api.GetObjectFieldValue<bool>(SettingsPanelActive, 30))
            api.CallMethod(SettingsCanvas, "OnClickSettingsButton", null);
        api.Wait(200);
    }

    // =======================================================================
    //  Scene / loading helpers
    // =======================================================================

    public static void WaitForScene(ApiClient api, string sceneName, int maxWaitMs = 30_000)
    {
        var waited = 0;
        while (api.GetSceneName() != sceneName && waited < maxWaitMs)
        {
            api.Wait(500);
            waited += 500;
        }
        Assert.That(api.GetSceneName(), Is.EqualTo(sceneName),
            $"Scene '{sceneName}' did not load in time");

        // MainMenu (and its SessionUIMediator etc.) is destroyed and reinstantiated
        // fresh on every return trip (ClientMainMenuState.Awake() re-runs sign-in
        // each time -- it's not a persisted singleton). If the Agent's HPath object
        // cache is enabled, a query resolved before this reload can keep returning
        // the OLD, now-destroyed instance until GC'd or explicitly flushed --
        // plausible root cause of the "works once, flaky on the next lap" pattern
        // seen repeatedly this session (2nd ClassRollThroughTests class, CombatTests'
        // one-off flicker, KeyboardInputTests failing to select the Create tab).
        // No-ops harmlessly if caching happens to be off.
        api.FlushObjectLookupCache(10);
    }

    public static void WaitForLoadingScreenComplete(ApiClient api)
    {
        bool done = api.WaitForObjectValue(LoadingScreen, "alpha", 0f, true, 30);
        Assert.That(done, Is.True,
            "Loading screen did not finish (CanvasGroup alpha never reached 0)");
    }

    /// <summary>
    /// Recovery/guard used between parameterized runs: if we are still in gameplay,
    /// quit back to the menu; then wait until the MainMenu is loaded.
    /// </summary>
    public static void EnsureAtMainMenu(ApiClient api)
    {
        // Wait out any bootstrap / transition first.
        var waited = 0;
        while (api.GetSceneName() == "Startup" && waited < 30_000)
        {
            api.Wait(200);
            waited += 200;
        }

        var quitFromMatch = false;

        if (api.GetSceneName() == BossRoom)
        {
            api.ClickObject(MouseButtons.LEFT, QuitButton, 30);
            api.WaitForEmptyInput();
            api.ClickObject(MouseButtons.LEFT, QuitConfirmButton, 30);
            api.WaitForEmptyInput();
            quitFromMatch = true;
        }
        else if (api.GetSceneName() == "PostGame")
        {
            api.CallMethod(PostGameUIComponent, "OnMainMenuClicked", null);
            quitFromMatch = true;
        }

        WaitForScene(api, MainMenu);

        // MultiplayerServicesFacade.m_RateLimitHost is a hard-coded 3s client-side
        // cooldown on hosting a session (RateLimitCooldown(3f)). It's a plain C#
        // timer, not backed by any scene object, so there's nothing to WaitForObject
        // on -- a fixed wait here is the correct tool, not a workaround. Without it,
        // back-to-back StartNewGame calls (as ClassRollThroughTests does, once per
        // class) can outrun the cooldown: TryCreateSessionAsync silently returns
        // (false, null) and the test hangs waiting for a CharSelect that never loads.
        if (quitFromMatch)
            api.Wait(3500);
    }
}
