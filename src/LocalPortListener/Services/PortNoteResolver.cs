namespace LocalPortListener.Services;

public sealed class PortNoteResolver
{
    private static readonly Dictionary<int, string> WellKnownPorts = new()
    {
        [21] = "FTP",
        [22] = "SSH",
        [23] = "Telnet",
        [25] = "SMTP",
        [53] = "DNS",
        [80] = "HTTP",
        [110] = "POP3",
        [135] = "RPC endpoint mapper",
        [139] = "NetBIOS",
        [143] = "IMAP",
        [443] = "HTTPS",
        [445] = "SMB",
        [1080] = "SOCKS proxy",
        [1433] = "SQL Server",
        [1521] = "Oracle DB",
        [2375] = "Docker (plain)",
        [2376] = "Docker (TLS)",
        [3000] = "Dev server",
        [3306] = "MySQL",
        [3389] = "RDP",
        [4200] = "Angular dev server",
        [5000] = "ASP.NET Core / dev server",
        [5001] = "ASP.NET Core HTTPS",
        [5173] = "Vite dev server",
        [5432] = "PostgreSQL",
        [5672] = "RabbitMQ",
        [6379] = "Redis",
        [7233] = "Temporal",
        [8000] = "Dev server",
        [8080] = "HTTP alternate",
        [8443] = "HTTPS alternate",
        [9000] = "Dev server",
        [9200] = "Elasticsearch",
        [11211] = "Memcached",
        [15672] = "RabbitMQ management",
        [27017] = "MongoDB"
    };

    public string Resolve(int port, ProcessDetails details)
    {
        var notes = new List<string>();

        if (WellKnownPorts.TryGetValue(port, out var service))
        {
            notes.Add(service);
        }

        if (port >= 49152)
        {
            notes.Add("ephemeral range");
        }

        if (details.AccessDenied)
        {
            notes.Add("no access, run as admin");
        }

        if (details.Exited)
        {
            notes.Add("process already gone");
        }

        return string.Join("; ", notes);
    }
}
