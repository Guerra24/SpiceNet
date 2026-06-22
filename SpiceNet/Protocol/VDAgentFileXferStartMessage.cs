using System;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace SpiceNet.Protocol;

public partial struct VDAgentFileXferStartMessage
{
    [NativeTypeName("uint32_t")]
    public uint id;

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
