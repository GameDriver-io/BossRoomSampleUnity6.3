#if false
using gdio.unity_api.v2;

namespace GameDriverBossRoomTests.Session1;

[TestFixture]
public class Session1Lab2TestFixture
{

    [OneTimeSetUp]
    public void OneTimeSetup()
    {
        Game.BootGame();
    }

    [Test]
    public void PlayerIsNotNull()
    {
        var player = Game.GetPlayer(); 
        Assert.That(/* YOUR CODE */);
    }
    
    [Test]
    public void GivenBootedGame_WhenGetLevelName_ContainsMission1()
    {
        string name = Game.GetLevelName();
        Assert.That(/* YOUR CODE */);
    }

    [Test]
    public void GivenBootedGame_WhenGetEnemyCount_GreaterThanZero()
    {
        int count = Game.GetEnemyCount();
        Assert.That(/* YOUR CODE */);
    }
}
#endif