using gdio.common.objects;
using gdio.unity_api.v2;

namespace GameDriverBossRoomTests.Session3;

public class LogTracker
{
    private bool isConsoleError;
    private readonly List<string> outputs = new();

    public void OnOneTimeSetUp(ApiClient api)
    {
        api.UnityLoggedMessage += OnUnityLog;
    }

    public void OnSetUp()
    {
        isConsoleError = false;
        outputs.Clear();
    }

    public void AssertThatClearOfErrors()
    {
        Console.WriteLine(string.Join("\n", outputs));
        Assert.That(isConsoleError, Is.False);
    }

    public void OnOneTimeTearDown(ApiClient api)
    {
        api.UnityLoggedMessage -= OnUnityLog;
    }
    
    private void OnUnityLog(object? sender, UnityLogEventEventArgs args)
    {
        isConsoleError |= args.type is LogType.Error or LogType.Exception or LogType.Assert;
        var msg = $"[{args.type}] {args.condition}";
        outputs.Add(msg);
    }
}
