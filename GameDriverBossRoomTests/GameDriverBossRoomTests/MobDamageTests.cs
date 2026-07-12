using gdio.common.objects;
using gdio.unity_api.v2;

namespace GameDriverBossRoomTests;

/// <summary>
/// Combat.MobDamage subdomain: dealing real, in-combat damage to a regular mob (Imp)
/// via an actual attack click, as distinct from GameOutcomeTests' direct-kill/cheat
/// shortcuts (those zero HP outright; this measures a live combat interaction).
///
/// This fixture was rewritten after a source investigation of Boss Room's movement,
/// combat, and AI systems. The earlier version fought the game's architecture in
/// three ways -- all now corrected:
///
///   (1) ATTACK IS RIGHT-CLICK, NOT LEFT. In Assets/InputSystem/PlayerActions.inputactions
///       the Player map binds Skill1 (the basic attack) to <Mouse>/rightButton and
///       Target (select / move-only) to <Mouse>/leftButton. A left click on the Imp
///       only targets/moves and NEVER deals damage -- the original root cause of
///       "HitPoints didn't drop". We now LEFT-click to set the stateful target, then
///       RIGHT-click to actually attack.
///
///   (2) NO NavAgentMoveToPoint. Boss Room movement is fully server-authoritative:
///       the player's NavMeshAgent lives on the server character, is authored disabled
///       and only enabled server-side (ServerCharacterMovement.cs), and the client
///       transform is owned by a server NetworkTransform. Driving that NavMeshAgent
///       from the client (as NavAgentMoveToPoint does) threw "Agent was disabled, but
///       still processing Mov" / "ResetPath ... on an active agent placed on a NavMesh",
///       cascaded into a dropped agent connection, and froze the scene on quit. We
///       don't move the player manually at all: clicking an out-of-range enemy sets
///       ShouldClose=true, so the server synthesizes a ChaseAction that walks the
///       player into the Tank's 2m melee range and then swings (TankBaseAttack, 10 dmg).
///       The Imp also auto-aggros (spawns at world origin; the player spawns <=~8m away,
///       inside the Imp's 10m detect range) and closes from its side too.
///
///   (3) NO manual raycast line-of-sight pre-check. api.Raycast casts camera->point
///       and returns the first hit; aimed at the Imp's feet (y~0) it hits the 180x160
///       GroundPlane collider before the Imp, every time -- a structural false
///       negative. The game never targets that way (its own click cast filters past
///       the ground to the first NetworkObject), and api.ClickObject already resolves
///       the Imp's own screen position. The HitPoints drop is the definitive proof the
///       attack landed, so the pre-check is deleted.
///
/// Imp starts at 15 HP; one Tank hit is 10, so a single connected swing drops it to 5.
/// Damage lands server-side after a chase + 0.3s windup, so HitPoints is POLLED for a
/// drop (with a timeout), never asserted on a fixed delay.
///
/// COVERAGE GAP: OneTimeSetUp only plays as BossRoomFlow.Tank, so this only proves
/// Tank's Basic Action deals damage. Archer/Mage/Rogue each have a different Basic
/// Action (and this is exactly the kind of per-class result that would feed the
/// TTK/balance-signal idea noted separately for the future "create JIRAs" work).
/// </summary>
[TestFixture]
[Category("Combat.MobDamage")]
public class MobDamageTests : GameDriverTest
{
    private const string DebugCheatsManager =
        "//*[fn:component('Unity.BossRoom.DebugCheats.DebugCheatsManager')]" +
        "/fn:component('Unity.BossRoom.DebugCheats.DebugCheatsManager')";

    private const string ServerCharacterComponent =
        "fn:component('Unity.BossRoom.Gameplay.GameplayObjects.Character.ServerCharacter')";

    [OneTimeSetUp]
    public void EnterGameplayAsTank()
    {
        BossRoomFlow.EnsureAtMainMenu(api);
        BossRoomFlow.StartNewGame(api);
        BossRoomFlow.SelectClassAndEnterGame(api, BossRoomFlow.Tank);
        // Note: the player is left at its spawn point (~<=8m from world origin). This
        // matters -- SpawnEnemy places the Imp at origin, and the Imp only auto-aggros
        // if a player is within its 10m detect range at spawn. Do not walk the player
        // deep into the dungeon before this test.
    }

    [Test]
    [Order(010)]
    public void T010_GivenSpawnedImp_WhenAttacked_TakesDamage()
    {
        api.CallMethod(DebugCheatsManager, "SpawnEnemy", null);

        var impHPath = WaitForSpawnedImp(api);
        Assert.That(impHPath, Is.Not.Null, "No alive NPC (Imp) appeared after SpawnEnemy()");

        var hpBefore = api.GetObjectFieldValue<int>($"{impHPath}/{ServerCharacterComponent}/@HitPoints", 30);
        Assert.That(hpBefore, Is.GreaterThan(0), "Imp had no starting HitPoints to lose");

        // Left-click sets the Imp as the stateful target (so the attack resolves to it
        // even if the attack ray is slightly off); right-click fires the basic attack.
        // ShouldClose auto-chases the player into 2m melee range -- no manual movement.
        api.ClickObject(MouseButtons.LEFT, impHPath, 10);
        api.WaitForEmptyInput();
        api.ClickObject(MouseButtons.RIGHT, impHPath, 10);
        api.WaitForEmptyInput();

        WaitForHitPointsToDrop(api, impHPath, hpBefore, maxWaitMs: 15_000);
    }

    /// <summary>Polls until a spawned NPC exists (the spawn RPC has to round-trip), returns the nearest to the player.</summary>
    private static string? WaitForSpawnedImp(ApiClient api, int maxWaitMs = 5_000)
    {
        var waited = 0;
        while (waited < maxWaitMs)
        {
            var nearest = FindNearestAliveNpc(api);
            if (nearest != null)
                return nearest;
            api.Wait(300);
            waited += 300;
        }
        return null;
    }

    /// <summary>Of every alive NPC (only ever the one spawned Imp here), returns the nearest to the local player.</summary>
    private static string? FindNearestAliveNpc(ApiClient api)
    {
        string? nearestHPath = null;
        var nearestDistance = float.MaxValue;

        foreach (var hpath in BossRoomFlow.FindAliveNpcs(api))
        {
            var distance = api.GetObjectDistance(BossRoomFlow.LocalPlayerHPath, hpath, 30);
            if (distance < nearestDistance)
            {
                nearestDistance = distance;
                nearestHPath = hpath;
            }
        }

        return nearestHPath;
    }

    /// <summary>
    /// Polls the NPC's HitPoints until it drops below its starting value, or the NPC
    /// despawns (a kill -- lethal damage, also a pass), or the timeout elapses. Covers
    /// the auto-chase travel time plus the 0.3s attack windup before damage applies.
    /// </summary>
    private static void WaitForHitPointsToDrop(ApiClient api, string npcHPath, int startingHp, int maxWaitMs)
    {
        var waited = 0;
        while (waited < maxWaitMs)
        {
            try
            {
                var hp = api.GetObjectFieldValue<int>($"{npcHPath}/{ServerCharacterComponent}/@HitPoints", 10);
                if (hp < startingHp)
                    return; // took damage
            }
            catch
            {
                return; // NPC despawned -> it was killed -> lethal damage landed
            }

            api.Wait(300);
            waited += 300;
        }

        Assert.Fail(
            $"Imp's HitPoints never dropped below {startingHp} within {maxWaitMs}ms after a right-click attack " +
            "(expected the Tank's basic attack to auto-chase into melee range and deal 10 damage).");
    }
}
