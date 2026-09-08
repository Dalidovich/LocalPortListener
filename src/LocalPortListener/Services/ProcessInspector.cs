using System.Diagnostics;
using LocalPortListener.Interop;

namespace LocalPortListener.Services;

public sealed class ProcessInspector
{
    private readonly Dictionary<int, ProcessDetails> _cache = new();

    public void Reset() => _cache.Clear();

    public ProcessDetails Describe(int processId)
    {
        if (_cache.TryGetValue(processId, out var cached))
        {
            return cached;
        }

        var details = Load(processId);
        _cache[processId] = details;
        return details;
    }

    private static ProcessDetails Load(int processId)
    {
        if (processId <= 0)
        {
            return new ProcessDetails(processId, "System Idle Process", null, null, false, false);
        }

        if (processId == 4)
        {
            return new ProcessDetails(processId, "System", null, null, false, false);
        }

        try
        {
            using var process = Process.GetProcessById(processId);
            var name = process.ProcessName;
            var (path, startedAt) = ProcessNativeInfo.Query(processId);
            var accessDenied = path is null && startedAt is null;

            return new ProcessDetails(processId, name, path, startedAt, accessDenied, false);
        }
        catch (ArgumentException)
        {
            return new ProcessDetails(processId, "<exited>", null, null, false, true);
        }
        catch (InvalidOperationException)
        {
            return new ProcessDetails(processId, "<exited>", null, null, false, true);
        }
    }
}
