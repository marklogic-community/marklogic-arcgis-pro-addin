using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;

namespace MarkLogic.Esri.ArcGISPro.AddIn.Converters
{
    public class EmptyArrayToVisibilityConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return value == null || (value != null && value is Array && ((Array)value).Length == 0) ? Visibility.Hidden : Visibility.Visible;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
