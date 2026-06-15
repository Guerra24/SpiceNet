using System;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace SpiceNet.Protocol;

public partial struct SpiceSubMessageList
{
    [NativeTypeName("uint16_t")]
    public ushort size;

    [NativeTypeName("uint32_t[0]")]
    public _sub_messages_e__FixedBuffer sub_messages;

    public partial struct _sub_messages_e__FixedBuffer
    {
        public uint e0;

        [UnscopedRef]
        public ref uint this[int index]
        {
            get
            {
                return ref Unsafe.Add(ref e0, index);
            }
        }

        [UnscopedRef]
        public Span<uint> AsSpan(int length) => MemoryMarshal.CreateSpan(ref e0, length);
    }
}
