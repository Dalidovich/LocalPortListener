using System.Runtime.InteropServices;

namespace LocalPortListener.Interop;

internal static class NativeMethods
{
    public const int AF_INET = 2;
    public const int AF_INET6 = 23;

    public const uint NO_ERROR = 0;
    public const uint ERROR_INSUFFICIENT_BUFFER = 122;

    [DllImport("iphlpapi.dll", SetLastError = true)]
    public static extern uint GetExtendedTcpTable(
        IntPtr pTcpTable,
        ref int pdwSize,
        bool bOrder,
        int ulAf,
        TcpTableClass tableClass,
        int reserved);
}
