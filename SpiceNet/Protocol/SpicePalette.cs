using System;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace SpiceNet;

public partial struct SpicePalette
{
    [NativeTypeName("uint64_t")]
    public ulong unique;

    [NativeTypeName("uint16_t")]
    public ushort num_ents;

    [NativeTypeName("uint32_t[0]")]
    public _ents_e__FixedBuffer ents;

    public partial struct _ents_e__FixedBuffer
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
