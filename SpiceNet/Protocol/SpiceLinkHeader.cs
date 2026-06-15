using System.Runtime.InteropServices;

namespace SpiceNet.Protocol;

[StructLayout(LayoutKind.Sequential, Pack = 1)]
public partial struct SpiceLinkHeader
{
    [NativeTypeName("uint32_t")]
    public uint magic;

    [NativeTypeName("uint32_t")]
    public uint major_version;

    [NativeTypeName("uint32_t")]
    public uint minor_version;

    [NativeTypeName("uint32_t")]
    public uint size;
}
