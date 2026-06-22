namespace SpiceNet.Protocol;

public partial struct VDAgentFileXferStatusNotEnoughSpace
{
    [NativeTypeName("uint64_t")]
    public ulong disk_free_space;
}
