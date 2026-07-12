using gdio.common.objects;
using gdio.unity_api.v2;

namespace GameDriverBossRoomTests;

/// <summary>
/// Game Outcome domain: the server's real win condition, traced from source
/// (ServerBossRoomState.cs):
///   - OnLifeStateChangedEventMessage listens for CharacterTypeEnum.ImpBoss going
///     LifeState.Dead -> BossDefeated() -> WinState.Win -> after a 7s delay, loads
///     the "PostGame" scene.
///   - All players Fainted takes the same coroutine down the Loss path instead.
///
/// IMPORTANT premise correction: killing regular mobs does NOT open the boss door
/// in the real game. Traced SwitchedDoor.cs -> the door is opened purely by a
/// FloorSwitch physics trigger (a player standing on a pressure plate), unrelated
/// to enemy kill count. Organic play would require character movement/pathfinding
/// to reach the switch and then the boss, which this test suite doesn't drive yet.
///
/// WHEN DOES THE BOSS ACTUALLY SPAWN? Traced fully, and it's a genuinely
/// non-obvious finding automation surfaces that manual testing wouldn't precisely
/// articulate: there is NO bespoke "boss encounter start" trigger anywhere in the
/// code. The boss is spawned by the exact same generic EnemyPortal + ServerWaveSpawner
/// system used for every regular trash-mob spawner in the dungeon (confirmed via
/// Assets/Prefabs/Game/StaticNetworkObjects/BossRoomStaticNetworkObjects.prefab,
/// whose "EnemySpawners" section wires one spawner's prefab reference to ImpBoss).
/// EnemyPortal.MaintainState() enables its ServerWaveSpawner immediately on
/// OnNetworkSpawn (as long as its breakables are unbroken, which is the default
/// state), and ServerWaveSpawner only starts spawning once ANY player is within
/// m_ProximityDistance AND has an unobstructed line of sight (Physics.RaycastNonAlloc).
/// So the boss's portal is "always on" from scene start exactly like a trash-mob
/// portal -- it just happens to sit physically behind the locked door, so proximity
/// alone naturally gates it without any special code path. A manual tester would
/// describe this as "the boss shows up once you get close," without being able to
/// confirm (as this trace does) that it is literally the identical spawner
/// component/logic as every other mob, not a scripted boss-fight-start event.
///
/// CORRECTION (confirmed live): trash-mob EnemyPortals near the BossRoom spawn point
/// are ALSO already in proximity+LOS range on fresh entry, so real Imp/VandalImp
/// instances spawn immediately -- "zero NPCs alive at fresh entry" does not hold.
/// The boss itself is still not spawned yet, so T005 checks specifically for that
/// (BossRoomFlow.IsBossAlive, name-filtered since CharacterType can't be read
/// remotely) rather than "no NPCs at all".
///
/// This suite verifies the "not yet spawned" half of that directly (T005) and uses
/// DebugCheatsManager.SpawnBoss() only as a stand-in for physically walking there
/// (called by component, not by hunting the Cheats panel's button names -- these
/// are plain public methods on a NetworkBehaviour, callable whether or not the
/// panel is open).
///
/// COVERAGE GAP: every test here plays as BossRoomFlow.Tank. The win condition
/// itself (boss LifeState -> Dead) is class-agnostic, so this is lower-risk than
/// the other Tank-only fixtures, but Archer/Mage/Rogue reaching PostGame the same
/// way hasn't been independently confirmed.
/// </summary>
[TestFixture]
[Category("GameOutcome")]
public class GameOutcomeTests : GameDriverTest
{
    private const string DebugCheatsManager =
        "//*[fn:component('Unity.BossRoom.DebugCheats.DebugCheatsManager')]" +
        "/fn:component('Unity.BossRoom.DebugCheats.DebugCheatsManager')";

    private const string WinMessageActive =
        $"{BossRoomFlow.PostGameUIComponent}/@m_WinEndMessage/@gameObject/@activeSelf";
    private const string LoseMessageActive =
        $"{BossRoomFlow.PostGameUIComponent}/@m_LoseGameMessage/@gameObject/@activeSelf";

