using SaveMo.Models;
using System.Globalization;

namespace SaveMo.Converters;

/// <summary>
/// Converts between TransactionType enum values and Picker selected index.
/// Used to bind Picker.SelectedIndex to Transaction.Type property.
/// </summary>
public class TransactionTypeIndexConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        if (value is TransactionType type)
        {
            return (int)type;
        }

        return -1; // No selection
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
        if (value is int index && index >= 0 && index < 2)
        {
            return (TransactionType)index;
        }

        return TransactionType.Expense; // Default
    }
}
