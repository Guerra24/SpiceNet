using System;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace SpiceNet;

[StructLayout(LayoutKind.Sequential, Pack = 1)]
public partial struct VDAgentMonitorsConfig
{
    [NativeTypeName("uint32_t")]
    public uint num_of_monitors;

    [NativeTypeName("uint32_t")]
    public uint flags;

    [NativeTypeName("VDAgentMonConfig[0]")]
    public _monitors_e__FixedBuffer monitors;

    public partial struct _monitors_e__FixedBuffer
    {
        public VDAgentMonConfig e0;

        [UnscopedRef]
        public ref VDAgentMonConfig this[int index]
        {
            get
            {
                return ref Unsafe.Add(ref e0, index);
            }
        }

        [UnscopedRef]
        public Span<VDAgentMonConfig> AsSpan(int length) => MemoryMarshal.CreateSpan(ref e0, length);
    }
}
