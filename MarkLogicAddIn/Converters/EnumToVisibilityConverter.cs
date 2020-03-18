using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;

namespace MarkLogic.Esri.ArcGISPro.AddIn.Converters
{
    public class EnumToVisibilityConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object property, CultureInfo culture)
        {
            if (value != null && value.GetType().IsEnum)
                return Enum.Equals(value, property) ? Visibility.Visible : Visibility.Collapsed;
            else
                return DependencyProperty.UnsetValue;
        }

        public object ConvertBack(object value, Type targetType, object property, CultureInfo culture)
        {
            if (value is Visibility && (Visibility)value == Visibility.Visible)
                return property;
            else
                return DependencyProperty.UnsetValue;
        }
    }
}
