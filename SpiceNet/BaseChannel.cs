using SpiceNet.Protocol;
using System.Buffers.Binary;
using System.Diagnostics;
using System.Net;
using System.Net.Sockets;
using System.Runtime.InteropServices;
using System.Security.Cryptography;

namespace SpiceNet;

public abstract class BaseChannel : IAsyncDisposable, IDisposable
{
    private TcpClient? client;
    private NetworkStream? stream;
    private Thread thread = null!;
    private int ack_window = 0, msgs_until_ack = 0;

    protected IPEndPoint endpoint;
    protected byte type;
    protected byte channelId;
    protected uint connectionId;

    protected List<BaseChannel> channels = new();

    public bool Ready { get; private set; }

    public event EventHandler? OnConnected;
    public event EventHandler? OnDisconnected;
    public event EventHandler<Exception>? OnError;

    public BaseChannel(IPEndPoint endPoint)
    {
        this.endpoint = endPoint;
    }

    protected abstract int GetChannelCaps();

    protected virtual void Connected()
    {
        OnConnected?.Invoke(this, new EventArgs());
        Ready = true;
    }

    protected virtual void Disconnected()
    {
        Ready = false;
        OnDisconnected?.Invoke(this, new EventArgs());
    }

    public void Start()
    {
        thread = new(Loop);
        thread.Start();
    }

