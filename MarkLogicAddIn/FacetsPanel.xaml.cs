using MarkLogic.Client.Search;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
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
    /// Interaction logic for FacetPanel.xaml
    /// </summary>
    public partial class FacetsPanel : UserControl
    {
        public FacetsPanel()
        {
            InitializeComponent();
        }

        private void FacetValue_SelectedChanged(object sender, RoutedEventArgs e)
        {
            Debug.Assert(sender is CheckBox);
            var checkBox = sender as CheckBox;
            var dc = checkBox.DataContext;
            Debug.Assert(dc is FacetValue);
            var facetValue = (FacetValue)dc;
            facetValue.Selected = checkBox.IsChecked.GetValueOrDefault();
            if (SelectFacet != null)
                SelectFacet.Execute(facetValue);
        }

        public static readonly DependencyProperty FacetsProperty = DependencyProperty.Register("Facets", typeof(IEnumerable<Facet>), typeof(FacetsPanel));
        public IEnumerable<Facet> Facets
        {
            get { return (IEnumerable<Facet>)GetValue(FacetsProperty); }
            set { SetValue(FacetsProperty, value); }
        }

        public static readonly DependencyProperty SelectFacetProperty = DependencyProperty.Register("SelectFacet", typeof(ICommand), typeof(FacetsPanel));
        public ICommand SelectFacet
        {
            get { return (ICommand)GetValue(SelectFacetProperty); }
            set { SetValue(SelectFacetProperty, value); }
        }
    }
}
