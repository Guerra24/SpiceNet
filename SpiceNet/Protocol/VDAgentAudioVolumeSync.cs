using System;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace SpiceNet.Protocol;

public partial struct VDAgentAudioVolumeSync
{
    [NativeTypeName("uint8_t")]
    public byte is_playback;

    [NativeTypeName("uint8_t")]
    public byte mute;

    [NativeTypeName("uint8_t")]
    public byte nchannels;

    [NativeTypeName("uint16_t[0]")]
    public _volume_e__FixedBuffer volume;

    public partial struct _volume_e__FixedBuffer
    {
        public ushort e0;

        [UnscopedRef]
        public ref ushort this[int index]
        {
            get
            {
                return ref Unsafe.Add(ref e0, index);
            }
        }

        [UnscopedRef]
        public Span<ushort> AsSpan(int length) => MemoryMarshal.CreateSpan(ref e0, length);
    }
}
