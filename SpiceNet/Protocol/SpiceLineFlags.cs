namespace SpiceNet.Protocol;

public enum SpiceLineFlags
{
    SPICE_LINE_FLAGS_START_WITH_GAP = (1 << 2),
    SPICE_LINE_FLAGS_STYLED = (1 << 3),
    SPICE_LINE_FLAGS_MASK = 0xc,
}
