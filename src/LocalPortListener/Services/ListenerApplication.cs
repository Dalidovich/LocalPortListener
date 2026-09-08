using System.Text;
using LocalPortListener.Models;
using LocalPortListener.Rendering;

namespace LocalPortListener.Services;

public sealed class ListenerApplication
{
    private readonly ListenerCollector _collector;
    private readonly ProcessKiller _killer;
    private readonly CommandParser _parser;
    private readonly ListenerTableRenderer _renderer;

    private IReadOnlyList<PortListener> _listeners = [];
    private bool _includeWindowsProcesses;
    private string? _status;
    private ConsoleColor _statusColor = ConsoleColor.Gray;

    public ListenerApplication(
        ListenerCollector collector,
        ProcessKiller killer,
        CommandParser parser,
        ListenerTableRenderer renderer)
    {
        _collector = collector;
        _killer = killer;
        _parser = parser;
        _renderer = renderer;
    }

    public void Run()
    {
        Console.OutputEncoding = Encoding.UTF8;
        Refresh(includeWindowsProcesses: false);

        while (true)
        {
            Draw();
            var input = Console.ReadLine();
            if (input is null)
            {
                return;
            }

            var command = _parser.Parse(input);
            if (command.Kind == CommandKind.Quit)
            {
                return;
            }

            Execute(command);
        }
    }

    private void Execute(ConsoleCommand command)
    {
        switch (command.Kind)
        {
            case CommandKind.Refresh:
                Refresh(includeWindowsProcesses: false);
                SetStatus("Refreshed.", ConsoleColor.Green);
                break;
            case CommandKind.RefreshWithWindows:
                Refresh(includeWindowsProcesses: true);
                SetStatus("Refreshed, Windows processes included.", ConsoleColor.Green);
                break;
            case CommandKind.KillById:
                KillById(command.Argument);
                break;
            case CommandKind.KillByPid:
                KillByPid(command.Argument);
                break;
            case CommandKind.Help:
                ShowHelp();
                break;
            default:
                SetStatus(command.Error ?? "Unknown command.", ConsoleColor.Red);
                break;
        }
    }

    private void KillById(int id)
    {
        var target = _listeners.FirstOrDefault(listener => listener.Id == id);
        if (target is null)
        {
            SetStatus($"No listener with id {id} in the current list.", ConsoleColor.Red);
            return;
        }

        ApplyKill(target.ProcessId);
    }

    private void KillByPid(int pid) => ApplyKill(pid);

    private void ApplyKill(int pid)
    {
        var result = _killer.Kill(pid);
        Refresh(_includeWindowsProcesses);
        SetStatus(result.Message, result.Success ? ConsoleColor.Green : ConsoleColor.Red);
    }

    private void Refresh(bool includeWindowsProcesses)
    {
        _includeWindowsProcesses = includeWindowsProcesses;
        _listeners = _collector.Collect(includeWindowsProcesses);
    }

    private void Draw()
    {
        ClearScreen();
        WriteBanner();
        _renderer.Render(_listeners, _includeWindowsProcesses);
        WriteCommands();
        WriteStatus();
        ConsoleWriter.Write("> ", ConsoleColor.White);
    }

    private static void ClearScreen()
    {
        try
        {
            Console.Clear();
        }
        catch (IOException)
        {
            Console.WriteLine();
        }
    }

    private static void WriteBanner()
    {
        ConsoleWriter.WriteLine("Local Port Listeners", ConsoleColor.Cyan);
        ConsoleWriter.WriteLine(new string('=', 20), ConsoleColor.DarkCyan);
        Console.WriteLine();
    }

    private static void WriteCommands()
    {
        ConsoleWriter.Write("r", ConsoleColor.Yellow);
        ConsoleWriter.Write(" refresh   ", ConsoleColor.Gray);
        ConsoleWriter.Write("a", ConsoleColor.Yellow);
        ConsoleWriter.Write(" refresh with Windows processes   ", ConsoleColor.Gray);
        ConsoleWriter.Write("k {id}", ConsoleColor.Yellow);
        ConsoleWriter.Write(" kill by id   ", ConsoleColor.Gray);
        ConsoleWriter.Write("kp {pid}", ConsoleColor.Yellow);
        ConsoleWriter.Write(" kill by pid   ", ConsoleColor.Gray);
        ConsoleWriter.Write("q", ConsoleColor.Yellow);
        ConsoleWriter.WriteLine(" quit", ConsoleColor.Gray);
        Console.WriteLine();
    }

    private void WriteStatus()
    {
        if (string.IsNullOrEmpty(_status))
        {
            return;
        }

        ConsoleWriter.WriteLine(_status, _statusColor);
        Console.WriteLine();
    }

    private void ShowHelp()
    {
        SetStatus(
            "r = refresh; a = refresh including Windows processes; k {id} = kill listed listener; kp {pid} = kill by pid; q = quit.",
            ConsoleColor.Cyan);
    }

    private void SetStatus(string message, ConsoleColor color)
    {
        _status = message;
        _statusColor = color;
    }
}
