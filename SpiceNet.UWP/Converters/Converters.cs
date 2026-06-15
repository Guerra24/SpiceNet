using SpiceNet.UWP.Models;
using Windows.UI.Xaml.Data;

namespace SpiceNet.UWP.Converters;

public partial class FitModeConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, string language)
    {
        return (FitMode)value == Enum.Parse<FitMode>((string)parameter);
    }

    public object ConvertBack(object value, Type targetType, object parameter, string language)
    {
        return Enum.Parse<FitMode>((string)parameter);
    }
}
