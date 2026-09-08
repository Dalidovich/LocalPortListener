using LocalPortListener.Models;

namespace LocalPortListener.Services;

public sealed class ListenerCollector
{
    private readonly TcpListenerEnumerator _enumerator;
    private readonly ProcessInspector _inspector;
    private readonly WindowsProcessClassifier _classifier;
    private readonly PortNoteResolver _noteResolver;

    public ListenerCollector(
        TcpListenerEnumerator enumerator,
        ProcessInspector inspector,
        WindowsProcessClassifier classifier,
        PortNoteResolver noteResolver)
    {
        _enumerator = enumerator;
        _inspector = inspector;
        _classifier = classifier;
        _noteResolver = noteResolver;
    }

    public IReadOnlyList<PortListener> Collect(bool includeWindowsProcesses)
    {
        _inspector.Reset();

        var listeners = new List<PortListener>();
        foreach (var endpoint in _enumerator.GetListeners())
        {
            var details = _inspector.Describe(endpoint.ProcessId);
            var isWindows = _classifier.IsWindowsProcess(details);
            if (isWindows && !includeWindowsProcesses)
            {
                continue;
            }

            listeners.Add(new PortListener
            {
                Protocol = endpoint.Protocol,
                LocalAddress = endpoint.LocalAddress,
                Port = endpoint.Port,
                ProcessId = endpoint.ProcessId,
                ProcessName = details.Name,
                ExecutablePath = details.ExecutablePath,
                StartedAt = details.StartedAt,
                IsWindowsProcess = isWindows,
                Note = _noteResolver.Resolve(endpoint.Port, details)
            });
        }

        var ordered = listeners
            .OrderBy(listener => listener.Port)
            .ThenBy(listener => listener.Protocol, StringComparer.Ordinal)
            .ThenBy(listener => listener.LocalAddress, StringComparer.Ordinal)
            .ToList();

        for (var index = 0; index < ordered.Count; index++)
        {
            ordered[index].Id = index + 1;
        }

        return ordered;
    }
}
