using System.Runtime.InteropServices;

namespace SpiceNet.Protocol;

[StructLayout(LayoutKind.Sequential, Pack = 1)]
public partial struct SpiceRect
{
    [NativeTypeName("int32_t")]
    public int top;

    [NativeTypeName("int32_t")]
    public int left;

    [NativeTypeName("int32_t")]
    public int bottom;

    [NativeTypeName("int32_t")]
    public int right;
}
