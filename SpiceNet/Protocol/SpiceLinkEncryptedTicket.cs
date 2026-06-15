using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace SpiceNet.Protocol;

[StructLayout(LayoutKind.Sequential, Pack = 1)]
public partial struct SpiceLinkEncryptedTicket
{
    [NativeTypeName("uint8_t[128]")]
    public _encrypted_data_e__FixedBuffer encrypted_data;

    [InlineArray(128)]
    public partial struct _encrypted_data_e__FixedBuffer
    {
        public byte e0;
    }
}
