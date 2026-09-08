namespace LocalPortListener.Services;

public sealed class WindowsProcessClassifier
{
    private static readonly string[] SystemDirectories = BuildSystemDirectories();

    private static readonly HashSet<string> SystemProcessNames = new(StringComparer.OrdinalIgnoreCase)
    {
        "System",
        "System Idle Process",
        "Idle",
        "svchost",
        "services",
        "lsass",
        "wininit",
        "winlogon",
        "smss",
        "csrss",
        "spoolsv",
        "SearchIndexer",
        "wininet"
    };

    public bool IsWindowsProcess(ProcessDetails details)
    {
        if (details.ProcessId <= 4)
        {
            return true;
        }

        if (details.ExecutablePath is { Length: > 0 } path)
        {
            return SystemDirectories.Any(directory => path.StartsWith(directory, StringComparison.OrdinalIgnoreCase));
        }

        return SystemProcessNames.Contains(details.Name);
    }

    private static string[] BuildSystemDirectories()
    {
        var windows = Environment.GetFolderPath(Environment.SpecialFolder.Windows);
        if (string.IsNullOrEmpty(windows))
        {
            windows = @"C:\Windows";
        }

        return
        [
            Path.Combine(windows, "System32"),
            Path.Combine(windows, "SysWOW64"),
            Path.Combine(windows, "WinSxS"),
            Path.Combine(windows, "servicing"),
            Path.Combine(windows, "SystemApps"),
            Path.Combine(windows, "ImmersiveControlPanel")
        ];
    }
}
