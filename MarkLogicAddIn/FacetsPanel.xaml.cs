using MarkLogic.Esri.ArcGISPro.AddIn.ViewModels;
using System.Collections.Generic;
using System.Diagnostics;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace MarkLogic.Esri.ArcGISPro.AddIn
{
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
            Debug.Assert(dc is FacetValueViewModel);
            var facetValue = (FacetValueViewModel)dc;
            facetValue.Selected = checkBox.IsChecked.GetValueOrDefault();
            if (SelectFacet != null)
                SelectFacet.Execute(facetValue);
        }

        public static readonly DependencyProperty FacetsProperty = DependencyProperty.Register("Facets", typeof(IEnumerable<FacetViewModel>), typeof(FacetsPanel));
        public IEnumerable<FacetViewModel> Facets
        {
            get { return (IEnumerable<FacetViewModel>)GetValue(FacetsProperty); }
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
