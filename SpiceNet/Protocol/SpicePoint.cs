using System.Runtime.InteropServices;

namespace SpiceNet.Protocol;

[StructLayout(LayoutKind.Sequential, Pack = 1)]
public partial struct SpicePoint
{
    [NativeTypeName("int32_t")]
    public int x;

    [NativeTypeName("int32_t")]
    public int y;
}
