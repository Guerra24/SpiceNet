using System;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace SpiceNet.Protocol;

public partial struct VDAgentDeviceDisplayInfo
{
    [NativeTypeName("uint32_t")]
    public uint channel_id;

    [NativeTypeName("uint32_t")]
    public uint monitor_id;

    [NativeTypeName("uint32_t")]
    public uint device_display_id;

    [NativeTypeName("uint32_t")]
    public uint device_address_len;

    [NativeTypeName("uint8_t[0]")]
    public _device_address_e__FixedBuffer device_address;

    public partial struct _device_address_e__FixedBuffer
    {
        public byte e0;

        [UnscopedRef]
        public ref byte this[int index]
        {
            get
            {
                return ref Unsafe.Add(ref e0, index);
            }
        }

        [UnscopedRef]
        public Span<byte> AsSpan(int length) => MemoryMarshal.CreateSpan(ref e0, length);
    }
}
