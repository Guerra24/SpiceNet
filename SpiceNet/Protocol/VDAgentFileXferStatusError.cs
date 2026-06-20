namespace SpiceNet;

public partial struct VDAgentFileXferStatusError
{
    [NativeTypeName("uint8_t")]
    public byte error_type;

    [NativeTypeName("uint32_t")]
    public uint error_code;
}
