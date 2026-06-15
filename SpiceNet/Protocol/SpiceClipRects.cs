using System;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace SpiceNet;

public partial struct SpiceClipRects
{
    [NativeTypeName("uint32_t")]
    public uint num_rects;

    [NativeTypeName("SpiceRect[0]")]
    public _rects_e__FixedBuffer rects;

    public partial struct _rects_e__FixedBuffer
    {
        public SpiceRect e0;

        [UnscopedRef]
        public ref SpiceRect this[int index]
        {
            get
            {
                return ref Unsafe.Add(ref e0, index);
            }
        }

        [UnscopedRef]
        public Span<SpiceRect> AsSpan(int length) => MemoryMarshal.CreateSpan(ref e0, length);
    }
}
