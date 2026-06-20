using System;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace SpiceNet;

public partial struct VDAgentMessage
{
    [NativeTypeName("uint32_t")]
    public uint protocol;

    [NativeTypeName("uint32_t")]
    public uint type;

    [NativeTypeName("uint64_t")]
    public ulong opaque;

    [NativeTypeName("uint32_t")]
    public uint size;

    [NativeTypeName("uint8_t[0]")]
    public _data_e__FixedBuffer data;

    public partial struct _data_e__FixedBuffer
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
