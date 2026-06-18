namespace SpiceNet.Protocol;

public enum SpiceImageFlags : byte
{
    SPICE_IMAGE_FLAGS_CACHE_ME = (1 << 0),
    SPICE_IMAGE_FLAGS_HIGH_BITS_SET = (1 << 1),
    SPICE_IMAGE_FLAGS_CACHE_REPLACE_ME = (1 << 2),
    SPICE_IMAGE_FLAGS_MASK = 0x7,
}
