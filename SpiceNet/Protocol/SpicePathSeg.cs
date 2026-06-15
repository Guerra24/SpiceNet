using System;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace SpiceNet;

public partial struct SpicePathSeg
{
    [NativeTypeName("uint32_t")]
    public uint flags;

    [NativeTypeName("uint32_t")]
    public uint count;

    [NativeTypeName("SpicePointFix[0]")]
    public _points_e__FixedBuffer points;

    public partial struct _points_e__FixedBuffer
    {
        public SpicePointFix e0;

        [UnscopedRef]
        public ref SpicePointFix this[int index]
        {
            get
            {
                return ref Unsafe.Add(ref e0, index);
            }
        }

        [UnscopedRef]
        public Span<SpicePointFix> AsSpan(int length) => MemoryMarshal.CreateSpan(ref e0, length);
    }
}
