namespace SpiceNet;

public partial struct SpiceString
{
    [NativeTypeName("uint16_t")]
    public ushort length;

    [NativeTypeName("uint16_t")]
    public ushort flags;

    [NativeTypeName("SpiceRasterGlyph *[0]")]
    public _glyphs_e__FixedBuffer glyphs;

    public unsafe partial struct _glyphs_e__FixedBuffer
    {
        public SpiceRasterGlyph* e0;

        public ref SpiceRasterGlyph* this[int index]
        {
            get
            {
                fixed (SpiceRasterGlyph** pThis = &e0)
                {
                    return ref pThis[index];
                }
            }
        }
    }
}
