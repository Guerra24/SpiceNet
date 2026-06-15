using System.Runtime.InteropServices;

namespace SpiceNet.Protocol;

public static unsafe partial class Quic
{
    [DllImport("spice-common", CallingConvention = CallingConvention.Cdecl, EntryPoint = "quic_encode", ExactSpelling = true)]
    public static extern int quic_encode([NativeTypeName("QuicContext *")] void** quic, QuicImageType type, int width, int height, [NativeTypeName("uint8_t *")] byte* lines, [NativeTypeName("unsigned int")] uint num_lines, int stride, [NativeTypeName("uint32_t *")] uint* io_ptr, [NativeTypeName("unsigned int")] uint num_io_words);

    [DllImport("spice-common", CallingConvention = CallingConvention.Cdecl, EntryPoint = "quic_decode_begin", ExactSpelling = true)]
    public static extern int quic_decode_begin([NativeTypeName("QuicContext *")] void** quic, [NativeTypeName("uint32_t *")] uint* io_ptr, [NativeTypeName("unsigned int")] uint num_io_words, QuicImageType* type, int* width, int* height);

    [DllImport("spice-common", CallingConvention = CallingConvention.Cdecl, EntryPoint = "quic_decode", ExactSpelling = true)]
    public static extern int quic_decode([NativeTypeName("QuicContext *")] void** quic, QuicImageType type, [NativeTypeName("uint8_t *")] byte* buf, int stride);

    [DllImport("spice-common", CallingConvention = CallingConvention.Cdecl, EntryPoint = "quic_create", ExactSpelling = true)]
    [return: NativeTypeName("QuicContext *")]
    public static extern void** quic_create(QuicUsrContext* usr);

    [DllImport("spice-common", CallingConvention = CallingConvention.Cdecl, EntryPoint = "quic_destroy", ExactSpelling = true)]
    public static extern void quic_destroy([NativeTypeName("QuicContext *")] void** quic);

    [DllImport("spice-common", CallingConvention = CallingConvention.Cdecl, EntryPoint = "quic_create_usr_context", ExactSpelling = true)]
    public static extern QuicUsrContext* quic_create_usr_context();

    [DllImport("spice-common", CallingConvention = CallingConvention.Cdecl, EntryPoint = "quic_destroy_usr_context", ExactSpelling = true)]
    public static extern void quic_destroy_usr_context(QuicUsrContext* usr);

}
