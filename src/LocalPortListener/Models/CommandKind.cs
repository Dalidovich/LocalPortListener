namespace LocalPortListener.Models;

public enum CommandKind
{
    Unknown,
    Refresh,
    RefreshWithWindows,
    KillById,
    KillByPid,
    Help,
    Quit
}
