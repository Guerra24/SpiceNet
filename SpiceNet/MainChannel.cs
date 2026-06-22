using SpiceNet.Protocol;
using System.Diagnostics;
using System.Net;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Text;
using static SpiceNet.Protocol.VDAgentMonitorsConfig;

namespace SpiceNet;

public class MainChannel : BaseChannel
{
    private uint agentTokens;
    private bool agentConneted;

    private uint agentCaps;

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

    public event EventHandler<string>? ReceiveClipboard;
    public event EventHandler<RequestClipboardArgs>? SendClipboard;

    public event EventHandler<string>? Name;
    public event EventHandler<Guid>? Guid;

    public MainChannel(IPEndPoint endPoint) : base(endPoint)
    {
        connectionId = 0;
        type = Spice.SPICE_CHANNEL_MAIN;
        channelId = 0;
    }

    protected override int GetChannelCaps()
    {
        return (1 << Spice.SPICE_MAIN_CAP_AGENT_CONNECTED_TOKENS) | (1 << Spice.SPICE_MAIN_CAP_NAME_AND_UUID);
    }

    protected override void Disconnected()
    {
        base.Disconnected();
        agentConneted = false;
    }

    protected override unsafe void ProcessMessage(SpiceMiniDataHeader hdr, Span<byte> _, void* ptr)
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

