using System.Globalization;
using System.Windows.Data;

namespace SpotifyClone.Desktop.Utilities.Converters;

public class WidthToHeightConverter : IMultiValueConverter
{
    public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
    {
        if (values[0] is double actualWidth)
        {
            return actualWidth * 0.58;
        }

        return 0;
    }

    public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
        => throw new NotImplementedException();
}