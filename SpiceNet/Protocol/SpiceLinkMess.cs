using System.Runtime.InteropServices;

namespace SpiceNet.Protocol;

[StructLayout(LayoutKind.Sequential, Pack = 1)]
public partial struct SpiceLinkMess
{
    [NativeTypeName("uint32_t")]
    public uint connection_id;

    [NativeTypeName("uint8_t")]
    public byte channel_type;

    [NativeTypeName("uint8_t")]
    public byte channel_id;

    [NativeTypeName("uint32_t")]
    public uint num_common_caps;

    [NativeTypeName("uint32_t")]
    public uint num_channel_caps;

    [NativeTypeName("uint32_t")]
    public uint caps_offset;
}
