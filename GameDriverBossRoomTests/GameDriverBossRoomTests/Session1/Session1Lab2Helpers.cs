namespace GameDriverBossRoomTests.Session1;

public static class Game
{
    public static string GetLevelName() => "Campaign_MissionMissing";
    
    public static int GetEnemyCount() => 5;
    
    public static Player GetPlayer() => new Player();

    public static void BootGame() {}

}

public class Player { }