                if (msg.agent_connected == 1)
                    ConnectAgent();

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
                ConnectAgent();
                break;
            case Spice.SPICE_MSG_MAIN_AGENT_CONNECTED_TOKENS:
                {
                    var connectedTokens = Unsafe.Read<SpiceMsgMainAgentTokens>(ptr);
                    agentTokens = connectedTokens.num_tokens;
                    ConnectAgent();
                }
                break;
            case Spice.SPICE_MSG_MAIN_AGENT_TOKEN:
                {
                    var connectedTokens = Unsafe.Read<SpiceMsgMainAgentTokens>(ptr);
                    agentTokens += connectedTokens.num_tokens;
                }
                break;
            case Spice.SPICE_MSG_MAIN_AGENT_DISCONNECTED:
                agentConneted = false;
                break;
            case Spice.SPICE_MSG_MAIN_AGENT_DATA:
                {
                    var agentData = Unsafe.Read<SpiceMsgcMainAgentData>(ptr);
                    var data = Unsafe.Add<SpiceMsgcMainAgentData>(ptr, 1);

                    var hasClipboardSelection = ((agentCaps >> VDAgent.VD_AGENT_CAP_CLIPBOARD_SELECTION) & 1) != 0;
                    switch (agentData.type)
                    {
                        case VDAgent.VD_AGENT_ANNOUNCE_CAPABILITIES:
                            {
                                var caps = Unsafe.Read<VDAgentAnnounceCapabilities>(data);
                                agentCaps = caps.caps;
                                if (caps.request != 0)
                                    AnnounceAgentCaps(0);
                            }
                            break;
                        case VDAgent.VD_AGENT_CLIPBOARD_GRAB:
                            {
                                Span<uint> req = stackalloc uint[hasClipboardSelection ? 2 : 1];

                                if (hasClipboardSelection)
                                    req[0] = 0;
                                req[^1] = VDAgent.VD_AGENT_CLIPBOARD_UTF8_TEXT;

                                var reply = new SpiceMsgcMainAgentData
                                {
                                    protocol = VDAgent.VD_AGENT_PROTOCOL,
                                    type = VDAgent.VD_AGENT_CLIPBOARD_REQUEST,
                                    opaque = 0,
                                    size = (uint)(sizeof(uint) * req.Length)
                                };

                                var header = new SpiceMiniDataHeader
                                {
                                    type = Spice.SPICE_MSGC_MAIN_AGENT_DATA,
                                };

                                SendAgentData(header, reply, req);
                                agentTokens--;
                            }
                            break;
                        case VDAgent.VD_AGENT_CLIPBOARD:
                            {
                                if (hasClipboardSelection)
                                    data = Unsafe.Add<uint>(data, 1);

                                var type = Unsafe.Read<uint>(data);
                                data = Unsafe.Add<uint>(data, 1);

                                if (type == VDAgent.VD_AGENT_CLIPBOARD_UTF8_TEXT)
                                {
                                    var length = (int)(agentData.size - sizeof(uint) * (hasClipboardSelection ? 2 : 1));
                                    if (length > 0)
                                    {
                                        var clipboard = Encoding.UTF8.GetString((byte*)data, length);

                                        ReceiveClipboard?.Invoke(this, clipboard);
                                    }
                                }
                            }
                            break;
                        case VDAgent.VD_AGENT_CLIPBOARD_REQUEST:
                            {
                                var args = new RequestClipboardArgs();
                                SendClipboard?.Invoke(this, args);

                                var clipboard = args.Clipboard ?? string.Empty;

                                var offset = hasClipboardSelection ? 2 : 1;
                                byte[] bytes = [.. Encoding.UTF8.GetBytes(clipboard), 0];

                                Span<byte> req = stackalloc byte[offset * sizeof(uint) + bytes.Length];

                                bytes.CopyTo(req[(offset * sizeof(uint))..]);

                                var casted = MemoryMarshal.Cast<byte, uint>(req);

                                if (hasClipboardSelection)
                                    casted[0] = 0;
                                casted[offset - 1] = VDAgent.VD_AGENT_CLIPBOARD_UTF8_TEXT;

                                var reply = new SpiceMsgcMainAgentData
                                {
                                    protocol = VDAgent.VD_AGENT_PROTOCOL,
                                    type = VDAgent.VD_AGENT_CLIPBOARD,
                                    opaque = 0,
                                    size = (uint)req.Length
                                };

                                var header = new SpiceMiniDataHeader
                                {
                                    type = Spice.SPICE_MSGC_MAIN_AGENT_DATA,
                                };

                                SendAgentData(header, reply, req);
                                agentTokens--;
                            }
                            break;
                        case VDAgent.VD_AGENT_CLIPBOARD_RELEASE:
                            break;
                    }
                }
                break;
            case Spice.SPICE_MSG_MAIN_MIGRATE_SWITCH_HOST:
                // TODO
                break;
            case Spice.SPICE_MSG_MAIN_MIGRATE_END:
                // TODO
                break;
            case Spice.SPICE_MSG_MAIN_NAME:
                {
                    var length = Unsafe.Read<uint>(ptr);
                    var name = Encoding.UTF8.GetString((byte*)Unsafe.Add<uint>(ptr, 1), (int)length).Trim();

                    Name?.Invoke(this, name);
                }
                break;
            case Spice.SPICE_MSG_MAIN_UUID:
                {
                    var uuid = Unsafe.Read<SpiceMsgMainUuid>(ptr);

                    var guid = new Guid(new Span<byte>(uuid.uuid, 16));

                    Guid?.Invoke(this, guid);
                }
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

    private unsafe void ConnectAgent()
    {
        var agentHeader = new SpiceMiniDataHeader
        {
            type = Spice.SPICE_MSGC_MAIN_AGENT_START,
            size = (uint)sizeof(SpiceMsgcMainAgentStart)
        };

        var agentStart = new SpiceMsgcMainAgentStart
        {
            num_tokens = uint.MaxValue
        };

        SendMiniDataHeader(agentHeader, agentStart);

        AnnounceAgentCaps(1);

        agentConneted = true;
    }

