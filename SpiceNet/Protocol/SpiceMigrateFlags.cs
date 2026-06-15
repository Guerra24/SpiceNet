namespace SpiceNet.Protocol;

public enum SpiceMigrateFlags
{
    SPICE_MIGRATE_NEED_FLUSH = (1 << 0),
    SPICE_MIGRATE_NEED_DATA_TRANSFER = (1 << 1),
    SPICE_MIGRATE_FLAGS_MASK = 0x3,
}
