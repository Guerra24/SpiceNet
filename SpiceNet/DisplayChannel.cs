using SpiceNet.Protocol;
using System.Diagnostics;
using System.Net;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace SpiceNet;

public class DisplayChannel : BaseChannel
{

    public event EventHandler<SpiceMsgDisplayMode>? Mode;
    public event EventHandler? Mark;
    public event EventHandler? Reset;
    public event EventHandler<SpiceSurface>? SurfaceCreate;
    public event EventHandler<SurfaceCopyBitsArgs>? SurfaceCopyBits;
    public event EventHandler<SurfaceDrawFill>? SurfaceDrawFill;
    public event EventHandler<uint>? SurfaceDestroy;
    public event EventHandler<SurfaceDrawCopyArgs>? SurfaceDrawCopy;
    public event EventHandler<List<ulong>>? SurfaceInvalidateList;

    public DisplayChannel(IPEndPoint endpoint, byte channelId, uint connectionId) : base(endpoint)
    {
        base.channelId = channelId;
        base.connectionId = connectionId;
        type = Spice.SPICE_CHANNEL_DISPLAY;
    }

    protected override int GetChannelCaps()
    {
        return
            (1 << Spice.SPICE_DISPLAY_CAP_SIZED_STREAM) |
            (1 << Spice.SPICE_DISPLAY_CAP_STREAM_REPORT) |
            (1 << Spice.SPICE_DISPLAY_CAP_MULTI_CODEC) |
            (1 << Spice.SPICE_DISPLAY_CAP_CODEC_MJPEG);
    }

    protected override void InitChannel()
    {
        var init = new SpiceMiniDataHeader
        {
            type = Spice.SPICE_MSGC_DISPLAY_INIT,
        };

        var data = new SpiceMsgDisplayInit
        {
            pixmap_cache_id = 1,
            pixmap_cache_size = 10 * 1024 * 1024,
            glz_dictionary_id = 0,
            glz_dictionary_window_size = 0
        };

        SendMiniDataHeader(init, data);
    }

