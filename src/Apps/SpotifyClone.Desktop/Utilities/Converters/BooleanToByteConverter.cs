using System.Globalization;
using System.Windows.Data;

namespace SpotifyClone.Desktop.Utilities.Converters;

public class BooleanToByteConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        if (value is bool boolean)
        {
            return boolean ? 1 : 0;
        }

        return 0;
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
        if (value is not byte byteValue)
        {
            return false;
        }

        return byteValue >= 1;
    }
}
