using System;

namespace SpiceNet.Protocol;

public partial struct QuicUsrContext
{
    [NativeTypeName("void (*)(QuicUsrContext *, const char *, ...)")]
    public IntPtr error;

    [NativeTypeName("void (*)(QuicUsrContext *, const char *, ...)")]
    public IntPtr warn;

    [NativeTypeName("void (*)(QuicUsrContext *, const char *, ...)")]
    public IntPtr info;

    [NativeTypeName("void *(*)(QuicUsrContext *, int)")]
    public IntPtr malloc;

    [NativeTypeName("void (*)(QuicUsrContext *, void *)")]
    public IntPtr free;

    [NativeTypeName("int (*)(QuicUsrContext *, uint32_t **, int)")]
    public IntPtr more_space;

    [NativeTypeName("int (*)(QuicUsrContext *, uint8_t **)")]
    public IntPtr more_lines;
}