    private unsafe void Loop()
    {
        try
        {
            client = new TcpClient(AddressFamily.InterNetwork)
            {
                NoDelay = true,
                ReceiveBufferSize = 1048576,
                SendBufferSize = 1048576
            };
            client.Connect(endpoint);
            stream = client.GetStream();

            unsafe
            {
                var hdr = new SpiceLinkHeader
                {
                    magic = Spice.SPICE_MAGIC,
                    major_version = Spice.SPICE_VERSION_MAJOR,
                    minor_version = Spice.SPICE_VERSION_MINOR
                };
                var msg = new SpiceLinkMess
                {
                    connection_id = connectionId,
                    channel_type = type,
                    channel_id = channelId,
                    num_common_caps = 1,
                    num_channel_caps = GetChannelCaps() != 0 ? 1u : 0u,
                    caps_offset = (uint)sizeof(SpiceLinkMess)
                };

                hdr.size = (uint)(sizeof(SpiceLinkMess) + sizeof(int) * (msg.num_common_caps + msg.num_channel_caps));

                var ptr = NativeMemory.AllocZeroed((nuint)(sizeof(SpiceLinkHeader) + hdr.size));
                Marshal.StructureToPtr(hdr, (nint)ptr, true);
                Marshal.StructureToPtr(msg, (nint)ptr + sizeof(SpiceLinkHeader), true);

                Marshal.WriteInt32((nint)((nint)ptr + sizeof(SpiceLinkHeader) + msg.caps_offset), (1 << Spice.SPICE_COMMON_CAP_PROTOCOL_AUTH_SELECTION) | (1 << Spice.SPICE_COMMON_CAP_MINI_HEADER));

                if (GetChannelCaps() != 0)
                    Marshal.WriteInt32((nint)((nint)ptr + sizeof(SpiceLinkHeader) + msg.caps_offset + sizeof(int)), GetChannelCaps());

                var span = new Span<byte>(ptr, (int)(sizeof(SpiceLinkHeader) + hdr.size));
                stream.Write(span);
                NativeMemory.Free(ptr);
            }

            SpiceLinkReply linkReply;
            unsafe
            {
                var ptr = NativeMemory.AllocZeroed((nuint)sizeof(SpiceLinkHeader));
                var span = new Span<byte>(ptr, sizeof(SpiceLinkHeader));
                stream.ReadExactly(span);

                var reply = Marshal.PtrToStructure<SpiceLinkHeader>((nint)ptr);
                NativeMemory.Free(ptr);

                if (reply.magic != Spice.SPICE_MAGIC)
                    return;

                ptr = NativeMemory.AllocZeroed(reply.size);
                span = new Span<byte>(ptr, (int)reply.size);
                stream.ReadExactly(span);

                linkReply = Marshal.PtrToStructure<SpiceLinkReply>((nint)ptr);

                var commonCaps = new List<int>();
                for (int i = 0; i < linkReply.num_common_caps; i++)
                    commonCaps.Add(Marshal.ReadInt32((nint)ptr + sizeof(SpiceLinkReply) + i * sizeof(int)));

                var channelCaps = new List<int>();
                for (int i = 0; i < linkReply.num_channel_caps; i++)
                    channelCaps.Add(Marshal.ReadInt32((nint)((nint)ptr + sizeof(SpiceLinkReply) + linkReply.num_common_caps * sizeof(int) + i * sizeof(int))));

                NativeMemory.Free(ptr);
            }

            using var rsa = RSA.Create();

            rsa.ImportSubjectPublicKeyInfo(linkReply.pub_key, out var bytesRead);

            var password = "\0"u8.ToArray();
            var ticket = rsa.Encrypt(password, RSAEncryptionPadding.OaepSHA1);

            unsafe
            {
                var auth = new SpiceLinkAuthMechanism
                {
                    auth_mechanism = Spice.SPICE_COMMON_CAP_AUTH_SPICE
                };
                var data = new SpiceLinkEncryptedTicket();
                ticket.CopyTo(data.encrypted_data);

                var ptr = NativeMemory.AllocZeroed((nuint)(sizeof(SpiceLinkAuthMechanism) + sizeof(SpiceLinkEncryptedTicket)));

                Marshal.StructureToPtr(auth, (nint)ptr, true);
                Marshal.StructureToPtr(data, (nint)ptr + sizeof(SpiceLinkAuthMechanism), true);

                var span = new Span<byte>(ptr, sizeof(SpiceLinkAuthMechanism) + sizeof(SpiceLinkEncryptedTicket));
                stream.Write(span);
                NativeMemory.Free(ptr);
            }


            unsafe
            {
                var ptr = NativeMemory.AllocZeroed(sizeof(int));
                var span = new Span<byte>(ptr, sizeof(int));
                stream.ReadExactly(span);

                var reply = (SpiceLinkErr)Marshal.ReadInt32((nint)ptr);
                NativeMemory.Free(ptr);

                if (reply != SpiceLinkErr.SPICE_LINK_ERR_OK)
                    return;
            }

            Connected();

            while (client.Connected)
            {
                var ptr = NativeMemory.AllocZeroed((nuint)sizeof(SpiceMiniDataHeader));
                try
                {
                    var span = new Span<byte>(ptr, sizeof(SpiceMiniDataHeader));
                    stream.ReadExactly(span);

                    var hdr = Marshal.PtrToStructure<SpiceMiniDataHeader>((nint)ptr);

                    //Debug.WriteLine($"Type: {hdr.type} Size: {hdr.size}");

                    if (hdr.size == 0)
                    {
                        ProcessCommonMessage(hdr);
                    }
                    else
                    {
                        ProcessCommonMessage(hdr);
                    }

                }
                catch
                {
                    break;
                }
                finally
                {
                    NativeMemory.Free(ptr);
                }
            }

            Disconnected();
        }
        catch (Exception e)
        {
            OnError?.Invoke(this, e);
        }
    }

