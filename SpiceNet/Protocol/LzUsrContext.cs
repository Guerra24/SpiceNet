using System;

namespace SpiceNet;

public partial struct LzUsrContext
{
    [NativeTypeName("void (*)(LzUsrContext *, const char *, ...)")]
    public IntPtr error;

    [NativeTypeName("void (*)(LzUsrContext *, const char *, ...)")]
    public IntPtr warn;

    [NativeTypeName("void (*)(LzUsrContext *, const char *, ...)")]
    public IntPtr info;

    [NativeTypeName("void *(*)(LzUsrContext *, int)")]
    public IntPtr malloc;

    [NativeTypeName("void (*)(LzUsrContext *, void *)")]
    public IntPtr free;

    [NativeTypeName("int (*)(LzUsrContext *, uint8_t **)")]
    public IntPtr more_space;

    [NativeTypeName("int (*)(LzUsrContext *, uint8_t **)")]
    public IntPtr more_lines;
}
