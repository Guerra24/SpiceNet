namespace SpiceNet.Protocol;

public unsafe partial struct SpiceText
{
    public SpiceString* str;

    public SpiceRect back_area;

    [NativeTypeName("uint32_t")]
    public uint fore_brush;

    [NativeTypeName("uint32_t")]
    public uint back_brush;

    [NativeTypeName("uint16_t")]
    public ushort fore_mode;

    [NativeTypeName("uint16_t")]
    public ushort back_mode;
}
