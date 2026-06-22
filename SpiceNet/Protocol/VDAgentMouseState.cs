namespace SpiceNet.Protocol;

public partial struct VDAgentMouseState
{
    [NativeTypeName("uint32_t")]
    public uint x;

    [NativeTypeName("uint32_t")]
    public uint y;

    [NativeTypeName("uint32_t")]
    public uint buttons;

    [NativeTypeName("uint8_t")]
    public byte display_id;
}
