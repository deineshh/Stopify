using System.Globalization;
using System.Windows.Data;

namespace SpotifyClone.Desktop.Utilities.Converters;

public class ArrayToStringConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        if (value is IEnumerable<string> array)
        {
            return string.Join(", ", array);
        }

        return string.Empty;
    }
    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        => throw new NotImplementedException();
}
