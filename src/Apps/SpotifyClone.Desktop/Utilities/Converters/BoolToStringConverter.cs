using System.Globalization;
using System.Windows.Data;

namespace SpotifyClone.Desktop.Utilities.Converters;

public class BooleanToStringConverter : IValueConverter
{
    // Format: "TrueValue|FalseValue|NullValue"
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        string[] values = (parameter as string)?.Split('|') ?? Array.Empty<string>();
        string trueValue = values.Length > 0 ? values[0] : "True";
        string falseValue = values.Length > 1 ? values[1] : "False";
        string nullValue = values.Length > 2 ? values[2] : "";

        return value switch
        {
            true => trueValue,
            false => falseValue,
            null => nullValue,
            _ => falseValue
        };
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) =>
        throw new NotImplementedException();
}
