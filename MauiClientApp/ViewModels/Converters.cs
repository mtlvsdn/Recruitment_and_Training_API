using System.Globalization;

namespace MauiClientApp.ViewModels
{
    /// <summary>
    /// Converts null values to empty strings for UI binding
    /// </summary>
    public class NullToEmptyStringConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return value?.ToString() ?? string.Empty;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return value;
        }
    }

    /// <summary>
    /// Converts null or invalid numeric values to zero for UI binding
    /// </summary>
    public class NullToZeroConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value == null)
                return 0;

            // Try to convert to int
            if (int.TryParse(value.ToString(), out int result))
                return result;

            return 0;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return value;
        }
    }
} 