    // ServerBossRoomState.k_WinDelay is 7.0s between BossDefeated() and the PostGame
    // scene load; give it headroom for the network scene load itself.
    private const int WinDelayMs = 12_000;

    [OneTimeSetUp]
    public void EnterGameplayAsTank()
    {
        BossRoomFlow.EnsureAtMainMenu(api);
        BossRoomFlow.StartNewGame(api);
        BossRoomFlow.SelectClassAndEnterGame(api, BossRoomFlow.Tank);
    }

    [Test]
    [Order(005)]
    public void T005_GivenFreshBossRoomEntry_BossHasNotSpawnedYet()
    {
        // Trash mobs ARE already alive at fresh entry (their own portals are in range
        // too -- see the fixture's doc comment), so this checks the boss specifically
        // rather than "no NPCs at all".
        Assert.That(BossRoomFlow.IsBossAlive(api), Is.False,
            "The boss was already alive on fresh BossRoom entry -- expected it to spawn only " +
            "once a player is within its EnemyPortal/ServerWaveSpawner's proximity + line-of-sight " +
            "range (see this fixture's doc comment for the fully traced trigger).");
    }

    [Test]
    [Order(010)]
    public void T010_GivenGameplay_WhenBossSpawnedAndKilled_GameIsWonAndPostGameShowsVictory()
    {
        api.CallMethod(DebugCheatsManager, "SpawnBoss", null);
        api.Wait(1000); // let the spawn RPC round-trip and the NetworkObject sync

        api.CallMethod(DebugCheatsManager, "KillAllEnemies", null);

        BossRoomFlow.WaitForScene(api, "PostGame", WinDelayMs);

        Assert.That(api.GetObjectFieldValue<bool>(WinMessageActive, 30), Is.True,
            "PostGame did not show the victory message after the boss was defeated");
        Assert.That(api.GetObjectFieldValue<bool>(LoseMessageActive, 30), Is.False,
            "PostGame incorrectly showed the defeat message after a win");
    }

    /// <summary>
    /// Same win condition as T010, but the KILL step doesn't touch DebugCheatsManager
    /// at all -- BossRoomFlow.KillDirectly zeroes NetworkHealthState.HitPoints directly
    /// on the boss specifically. SpawnBoss() is still used (as a stand-in for
    /// physically walking to the boss's portal); only the kill mechanism is cheat-free.
    /// This is the mechanism to prefer for anything that must also work against a
    /// build with cheats compiled out.
    /// (Targets the boss via FindAliveBoss rather than KillAllNpcsDirectly: this
    /// fixture's world also has real trash mobs alive by the time this runs, spawned
    /// by a ServerWaveSpawner that keeps replenishing them while a player is nearby --
    /// confirmed live that "kill every alive NPC" chases that endlessly-refilling pool
    /// for minutes instead of ever reaching the boss. Killing only the boss is also
    /// what this test is actually meant to verify.)
    /// </summary>
    [Test]
    [Order(020)]
    public void T020_GivenGameplay_WhenBossKilledDirectly_GameIsWonAndPostGameShowsVictory()
    {
        BossRoomFlow.EnsureAtMainMenu(api);
        BossRoomFlow.StartNewGame(api);
        BossRoomFlow.SelectClassAndEnterGame(api, BossRoomFlow.Tank);

        api.CallMethod(DebugCheatsManager, "SpawnBoss", null);
        api.Wait(1000);

        var bossHPath = BossRoomFlow.FindAliveBoss(api);
        Assert.That(bossHPath, Is.Not.Null, "SpawnBoss did not produce a findable alive ImpBoss");
        BossRoomFlow.KillDirectly(api, bossHPath);

        BossRoomFlow.WaitForScene(api, "PostGame", WinDelayMs);

        Assert.That(api.GetObjectFieldValue<bool>(WinMessageActive, 30), Is.True,
            "PostGame did not show the victory message after the boss was killed directly");
        Assert.That(api.GetObjectFieldValue<bool>(LoseMessageActive, 30), Is.False,
            "PostGame incorrectly showed the defeat message after a win");
    }
}
