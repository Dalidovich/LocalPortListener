namespace LocalPortListener.Models;

public sealed record ConsoleCommand(CommandKind Kind, int Argument = 0, string? Error = null)
{
    public static ConsoleCommand Invalid(string error) => new(CommandKind.Unknown, 0, error);
}
