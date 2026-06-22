namespace SpiceNet.Protocol;

public partial struct VDAgentReply
{
    [NativeTypeName("uint32_t")]
    public uint type;

    [NativeTypeName("uint32_t")]
    public uint error;
}
