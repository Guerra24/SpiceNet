namespace SpiceNet.Protocol;

public enum SpiceSurfaceFlags
{
    SPICE_SURFACE_FLAGS_PRIMARY = (1 << 0),
    SPICE_SURFACE_FLAGS_STREAMING_MODE = (1 << 1),
    SPICE_SURFACE_FLAGS_MASK = 0x3,
}
