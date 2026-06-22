namespace SpiceNet.Protocol;

public unsafe partial struct SpiceClip
{
    [NativeTypeName("uint8_t")]
    public byte type;

    public SpiceClipRects* rects;
}