    private unsafe void ProcessCommonMessage(SpiceMiniDataHeader hdr)
    {
        var ptr = NativeMemory.AllocZeroed(hdr.size);
        try
        {
            var data = new Span<byte>(ptr, (int)hdr.size);
            stream!.ReadExactly(data);

            var handled = false;
            switch (hdr.type)
            {
                case Spice.SPICE_MSG_SET_ACK:
                    ReplyMsgSetAck(data, ptr);
                    handled = true;
                    break;
                case Spice.SPICE_MSG_PING:
                    ReplyMsgPing(data);
                    handled = true;
                    break;
                case Spice.SPICE_MSG_NOTIFY:
                    handled = true;
                    break;
            }

            if (!handled)
                ProcessMessage(hdr, data, ptr);

            if (msgs_until_ack > 0)
            {
                msgs_until_ack--;
                if (msgs_until_ack <= 0)
                {
                    msgs_until_ack = ack_window;

                    var ackReply = new SpiceMiniDataHeader
                    {
                        type = Spice.SPICE_MSGC_ACK,
                        size = 0
                    };

                    SendMiniDataHeader(ackReply);
                }
            }

        }
        finally
        {
            NativeMemory.Free(ptr);
        }

    }

    protected abstract unsafe void ProcessMessage(SpiceMiniDataHeader hdr, Span<byte> data, void* ptr);

    private unsafe void ReplyMsgSetAck(Span<byte> _, void* ptr)
    {
        var generation = Marshal.ReadInt32((nint)ptr);
        var window = Marshal.ReadInt32((nint)ptr + sizeof(int));

        ack_window = msgs_until_ack = window;

        var reply = new SpiceMiniDataHeader
        {
            type = Spice.SPICE_MSGC_ACK_SYNC,
            size = 4u
        };

        Span<byte> data = stackalloc byte[4];
        BinaryPrimitives.WriteInt32LittleEndian(data, generation);

        SendMiniDataHeader(reply, data);

    }

    private void ReplyMsgPing(Span<byte> data)
    {
        var reply = new SpiceMiniDataHeader
        {
            type = Spice.SPICE_MSGC_PONG,
            size = 12u
        };

        SendMiniDataHeader(reply, data);
    }

    protected unsafe void SendMiniDataHeader(SpiceMiniDataHeader reply, Span<byte> data)
    {
        var ptr = NativeMemory.AllocZeroed((nuint)(sizeof(SpiceMiniDataHeader) + reply.size));

        Marshal.StructureToPtr(reply, (nint)ptr, true);
        //Marshal.Copy(data, 0, (nint)ptr + sizeof(SpiceMiniDataHeader), reply.size);

        var span = new Span<byte>(ptr, (int)(sizeof(SpiceMiniDataHeader) + reply.size));

        data[..(int)reply.size].CopyTo(span.Slice(sizeof(SpiceMiniDataHeader), (int)reply.size));

        stream!.Write(span);
        NativeMemory.Free(ptr);
    }

    protected unsafe void SendMiniDataHeader(SpiceMiniDataHeader reply)
    {
        var ptr = NativeMemory.AllocZeroed((nuint)sizeof(SpiceMiniDataHeader));

        Marshal.StructureToPtr(reply, (nint)ptr, true);

        var span = new Span<byte>(ptr, sizeof(SpiceMiniDataHeader));

        stream!.Write(span);
        NativeMemory.Free(ptr);
    }

    protected unsafe void SendMiniDataHeader<T>(SpiceMiniDataHeader reply, T data) where T : unmanaged
    {
        var size = sizeof(SpiceMiniDataHeader) + sizeof(T);
        var ptr = NativeMemory.AllocZeroed((nuint)size);

        reply.size = (uint)sizeof(T);

        Marshal.StructureToPtr(reply, (nint)ptr, true);
        Marshal.StructureToPtr(data, (nint)ptr + sizeof(SpiceMiniDataHeader), true);

        var span = new Span<byte>(ptr, size);

        stream!.Write(span);
        NativeMemory.Free(ptr);
    }

    public async ValueTask DisposeAsync()
    {
        foreach (var channel in channels)
            await channel.DisposeAsync();
        if (stream != null)
            await stream.DisposeAsync();
        client?.Dispose();
    }

    public void Dispose()
    {
        foreach (var channel in channels)
            channel.Dispose();
        stream?.Dispose();
        client?.Dispose();
    }
}
