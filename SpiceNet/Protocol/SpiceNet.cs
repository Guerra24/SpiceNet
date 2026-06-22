using System.Reflection;
using System.Runtime.InteropServices;

namespace SpiceNet.Protocol;

public static partial class Lz
{
    static Lz()
    {
        try
        {
            NativeLibrary.Load("spice-common", Assembly.GetExecutingAssembly(), null);
        }
        catch { }
    }
}

public static partial class Quic
{
    static Quic()
    {
        try
        {
            NativeLibrary.Load("spice-common", Assembly.GetExecutingAssembly(), null);
        }
        catch { }
    }
}
