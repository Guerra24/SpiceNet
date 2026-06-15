namespace SpiceNet.Protocol;

public enum SpiceMouseMode
{
    SPICE_MOUSE_MODE_SERVER = (1 << 0),
    SPICE_MOUSE_MODE_CLIENT = (1 << 1),
    SPICE_MOUSE_MODE_MASK = 0x3,
}
