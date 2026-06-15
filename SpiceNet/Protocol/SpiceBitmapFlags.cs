namespace SpiceNet.Protocol;

public enum SpiceBitmapFlags
{
    SPICE_BITMAP_FLAGS_PAL_CACHE_ME = (1 << 0),
    SPICE_BITMAP_FLAGS_PAL_FROM_CACHE = (1 << 1),
    SPICE_BITMAP_FLAGS_TOP_DOWN = (1 << 2),
    SPICE_BITMAP_FLAGS_MASK = 0x7,
}
