using SpiceNet.Protocol;
using System.Diagnostics;
using System.Net;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace SpiceNet;

public class MainChannel : BaseChannel
{
    private uint agentTokens;

    public int CurrentMouseMode { get; private set; } = Spice.SPICE_MOUSE_MODE_SERVER;

    public DisplayChannel? Display { get; private set; }
    public CursorChannel? Cursor { get; private set; }
    public InputsChannel? Inputs { get; private set; }
    public PlaybackChannel? Playback { get; private set; }

    public event EventHandler<DisplayChannel>? DisplayInit;
    public event EventHandler<CursorChannel>? CursorInit;
    public event EventHandler<InputsChannel>? InputsInit;
    public event EventHandler<PlaybackChannel>? PlaybackInit;

    public event EventHandler<ushort>? MouseModeChanged;

    public MainChannel(IPEndPoint endPoint) : base(endPoint)
    {
        connectionId = 0;
        type = Spice.SPICE_CHANNEL_MAIN;
        channelId = 0;
    }

    protected override int GetChannelCaps()
    {
        return 1 << Spice.SPICE_MAIN_CAP_AGENT_CONNECTED_TOKENS;
    }

    protected override unsafe void ProcessMessage(SpiceMiniDataHeader hdr, Span<byte> data, void* ptr)
    {
        nint relPtr = (nint)ptr;
        switch (hdr.type)
        {
            case Spice.SPICE_MSG_MAIN_MIGRATE_BEGIN:
                // TODO
                break;
            case Spice.SPICE_MSG_MAIN_MIGRATE_CANCEL:
                // TODO
                break;
            case Spice.SPICE_MSG_MAIN_INIT:

                var msg = Marshal.PtrToStructure<SpiceMsgMainInit>(relPtr);

                Debug.WriteLine($"Session Id: {msg.session_id} Display channels hint: {msg.display_channels_hint} Supported mouse modes: {msg.supported_mouse_modes} Current mouse mode: {msg.current_mouse_mode} Agent connected: {msg.agent_connected} Agent tokens: {msg.agent_tokens} Multi media time: {msg.multi_media_time} Ram hint: {msg.ram_hint}");

                connectionId = msg.session_id;
                agentTokens = msg.agent_tokens;

                ChangeMouseMode((ushort)msg.current_mouse_mode, (ushort)msg.supported_mouse_modes);

                var attach = new SpiceMiniDataHeader
                {
                    type = Spice.SPICE_MSGC_MAIN_ATTACH_CHANNELS,
                    size = 0
                };

                SendMiniDataHeader(attach);

                break;
            case Spice.SPICE_MSG_MAIN_MOUSE_MODE:
                {
                    var mode = Unsafe.Read<SpiceMsgcMainMouseMode>(ptr);

                    ChangeMouseMode(mode.current_mode, mode.supported_modes);
                }
                break;
            case Spice.SPICE_MSG_MAIN_MULTI_MEDIA_TIME:
                // TODO
                break;
            case Spice.SPICE_MSG_MAIN_CHANNELS_LIST:

                var num_of_channels = Marshal.ReadInt32(relPtr);
                relPtr += sizeof(int);

                for (int i = 0; i < num_of_channels; i++)
                {
                    var channel = Marshal.PtrToStructure<SpiceMsgChannelId>(relPtr + i * sizeof(SpiceMsgChannelId));
                    switch (channel.type)
                    {
                        case Spice.SPICE_CHANNEL_DISPLAY:
                            if (channel.id == 0)
                            {
                                Display = new(endpoint, channel.id, connectionId);
                                DisplayInit?.Invoke(this, Display);
                                Display.Start();
                                channels.Add(Display);
                            }
                            break;
                        case Spice.SPICE_CHANNEL_INPUTS:
                            Inputs = new(endpoint, channel.id, connectionId);
                            InputsInit?.Invoke(this, Inputs);
                            Inputs.Start();
                            channels.Add(Inputs);
                            break;
                        case Spice.SPICE_CHANNEL_CURSOR:
                            Cursor = new(endpoint, channel.id, connectionId);
                            CursorInit?.Invoke(this, Cursor);
                            Cursor.Start();
                            channels.Add(Cursor);
                            break;
                        case Spice.SPICE_CHANNEL_PLAYBACK:
                            Playback = new(endpoint, channel.id, connectionId);
                            PlaybackInit?.Invoke(this, Playback);
                            Playback.Start();
                            channels.Add(Playback);
                            break;
                        case Spice.SPICE_CHANNEL_PORT:
                            // TODO what?
                            break;
                    }
                }

                break;
            case Spice.SPICE_MSG_MAIN_AGENT_CONNECTED:
                // TODO
                break;
            case Spice.SPICE_MSG_MAIN_AGENT_CONNECTED_TOKENS:
                // TODO
                break;
            case Spice.SPICE_MSG_MAIN_AGENT_TOKEN:
                // TODO
                break;
            case Spice.SPICE_MSG_MAIN_AGENT_DISCONNECTED:
                // TODO
                break;
            case Spice.SPICE_MSG_MAIN_AGENT_DATA:
                // TODO
                break;
            case Spice.SPICE_MSG_MAIN_MIGRATE_SWITCH_HOST:
                // TODO
                break;
            case Spice.SPICE_MSG_MAIN_MIGRATE_END:
                // TODO
                break;
            case Spice.SPICE_MSG_MAIN_NAME:
                // TODO
                break;
            case Spice.SPICE_MSG_MAIN_UUID:
                // TODO
                break;
            case Spice.SPICE_MSG_MAIN_MIGRATE_BEGIN_SEAMLESS:
                // TODO
                break;
            case Spice.SPICE_MSG_MAIN_MIGRATE_DST_SEAMLESS_ACK:
                // TODO
                break;
            case Spice.SPICE_MSG_MAIN_MIGRATE_DST_SEAMLESS_NACK:
                // TODO
                break;
        }
    }

    private unsafe void ChangeMouseMode(ushort current, ushort supported)
    {
        CurrentMouseMode = current;

        MouseModeChanged?.Invoke(this, current);

        if (current != Spice.SPICE_MOUSE_MODE_CLIENT && (supported & Spice.SPICE_MOUSE_MODE_CLIENT) != 0)
        {
            var mouseMode = new SpiceMiniDataHeader
            {
                type = Spice.SPICE_MSGC_MAIN_MOUSE_MODE_REQUEST,
                size = (uint)sizeof(SpiceMsgcMainMouseModeRequest)
            };

            var param = new SpiceMsgcMainMouseModeRequest
            {
                mode = Spice.SPICE_MOUSE_MODE_CLIENT
            };

            SendMiniDataHeader(mouseMode, param);
        }
    }

}


[StructLayout(LayoutKind.Sequential, Pack = 1)]
public struct SpiceMsgMainInit
{
    public uint session_id;
    public uint display_channels_hint;
    public uint supported_mouse_modes;
    public uint current_mouse_mode;
    public uint agent_connected;
    public uint agent_tokens;
    public uint multi_media_time;
    public uint ram_hint;
}

[StructLayout(LayoutKind.Sequential, Pack = 1)]
public struct SpiceMsgChannelId
{
    public byte type;
    public byte id;
}

[StructLayout(LayoutKind.Sequential, Pack = 1)]
public struct SpiceMsgcMainMouseModeRequest
{
    public ushort mode;
}

[StructLayout(LayoutKind.Sequential, Pack = 1)]
public struct SpiceMsgcMainMouseMode
{
    public ushort supported_modes;
    public ushort current_mode;
}
