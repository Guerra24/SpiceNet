namespace SpiceNet.UWP.Extensions;

public static class SpiceRectExtensions
{
    public static Rect ToRect(this SpiceRect rect)
    {
        return new Rect(rect.left, rect.top, rect.right - rect.left, rect.bottom - rect.top);
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
