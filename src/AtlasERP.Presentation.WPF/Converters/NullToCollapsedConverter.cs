using System.Globalization;
using System.Windows;
using System.Windows.Data;

namespace AtlasERP.Presentation.WPF.Converters;

/// <summary>Null or empty string -> Collapsed; anything else -> Visible.</summary>
public class NullToCollapsedConverter : IValueConverter
{
    public object Convert(object? value, Type targetType, object parameter, CultureInfo culture)
    {
        var isEmpty = value is null || (value is string s && string.IsNullOrWhiteSpace(s));
        return isEmpty ? Visibility.Collapsed : Visibility.Visible;
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        => throw new NotSupportedException();
}
