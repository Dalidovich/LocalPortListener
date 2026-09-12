using LocalPortListener.Models;

namespace LocalPortListener.Rendering;

public sealed class ListenerTableRenderer
{
    private const ConsoleColor BorderColor = ConsoleColor.DarkGray;
    private const ConsoleColor HeaderColor = ConsoleColor.Cyan;

    private static readonly TableColumn[] Columns =
    [
        new("#", listener => listener.Id.ToString(), _ => ConsoleColor.White, RightAligned: true, MaxWidth: 5),
        new("PROCESS", listener => listener.ProcessName, ProcessColor, MaxWidth: 28),
        new("ADDRESS", listener => listener.LocalAddress, Dim, MaxWidth: 24),
        new("PORT", listener => listener.Port.ToString(), PortColor, RightAligned: true, MaxWidth: 6),
        new("PROTO", listener => listener.Protocol, Dim, MaxWidth: 5),
        new("PID", listener => listener.ProcessId.ToString(), _ => ConsoleColor.Magenta, RightAligned: true, MaxWidth: 7),
        new("STARTED", FormatStartedAt, _ => ConsoleColor.DarkYellow, MaxWidth: 19),
        new("NOTE", listener => listener.Note, _ => ConsoleColor.DarkCyan, MaxWidth: 34)
    ];

    public void Render(IReadOnlyList<PortListener> listeners, bool includingWindowsProcesses)
    {
        var scope = includingWindowsProcesses ? "all processes" : "Windows processes hidden";
        ConsoleWriter.WriteLine($"Listeners: {listeners.Count} ({scope})", ConsoleColor.White);

        if (listeners.Count == 0)
        {
            ConsoleWriter.WriteLine("Nothing is listening under the current filter.", ConsoleColor.DarkYellow);
            Console.WriteLine();
            return;
        }

        var cells = listeners
            .Select(listener => Columns.Select(column => Truncate(column.ValueSelector(listener), column.MaxWidth)).ToArray())
            .ToArray();

        var widths = new int[Columns.Length];
        for (var i = 0; i < Columns.Length; i++)
        {
            widths[i] = Math.Max(Columns[i].Header.Length, cells.Max(row => row[i].Length));
        }

        WriteBorder('┌', '┬', '┐', widths);
        WriteHeader(widths);
        WriteBorder('├', '┼', '┤', widths);

        for (var rowIndex = 0; rowIndex < listeners.Count; rowIndex++)
        {
            WriteRow(listeners[rowIndex], cells[rowIndex], widths);
        }

        WriteBorder('└', '┴', '┘', widths);
        Console.WriteLine();
    }

    private static void WriteHeader(int[] widths)
    {
        ConsoleWriter.Write("│", BorderColor);
        for (var i = 0; i < Columns.Length; i++)
        {
            ConsoleWriter.Write(" " + Align(Columns[i].Header, widths[i], Columns[i].RightAligned) + " ", HeaderColor);
            ConsoleWriter.Write("│", BorderColor);
        }

        Console.WriteLine();
    }

    private static void WriteRow(PortListener listener, string[] values, int[] widths)
    {
        ConsoleWriter.Write("│", BorderColor);
        for (var i = 0; i < Columns.Length; i++)
        {
            var color = listener.IsWindowsProcess ? ConsoleColor.DarkGray : Columns[i].ColorSelector(listener);
            ConsoleWriter.Write(" " + Align(values[i], widths[i], Columns[i].RightAligned) + " ", color);
            ConsoleWriter.Write("│", BorderColor);
        }

        Console.WriteLine();
    }

    private static void WriteBorder(char left, char middle, char right, int[] widths)
    {
        var segments = widths.Select(width => new string('─', width + 2));
        ConsoleWriter.WriteLine(left + string.Join(middle, segments) + right, BorderColor);
    }

    private static string Align(string value, int width, bool rightAligned) =>
        rightAligned ? value.PadLeft(width) : value.PadRight(width);

    private static string Truncate(string value, int maxWidth) =>
        value.Length <= maxWidth ? value : value[..(maxWidth - 1)] + "…";

    private static string FormatStartedAt(PortListener listener) =>
        listener.StartedAt?.ToString("yyyy-MM-dd HH:mm:ss") ?? "unknown";

    private static ConsoleColor Dim(PortListener listener) => ConsoleColor.Gray;

    private static ConsoleColor PortColor(PortListener listener) => ConsoleColor.Yellow;

    private static ConsoleColor ProcessColor(PortListener listener) =>
        listener.ProcessName.StartsWith('<') ? ConsoleColor.DarkGray : ConsoleColor.Green;
}
