namespace SpiceNet.Protocol;

public enum SpiceAlphaFlags
{
    SPICE_ALPHA_FLAGS_DEST_HAS_ALPHA = (1 << 0),
    SPICE_ALPHA_FLAGS_SRC_SURFACE_HAS_ALPHA = (1 << 1),
    SPICE_ALPHA_FLAGS_MASK = 0x3,
}
