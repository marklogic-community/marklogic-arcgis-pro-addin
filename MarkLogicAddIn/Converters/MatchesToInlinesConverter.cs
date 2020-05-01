using MarkLogic.Client.Search;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Windows.Data;
using System.Windows.Documents;

namespace MarkLogic.Esri.ArcGISPro.AddIn.Converters
{
    public class MatchesToInlinesConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            var matches = value as IEnumerable<MatchComponent>;
            if (matches == null)
                return null;
            return matches.Select(match =>
            {
                Inline inline = new Run(match.Text);
                if (match.IsHighlight)
                    inline = new Bold(inline);
                return inline;
            });
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
