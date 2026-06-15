namespace SpiceNet.Protocol;

public partial struct SpiceSubMessage
{
    [NativeTypeName("uint16_t")]
    public ushort type;

    [NativeTypeName("uint32_t")]
    public uint size;
}
