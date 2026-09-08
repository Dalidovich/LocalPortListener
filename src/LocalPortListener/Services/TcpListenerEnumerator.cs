using System.Net;
using System.Runtime.InteropServices;
using LocalPortListener.Interop;

namespace LocalPortListener.Services;

public sealed record TcpEndpointRecord(string Protocol, string LocalAddress, int Port, int ProcessId);

public sealed class TcpListenerEnumerator
{
    public IReadOnlyList<TcpEndpointRecord> GetListeners()
    {
        var result = new List<TcpEndpointRecord>();
        result.AddRange(ReadIpv4());
        result.AddRange(ReadIpv6());
        return result;
    }

    private static IEnumerable<TcpEndpointRecord> ReadIpv4()
    {
        var rows = ReadTable(NativeMethods.AF_INET, Marshal.SizeOf<MibTcpRowOwnerPid>(), ptr => Marshal.PtrToStructure<MibTcpRowOwnerPid>(ptr));
        foreach (var row in rows)
        {
            if (row.State != (uint)MibTcpState.Listen)
            {
                continue;
            }

            yield return new TcpEndpointRecord(
                "TCP",
                new IPAddress(row.LocalAddr).ToString(),
                DecodePort(row.LocalPort),
                (int)row.OwningPid);
        }
    }

    private static IEnumerable<TcpEndpointRecord> ReadIpv6()
    {
        var rows = ReadTable(NativeMethods.AF_INET6, Marshal.SizeOf<MibTcp6RowOwnerPid>(), ptr => Marshal.PtrToStructure<MibTcp6RowOwnerPid>(ptr));
        foreach (var row in rows)
        {
            if (row.State != (uint)MibTcpState.Listen)
            {
                continue;
            }

            yield return new TcpEndpointRecord(
                "TCP6",
                new IPAddress(row.LocalAddr, row.LocalScopeId).ToString(),
                DecodePort(row.LocalPort),
                (int)row.OwningPid);
        }
    }

    private static List<T> ReadTable<T>(int addressFamily, int rowSize, Func<IntPtr, T> reader) where T : struct
    {
        var rows = new List<T>();
        var bufferSize = 0;
        var status = NativeMethods.GetExtendedTcpTable(IntPtr.Zero, ref bufferSize, true, addressFamily, TcpTableClass.OwnerPidAll, 0);
        if (status != NativeMethods.ERROR_INSUFFICIENT_BUFFER && status != NativeMethods.NO_ERROR)
        {
            throw new InvalidOperationException($"GetExtendedTcpTable failed with code {status}.");
        }

        var buffer = Marshal.AllocHGlobal(bufferSize);
        try
        {
            status = NativeMethods.GetExtendedTcpTable(buffer, ref bufferSize, true, addressFamily, TcpTableClass.OwnerPidAll, 0);
            if (status != NativeMethods.NO_ERROR)
            {
                throw new InvalidOperationException($"GetExtendedTcpTable failed with code {status}.");
            }

            var entryCount = Marshal.ReadInt32(buffer);
            var cursor = buffer + Marshal.SizeOf<int>();
            for (var i = 0; i < entryCount; i++)
            {
                rows.Add(reader(cursor));
                cursor += rowSize;
            }
        }
        finally
        {
            Marshal.FreeHGlobal(buffer);
        }

        return rows;
    }

    private static int DecodePort(uint rawPort) => (int)(((rawPort & 0xFF) << 8) | ((rawPort >> 8) & 0xFF));
}
