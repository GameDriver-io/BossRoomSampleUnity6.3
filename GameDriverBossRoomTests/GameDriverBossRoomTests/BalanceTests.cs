using gdio.common.objects;
using gdio.unity_api.v2;

namespace GameDriverBossRoomTests;

/// <summary>
/// Balance domain: a game-DESIGN spec check, not a functional one -- the flagship
/// example of GameDriver surfacing an out-of-spec balance value that would become a
/// JIRA (see [[project_qaas_balance_ttk_signal]] and the create-JIRAs work).
///
/// DEMO INTENT (read before "fixing" this): this test is DESIGNED TO FAIL. The suite
/// is a QaaS demo and deliberately needs a realistic "failure" signal in the picture.
/// The failure here is a genuine, explainable, data-driven balance finding -- the Imp's
/// max HP measured live against a declared design-spec floor -- not a synthetic
/// assertion error. It is intentionally the "out of spec -> create a JIRA" story, so
/// leave it red unless the design spec or the game data actually changes.
///
/// It reads only the RELIABLE primitives (spawn an Imp, read its starting HitPoints --
/// a plain int NetworkVariable), deliberately NOT the fragile live attack/movement flow,
/// so the failure reads cleanly as "value out of spec" rather than "test couldn't run".
///
/// FUTURE (the richer version, not built): per-class time-to-kill / damage-per-hit
/// compared across Tank/Archer/Mage/Rogue to catch relative balance drift. That needs
/// the reliable live-combat measurement to be trustworthy first.
/// </summary>
[TestFixture]
[Category("Balance")]
public class BalanceTests : GameDriverTest
{
    private const string DebugCheatsManager =
        "//*[fn:component('Unity.BossRoom.DebugCheats.DebugCheatsManager')]" +
        "/fn:component('Unity.BossRoom.DebugCheats.DebugCheatsManager')";
    private const string ServerCharacterComponent =
        "fn:component('Unity.BossRoom.Gameplay.GameplayObjects.Character.ServerCharacter')";

    // Declared design-spec floor for standard trash-mob health. Set ABOVE the Imp's
    // actual 15 HP so the demo reliably surfaces an out-of-spec balance finding.
    // (A real spec would live in design docs / a shared config; hard-coded here for the demo.)
    private const int MinStandardMobHp = 20;

    [OneTimeSetUp]
    public void EnterGameplayAsTank()
    {
        BossRoomFlow.EnsureAtMainMenu(api);
        BossRoomFlow.StartNewGame(api);
        BossRoomFlow.SelectClassAndEnterGame(api, BossRoomFlow.Tank);
    }

    [Test]
    [Order(010)]
    public void T010_StandardMobHealth_MeetsDesignSpecFloor()
    {
        api.CallMethod(DebugCheatsManager, "SpawnEnemy", null);

        string? impHPath = null;
        var waited = 0;
        while (waited < 5_000 && impHPath == null)
        {
            var npcs = BossRoomFlow.FindAliveNpcs(api);
            if (npcs.Count > 0) impHPath = npcs[0];
            else { api.Wait(300); waited += 300; }
        }
        Assert.That(impHPath, Is.Not.Null, "No Imp spawned to measure — cannot evaluate the balance spec");

        // Fresh-spawn HitPoints == max HP for a standard trash mob.
        var mobMaxHp = api.GetObjectFieldValue<int>($"{impHPath}/{ServerCharacterComponent}/@HitPoints", 30);

        Assert.That(mobMaxHp, Is.GreaterThanOrEqualTo(MinStandardMobHp),
            $"BALANCE (out of spec): standard mob max HP is {mobMaxHp}, below the design floor of " +
            $"{MinStandardMobHp}. Trash mobs die too fast for the intended difficulty curve. " +
            "(Intentional demo finding — this is the 'GameDriver surfaces a design issue -> JIRA' example.)");
    }

    // --- Passing companions: the balance domain's reliable primitives. These read the
    // same trustworthy state the spec check depends on, so the domain reads as "healthy
    // with one out-of-spec finding" rather than a single all-red test. Reuse the session.

    [Test]
    [Order(020)]
    public void T020_GivenGameplay_LocalPlayerOwnershipResolves()
    {
        Assert.That(api.WaitForObject(BossRoomFlow.LocalPlayerHPath, 30), Is.True,
            "Local player did not resolve in gameplay");
    }

    [Test]
    [Order(030)]
    public void T030_GivenGameplay_InBossRoomScene()
    {
        Assert.That(api.GetSceneName(), Is.EqualTo(BossRoomFlow.BossRoom),
            "Not in the BossRoom scene");
    }

    [Test]
    [Order(040)]
    public void T040_SpawnedMob_HasPositiveStartingHealth()
    {
        // The measurable primitive the spec check builds on: a spawned mob reports a
        // positive starting HP. (The spec FLOOR check — whether it meets 20 — is T010.)
        api.CallMethod(DebugCheatsManager, "SpawnEnemy", null);

        string? impHPath = null;
        var waited = 0;
        while (waited < 5_000 && impHPath == null)
        {
            var npcs = BossRoomFlow.FindAliveNpcs(api);
            if (npcs.Count > 0) impHPath = npcs[0];
            else { api.Wait(300); waited += 300; }
        }
        Assert.That(impHPath, Is.Not.Null, "No mob spawned to measure");
        var hp = api.GetObjectFieldValue<int>($"{impHPath}/{ServerCharacterComponent}/@HitPoints", 30);
        Assert.That(hp, Is.GreaterThan(0), "Spawned mob reported non-positive starting HitPoints");
    }
}
