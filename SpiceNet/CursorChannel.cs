using SpiceNet.Protocol;
using System.Net;

namespace SpiceNet;

public class CursorChannel : BaseChannel
{
    public CursorChannel(IPEndPoint endPoint, byte channelId, uint connectionId) : base(endPoint)
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
                // TODO
                break;
            case Spice.SPICE_MSG_CURSOR_SET:
                // TODO
                break;
            case Spice.SPICE_MSG_CURSOR_MOVE:
                // TODO
                break;
            case Spice.SPICE_MSG_CURSOR_HIDE:
                // TODO
                break;
            case Spice.SPICE_MSG_CURSOR_TRAIL:
                // TODO
                break;
            case Spice.SPICE_MSG_CURSOR_RESET:
                // TODO
                break;
            case Spice.SPICE_MSG_CURSOR_INVAL_ONE:
                // TODO
                break;
            case Spice.SPICE_MSG_CURSOR_INVAL_ALL:
                // TODO
                break;
        }
    }
}
