namespace SpiceNet;

public partial struct SpiceCursorHeader
{
    [NativeTypeName("uint64_t")]
    public ulong unique;

    [NativeTypeName("uint16_t")]
    public ushort type;

    [NativeTypeName("uint16_t")]
    public ushort width;

    [NativeTypeName("uint16_t")]
    public ushort height;

    [NativeTypeName("uint16_t")]
    public ushort hot_spot_x;

    [NativeTypeName("uint16_t")]
    public ushort hot_spot_y;
}
