#if false
using gdio.common.objects;
using gdio.unity_api.v2;

namespace GameDriverBossRoomTests.Session1;

[TestFixture]
public class Lab3GameSmokeTestFixture
{
    private ApiClient api;
    private bool isConsoleError;
    private readonly List<string> outputs = new();

    [OneTimeSetUp]
    public void ConnectApi()
    {
        api = new ApiClient();
        api.Connect("127.0.0.1", 19734, true);
        api.UnityLoggedMessage += OnUnityLog;
    }

    [SetUp]
    public void SetUp()
    {
        isConsoleError = false;
        outputs.Clear();
        while(api.GetSceneName() == "Startup")
        {
            api.Wait(100);
        }
    }
    
    private void OnUnityLog(object? sender, UnityLogEventEventArgs args)
    {
        isConsoleError |= args.type is LogType.Error or LogType.Exception or LogType.Assert;
        var msg = $"[{args.type}] {args.condition}";
        outputs.Add(msg);
    }

    [Test]
    [Order(010)]
    public void T010_GivenNothing_WhenGameFullyBooted_NoErrorsInConsole()
    {
        api.Wait(500);
        Console.Write(string.Join("\n", outputs));
        Assert.That(isConsoleError, Is.False);
    }

    [OneTimeTearDown]
    public void Disconnect()
    {
        api.UnityLoggedMessage -= OnUnityLog;
        api.StopEditorPlay();
        api.Disconnect();
    }
}
#endif