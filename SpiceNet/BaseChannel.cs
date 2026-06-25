using SpiceNet.Protocol;
using System.Buffers.Binary;
using System.Diagnostics;
using System.Net.Security;
using System.Net.Sockets;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Security.Cryptography;
using System.Security.Cryptography.X509Certificates;
using System.Text;

namespace SpiceNet;

public abstract class BaseChannel : IDisposable
{
    private TcpClient? client;
    private Stream? stream;
    private Thread thread = null!;
    private int ack_window = 0, msgs_until_ack = 0;

    protected readonly string host;
    protected readonly int port;
    protected readonly string password;
    protected byte type;
    protected byte channelId;
    protected uint connectionId;

    protected List<BaseChannel> channels = new();

    public bool Ready { get; private set; }
    public string? Proxy { get; set; }
    public string? CA { get; set; }

    public event EventHandler? OnConnected;
    public event EventHandler? OnDisconnected;
    public event EventHandler<Exception>? OnError;

    public BaseChannel(string host, int port, string password)
    {
        this.host = host;
        this.port = port;
        this.password = password;
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
        thread = new(Loop)
        {
            Name = $"Channel-{type}-Connection-{connectionId}"
        };
        thread.Start();
    }

    public void Stop()
    {
        foreach (var channel in channels)
            channel.Stop();
        try
        {
            client?.Client.Shutdown(SocketShutdown.Both);
        }
        catch { }
        // TODO: SPICE_MSGC_DISCONNECTING
    }

