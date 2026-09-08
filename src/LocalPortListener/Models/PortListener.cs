namespace LocalPortListener.Models;

public sealed class PortListener
{
    public int Id { get; set; }

    public required string Protocol { get; init; }

    public required string LocalAddress { get; init; }

    public required int Port { get; init; }

    public required int ProcessId { get; init; }

    public required string ProcessName { get; init; }

    public string? ExecutablePath { get; init; }

    public DateTime? StartedAt { get; init; }

    public bool IsWindowsProcess { get; init; }

    public string Note { get; init; } = string.Empty;
}
