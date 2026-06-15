using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace SpiceNet.Protocol;

[StructLayout(LayoutKind.Sequential, Pack = 1)]
public partial struct SpiceLinkReply
{
    [NativeTypeName("uint32_t")]
    public uint error;

    [NativeTypeName("uint8_t[162]")]
    public _pub_key_e__FixedBuffer pub_key;

    [NativeTypeName("uint32_t")]
    public uint num_common_caps;

    [NativeTypeName("uint32_t")]
    public uint num_channel_caps;

    [NativeTypeName("uint32_t")]
    public uint caps_offset;

    [InlineArray(162)]
    public partial struct _pub_key_e__FixedBuffer
    {
        public byte e0;
    }
}
