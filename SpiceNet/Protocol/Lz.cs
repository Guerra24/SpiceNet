using System.Runtime.InteropServices;

namespace SpiceNet;

public static unsafe partial class Lz
{
    [DllImport("spice-common", CallingConvention = CallingConvention.Cdecl, EntryPoint = "lz_encode", ExactSpelling = true)]
    public static extern int lz_encode([NativeTypeName("LzContext *")] void** lz, LzImageType type, int width, int height, int top_down, [NativeTypeName("uint8_t *")] byte* lines, [NativeTypeName("unsigned int")] uint num_lines, int stride, [NativeTypeName("uint8_t *")] byte* io_ptr, [NativeTypeName("unsigned int")] uint num_io_bytes);

    [DllImport("spice-common", CallingConvention = CallingConvention.Cdecl, EntryPoint = "lz_decode_begin", ExactSpelling = true)]
    public static extern void lz_decode_begin([NativeTypeName("LzContext *")] void** lz, [NativeTypeName("uint8_t *")] byte* io_ptr, [NativeTypeName("unsigned int")] uint num_io_bytes, LzImageType* out_type, int* out_width, int* out_height, int* out_n_pixels, int* out_top_down, [NativeTypeName("const SpicePalette *")] SpicePalette* palette);

    [DllImport("spice-common", CallingConvention = CallingConvention.Cdecl, EntryPoint = "lz_decode", ExactSpelling = true)]
    public static extern void lz_decode([NativeTypeName("LzContext *")] void** lz, LzImageType to_type, [NativeTypeName("uint8_t *")] byte* buf);

    [DllImport("spice-common", CallingConvention = CallingConvention.Cdecl, EntryPoint = "lz_create", ExactSpelling = true)]
    [return: NativeTypeName("LzContext *")]
    public static extern void** lz_create(LzUsrContext* usr);

    [DllImport("spice-common", CallingConvention = CallingConvention.Cdecl, EntryPoint = "lz_destroy", ExactSpelling = true)]
    public static extern void lz_destroy([NativeTypeName("LzContext *")] void** lz);

    [DllImport("spice-common", CallingConvention = CallingConvention.Cdecl, EntryPoint = "lz_create_usr_context", ExactSpelling = true)]
    public static extern LzUsrContext* lz_create_usr_context();

    [DllImport("spice-common", CallingConvention = CallingConvention.Cdecl, EntryPoint = "lz_destroy_usr_context", ExactSpelling = true)]
    public static extern void lz_destroy_usr_context(LzUsrContext* usr);
}