    private void Connect()
    {
        client = new TcpClient()
        {
            NoDelay = true,
            ReceiveBufferSize = 1048576,
            SendBufferSize = 1048576
        };

        if (Uri.TryCreate(Proxy, new UriCreationOptions(), out var proxy))
        {
            client.Connect(proxy.Host, proxy.Port);
            var proxyStream = client.GetStream();

            var request = $"CONNECT {host}:{port} HTTP/1.1\r\nHost: {host}:{port}\r\n\r\n";

            proxyStream.Write(Encoding.ASCII.GetBytes(request));

            Span<byte> buffer = stackalloc byte[1024];
            var length = proxyStream.Read(buffer);

            var response = Encoding.ASCII.GetString(buffer).Trim();

            if (!response.Contains("200 OK"))
                throw new Exception(response);

            var sslStream = new SslStream(proxyStream);

            var options = new SslClientAuthenticationOptions()
            {
                RemoteCertificateValidationCallback = (sender, certificate, chain, sslPolicyErrors) =>
                {
                    if (sslPolicyErrors == SslPolicyErrors.None)
                        return true;

                    if (certificate == null || chain == null) return false;

                    chain.ChainPolicy.TrustMode = X509ChainTrustMode.CustomRootTrust;
                    chain.ChainPolicy.CustomTrustStore.Add(X509Certificate2.CreateFromPem(CA));

                    chain.ChainPolicy.RevocationMode = X509RevocationMode.NoCheck;

                    var element = certificate as X509Certificate2 ?? new X509Certificate2(certificate);

                    return chain.Build(element);
                }
            };

            sslStream.AuthenticateAsClient(options);

            stream = sslStream;
        }
        else
        {
            client.Connect(host, port);
            stream = client.GetStream();
        }

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
                throw new Exception("Invalid Magic");

            ptr = NativeMemory.AllocZeroed(reply.size);
            span = new Span<byte>(ptr, (int)reply.size);
            stream.ReadExactly(span);

            linkReply = Unsafe.Read<SpiceLinkReply>(ptr);

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

        var ticket = rsa.Encrypt(Encoding.UTF8.GetBytes($"{password}\0"), RSAEncryptionPadding.OaepSHA1);

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
            var ptr = NativeMemory.AllocZeroed(sizeof(uint));
            var span = new Span<byte>(ptr, sizeof(uint));
            stream.ReadExactly(span);

            var reply = Unsafe.Read<SpiceLinkErr>(ptr);
            NativeMemory.Free(ptr);

            if (reply != SpiceLinkErr.SPICE_LINK_ERR_OK)
                throw new Exception(reply.ToString());
        }

    }

    private unsafe void Loop()
    {
        try
        {
            Connect();
            Connected();

            while (client!.Connected)
            {
                var ptr = NativeMemory.AllocZeroed((nuint)sizeof(SpiceMiniDataHeader));
                try
                {
                    var span = new Span<byte>(ptr, sizeof(SpiceMiniDataHeader));
                    stream!.ReadExactly(span);

                    var hdr = Unsafe.Read<SpiceMiniDataHeader>(ptr);

                    //Debug.WriteLine($"Type: {hdr.type} Size: {hdr.size}");

                    ProcessCommonMessage(hdr);
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

            switch (hdr.type)
            {
                case Spice.SPICE_MSG_SET_ACK:
                    ReplyMsgSetAck(data, ptr);
                    break;
                case Spice.SPICE_MSG_PING:
                    ReplyMsgPing(data);
                    break;
                case Spice.SPICE_MSG_NOTIFY:
                    {
                        var notify = Unsafe.Read<SpiceMsgNotify>(ptr);

                        var message = Encoding.UTF8.GetString((byte*)((nint)ptr + sizeof(SpiceMsgNotify)), (int)notify.message_len);
                        Debug.WriteLine($"{notify.time_stamp} {notify.severity} {notify.visibility} {notify.what} {message}");
                    }
                    break;
                case Spice.SPICE_MSG_DISCONNECTING:
                    break;
                default:
                    ProcessMessage(hdr, data, ptr);
                    break;
            }

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

        Unsafe.Write(ptr, reply);

        var span = new Span<byte>(ptr, (int)(sizeof(SpiceMiniDataHeader) + reply.size));

        data[..(int)reply.size].CopyTo(span.Slice(sizeof(SpiceMiniDataHeader), (int)reply.size));

        stream!.Write(span);
        NativeMemory.Free(ptr);
    }

    protected unsafe void SendMiniDataHeader(SpiceMiniDataHeader reply)
    {
        var ptr = NativeMemory.AllocZeroed((nuint)sizeof(SpiceMiniDataHeader));

        Unsafe.Write(ptr, reply);

        var span = new Span<byte>(ptr, sizeof(SpiceMiniDataHeader));

        stream!.Write(span);
        NativeMemory.Free(ptr);
    }

    protected unsafe void SendMiniDataHeader<T>(SpiceMiniDataHeader reply, T data) where T : unmanaged
    {
        var size = sizeof(SpiceMiniDataHeader) + sizeof(T);
        var ptr = NativeMemory.AllocZeroed((nuint)size);

        reply.size = (uint)sizeof(T);

        Unsafe.Write(ptr, reply);
        Marshal.StructureToPtr(data, (nint)ptr + sizeof(SpiceMiniDataHeader), true);

        var span = new Span<byte>(ptr, size);

        stream!.Write(span);
        NativeMemory.Free(ptr);
    }

    protected unsafe void SendAgentData<T>(SpiceMiniDataHeader header, SpiceMsgcMainAgentData agent, T data) where T : unmanaged
    {
        var size = sizeof(SpiceMiniDataHeader) + sizeof(SpiceMsgcMainAgentData) + agent.size;
        var ptr = NativeMemory.AllocZeroed((nuint)size);

        header.size = (uint)(sizeof(SpiceMsgcMainAgentData) + agent.size);

        Unsafe.Write(ptr, header);
        Marshal.StructureToPtr(agent, (nint)ptr + sizeof(SpiceMiniDataHeader), true);
        Marshal.StructureToPtr(data, (nint)ptr + sizeof(SpiceMiniDataHeader) + sizeof(SpiceMsgcMainAgentData), true);

        var span = new Span<byte>(ptr, (int)size);

        stream!.Write(span);
        NativeMemory.Free(ptr);
    }

    protected unsafe void SendAgentData<T>(SpiceMiniDataHeader header, SpiceMsgcMainAgentData agent, Span<T> data) where T : unmanaged
    {
        var size = sizeof(SpiceMiniDataHeader) + sizeof(SpiceMsgcMainAgentData) + agent.size;
        var ptr = NativeMemory.AllocZeroed((nuint)size);

        header.size = (uint)(sizeof(SpiceMsgcMainAgentData) + agent.size);

        Unsafe.Write(ptr, header);
        Marshal.StructureToPtr(agent, (nint)ptr + sizeof(SpiceMiniDataHeader), true);

        var span = new Span<byte>(ptr, (int)size);

        MemoryMarshal.Cast<T, byte>(data)[..(int)agent.size].CopyTo(span.Slice(sizeof(SpiceMiniDataHeader) + sizeof(SpiceMsgcMainAgentData), (int)agent.size));

        stream!.Write(span);
        NativeMemory.Free(ptr);
    }

    public void Dispose()
    {
        foreach (var channel in channels)
            channel.Dispose();
        stream?.Dispose();
        client?.Dispose();
    }
}

[StructLayout(LayoutKind.Sequential, Pack = 1)]
public struct SpiceMsgNotify
{
    public ulong time_stamp;
    public SpiceNotifySeverity severity;
    public SpiceNotifyVisibility visibility;
    public uint what;
    public uint message_len;
}
