namespace SpiceNet.Protocol;

public partial struct VDAgentDisplayConfig
{
    [NativeTypeName("uint32_t")]
    public uint flags;

    [NativeTypeName("uint32_t")]
    public uint depth;
}
