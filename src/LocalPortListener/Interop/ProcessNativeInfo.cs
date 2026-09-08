using System.Runtime.InteropServices;
using System.Text;

namespace LocalPortListener.Interop;

internal static class ProcessNativeInfo
{
    private const int PROCESS_QUERY_LIMITED_INFORMATION = 0x1000;

    public static (string? Path, DateTime? StartedAt) Query(int processId)
    {
        var handle = OpenProcess(PROCESS_QUERY_LIMITED_INFORMATION, false, processId);
        if (handle == IntPtr.Zero)
        {
            return (null, null);
        }

        try
        {
            return (QueryPath(handle), QueryStartTime(handle));
        }
        finally
        {
            CloseHandle(handle);
        }
    }

    private static string? QueryPath(IntPtr handle)
    {
        var buffer = new StringBuilder(1024);
        var size = buffer.Capacity;
        return QueryFullProcessImageName(handle, 0, buffer, ref size) ? buffer.ToString() : null;
    }

    private static DateTime? QueryStartTime(IntPtr handle)
    {
        if (!GetProcessTimes(handle, out var creation, out _, out _, out _))
        {
            return null;
        }

        return DateTime.FromFileTime(creation);
    }

    [DllImport("kernel32.dll", SetLastError = true)]
    private static extern IntPtr OpenProcess(int desiredAccess, bool inheritHandle, int processId);

    [DllImport("kernel32.dll", SetLastError = true)]
    private static extern bool CloseHandle(IntPtr handle);

    [DllImport("kernel32.dll", SetLastError = true, CharSet = CharSet.Unicode)]
    private static extern bool QueryFullProcessImageName(IntPtr handle, int flags, StringBuilder exeName, ref int size);

    [DllImport("kernel32.dll", SetLastError = true)]
    private static extern bool GetProcessTimes(IntPtr handle, out long creation, out long exit, out long kernel, out long user);
}
