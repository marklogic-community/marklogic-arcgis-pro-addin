using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;


namespace MarkLogic.Esri.ArcGISPro.AddIn
{
    /// <summary>
    /// Interaction logic for SearchResultsDockPaneView.xaml
    /// </summary>
    public partial class SearchResultsDockPaneView : UserControl
    {
        public SearchResultsDockPaneView()
        {
            InitializeComponent();
        }

        public static readonly DependencyProperty HtmlProperty = DependencyProperty.RegisterAttached("Html", typeof(string), typeof(SearchResultsDockPaneView), new FrameworkPropertyMetadata(OnHtmlChanged));

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
