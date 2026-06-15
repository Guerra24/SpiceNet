namespace SpiceNet.Protocol;

public enum SpicePathFlags
{
    SPICE_PATH_BEGIN = (1 << 0),
    SPICE_PATH_END = (1 << 1),
    SPICE_PATH_CLOSE = (1 << 3),
    SPICE_PATH_BEZIER = (1 << 4),
    SPICE_PATH_FLAGS_MASK = 0x1b,
}
