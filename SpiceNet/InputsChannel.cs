using SpiceNet.Protocol;
using System.Net;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace SpiceNet;

public class InputsChannel : BaseChannel
{
    private int waitingForAck;
    private ushort internalButtonState;

    public event EventHandler<InputKeyModifiers>? Init;
    public event EventHandler<InputKeyModifiers>? KeyModifiersChanged;

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
                    var scrollLock = (keyModifiers & Spice.SPICE_SCROLL_LOCK_MODIFIER) != 0;
                    var numLock = (keyModifiers & Spice.SPICE_NUM_LOCK_MODIFIER) != 0;
                    var capsLock = (keyModifiers & Spice.SPICE_CAPS_LOCK_MODIFIER) != 0;

                    Init?.Invoke(this, new InputKeyModifiers(scrollLock, numLock, capsLock));
                }
                break;
            case Spice.SPICE_MSG_INPUTS_KEY_MODIFIERS:
                {
                    var keyModifiers = Unsafe.Read<ushort>(ptr);
                    var scrollLock = (keyModifiers & Spice.SPICE_SCROLL_LOCK_MODIFIER) != 0;
                    var numLock = (keyModifiers & Spice.SPICE_NUM_LOCK_MODIFIER) != 0;
                    var capsLock = (keyModifiers & Spice.SPICE_CAPS_LOCK_MODIFIER) != 0;

                    KeyModifiersChanged?.Invoke(this, new InputKeyModifiers(scrollLock, numLock, capsLock));
                }
                break;
            case Spice.SPICE_MSG_INPUTS_MOUSE_MOTION_ACK:
                waitingForAck -= Spice.SPICE_INPUT_MOTION_ACK_BUNCH;
                break;
        }
    }

    public unsafe void MouseMove(uint x, uint y, byte displayId)
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

    public unsafe void MouseMove(int dx, int dy)
    {
        var mouseMode = new SpiceMiniDataHeader
        {
            type = Spice.SPICE_MSGC_INPUTS_MOUSE_MOTION,
            size = (uint)sizeof(SpiceMouseMotion)
        };

        var param = new SpiceMouseMotion
        {
            dx = dx,
            dy = dy,
            button_state = internalButtonState
        };

        if (waitingForAck < (2 * Spice.SPICE_INPUT_MOTION_ACK_BUNCH))
        {
            SendMiniDataHeader(mouseMode, param);
            waitingForAck++;
        }
    }

    public unsafe void MouseDown(byte button, ushort buttonState)
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

    public unsafe void MouseUp(byte button, ushort buttonState)
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

    public unsafe void KeyDown(uint key)
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

    public unsafe void KeyUp(uint key)
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

public sealed class InputKeyModifiers : EventArgs
{
    public bool ScrollLock { get; }
    public bool NumLock { get; }
    public bool CapsLock { get; }

    public InputKeyModifiers(bool scrollLock, bool numLock, bool capsLock)
    {
        ScrollLock = scrollLock;
        NumLock = numLock;
        CapsLock = capsLock;
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
public struct SpiceMouseMotion
{
    public int dx;
    public int dy;
    public ushort button_state;
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
