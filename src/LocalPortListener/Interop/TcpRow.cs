using System.Runtime.InteropServices;

namespace LocalPortListener.Interop;

[StructLayout(LayoutKind.Sequential)]
internal struct MibTcpRowOwnerPid
{
    public uint State;
    public uint LocalAddr;
    public uint LocalPort;
    public uint RemoteAddr;
    public uint RemotePort;
    public uint OwningPid;
}

[StructLayout(LayoutKind.Sequential)]
internal struct MibTcp6RowOwnerPid
{
    [MarshalAs(UnmanagedType.ByValArray, SizeConst = 16)]
    public byte[] LocalAddr;
    public uint LocalScopeId;
    public uint LocalPort;
    [MarshalAs(UnmanagedType.ByValArray, SizeConst = 16)]
    public byte[] RemoteAddr;
    public uint RemoteScopeId;
    public uint RemotePort;
    public uint State;
    public uint OwningPid;
}
