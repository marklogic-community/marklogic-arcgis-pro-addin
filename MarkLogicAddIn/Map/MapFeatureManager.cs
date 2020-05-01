using ArcGIS.Core.CIM;
using ArcGIS.Core.Data;
using ArcGIS.Desktop.Editing;
using ArcGIS.Desktop.Framework;
using ArcGIS.Desktop.Framework.Threading.Tasks;
using ArcGIS.Desktop.Mapping;
using MarkLogic.Client.Search;
using MarkLogic.Esri.ArcGISPro.AddIn.Messaging;
using MarkLogic.Esri.ArcGISPro.AddIn.ViewModels.Messages;
using MarkLogic.Extensions.Koop;
using System;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;

namespace MarkLogic.Esri.ArcGISPro.AddIn.Map
{
    public class MapFeatureManager
    {
        public MapFeatureManager(MessageBus messageBus)
        {
            MessageBus = messageBus ?? throw new ArgumentNullException("messageBus");
            MessageBus.Subscribe<ServerSettingsChangedMessage>(m => CheckServiceModelGroupLayer(m.ServiceModel));
            MessageBus.Subscribe<SearchSavedMessage>(async m => await UpdateFeatureLayers(m.Results, m.ServiceModel));
        }

        private MessageBus MessageBus { get; set; }

        private async void CheckServiceModelGroupLayer(ServiceModel serviceModel)
        {
            if (serviceModel == null)
                return;

            var serviceUrl = serviceModel.FeatureService;
            var layerExists = await GetGroupLayer(serviceModel.FeatureService) != null;
            if (!layerExists)
            {
                var add = FrameworkApplication.Current.Dispatcher.Invoke(() =>
                {
                    var result = ArcGIS.Desktop.Framework.Dialogs.MessageBox.Show(
                        $"The feature service \"{serviceModel.Name}\" will need to be added to save your searches.  Would you like to add it to your project now?",
                        "MarkLogic",
                        MessageBoxButton.YesNo,
                        MessageBoxImage.Warning);
                    return result == MessageBoxResult.Yes;
                });

                if (add)
                    await AddGroupLayer(serviceModel.Name, serviceUrl);
            }
        }

        private Task<FeatureLayer> GetGroupLayer(Uri serviceUrl)
        {
            var mapView = MapView.Active;
            Debug.Assert(mapView != null);
            if (mapView == null)
                return Task.FromResult<FeatureLayer>(null);

            return QueuedTask.Run(() =>
            {
                return FindLayerByWorkspaceConnection<FeatureLayer>(mapView.Map, serviceUrl.AbsoluteUri); // find any feature layer using the serviceUrl
            });
        }

        private async Task AddGroupLayer(string groupName, Uri serviceUrl)
        {
            var mapView = MapView.Active;
            Debug.Assert(mapView != null);
            if (mapView == null)
                return;

            var progressDlg = new ProgressDialog($"Adding group layer '{groupName}'...");
            try
            {
                progressDlg.Show();

                await QueuedTask.Run(() =>
                {
                    // check if the group layer already exists (e.g. save search -> replace)
                    var groupLayer = FindLayerByWorkspaceConnection<GroupLayer>(mapView.Map, serviceUrl.AbsoluteUri);
                    if (groupLayer == null)
                    {
                        // add the new group layer 
                        using (var geoDb = new Geodatabase(new ServiceConnectionProperties(serviceUrl)))
                        {
                            groupLayer = (GroupLayer)LayerFactory.Instance.CreateLayer(serviceUrl, mapView.Map);
                            groupLayer.SetName(groupName);
                            foreach (var layer in (groupLayer as GroupLayer).GetLayersAsFlattenedList())
                                layer.SetVisibility(false);
                        }
                    }
                    else { 
                        // update layer name
                        if (groupLayer.Name != groupName)
                            groupLayer.SetName(groupName);
                    }
                });
            }
            catch (Exception e)
            {
                e.HandleAsUserNotification();
            }
            finally
            {
                progressDlg.Hide();
            }
        }

        private async Task UpdateFeatureLayers(SaveSearchResults results, ServiceModel serviceModel)
        {
            Debug.Assert(MapView.Active != null);
            var mapView = MapView.Active;
            
            var progressDlg = new ProgressDialog("Updating map...");
            try
            {
                progressDlg.Show();

                await QueuedTask.Run(() =>
                {
                    var serviceUri = results.FeatureService;
                    foreach (var resultLayer in results.FeatureLayers)
                    {
                        // check if the feature layer already exists (e.g. save search -> replace)
                        var featureLayer = FindLayerByDataset<FeatureLayer>(mapView.Map, serviceUri.AbsoluteUri, resultLayer.Id.ToString());
                        if (featureLayer == null)
                        {
                            // add the entire feature service as a group layer 
                            using (var geoDb = new Geodatabase(new ServiceConnectionProperties(serviceUri)))
                            {
                                GroupLayer groupLayer = null;

                                // add the new group layer
                                groupLayer = (GroupLayer)LayerFactory.Instance.CreateLayer(serviceUri, mapView.Map);
                                groupLayer.SetName(serviceModel.Name);
                                foreach (var childLayer in groupLayer.GetLayersAsFlattenedList())
                                {
                                    var match = MatchLayer(childLayer, serviceUri.AbsoluteUri, resultLayer.Id.ToString());
                                    if (match && childLayer.Name != resultLayer.Name)
                                        childLayer.SetName(resultLayer.Name);
                                    childLayer.SetVisibility(match);
                                }
                            }
                        }
                        else
                        {
                            // force a refresh
                            featureLayer.ClearDisplayCache();
                            featureLayer.SetDefinitionQuery(featureLayer.DefinitionQuery);

                            // ensure the feature layer is visible
                            if (!featureLayer.IsVisible)
                                featureLayer.SetVisibility(true);

                            // update layer name
                            if (featureLayer.Name != resultLayer.Name)
                                featureLayer.SetName(resultLayer.Name);
                        }
                    }
                });
            }
            catch (Exception e)
            {
                e.HandleAsUserNotification();
            }
            finally
            {
                progressDlg.Hide();
            }
        }

        private T FindLayerByWorkspaceConnection<T>(ILayerContainer container, string workspaceConnection) where T : Layer
        {
            var layers = container.GetLayersAsFlattenedList().OfType<T>();
            var layer = layers
                .Where(l => {
                    if (!(l.GetDataConnection() is CIMStandardDataConnection))
                        return false;
                    var conn = (CIMStandardDataConnection)l.GetDataConnection();
                    return conn.WorkspaceConnectionString.Contains(workspaceConnection);
                })
                .FirstOrDefault();
            return layer;
        }

        private T FindLayerByDataset<T>(ILayerContainer container, string workspaceConnection, string dataset) where T : Layer
        {
            var layers = container.GetLayersAsFlattenedList().OfType<T>();
            var layer = layers
                .Where(l => {
                    if (!(l.GetDataConnection() is CIMStandardDataConnection))
                        return false;
                    var conn = (CIMStandardDataConnection)l.GetDataConnection();
                    return conn.WorkspaceConnectionString.Contains(workspaceConnection) && conn.Dataset == dataset;
                })
                .FirstOrDefault();
            return layer;
        }

        private bool MatchLayer(Layer layer, string workspaceConnection, string dataset)
        {
            if (!(layer.GetDataConnection() is CIMStandardDataConnection))
                return false;
            var conn = (CIMStandardDataConnection)layer.GetDataConnection();
            return conn.WorkspaceConnectionString.Contains(workspaceConnection) && conn.Dataset == dataset;
        }
    }
}
