namespace SpiceNet.Protocol;

public enum SpiceStringFlags
{
    SPICE_STRING_FLAGS_RASTER_A1 = (1 << 0),
    SPICE_STRING_FLAGS_RASTER_A4 = (1 << 1),
    SPICE_STRING_FLAGS_RASTER_A8 = (1 << 2),
    SPICE_STRING_FLAGS_RASTER_TOP_DOWN = (1 << 3),
    SPICE_STRING_FLAGS_MASK = 0xf,
}
