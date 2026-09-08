using LocalPortListener.Rendering;
using LocalPortListener.Services;

namespace LocalPortListener;

public class Program
{
    public static void Main(string[] args)
    {
        if (!OperatingSystem.IsWindows())
        {
            ConsoleWriter.WriteLine("This application runs on Windows only.", ConsoleColor.Red);
            return;
        }

        var application = new ListenerApplication(
            new ListenerCollector(
                new TcpListenerEnumerator(),
                new ProcessInspector(),
                new WindowsProcessClassifier(),
                new PortNoteResolver()),
            new ProcessKiller(),
            new CommandParser(),
            new ListenerTableRenderer());

        application.Run();
    }
}
