namespace SpiceNet.Protocol;

public enum SpiceRopd : ushort
{
    SPICE_ROPD_INVERS_SRC = 1 << 0,
    SPICE_ROPD_INVERS_BRUSH = 1 << 1,
    SPICE_ROPD_INVERS_DEST = 1 << 2,
    SPICE_ROPD_OP_PUT = 1 << 3,
    SPICE_ROPD_OP_OR = 1 << 4,
    SPICE_ROPD_OP_AND = 1 << 5,
    SPICE_ROPD_OP_XOR = 1 << 6,
    SPICE_ROPD_OP_BLACKNESS = 1 << 7,
    SPICE_ROPD_OP_WHITENESS = 1 << 8,
    SPICE_ROPD_OP_INVERS = 1 << 9,
    SPICE_ROPD_INVERS_RES = 1 << 10,
    SPICE_ROPD_MASK = 0x7ff,
}
