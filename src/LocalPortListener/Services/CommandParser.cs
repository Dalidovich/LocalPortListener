using System.Globalization;
using LocalPortListener.Models;

namespace LocalPortListener.Services;

public sealed class CommandParser
{
    public ConsoleCommand Parse(string? input)
    {
        var trimmed = input?.Trim();
        if (string.IsNullOrEmpty(trimmed))
        {
            return new ConsoleCommand(CommandKind.Refresh);
        }

        var parts = trimmed.Split(' ', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
        var verb = parts[0].ToLowerInvariant();

        return verb switch
        {
            "r" => new ConsoleCommand(CommandKind.Refresh),
            "a" => new ConsoleCommand(CommandKind.RefreshWithWindows),
            "h" or "?" or "help" => new ConsoleCommand(CommandKind.Help),
            "q" or "quit" or "exit" => new ConsoleCommand(CommandKind.Quit),
            "k" => ParseWithNumber(parts, CommandKind.KillById, "k {id}"),
            "kp" => ParseWithNumber(parts, CommandKind.KillByPid, "kp {pid}"),
            _ => ConsoleCommand.Invalid($"Unknown command '{parts[0]}'. Type 'h' for help.")
        };
    }

    private static ConsoleCommand ParseWithNumber(string[] parts, CommandKind kind, string usage)
    {
        if (parts.Length < 2)
        {
            return ConsoleCommand.Invalid($"Missing number. Usage: {usage}.");
        }

        if (!int.TryParse(parts[1], NumberStyles.Integer, CultureInfo.InvariantCulture, out var number) || number <= 0)
        {
            return ConsoleCommand.Invalid($"'{parts[1]}' is not a valid positive number. Usage: {usage}.");
        }

        return new ConsoleCommand(kind, number);
    }
}
