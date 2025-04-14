using System.Globalization;
using Microsoft.Maui.Controls;

namespace MauiClientApp.Converters
{
    public class BoolToFontAttributesConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is bool boolValue)
            {
                return boolValue ? FontAttributes.Bold : FontAttributes.None;
            }
            return FontAttributes.None;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is FontAttributes fontAttr)
            {
                return fontAttr == FontAttributes.Bold;
            }
            return false;
        }
    }
} 