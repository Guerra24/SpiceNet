namespace SpiceNet.Protocol;

public partial struct SpicePath
{
    [NativeTypeName("uint32_t")]
    public uint num_segments;

    [NativeTypeName("SpicePathSeg *[0]")]
    public _segments_e__FixedBuffer segments;

    public unsafe partial struct _segments_e__FixedBuffer
    {
        public SpicePathSeg* e0;

        public ref SpicePathSeg* this[int index]
        {
            get
            {
                fixed (SpicePathSeg** pThis = &e0)
                {
                    return ref pThis[index];
                }
            }
        }
    }
}
