using SpiceNet.Protocol;
using Windows.Win32;
using Windows.Win32.UI.Input.KeyboardAndMouse;

namespace SpiceNet.UWP.Extensions;

public static class SpiceRectExtensions
{
    public static Rect ToRect(this SpiceRect rect)
    {
        return new Rect(rect.left, rect.top, rect.right - rect.left, rect.bottom - rect.top);
    }
}

public static class VirtualKeyExtensions
{
    public static uint ToScancode(this VirtualKey key)
    {
        return PInvoke.MapVirtualKey((uint)key, MAP_VIRTUAL_KEY_TYPE.MAPVK_VK_TO_VSC_EX);
    }
}

[MarkupExtensionReturnType(ReturnType = typeof(object))]
public partial class EnumValueExtension : MarkupExtension
{
    public Type Type { get; set; } = null!;

    public string Member { get; set; } = null!;

    protected override object ProvideValue()
    {
        return Enum.Parse(Type, Member);
    }
}
