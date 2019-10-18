using MarkLogic.Client.Search;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Input;

namespace MarkLogic.Esri.ArcGISPro.AddIn
{
    /// <summary>
    /// Interaction logic for ResultsPanel.xaml
    /// </summary>
    public partial class ResultsPanel : UserControl
    {
        public ResultsPanel()
        {
            InitializeComponent();
        }

        public static readonly DependencyProperty ResultsProperty = DependencyProperty.Register("Results", typeof(IEnumerable<SearchResult>), typeof(ResultsPanel));
        public IEnumerable<SearchResult> Results
        {
            get { return (IEnumerable<SearchResult>)GetValue(ResultsProperty); }
            set { SetValue(ResultsProperty, value); }
        }

        public static readonly DependencyProperty CurrentPageProperty = DependencyProperty.Register("CurrentPage", typeof(long), typeof(ResultsPanel));
        public long CurrentPage
        {
            get { return (long)GetValue(CurrentPageProperty); }
            set { SetValue(CurrentPageProperty, value); }
        }

        public static readonly DependencyProperty TotalPagesProperty = DependencyProperty.Register("TotalPages", typeof(long), typeof(ResultsPanel));
        public long TotalPages
        {
            get { return (long)GetValue(TotalPagesProperty); }
            set { SetValue(TotalPagesProperty, value); }
        }

        public static readonly DependencyProperty SelectResultProperty = DependencyProperty.Register("SelectResult", typeof(ICommand), typeof(ResultsPanel));
        public ICommand SelectResult
        {
            get { return (ICommand)GetValue(SelectResultProperty); }
            set { SetValue(SelectResultProperty, value); }
        }

        public static readonly DependencyProperty PagePrevResultsProperty = DependencyProperty.Register("PagePrevResults", typeof(ICommand), typeof(ResultsPanel));
        public ICommand PagePrevResults
        {
            get { return (ICommand)GetValue(PagePrevResultsProperty); }
            set { SetValue(PagePrevResultsProperty, value); }
        }

        public static readonly DependencyProperty PageNextResultsProperty = DependencyProperty.Register("PageNextResults", typeof(ICommand), typeof(ResultsPanel));
        public ICommand PageNextResults
        {
            get { return (ICommand)GetValue(PageNextResultsProperty); }
            set { SetValue(PageNextResultsProperty, value); }
        }
    }

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
