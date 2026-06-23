using gdio.unity_api.v2;

namespace GameDriverBossRoomTests;

public class Tests
{
    private ApiClient api;
    
    [OneTimeSetUp]
    public void Connect()
    {
        api = new ApiClient();
        api.Connect("127.0.0.1");
      
      
    }

    [OneTimeTearDown]
    public void Disconnect()
    {
        api.Wait(500);
        api.Disconnect();
    }
    
    [SetUp]
    public void Setup()
    {
        
    }

    [Test]
    public void Test1()
    {
        api.Wait(1000);
        Assert.Pass();
    }
}