    protected override unsafe void ProcessMessage(SpiceMiniDataHeader hdr, Span<byte> data, void* ptr)
    {
        nint relPtr = (nint)ptr;
        switch (hdr.type)
        {
            case Spice.SPICE_MSG_DISPLAY_MODE:
                {
                    var mode = Unsafe.Read<SpiceMsgDisplayMode>(ptr);
                    Mode?.Invoke(this, mode);
                }
                break;
            case Spice.SPICE_MSG_DISPLAY_MARK:
                Mark?.Invoke(this, new EventArgs());
                break;
            case Spice.SPICE_MSG_DISPLAY_RESET:
                Reset?.Invoke(this, new EventArgs());
                break;
            case Spice.SPICE_MSG_DISPLAY_DRAW_COPY:
                {
                    var outputImage = Array.Empty<byte>();
                    var bas = Marshal.PtrToStructure<SpiceMsgDisplayBase>(relPtr);
                    relPtr += sizeof(SpiceMsgDisplayBase);

                    List<SpiceRect> clipRects = new();

                    if ((SpiceClipType)bas.clip_type == SpiceClipType.SPICE_CLIP_TYPE_RECTS)
                    {
                        var num_rects = Marshal.ReadInt32(relPtr);
                        relPtr += sizeof(int);

                        for (int i = 0; i < num_rects; i++)
                        {
                            var rect = Marshal.PtrToStructure<SpiceRect>(relPtr + i * sizeof(SpiceRect));
                            clipRects.Add(rect);
                        }

                        clipRects.TrimExcess();
                        relPtr += num_rects * sizeof(SpiceRect);
                    }

                    var copy = Marshal.PtrToStructure<SpiceCopy>(relPtr);

                    if (copy.offset != 0)
                    {
                        nint descriptorBase = (nint)((nint)ptr + copy.offset);
                        var imageDescriptor = Marshal.PtrToStructure<SpiceImageDescriptor>(descriptorBase);
                        descriptorBase += sizeof(SpiceImageDescriptor);
                        bool flipY = false;

                        Debug.WriteLine((SpiceImageType)imageDescriptor.type);

                        switch ((SpiceImageType)imageDescriptor.type)
                        {
                            case SpiceImageType.SPICE_IMAGE_TYPE_BITMAP:
                                {
                                    var format = (SpiceBitmapFmt)Unsafe.Read<byte>(descriptorBase.ToPointer());
                                    descriptorBase += sizeof(byte);

                                    var flags = Unsafe.Read<byte>(descriptorBase.ToPointer());
                                    descriptorBase += sizeof(byte);

                                    var x = Unsafe.Read<uint>(descriptorBase.ToPointer());
                                    descriptorBase += sizeof(uint);

                                    var y = Unsafe.Read<uint>(descriptorBase.ToPointer());
                                    descriptorBase += sizeof(uint);

                                    var stride = Unsafe.Read<uint>(descriptorBase.ToPointer());
                                    descriptorBase += sizeof(uint);

                                    if ((flags & (1 << 1)) == 1)
                                    {
                                        // palette id long
                                        descriptorBase += sizeof(ulong);
                                    }

                                    //var offset = Unsafe.Read<uint>(descriptorBase.ToPointer());
                                    descriptorBase += sizeof(uint);

                                    /*if(offset != 0)
                                    {
                                        // unique
                                        descriptorBase += sizeof(ulong);
                                        var num_ents = Unsafe.Read<ushort>(descriptorBase.ToPointer());
                                        descriptorBase += sizeof(ushort);

                                        descriptorBase += sizeof(uint) * num_ents;
                                    }*/

                                    switch (format)
                                    {
                                        case SpiceBitmapFmt.SPICE_BITMAP_FMT_INVALID:
                                            break;
                                        case SpiceBitmapFmt.SPICE_BITMAP_FMT_1BIT_LE:
                                            break;
                                        case SpiceBitmapFmt.SPICE_BITMAP_FMT_1BIT_BE:
                                            break;
                                        case SpiceBitmapFmt.SPICE_BITMAP_FMT_4BIT_LE:
                                            break;
                                        case SpiceBitmapFmt.SPICE_BITMAP_FMT_4BIT_BE:
                                            break;
                                        case SpiceBitmapFmt.SPICE_BITMAP_FMT_8BIT:
                                            break;
                                        case SpiceBitmapFmt.SPICE_BITMAP_FMT_16BIT:
                                            break;
                                        case SpiceBitmapFmt.SPICE_BITMAP_FMT_24BIT:
                                            break;
                                        case SpiceBitmapFmt.SPICE_BITMAP_FMT_32BIT:
                                        case SpiceBitmapFmt.SPICE_BITMAP_FMT_RGBA:

                                            var pixels = x * y;
                                            var output = (byte*)NativeMemory.Alloc(pixels * 4);

                                            for (int i = 0; i < pixels * 4; i += 4)
                                            {
                                                output[i] = Unsafe.Read<byte>((descriptorBase + i).ToPointer());
                                                output[i + 1] = Unsafe.Read<byte>((descriptorBase + i + 1).ToPointer());
                                                output[i + 2] = Unsafe.Read<byte>((descriptorBase + i + 2).ToPointer());
                                                if (format == SpiceBitmapFmt.SPICE_BITMAP_FMT_RGBA)
                                                {
                                                    output[i + 3] = Unsafe.Read<byte>((descriptorBase + i + 3).ToPointer());

                                                    var alpha = output[i + 3] / 255;
                                                    output[i] = (byte)(output[i] * alpha);
                                                    output[i + 1] = (byte)(output[i + 1] * alpha);
                                                    output[i + 2] = (byte)(output[i + 2] * alpha);
                                                }
                                                else
                                                {
                                                    output[i + 3] = 0xff;
                                                }
                                            }

                                            if ((flags & (1 << 2)) == 0)
                                            {
                                                var temp = (byte*)NativeMemory.Alloc(pixels * 4);
                                                for (int i = 0; i < y; i++)
                                                {
                                                    NativeMemory.Copy(output + i * stride, temp + (y - 1 - i) * stride, stride);
                                                }
                                                NativeMemory.Free(output);
                                                output = temp;
                                            }

                                            var span = new Span<byte>(output, (int)pixels * 4);

                                            outputImage = span.ToArray();

                                            NativeMemory.Free(output);
                                            break;
                                        case SpiceBitmapFmt.SPICE_BITMAP_FMT_8BIT_A:
                                            break;
                                        case SpiceBitmapFmt.SPICE_BITMAP_FMT_ENUM_END:
                                            break;
                                    }

                                }
                                break;
                            case SpiceImageType.SPICE_IMAGE_TYPE_QUIC:
                                {
                                    var dataSize = Marshal.ReadInt32(descriptorBase);
                                    descriptorBase += sizeof(int);

                                    var usrContext = Quic.quic_create_usr_context();
                                    var decoder = Quic.quic_create(usrContext);

                                    var type = new QuicImageType();
                                    int width, height;
                                    var res = Quic.quic_decode_begin(decoder, (uint*)descriptorBase, (uint)dataSize, &type, &width, &height);

                                    var output = (byte*)NativeMemory.Alloc((nuint)(width * height * 4));

                                    res = Quic.quic_decode(decoder, type, output, width * 4);

                                    var span = new Span<byte>(output, width * height * 4);

                                    for (int i = 0; i < width * height * 4; i += 4)
                                    {
                                        if (type == QuicImageType.QUIC_IMAGE_TYPE_RGBA)
                                        {
                                            var alpha = output[i + 3] / 255;
                                            output[i] = (byte)(output[i] * alpha);
                                            output[i + 1] = (byte)(output[i + 1] * alpha);
                                            output[i + 2] = (byte)(output[i + 2] * alpha);
                                        }
                                        else
                                        {
                                            output[i + 3] = 0xff;
                                        }
                                    }

                                    outputImage = span.ToArray();

                                    Quic.quic_destroy(decoder);
                                    Quic.quic_destroy_usr_context(usrContext);
                                    NativeMemory.Free(output);
                                }
                                break;
                            case SpiceImageType.SPICE_IMAGE_TYPE_RESERVED:
                                break;
                            case SpiceImageType.SPICE_IMAGE_TYPE_LZ_PLT:
                                break;
                            case SpiceImageType.SPICE_IMAGE_TYPE_LZ_RGB:
                                {
                                    var dataSize = Marshal.ReadInt32(descriptorBase);
                                    descriptorBase += sizeof(int);

                                    var lzUsrContext = Lz.lz_create_usr_context();
                                    var lzDecoder = Lz.lz_create(lzUsrContext);

                                    var type = new LzImageType();
                                    int width, height, pixels, topDown;
                                    var palette = new SpicePalette();
                                    Lz.lz_decode_begin(lzDecoder, (byte*)descriptorBase, (uint)dataSize, &type, &width, &height, &pixels, &topDown, &palette);

                                    flipY = topDown == 0;

                                    var output = (byte*)NativeMemory.Alloc((nuint)(pixels * 4));

                                    Lz.lz_decode(lzDecoder, type, output);

                                    if (topDown == 0)
                                    {
                                        var temp = (byte*)NativeMemory.Alloc((nuint)(pixels * 4));
                                        var stride = width * 4;
                                        for (int i = 0; i < height; i++)
                                        {
                                            NativeMemory.Copy(output + i * stride, temp + (height - 1 - i) * stride, (nuint)stride);
                                        }
                                        NativeMemory.Free(output);
                                        output = temp;
                                    }

                                    var span = new Span<byte>(output, pixels * 4);

                                    for (int i = 0; i < pixels * 4; i += 4)
                                    {
                                        if (type == LzImageType.LZ_IMAGE_TYPE_RGBA)
                                        {
                                            var alpha = output[i + 3] / 255;
                                            output[i] = (byte)(output[i] * alpha);
                                            output[i + 1] = (byte)(output[i + 1] * alpha);
                                            output[i + 2] = (byte)(output[i + 2] * alpha);
                                        }
                                        else
                                        {
                                            output[i + 3] = 0xff;
                                        }
                                    }

                                    outputImage = span.ToArray();

                                    Lz.lz_destroy(lzDecoder);
                                    Lz.lz_destroy_usr_context(lzUsrContext);
                                    NativeMemory.Free(output);
                                }
                                break;
                            case SpiceImageType.SPICE_IMAGE_TYPE_GLZ_RGB:
                                break;
                            case SpiceImageType.SPICE_IMAGE_TYPE_FROM_CACHE:
                                break;
                            case SpiceImageType.SPICE_IMAGE_TYPE_SURFACE:
                                break;
                            case SpiceImageType.SPICE_IMAGE_TYPE_JPEG:
                                break;
                            case SpiceImageType.SPICE_IMAGE_TYPE_FROM_CACHE_LOSSLESS:
                                break;
                            case SpiceImageType.SPICE_IMAGE_TYPE_ZLIB_GLZ_RGB:
                                break;
                            case SpiceImageType.SPICE_IMAGE_TYPE_JPEG_ALPHA:
                                break;
                            case SpiceImageType.SPICE_IMAGE_TYPE_LZ4:
                                break;
                            case SpiceImageType.SPICE_IMAGE_TYPE_ENUM_END:
                                break;
                        }

                        SurfaceDrawCopy?.Invoke(this, new SurfaceDrawCopyArgs(bas, copy, imageDescriptor, clipRects, outputImage, flipY));
                    }

                    relPtr += sizeof(SpiceCopy);

                    var masq = Marshal.PtrToStructure<SpiceQMask>(relPtr);
                    relPtr += sizeof(SpiceQMask);

                    if (masq.offset != 0)
                    {

                    }
                }
                break;
            case Spice.SPICE_MSG_DISPLAY_DRAW_FILL:
                {
                    var bas = Marshal.PtrToStructure<SpiceMsgDisplayBase>(relPtr);
                    relPtr += sizeof(SpiceMsgDisplayBase);

                    List<SpiceRect> clipRects = new();

                    if ((SpiceClipType)bas.clip_type == SpiceClipType.SPICE_CLIP_TYPE_RECTS)
                    {
                        var num_rects = Unsafe.Read<uint>(relPtr.ToPointer());
                        relPtr += sizeof(int);

                        for (int i = 0; i < num_rects; i++)
                        {
                            var rect = Marshal.PtrToStructure<SpiceRect>(relPtr + i * sizeof(SpiceRect));
                            clipRects.Add(rect);
                        }

                        clipRects.TrimExcess();
                        relPtr += (nint)(num_rects * sizeof(SpiceRect));
                    }

                    var type = (SpiceBrushType)Unsafe.Read<byte>(relPtr.ToPointer());
                    relPtr += sizeof(byte);

                    uint color = 0;

                    switch (type)
                    {
                        case SpiceBrushType.SPICE_BRUSH_TYPE_NONE:
                            break;
                        case SpiceBrushType.SPICE_BRUSH_TYPE_SOLID:
                            color = Unsafe.Read<uint>(relPtr.ToPointer());
                            SurfaceDrawFill?.Invoke(this, new SurfaceDrawFill(bas, type, color, clipRects));
                            break;
                        case SpiceBrushType.SPICE_BRUSH_TYPE_PATTERN:
                            break;
                    }

                }
                break;
            case Spice.SPICE_MSG_DISPLAY_DRAW_OPAQUE:
                // TODO
                break;
            case Spice.SPICE_MSG_DISPLAY_DRAW_BLEND:
                // TODO
                break;
            case Spice.SPICE_MSG_DISPLAY_DRAW_BLACKNESS:
                // TODO
                break;
            case Spice.SPICE_MSG_DISPLAY_DRAW_WHITENESS:
                // TODO
                break;
            case Spice.SPICE_MSG_DISPLAY_DRAW_INVERS:
                // TODO
                break;
            case Spice.SPICE_MSG_DISPLAY_DRAW_ROP3:
                // TODO
                break;
            case Spice.SPICE_MSG_DISPLAY_DRAW_STROKE:
                // TODO
                break;
            case Spice.SPICE_MSG_DISPLAY_DRAW_TRANSPARENT:
                // TODO
                break;
            case Spice.SPICE_MSG_DISPLAY_DRAW_ALPHA_BLEND:
                // TODO
                break;
            case Spice.SPICE_MSG_DISPLAY_COPY_BITS:
                {
                    var bas = Marshal.PtrToStructure<SpiceMsgDisplayBase>(relPtr);
                    relPtr += sizeof(SpiceMsgDisplayBase);

                    List<SpiceRect> clipRects = new();

                    if ((SpiceClipType)bas.clip_type == SpiceClipType.SPICE_CLIP_TYPE_RECTS)
                    {
                        var num_rects = Unsafe.Read<uint>(relPtr.ToPointer());
                        relPtr += sizeof(int);

                        for (int i = 0; i < num_rects; i++)
                        {
                            var rect = Marshal.PtrToStructure<SpiceRect>(relPtr + i * sizeof(SpiceRect));
                            clipRects.Add(rect);
                        }

                        clipRects.TrimExcess();
                        relPtr += (nint)(num_rects * sizeof(SpiceRect));
                    }

                    var srcPoint = Marshal.PtrToStructure<SpicePoint>(relPtr);

                    SurfaceCopyBits?.Invoke(this, new SurfaceCopyBitsArgs(bas, srcPoint, clipRects));
                }
                break;
            case Spice.SPICE_MSG_DISPLAY_INVAL_ALL_PIXMAPS:
                // TODO
                break;
            case Spice.SPICE_MSG_DISPLAY_INVAL_PALETTE:
                // TODO
                break;
            case Spice.SPICE_MSG_DISPLAY_INVAL_ALL_PALETTES:
                // TODO
                break;
            case Spice.SPICE_MSG_DISPLAY_SURFACE_CREATE:

                var surface = Marshal.PtrToStructure<SpiceSurface>(relPtr);

                Debug.WriteLine($"Id: {surface.surface_id} Width: {surface.width} Height: {surface.height} Format: {surface.format} Flags: {surface.flags}");

                SurfaceCreate?.Invoke(this, surface);

                break;
            case Spice.SPICE_MSG_DISPLAY_SURFACE_DESTROY:

                var surfaceId = Unsafe.Read<uint>(ptr);

                SurfaceDestroy?.Invoke(this, surfaceId);

                break;
            case Spice.SPICE_MSG_DISPLAY_STREAM_CREATE:
                // TODO
                break;
            case Spice.SPICE_MSG_DISPLAY_STREAM_DATA:
            case Spice.SPICE_MSG_DISPLAY_STREAM_DATA_SIZED:
                // TODO
                break;
            case Spice.SPICE_MSG_DISPLAY_STREAM_ACTIVATE_REPORT:
                // TODO
                break;
            case Spice.SPICE_MSG_DISPLAY_STREAM_CLIP:
                // TODO
                break;
            case Spice.SPICE_MSG_DISPLAY_STREAM_DESTROY:
                // TODO
                break;
            case Spice.SPICE_MSG_DISPLAY_STREAM_DESTROY_ALL:
                // TODO
                break;
            case Spice.SPICE_MSG_DISPLAY_INVAL_LIST:
                {
                    var count = Unsafe.Read<ushort>(relPtr.ToPointer());
                    relPtr += sizeof(ushort);

                    var resources = new List<ulong>();

                    for (int i = 0; i < count; i++)
                    {
                        var type = Unsafe.Read<byte>(relPtr.ToPointer());
                        relPtr += sizeof(byte);
                        var id = Unsafe.Read<ulong>(relPtr.ToPointer());
                        relPtr += sizeof(ulong);
                        resources.Add(id);
                    }

                    SurfaceInvalidateList?.Invoke(this, resources);
                }
                break;
            case Spice.SPICE_MSG_DISPLAY_MONITORS_CONFIG:
                // TODO
                break;
            case Spice.SPICE_MSG_DISPLAY_DRAW_COMPOSITE:
                // TODO
                break;
        }
    }
}

