using System;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace SpiceNet;

public partial struct VDAgentGraphicsDeviceInfo
{
    [NativeTypeName("uint32_t")]
    public uint count;

    [NativeTypeName("uint8_t[0]")]
    public _display_info_e__FixedBuffer display_info;

    public partial struct _display_info_e__FixedBuffer
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
