using System.Globalization;

namespace SaveMo.Converters;

public class PercentageConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        if (value is double d)
            return Math.Min(1.0, d / 100.0);
        if (value is int i)
            return Math.Min(1.0, i / 100.0);
        return 0.0;
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}
