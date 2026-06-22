using System;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace SpiceNet.Protocol;

[StructLayout(LayoutKind.Sequential, Pack = 1)]
public partial struct VDAgentAnnounceCapabilities
{
    [NativeTypeName("uint32_t")]
    public uint request;

    [NativeTypeName("uint32_t[0]")]
    public uint caps;

}
