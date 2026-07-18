using System.Globalization;
using System.Windows;
using System.Windows.Data;

namespace AtlasERP.Presentation.WPF.Converters;

/// <summary>true -> Visible; false -> Collapsed. Pass "Invert" as ConverterParameter to flip it.</summary>
public class BoolToVisibleConverter : IValueConverter
{
    public object Convert(object? value, Type targetType, object parameter, CultureInfo culture)
    {
        var boolValue = value is true;
        if (string.Equals(parameter as string, "Invert", StringComparison.OrdinalIgnoreCase))
        {
            boolValue = !boolValue;
        }

        return boolValue ? Visibility.Visible : Visibility.Collapsed;
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        => throw new NotSupportedException();
}
