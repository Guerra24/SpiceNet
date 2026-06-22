namespace SpiceNet.Protocol;

public partial struct VDIChunkHeader
{
    [NativeTypeName("uint32_t")]
    public uint port;

    [NativeTypeName("uint32_t")]
    public uint size;
}
