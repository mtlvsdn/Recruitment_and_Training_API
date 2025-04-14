using System.Globalization;

namespace MauiClientApp.Converters
{
    public class StringEqualityConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            // Return true if the value and parameter strings are equal
            if (value == null || parameter == null)
                return false;
            
            return value.ToString() == parameter.ToString();
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            // Return the parameter if the value is true
            if (value is bool isChecked && isChecked && parameter != null)
                return parameter.ToString();
            
            return null;
        }
    }
} 