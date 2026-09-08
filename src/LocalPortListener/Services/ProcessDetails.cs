namespace LocalPortListener.Services;

public sealed record ProcessDetails(
    int ProcessId,
    string Name,
    string? ExecutablePath,
    DateTime? StartedAt,
    bool AccessDenied,
    bool Exited);
