using System.Runtime.InteropServices;

namespace SpiceNet;

[StructLayout(LayoutKind.Sequential, Pack = 1)]
public partial struct VDAgentMonConfig
{
    [NativeTypeName("uint32_t")]
    public uint height;

    [NativeTypeName("uint32_t")]
    public uint width;

    [NativeTypeName("uint32_t")]
    public uint depth;

    [NativeTypeName("int32_t")]
    public int x;

    [NativeTypeName("int32_t")]
    public int y;
}
