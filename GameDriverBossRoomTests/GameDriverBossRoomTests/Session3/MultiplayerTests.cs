using System.Diagnostics;
using gdio.common.objects;
using gdio.unity_api.v2;

namespace GameDriverBossRoomTests.Session3;

[TestFixture]
public class MultiplayerTests : GameDriverTest
{
    private Process standaloneProcess;
    private ApiClient apiP2;

    [OneTimeSetUp]
    public void TwoPlayersToGameplay()
    {
        // Launch the standalone build and connect P2 to it
        standaloneProcess = StandaloneProcess.Launch();
        apiP2 = new ApiClient();
        apiP2.Connect("localhost", 19735);
        
        BossRoomNavigators.WaitUntilBooted(api);
        BossRoomNavigators.HostLocalIp(api);

        BossRoomNavigators.WaitUntilBooted(apiP2);
        BossRoomNavigators.JoinLocalIp(apiP2);
        
        BossRoomNavigators.SelectCharAndReady(apiP2, 1);
        BossRoomNavigators.SelectCharAndReady(api, 0);

        BossRoomNavigators.WaitForIdleGameplay(api);
        BossRoomNavigators.WaitForIdleGameplay(apiP2);
        BossRoomNavigators.WaitForAllPlayersJoined(api, expectedPlayerCount:2);
    }

    [OneTimeTearDown]
    public void DisconnectP2AndCleanupLogging()
    {
        apiP2.Disconnect();
        // Kill the standalone build process
        standaloneProcess?.Kill();
        standaloneProcess?.Dispose();
    }

    public enum Protagonist
    {
        Server,
        Client,
    } 

    [TestCase(Protagonist.Client), TestCase(Protagonist.Server)]
    public void GivenBothInGame_WhenProtagonistMoves_MovementReflectedOnOther(Protagonist protagonist)
    {
        var protagApi = protagonist == Protagonist.Server ? api : apiP2;
        var viewerApi = protagonist == Protagonist.Server ? apiP2 : api;
        
        var startViewerPositionOfProtagonist = BossRoomNavigators.GetRemotePlayerWorldPosition(viewerApi);
        
        BossRoomNavigators.MoveLocalPlayer(protagApi);
        
        var endViewerPositionOfProtagonist = BossRoomNavigators.GetRemotePlayerWorldPosition(viewerApi);
        
        var playerMovement = Vector3.Distance(startViewerPositionOfProtagonist, endViewerPositionOfProtagonist);
        Console.WriteLine(endViewerPositionOfProtagonist);
        Assert.That(playerMovement, Is.GreaterThan(0.25f));
    }
}
