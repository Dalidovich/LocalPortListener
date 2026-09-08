using System.ComponentModel;
using System.Diagnostics;

namespace LocalPortListener.Services;

public sealed class ProcessKiller
{
    public KillResult Kill(int processId)
    {
        if (processId <= 4)
        {
            return new KillResult(false, $"PID {processId} belongs to the Windows kernel and cannot be killed.");
        }

        try
        {
            using var process = Process.GetProcessById(processId);
            var name = process.ProcessName;
            process.Kill(true);
            process.WaitForExit(5000);
            return new KillResult(true, $"Killed {name} (PID {processId}).");
        }
        catch (ArgumentException)
        {
            return new KillResult(false, $"No process with PID {processId} is running.");
        }
        catch (Win32Exception exception)
        {
            return new KillResult(false, $"Cannot kill PID {processId}: {exception.Message}. Try running as administrator.");
        }
        catch (InvalidOperationException)
        {
            return new KillResult(false, $"Process with PID {processId} has already exited.");
        }
    }
}
