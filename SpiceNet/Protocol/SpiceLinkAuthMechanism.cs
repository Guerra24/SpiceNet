using System.Runtime.InteropServices;

namespace SpiceNet.Protocol;

[StructLayout(LayoutKind.Sequential, Pack = 1)]
public partial struct SpiceLinkAuthMechanism
{
    [NativeTypeName("uint32_t")]
    public uint auth_mechanism;
}
