using System.Windows;
using System.Windows.Controls;


namespace MarkLogic.Esri.ArcGISPro.AddIn
{
    /// <summary>
    /// Interaction logic for SearchResultsDockPaneView.xaml
    /// </summary>
    public partial class SearchResultsDockPane : UserControl
    {
        public SearchResultsDockPane()
        {
            InitializeComponent();
        }

        public static readonly DependencyProperty HtmlProperty = DependencyProperty.RegisterAttached("Html", typeof(string), typeof(SearchResultsDockPane), new FrameworkPropertyMetadata(OnHtmlChanged));

        [AttachedPropertyBrowsableForType(typeof(WebBrowser))]
        public static string GetHtml(WebBrowser wb)
        {
            return (string)wb.GetValue(HtmlProperty);
        }

        public static void SetHtml(WebBrowser wb, string html)
        {
            wb.SetValue(HtmlProperty, html);
        }

        private static void OnHtmlChanged(DependencyObject o, DependencyPropertyChangedEventArgs e)
        {
            var wb = o as WebBrowser;
            if (wb != null)
                wb.NavigateToString(e.NewValue as string);
        }
    }
}
