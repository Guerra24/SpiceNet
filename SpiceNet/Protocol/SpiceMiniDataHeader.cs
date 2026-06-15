using System.Runtime.InteropServices;

namespace SpiceNet.Protocol;

[StructLayout(LayoutKind.Sequential, Pack = 1)]
public partial struct SpiceMiniDataHeader
{
    [NativeTypeName("uint16_t")]
    public ushort type;

    [NativeTypeName("uint32_t")]
    public uint size;
}
