namespace SpiceNet.Protocol;

public partial struct VDAgentMonitorMM
{
    [NativeTypeName("uint16_t")]
    public ushort height;

    [NativeTypeName("uint16_t")]
    public ushort width;
}
