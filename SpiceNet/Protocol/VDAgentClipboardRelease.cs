using System;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace SpiceNet.Protocol;

public partial struct VDAgentClipboardRelease
{
    [NativeTypeName("uint8_t[0]")]
    public _dummy_empty_field_e__FixedBuffer dummy_empty_field;

    public partial struct _dummy_empty_field_e__FixedBuffer
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
