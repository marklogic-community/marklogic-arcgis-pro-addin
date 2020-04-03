using ArcGIS.Core.CIM;
using ArcGIS.Core.Data;
using ArcGIS.Desktop.Editing;
using ArcGIS.Desktop.Framework.Threading.Tasks;
using ArcGIS.Desktop.Mapping;
using MarkLogic.Esri.ArcGISPro.AddIn.Messaging;
using MarkLogic.Esri.ArcGISPro.AddIn.ViewModels.Messages;
using System;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;

namespace MarkLogic.Esri.ArcGISPro.AddIn.Map
{
    public class MapFeatureManager
    {
        public MapFeatureManager(MessageBus messageBus)
        {
            MessageBus = messageBus ?? throw new ArgumentNullException("messageBus");
            MessageBus.Subscribe<SearchSavedMessage>(async m =>
            {
                // add feature layers here
            });
        }

        private MessageBus MessageBus { get; set; }

        private async Task AddGroupLayer(string groupName, string serviceUrl)
        {
            Debug.Assert(MapView.Active != null);
            var mapView = MapView.Active;

            var progressDlg = new ProgressDialog($"Adding '{groupName}'...");
            try
            {
                progressDlg.Show();

                await QueuedTask.Run(() =>
                {
                    // check if the group layer already exists (e.g. save search -> replace)
                    var groupLayer = FindLayerByWorkspaceConnection<GroupLayer>(mapView.Map, serviceUrl);
                    if (groupLayer == null)
                    {
                        // add the new group layer 
                        using (var geoDb = new Geodatabase(new ServiceConnectionProperties(new Uri(serviceUrl))))
                        {
                            var uri = new Uri(serviceUrl);
                            groupLayer = (GroupLayer)LayerFactory.Instance.CreateLayer(uri, mapView.Map);
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

        private async Task AddFeatureLayers(string groupName, Uri serviceUri, string layerName, int layerId)
        {
            Debug.Assert(MapView.Active != null);
            var mapView = MapView.Active;
            
            var progressDlg = new ProgressDialog("Updating map...");
            try
            {
                progressDlg.Show();

                await QueuedTask.Run(() =>
                {   
                    // check if the feature layer already exists (e.g. save search -> replace)
                    var featureLayer = FindLayerByDataset<FeatureLayer>(mapView.Map, serviceUri.AbsoluteUri, layerId.ToString());
                    if (featureLayer == null)
                    {
                        // add the new feature layer 
                        using (var geoDb = new Geodatabase(new ServiceConnectionProperties(serviceUri)))
                        {
                            GroupLayer groupLayer = null;

                            // add the new group layer
                            groupLayer = (GroupLayer)LayerFactory.Instance.CreateLayer(serviceUri, mapView.Map);
                            groupLayer.SetName(groupName);
                            foreach (var childLayer in groupLayer.GetLayersAsFlattenedList())
                            {
                                var match = MatchLayer(childLayer, serviceUri.AbsoluteUri, layerId.ToString());
                                if (match && childLayer.Name != layerName)
                                    childLayer.SetName(layerName);
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
                        if (featureLayer.Name != layerName)
                            featureLayer.SetName(layerName);
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
