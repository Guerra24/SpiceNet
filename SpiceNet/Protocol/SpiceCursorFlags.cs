namespace SpiceNet.Protocol;

public enum SpiceCursorFlags : ushort
{
    SPICE_CURSOR_FLAGS_NONE = (1 << 0),
    SPICE_CURSOR_FLAGS_CACHE_ME = (1 << 1),
    SPICE_CURSOR_FLAGS_FROM_CACHE = (1 << 2),
    SPICE_CURSOR_FLAGS_MASK = 0x7,
}
