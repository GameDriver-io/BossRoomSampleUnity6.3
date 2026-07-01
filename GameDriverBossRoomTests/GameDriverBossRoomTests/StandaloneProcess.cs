using System.Diagnostics;
using System.Runtime.InteropServices;

namespace GameDriverBossRoomTests;

public static class StandaloneProcess
{
    // Windows-only P/Invoke — only called on Windows
    [DllImport("user32.dll")] static extern bool SetWindowPos(IntPtr hWnd, IntPtr hWndInsertAfter, int x, int y, int cx, int cy, uint uFlags);
    [DllImport("user32.dll")] static extern bool GetWindowRect(IntPtr hWnd, out Rect rect);
    [DllImport("user32.dll")] static extern int GetSystemMetrics(int nIndex);

    [StructLayout(LayoutKind.Sequential)]
    private struct Rect { public int Left, Top, Right, Bottom; }

    private const int k_SmCxScreen = 0;
    private const uint k_SwpNozorder = 0x0004;
    private const uint k_SwpNoactivate = 0x0010;

    public static Process Launch(string? buildDirName = null)
    {
        buildDirName ??= RuntimeInformation.IsOSPlatform(OSPlatform.OSX) ? "Build_Mac" : "Build_Win";
        var buildDir = FindBuildDir(buildDirName);
        var exePath = FindExecutable(buildDir);
        var process = Process.Start(new ProcessStartInfo(exePath) { WorkingDirectory = buildDir })!;
        MoveToTopRight(process);
        return process;
    }

    private static string FindExecutable(string buildDir)
    {
        if (RuntimeInformation.IsOSPlatform(OSPlatform.OSX))
        {
            var appBundle = Directory.GetDirectories(buildDir, "*.app").First();
            var appName = Path.GetFileNameWithoutExtension(appBundle);
            return Path.Combine(appBundle, "Contents", "MacOS", appName);
        }
        return Directory.GetFiles(buildDir, "*.exe").First();
    }

    private static void MoveToTopRight(Process process)
    {
        if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
            MoveToTopRightWindows(process);
        else if (RuntimeInformation.IsOSPlatform(OSPlatform.OSX))
            MoveToTopRightMac(process);
    }

    private static void MoveToTopRightWindows(Process process)
    {
        while (process.MainWindowHandle == IntPtr.Zero)
        {
            Thread.Sleep(200);
            process.Refresh();
        }

        GetWindowRect(process.MainWindowHandle, out var rect);
        int windowWidth = rect.Right - rect.Left;
        int windowHeight = rect.Bottom - rect.Top;
        int screenWidth = GetSystemMetrics(k_SmCxScreen);

        SetWindowPos(process.MainWindowHandle, IntPtr.Zero,
            screenWidth - windowWidth, 0,
            windowWidth, windowHeight,
            k_SwpNozorder | k_SwpNoactivate);
    }

    private static void MoveToTopRightMac(Process process)
    {
        // Poll until the app window is available in System Events
        var appName = process.ProcessName;
        var script = $$"""
            repeat
                tell application "System Events"
                    if exists (window 1 of process "{{appName}}") then exit repeat
                end tell
                delay 0.2
            end repeat
            tell application "System Events"
                tell process "{{appName}}"
                    set {winW, winH} to size of window 1
                end tell
            end tell
            set scrW to item 3 of (bounds of window of desktop of application "Finder")
            tell application "System Events"
                tell process "{{appName}}"
                    set position of window 1 to {scrW - winW, 0}
                end tell
            end tell
            """;

        var psi = new ProcessStartInfo("osascript")
        {
            UseShellExecute = false,
            RedirectStandardInput = true,
        };
        var p = Process.Start(psi)!;
        p.StandardInput.Write(script);
        p.StandardInput.Close();
        p.WaitForExit(15000);
    }

    private static string FindBuildDir(string name)
    {
        var dir = new DirectoryInfo(AppDomain.CurrentDomain.BaseDirectory);
        while (dir != null)
        {
            var candidate = Path.Combine(dir.FullName, name);
            if (Directory.Exists(candidate)) return candidate;
            dir = dir.Parent;
        }
        throw new DirectoryNotFoundException($"Could not find '{name}' in any parent directory of {AppDomain.CurrentDomain.BaseDirectory}");
    }
}