public sealed class SurfaceDrawCopyArgs : EventArgs
{
    public SpiceMsgDisplayBase Display { get; }
    public SpiceCopy Copy { get; }
    public SpiceImageDescriptor ImageDescriptor { get; }
    public List<SpiceRect> ClipRects { get; }
    public byte[] Image { get; }
    public bool FlipY { get; }

    public SurfaceDrawCopyArgs(SpiceMsgDisplayBase display, SpiceCopy copy, SpiceImageDescriptor imageDescriptor, List<SpiceRect> clipRects, byte[] image, bool flipY)
    {
        Display = display;
        Copy = copy;
        ImageDescriptor = imageDescriptor;
        ClipRects = clipRects;
        Image = image;
        FlipY = flipY;
    }
}

public sealed class SurfaceCopyBitsArgs : EventArgs
{
    public SpiceMsgDisplayBase Display { get; }
    public SpicePoint Point { get; }
    public List<SpiceRect> ClipRects { get; }

    public SurfaceCopyBitsArgs(SpiceMsgDisplayBase display, SpicePoint point, List<SpiceRect> clipRects)
    {
        Display = display;
        Point = point;
        ClipRects = clipRects;
    }
}

public sealed class SurfaceDrawFill : EventArgs
{
    public SpiceMsgDisplayBase Display { get; }
    public SpiceBrushType Type { get; }
    public uint Color { get; }
    public List<SpiceRect> ClipRects { get; }

