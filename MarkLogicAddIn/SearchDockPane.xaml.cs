using ArcGIS.Desktop.Framework.Controls;
using ArcGIS.Desktop.Mapping;
using MarkLogic.Esri.ArcGISPro.AddIn.Feature;
using MarkLogic.Extensions.Koop;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Net;
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
    /// Interaction logic for SearchDockPaneView.xaml
    /// </summary>
    public partial class SearchDockPaneView : UserControl
    {
        public SearchDockPaneView()
        {
            InitializeComponent();
        }

        private void SaveButton_Click(object sender, RoutedEventArgs e)
        {
            /*var viewModel = (SearchDockPaneViewModel)DataContext;

            Debug.Assert(viewModel != null);
            Debug.Assert(viewModel.HasSelectedServiceModel);

            var saveSearchViewModel = new SaveSearchViewModel();
            saveSearchViewModel.Connection = viewModel.Connection;
            //saveSearchViewModel.ServiceName = viewModel.SelectedSearchServiceProfile.ServiceName; TODO: replace

            SaveSearchWindow dlg = new SaveSearchWindow();
            dlg.DataContext = saveSearchViewModel;
            var dlgResult = dlg.ShowDialog();
            dlg.Close();

            if (dlgResult.HasValue && dlgResult.Value == true)
                viewModel.SaveToNewLayerCommand.Execute(dlg.DataContext as ISaveSearchOptions);*/
        }

        private void SearchInfoButton_Click(object sender, RoutedEventArgs e)
        {
            var viewModel = (SearchDockPaneViewModel)DataContext;

            Debug.Assert(viewModel != null);
            //Debug.Assert(viewModel.HasQueryOptions); TODO: replace

            var model = new SearchInfoModel();
            //model.Initialize(viewModel.QueryOptions, viewModel); TODO: replace

            var dlg = new SearchInfoWindow();
            dlg.DataContext = model;
            dlg.Show();
        }

        private async void ServiceModels_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            /*var serviceModel = e.AddedItems.Count > 0 ? (ServiceModel)e.AddedItems[0] : null;
            if (serviceModel == null)
                return;

            // TODO: replace
            var serviceUrl = $"http://localhost:8095/marklogic/{serviceModel.Id}/FeatureServer";

            var layerExists = await FeatureLayerBuilder.Instance.GroupLayerExists(MapView.Active, serviceUrl);
            if (!layerExists)
            {
                var result = ArcGIS.Desktop.Framework.Dialogs.MessageBox.Show($"The feature service \"{serviceModel.Name}\" will need to be added to save your searches.  Would you like to add it to your project now?", "MarkLogic", MessageBoxButton.YesNo, MessageBoxImage.Warning);
                if (result == MessageBoxResult.Yes)
                {
                    await FeatureLayerBuilder.Instance.AddGroupLayer(MapView.Active, serviceModel.Name, serviceUrl);
                }
            }*/
        }
    }
}
