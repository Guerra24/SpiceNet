using SpiceNet.Protocol;
using System.Net;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace SpiceNet;

public class InputsChannel : BaseChannel
{
    private int waitingForAck;
    private ushort internalButtonState;

    public InputsChannel(IPEndPoint endPoint, byte channelId, uint connectionId) : base(endPoint)
    {
        base.channelId = channelId;
        base.connectionId = connectionId;
        type = Spice.SPICE_CHANNEL_INPUTS;
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
            case Spice.SPICE_MSG_INPUTS_INIT:
                {
                    var keyModifiers = Unsafe.Read<ushort>(ptr);
                    // TODO
                }
                break;
            case Spice.SPICE_MSG_INPUTS_KEY_MODIFIERS:
                {
                    var keyModifiers = Unsafe.Read<ushort>(ptr);
                    // TODO
                }
                break;
            case Spice.SPICE_MSG_INPUTS_MOUSE_MOTION_ACK:
                // TODO
                waitingForAck -= Spice.SPICE_INPUT_MOTION_ACK_BUNCH;
                break;
        }
    }

    public void MouseMove(uint x, uint y, byte displayId)
    {
        unsafe
        {
            var mouseMode = new SpiceMiniDataHeader
            {
                type = Spice.SPICE_MSGC_INPUTS_MOUSE_POSITION,
                size = (uint)sizeof(SpiceMousePosition)
            };

            var param = new SpiceMousePosition
            {
                x = x,
                y = y,
                button_state = internalButtonState,
                display_id = displayId
            };

            if (waitingForAck < (2 * Spice.SPICE_INPUT_MOTION_ACK_BUNCH))
            {
                SendMiniDataHeader(mouseMode, param);
                waitingForAck++;
            }
        }
    }

    public void MouseDown(byte button, ushort buttonState)
    {
        unsafe
        {
            var mouseMode = new SpiceMiniDataHeader
            {
                type = Spice.SPICE_MSGC_INPUTS_MOUSE_PRESS,
                size = (uint)sizeof(SpiceMousePress)
            };

            var param = new SpiceMousePress
            {
                button = button,
                button_state = internalButtonState = buttonState
            };

            SendMiniDataHeader(mouseMode, param);
        }
    }

    public void MouseUp(byte button, ushort buttonState)
    {
        unsafe
        {
            var mouseMode = new SpiceMiniDataHeader
            {
                type = Spice.SPICE_MSGC_INPUTS_MOUSE_RELEASE,
                size = (uint)sizeof(SpiceMousePress)
            };

            var param = new SpiceMousePress
            {
                button = button,
                button_state = internalButtonState = buttonState
            };

            SendMiniDataHeader(mouseMode, param);
        }
    }

    public void KeyDown(uint key)
    {
        unsafe
        {
            var mouseMode = new SpiceMiniDataHeader
            {
                type = Spice.SPICE_MSGC_INPUTS_KEY_DOWN,
                size = (uint)sizeof(SpiceKey)
            };

            var param = new SpiceKey
            {
                code = key
            };

            SendMiniDataHeader(mouseMode, param);
        }
    }

    public void KeyUp(uint key)
    {
        unsafe
        {
            var mouseMode = new SpiceMiniDataHeader
            {
                type = Spice.SPICE_MSGC_INPUTS_KEY_UP,
                size = (uint)sizeof(SpiceKey)
            };

            var param = new SpiceKey
            {
                code = key < 0x100 ? key | 0x80 : key | 0x8000,
            };

            SendMiniDataHeader(mouseMode, param);
        }
    }
}

[StructLayout(LayoutKind.Sequential, Pack = 1)]
public struct SpiceMousePosition
{
    public uint x;
    public uint y;
    public ushort button_state;
    public byte display_id;
}

[StructLayout(LayoutKind.Sequential, Pack = 1)]
public struct SpiceMousePress
{
    public byte button;
    public ushort button_state;
}

[StructLayout(LayoutKind.Sequential, Pack = 1)]
public struct SpiceKey
{
    public uint code;
}
