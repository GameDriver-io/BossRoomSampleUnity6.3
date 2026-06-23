namespace GameDriverBossRoomTests.Session1Lab2;

public static class Game
{
    public static string GetLevelName() => "Campaign_Mission1";
    
    public static int GetEnemyCount() => 5;
    
    public static Player GetPlayer() => new Player();

    public static void BootGame() {}

}

public class Player { }