using gdio.common.objects;
using gdio.unity_api.v2;

namespace GameDriverBossRoomTests;

/// <summary>
/// Netcode domain: Boss Room is built on Unity Netcode for GameObjects + Multiplayer
/// Services (server-authoritative movement via NetworkTransform, RPCs, NetworkVariables,
/// NetworkObject spawning, relay/session connection management). It is one of the
/// largest systems in the game -- and this suite has almost NO intentional coverage of
/// it. This domain should therefore read as a VERY LOW DSC: the bulk below is skipped
/// via Assert.Ignore (reported as "blocked"), and only a few netcode primitives that were
/// INCIDENTALLY exercised by other domains produce any real signal at all.
///
/// WHY MOST OF IT CANNOT BE COVERED HERE: this suite runs a single solo-HOST instance.
/// With server and client in the same process, "replication" is degenerate -- there is
/// no second peer for state to replicate TO. Genuinely testing netcode (a client seeing
/// a host's replicated state, NetworkTransform sync between peers, disconnect/reconnect,
/// relay join-by-code, late-join) requires a SECOND networked participant: a second
/// Editor/build instance, or a headless server + client. That is real infrastructure
/// this lab doesn't have, so those tests are [Ignore]d, not failed -- a true coverage
/// gap, exactly what a low DSC should represent.
///
/// THE "ACCIDENTALLY TOUCHED" EXCEPTIONS (the only real signal this domain earns):
///   - NetworkObject OWNERSHIP: BossRoomFlow.LocalPlayerHPath resolves the player via
///     fn:component('Unity.Netcode.NetworkObject')[@IsOwner='true'] -- used by
///     MobDamageTests / MouseInputTests. (T010)
///   - NetworkVariable READ: ServerCharacter.HitPoints is a NetworkVariable&lt;int&gt;
///     (NetworkHealthState); HudTests, MobDamageTests, GameOutcomeTests all read it
///     through the replicated object. (T020)
///   - NetworkVariable WRITE -> game logic: GameOutcomeTests' KillDirectly sets
///     NetworkHealthState.HitPoints.Value and the server win-condition fires.
///   - Session HOSTING + networked scene loads: every gameplay fixture, via
///     BossRoomFlow.StartNewGame, brings up a host session and loads CharSelect/BossRoom
///     through NetworkManager's scene manager.
///   - NetworkObject SPAWNING: SpawnEnemy/SpawnBoss spawn networked objects.
/// These are thin and (solo-host) partly degenerate, but they are genuinely exercised,
/// so they are real [Test]s here rather than gaps.
/// </summary>
[TestFixture]
[Category("Netcode")]
public class NetcodeTests : GameDriverTest
{
    private const string LocalPlayerNetworkObject =
        "//Player/fn:component('Unity.Netcode.NetworkObject')[@IsOwner='true']";
    private const string LocalPlayerHitPoints =
        BossRoomFlow.LocalPlayerHPath +
        "/fn:component('Unity.BossRoom.Gameplay.GameplayObjects.Character.ServerCharacter')/@HitPoints";

    [OneTimeSetUp]
    public void EnterGameplayAsTank()
    {
        BossRoomFlow.EnsureAtMainMenu(api);
        BossRoomFlow.StartNewGame(api);
        BossRoomFlow.SelectClassAndEnterGame(api, BossRoomFlow.Tank);
    }

    // =======================================================================
    //  Real signal -- the incidentally-exercised netcode primitives
    // =======================================================================

    [Test]
    [Order(010)]
    public void T010_LocalPlayerHasNetworkObjectOwnership()
    {
        // The object only resolves if a NetworkObject with IsOwner==true exists, i.e.
        // Netcode assigned ownership of the local player to this client. Genuinely
        // exercised (this is the path MobDamage/MouseInput rely on), and true even in
        // solo host (the host owns its own player).
        Assert.That(api.WaitForObject(LocalPlayerNetworkObject, 30), Is.True,
            "No NetworkObject with IsOwner=true resolved -- local player network ownership was not established");
    }

    [Test]
    [Order(020)]
    public void T020_ReplicatedHealthNetworkVariableIsReadable()
    {
        // ServerCharacter.HitPoints is backed by a NetworkVariable<int> (NetworkHealthState).
        // Reading it through the replicated player object exercises the NetworkVariable
        // read path. CAVEAT: solo host, so this reads the server's own value locally --
        // it does NOT prove cross-peer replication (that's the gap in T110/T100 below).
        var hp = api.GetObjectFieldValue<int>(LocalPlayerHitPoints, 30);
        Assert.That(hp, Is.GreaterThan(0),
            "Local player's replicated HitPoints NetworkVariable read back as <= 0");
    }

    [Test]
    [Order(030)]
    public void T030_NetworkedSceneLoad_ReachedBossRoom()
    {
        // The BossRoom scene is loaded through NetworkManager's networked scene manager
        // (SceneLoaderWrapper) as part of hosting -- a genuine, solo-host-verifiable
        // netcode primitive.
        Assert.That(api.GetSceneName(), Is.EqualTo(BossRoomFlow.BossRoom),
            "NetworkManager scene manager did not bring up the BossRoom scene");
    }

    [Test]
    [Order(040)]
    public void T040_ReplicatedLocalPlayerObject_Resolves()
    {
        // The server-authoritative local player NetworkObject resolves via the shared
        // ownership path other domains rely on -- exercises NetworkObject spawn+ownership.
        Assert.That(api.WaitForObject(BossRoomFlow.LocalPlayerHPath, 30), Is.True,
            "Replicated local player NetworkObject did not resolve");
    }

    // =======================================================================
    //  Coverage gaps -- untestable in a single solo-host instance.
    //  Assert.Ignore() (runtime skip) => reported "blocked" => keeps this domain's
    //  DSC very low. NOTE: an [Ignore] ATTRIBUTE is NOT captured by the reporter's
    //  live ITestAction -- the test never runs, so AfterTest never fires and the
    //  "blocked" outcome is silently dropped. We skip at RUNTIME with Assert.Ignore
    //  so the blocked signal actually reaches the QaaS upload.
    //  Each needs a SECOND networked participant (2nd Editor/build, or headless
    //  server + client) that this lab does not have.
    // =======================================================================

    [Test]
    [Order(100)]
    public void T100_ClientJoiningHost_SeesReplicatedState()
    {
        // Would connect a second client to the host session and assert it observes
        // host-owned NetworkObjects (players, spawned enemies) with correct state.
        Assert.Ignore("Needs a second networked peer. Solo host cannot verify a joining CLIENT sees " +
                      "the host's replicated state -- server and client are the same process here.");
    }

}