    private unsafe void AnnounceAgentCaps(uint request)
    {
        var caps = new VDAgentAnnounceCapabilities
        {
            request = request,
            caps = (1 << VDAgent.VD_AGENT_CAP_MOUSE_STATE) |
                (1 << VDAgent.VD_AGENT_CAP_MONITORS_CONFIG) |
                (1 << VDAgent.VD_AGENT_CAP_REPLY) |
                (1 << VDAgent.VD_AGENT_CAP_CLIPBOARD_SELECTION) |
                (1 << VDAgent.VD_AGENT_CAP_CLIPBOARD_BY_DEMAND)
        };

        var agentData = new SpiceMsgcMainAgentData
        {
            protocol = VDAgent.VD_AGENT_PROTOCOL,
            type = VDAgent.VD_AGENT_ANNOUNCE_CAPABILITIES,
            opaque = 0,
            size = (uint)sizeof(VDAgentAnnounceCapabilities)
        };

        var header = new SpiceMiniDataHeader
        {
            type = Spice.SPICE_MSGC_MAIN_AGENT_DATA,
        };

        SendAgentData(header, agentData, caps);
        agentTokens--;
    }

    public unsafe void ResizeMonitor(uint width, uint height)
    {
        if (!agentConneted)
            return;

        var config = new VDAgentMonitorsConfig
        {
            num_of_monitors = 1,
            flags = 0
        };

        var monitors = stackalloc _monitors_e__FixedBuffer[(int)config.num_of_monitors];
        var span = monitors->AsSpan(1);
        span[0] = new VDAgentMonConfig
        {
            width = width,
            height = height,
            depth = 32,
            x = 0,
            y = 0
        };
        config.monitors = *monitors;

        var agentData = new SpiceMsgcMainAgentData
        {
            protocol = VDAgent.VD_AGENT_PROTOCOL,
            type = VDAgent.VD_AGENT_MONITORS_CONFIG,
            opaque = 0,
            size = (uint)(sizeof(VDAgentMonitorsConfig) - sizeof(VDAgentMonConfig) + sizeof(VDAgentMonConfig) * config.num_of_monitors)
        };

        var header = new SpiceMiniDataHeader
        {
            type = Spice.SPICE_MSGC_MAIN_AGENT_DATA,
        };

        SendAgentData(header, agentData, config);
        agentTokens--;
    }

    public void NotifyClipboard()
    {
        if (!agentConneted)
            return;

        var hasClipboardSelection = ((agentCaps >> VDAgent.VD_AGENT_CAP_CLIPBOARD_SELECTION) & 1) != 0;
        Span<uint> req = stackalloc uint[hasClipboardSelection ? 2 : 1];

        if (hasClipboardSelection)
            req[0] = 0;
        req[^1] = VDAgent.VD_AGENT_CLIPBOARD_UTF8_TEXT;

        var reply = new SpiceMsgcMainAgentData
        {
            protocol = VDAgent.VD_AGENT_PROTOCOL,
            type = VDAgent.VD_AGENT_CLIPBOARD_GRAB,
            opaque = 0,
            size = (uint)(sizeof(uint) * req.Length)
        };

        var header = new SpiceMiniDataHeader
        {
            type = Spice.SPICE_MSGC_MAIN_AGENT_DATA,
        };

        SendAgentData(header, reply, req);
        agentTokens--;
    }

}

public sealed class RequestClipboardArgs : EventArgs
{
    public string? Clipboard { get; set; }
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

[StructLayout(LayoutKind.Sequential, Pack = 1)]
public struct SpiceMsgcMainAgentStart
{
    public uint num_tokens;
}

[StructLayout(LayoutKind.Sequential, Pack = 1)]
public struct SpiceMsgcMainAgentData
{
    public uint protocol;
    public uint type;
    public ulong opaque;
    public uint size;
}

[StructLayout(LayoutKind.Sequential, Pack = 1)]
public struct SpiceMsgMainAgentTokens
{
    public uint num_tokens;
}

[StructLayout(LayoutKind.Sequential, Pack = 1)]
public struct SpiceMsgClipboardRequest
{
    public uint num_tokens;
}

[StructLayout(LayoutKind.Sequential, Pack = 1)]
public unsafe struct SpiceMsgMainUuid
{
    public fixed byte uuid[16];
}
