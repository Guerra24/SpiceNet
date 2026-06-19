using SpiceNet.Protocol;
using System.Net;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace SpiceNet;

public class PlaybackChannel : BaseChannel
{
    public event EventHandler<SpiceAudioDataMode>? Mode;
    public event EventHandler<SpiceMsgRecordStart>? StartPlayback;
    public event EventHandler<PlaybackData>? Data;
    public event EventHandler? StopPlayback;

    public PlaybackChannel(IPEndPoint endPoint, byte channelId, uint connectionId) : base(endPoint)
    {
        base.channelId = channelId;
        base.connectionId = connectionId;
        type = Spice.SPICE_CHANNEL_PLAYBACK;
    }

    protected override int GetChannelCaps()
    {
        return 0;// 1 << Spice.SPICE_PLAYBACK_CAP_OPUS;
    }

    protected override unsafe void ProcessMessage(SpiceMiniDataHeader hdr, Span<byte> data, void* ptr)
    {
        nint relPtr = (nint)ptr;
        switch (hdr.type)
        {
            case Spice.SPICE_MSG_PLAYBACK_START:
                {
                    var start = Unsafe.Read<SpiceMsgRecordStart>(ptr);
                    StartPlayback?.Invoke(this, start);
                }
                break;
            case Spice.SPICE_MSG_PLAYBACK_MODE:
                {
                    var time = Unsafe.Read<uint>(relPtr.ToPointer());
                    relPtr += sizeof(uint);
                    var mode = Unsafe.Read<SpiceAudioDataMode>(relPtr.ToPointer());

                    Mode?.Invoke(this, mode);
                }
                break;
            case Spice.SPICE_MSG_PLAYBACK_DATA:
                {
                    var time = Unsafe.Read<uint>(ptr);

                    var dt = MemoryMarshal.Cast<byte, short>(data[sizeof(uint)..]);

                    Data?.Invoke(this, new(time, dt.ToArray()));
                }
                break;
            case Spice.SPICE_MSG_PLAYBACK_STOP:
                {
                    StopPlayback?.Invoke(this, new EventArgs());
                }
                break;
        }
    }
}

public sealed class PlaybackData : EventArgs
{
    public uint Time { get; }
    public short[] Data { get; }

    public PlaybackData(uint time, short[] data)
    {
        Time = time;
        Data = data;
    }
}

[StructLayout(LayoutKind.Sequential, Pack = 1)]
public struct SpiceMsgRecordStart
{
    public uint channels;
    public SpiceAudioFmt format;
    public uint frequency;
    public uint time;
}
