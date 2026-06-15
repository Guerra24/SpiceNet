namespace SpiceNet.Protocol;

public partial struct SpiceDataHeader
{
    [NativeTypeName("uint64_t")]
    public ulong serial;

    [NativeTypeName("uint16_t")]
    public ushort type;

    [NativeTypeName("uint32_t")]
    public uint size;

    [NativeTypeName("uint32_t")]
    public uint sub_list;
}