    public SurfaceDrawFill(SpiceMsgDisplayBase display, SpiceBrushType type, uint color, List<SpiceRect> clipRects)
    {
        Display = display;
        Type = type;
        Color = color;
        ClipRects = clipRects;
    }
}

[StructLayout(LayoutKind.Sequential, Pack = 1)]
public struct SpiceMsgDisplayInit
{
    public byte pixmap_cache_id;
    public ulong pixmap_cache_size;
    public byte glz_dictionary_id;
    public uint glz_dictionary_window_size;
}

[StructLayout(LayoutKind.Sequential, Pack = 1)]
public struct SpiceSurface
{
    public uint surface_id;
    public uint width;
    public uint height;
    public uint format;
    public uint flags;
}

[StructLayout(LayoutKind.Sequential, Pack = 1)]
public struct SpiceQMask
{
    public byte flags;
    public SpicePoint pos;
    public uint offset;
}

[StructLayout(LayoutKind.Sequential, Pack = 1)]
public struct SpiceMsgDisplayBase
{
    public uint surface_id;
    public SpiceRect box;
    public byte clip_type;
}

[StructLayout(LayoutKind.Sequential, Pack = 1)]
public struct SpiceCopy
{
    public uint offset;
    public SpiceRect src_area;
    public ushort rop_descriptor;
    public byte scale_mode;
}

[StructLayout(LayoutKind.Sequential, Pack = 1)]
public struct SpiceImageDescriptor
{
    public ulong id;
    public byte type;
    public byte flags;
    public uint width;
    public uint height;
}

[StructLayout(LayoutKind.Sequential, Pack = 1)]
public struct LzRgb
{
    public uint length;
    public _magic_e__FixedBuffer magic;
    public uint version;
    public uint type;
    public uint width;
    public uint height;
    public uint stride;
    public uint top_down;

    [InlineArray(4)]
    public partial struct _magic_e__FixedBuffer
    {
        public char e0;
    }
}

[StructLayout(LayoutKind.Sequential, Pack = 1)]
public struct SpiceMsgDisplayMode
{
    public uint width;
    public uint height;
    public uint depth;
}
