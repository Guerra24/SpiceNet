using System.Runtime.InteropServices;

namespace SpiceNet;

[StructLayout(LayoutKind.Sequential, Pack = 1)]
public partial struct SpicePoint16
{
    [NativeTypeName("int16_t")]
    public short x;

    [NativeTypeName("int16_t")]
    public short y;
}
