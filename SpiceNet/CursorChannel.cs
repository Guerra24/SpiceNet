using SpiceNet.Protocol;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace SpiceNet;

public class CursorChannel : BaseChannel
{
    public event EventHandler<CursorSet>? Set;
    public event EventHandler<SpicePoint16>? Move;
    public event EventHandler<ulong>? InvalidateOne;
    public event EventHandler? InvalidateAll;
    public event EventHandler? Hide;

    public CursorChannel(string host, int port, string password, byte channelId, uint connectionId) : base(host, port, password)
    {
        base.channelId = channelId;
        base.connectionId = connectionId;
        type = Spice.SPICE_CHANNEL_CURSOR;
    }

    protected override int GetChannelCaps()
    {
        return 0;
    }

    protected override unsafe void ProcessMessage(SpiceMiniDataHeader hdr, Span<byte> data, void* ptr)
    {
        nint relPtr = (nint)ptr;
        switch (hdr.type)
        {
            case Spice.SPICE_MSG_CURSOR_INIT:
                {
                    var msg = Unsafe.Read<SpiceMsgCursorInit>(relPtr.ToPointer());
                    relPtr += sizeof(SpiceMsgCursorInit);

                    ReadCursor(ref relPtr, msg.visible);
                }
                break;
            case Spice.SPICE_MSG_CURSOR_SET:
                {
                    var msg = Unsafe.Read<SpiceMsgCursorSet>(relPtr.ToPointer());
                    relPtr += sizeof(SpiceMsgCursorSet);

                    ReadCursor(ref relPtr, msg.visible);
                }
                break;
            case Spice.SPICE_MSG_CURSOR_MOVE:
                {
                    var position = Unsafe.Read<SpicePoint16>(ptr);

                    Move?.Invoke(this, position);
                }
                break;
            case Spice.SPICE_MSG_CURSOR_HIDE:
                Hide?.Invoke(this, new EventArgs());
                break;
            case Spice.SPICE_MSG_CURSOR_TRAIL:
                // TODO
                break;
            case Spice.SPICE_MSG_CURSOR_RESET:
                Hide?.Invoke(this, new EventArgs());
                InvalidateAll?.Invoke(this, new EventArgs());
                break;
            case Spice.SPICE_MSG_CURSOR_INVAL_ONE:
                {
                    var unique = Unsafe.Read<ulong>(relPtr.ToPointer());
                    InvalidateOne?.Invoke(this, unique);
                }
                break;
            case Spice.SPICE_MSG_CURSOR_INVAL_ALL:
                {
                    InvalidateAll?.Invoke(this, new EventArgs());
                }
                break;
        }
    }

    private unsafe void ReadCursor(ref nint relPtr, byte visible)
    {
        var outputImage = Array.Empty<byte>();
        var flags = Unsafe.Read<SpiceCursorFlags>(relPtr.ToPointer());
        relPtr += sizeof(SpiceCursorFlags);

        if (flags.HasFlag(SpiceCursorFlags.SPICE_CURSOR_FLAGS_FROM_CACHE))
        {
            var unique = Unsafe.Read<ulong>(relPtr.ToPointer());

            Set?.Invoke(this, new CursorSet(new SpiceCursorHeader { unique = unique }, flags, visible == 1, outputImage));
        }
        else if (!flags.HasFlag(SpiceCursorFlags.SPICE_CURSOR_FLAGS_NONE))
        {
            var header = Unsafe.Read<SpiceCursorHeader>(relPtr.ToPointer());
            relPtr += sizeof(SpiceCursorHeader);

            switch (header.type)
            {
                case SpiceCursorType.SPICE_CURSOR_TYPE_ALPHA:
                    {
                        var span = new Span<byte>(relPtr.ToPointer(), header.width * header.height * 4);

                        outputImage = span.ToArray();
                    }
                    break;
                case SpiceCursorType.SPICE_CURSOR_TYPE_MONO:
                    {
                        /*var pixels = header.width * header.height;

                        var original = new Span<byte>(relPtr.ToPointer(), pixels);
                        var output = (byte*)NativeMemory.Alloc((nuint)(pixels * 4));

                        for (int i = 0; i < pixels * 4; i += 4)
                        {
                            var alpha = original[i / 4];
                            output[i] = alpha;
                            output[i + 1] = alpha;
                            output[i + 2] = alpha;
                            output[i + 3] = alpha;
                        }

                        var span = new Span<byte>(output, pixels * 4);
                        outputImage = span.ToArray();
                        NativeMemory.Free(output);*/
                    }
                    break;
                case SpiceCursorType.SPICE_CURSOR_TYPE_COLOR4:
                    break;
                case SpiceCursorType.SPICE_CURSOR_TYPE_COLOR8:
                    break;
                case SpiceCursorType.SPICE_CURSOR_TYPE_COLOR16:
                    break;
                case SpiceCursorType.SPICE_CURSOR_TYPE_COLOR24:
                    break;
                case SpiceCursorType.SPICE_CURSOR_TYPE_COLOR32:
                    break;
                case SpiceCursorType.SPICE_CURSOR_TYPE_ENUM_END:
                    break;
            }

            Set?.Invoke(this, new CursorSet(header, flags, visible == 1, outputImage));
        }
    }
}

public sealed class CursorSet : EventArgs
{
    public SpiceCursorHeader Header { get; }
    public SpiceCursorFlags Flags { get; }
    public bool Visible { get; }
    public byte[] Image { get; }

    public CursorSet(SpiceCursorHeader header, SpiceCursorFlags flags, bool visible, byte[] image)
    {
        Header = header;
        Flags = flags;
        Visible = visible;
        Image = image;
    }
}

[StructLayout(LayoutKind.Sequential, Pack = 1)]
public struct SpiceMsgCursorInit
{
    public SpicePoint16 position;
    public ushort trail_length;
    public ushort trail_frequency;
    public byte visible;
}

[StructLayout(LayoutKind.Sequential, Pack = 1)]
public struct SpiceMsgCursorSet
{
    public SpicePoint16 position;
    public byte visible;
}

