using System.Globalization;

namespace PwSafeClient.Maui.Converters;

public sealed class GroupIndentConverter : IValueConverter
{
    private const double IndentSize = 12;

    public object Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (value is int depth && depth > 0)
        {
            return new Microsoft.Maui.Thickness(depth * IndentSize, 0, 0, 0);
        }

        return new Microsoft.Maui.Thickness(0);
    }

    public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        throw new NotSupportedException();
    }
}
