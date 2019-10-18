using ArcGIS.Core.CIM;
using ArcGIS.Core.Data;
using ArcGIS.Core.Geometry;
using ArcGIS.Desktop.Core;
using ArcGIS.Desktop.Core.Geoprocessing;
using ArcGIS.Desktop.Editing;
using ArcGIS.Desktop.Framework.Threading.Tasks;
using ArcGIS.Desktop.Mapping;
using MarkLogic.Client.Search;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows;

namespace MarkLogic.Esri.ArcGISPro.AddIn.Feature
{
    public class FeatureLayerBuilder
    {
        private static FeatureLayerBuilder _instance;

        private FeatureLayerBuilder()
        {
        }

        public static FeatureLayerBuilder Instance => _instance ?? (_instance = new FeatureLayerBuilder());

        private bool FieldExists(FeatureClassDefinition definition, string fieldName)
        {
            var fields = definition.GetFields();
            return fields.Where(f => f.Name == fieldName).FirstOrDefault() != null;
        }

        private Task<IGPResult> AddField(string featureClassName, string fieldName, string fieldType, string fieldLength)
        {
            object[] args =
            {
                featureClassName,
                fieldName,
                fieldType,
                "", // precision
                "", // scale
                fieldLength, // field length
                "", // alias
                "NULLABLE",
                "NON_REQUIRED",
                ""
            };
            return Geoprocessing.ExecuteToolAsync("AddField_management", Geoprocessing.MakeValueArray(args));
        }

        private Task<string> CreatePointFeatureClass(string layerName, MapView mapView)
        {
            var baseFeatureClassName = Regex.Replace(layerName.ToLower(), "[^a-zA-z]", "_").Trim('_');
            string featureClassName = baseFeatureClassName;

            return QueuedTask.Run(async () =>
            {
                var existingFeatureClasses = mapView.Map.GetLayersAsFlattenedList().OfType<FeatureLayer>().Select(l => l.GetFeatureClass());
                FeatureClass existingFeatureClass = null;
                var fcCtr = 0;
                do
                {
                    existingFeatureClass = existingFeatureClasses.Where(c => c.GetName() == featureClassName).FirstOrDefault();
                    if (existingFeatureClass != null)
                        featureClassName = $"{baseFeatureClassName}_{++fcCtr}";
                }
                while (existingFeatureClass != null);

                // construct feature class
                object[] args =
                {
                    CoreModule.CurrentProject.DefaultGeodatabasePath, // project's default geodatabase
                    featureClassName, // name
                    "POINT", // geometry type
                    "", // no template
                    "DISABLED", // no z values
                    "DISABLED", // no m values
                    SpatialReferences.WGS84
                };
                IGPResult createClassResult = await Geoprocessing.ExecuteToolAsync("CreateFeatureclass_management", Geoprocessing.MakeValueArray(args));
                if (createClassResult.IsFailed)
                    throw new InvalidOperationException($"Failed to create new feature class {featureClassName}.");

                // add field(s)
                IGPResult createFreqFieldResult = await AddField(featureClassName, "frequency", "LONG", "");
                if (createFreqFieldResult.IsFailed)
                    throw new InvalidOperationException($"Failed to add frequency field to feature class {featureClassName}.");

                // set name of newly created layer
                var layer = mapView.Map.GetLayersAsFlattenedList().OfType<FeatureLayer>().Where(l => l.GetFeatureClass().GetName() == featureClassName).FirstOrDefault();
                layer.SetName(layerName);

                return featureClassName;
            });
        }

        public async Task CreateFeatureLayer(MapView mapView, string layerName, ValuesResults results)
        {
            if (mapView == null)
                throw new ArgumentNullException("mapView");
            if (string.IsNullOrWhiteSpace(layerName))
                throw new ArgumentNullException("layerName");
            if (results == null)
                throw new ArgumentNullException("results");

            if (mapView.Map.GetLayersAsFlattenedList().OfType<FeatureLayer>().Where(l => l.Name == layerName).FirstOrDefault() != null)
                throw new InvalidOperationException($"Feature layer with name '{layerName}' already exists.");

            var progressDlg = new ProgressDialog("Creating new feature layer...");
            progressDlg.Show();

            var featureClassName = await CreatePointFeatureClass(layerName, mapView);

            // add new rows
            if (results.Count > 0)
            {
                var addRowsResult = await QueuedTask.Run(async () =>
                {
                    var layer = mapView.Map.GetLayersAsFlattenedList().OfType<FeatureLayer>().Where(l => l.GetFeatureClass().GetName() == featureClassName).FirstOrDefault();
                    Debug.Assert(layer != null, $"Unable to find layer with feature class name {featureClassName}.");
                    var featureClass = layer.GetFeatureClass();
                    var createOp = new EditOperation() { Name = "Update Search Results", SelectNewFeatures = false };
                    createOp.Callback((context) =>
                    {
                        foreach (var result in results)
                        {
                            var mapPoint = MapPointBuilder.CreateMapPoint(result.Long, result.Lat, SpatialReferences.WGS84);
                            var row = featureClass.CreateRowBuffer();
                            row["SHAPE"] = mapPoint;
                            row["frequency"] = result.Frequency;
                            featureClass.CreateRow(row);
                        }
                        context.Invalidate(featureClass);
                    }, featureClass);

                    return await createOp.ExecuteAsync();
                });
            }

            await QueuedTask.Run(async () =>
            {
                if (Project.Current.HasEdits)
                {
                    var saveEditsResult = await Project.Current.SaveEditsAsync();
                    Debug.Assert(saveEditsResult == true, "SaveEditsAsync() returned false.");

                    var layer = mapView.Map.GetLayersAsFlattenedList().OfType<FeatureLayer>().Where(l => l.Name == layerName).FirstOrDefault();
                    if (layer != null)
                        await mapView.ZoomToAsync(layer);
                }
            });

            progressDlg.Hide();
        }

        public async Task<bool> GroupLayerExists(MapView mapView, string serviceUrl)
        {
            if (mapView == null)
                throw new ArgumentNullException("mapView");
            if (serviceUrl == null)
                throw new ArgumentNullException("serviceUrl");

            return await QueuedTask.Run(() =>
            {
                var layer = FindLayerByWorkspaceConnection<FeatureLayer>(mapView.Map, serviceUrl); // find any feature layer connecting to the serviceUrl
                return layer != null;
            });
        }

        public async Task AddGroupLayer(MapView mapView, string groupName, string serviceUrl)
        {
            if (mapView == null)
                throw new ArgumentNullException("mapView");
            if (groupName == null)
                throw new ArgumentNullException("groupName");
            if (serviceUrl == null)
                throw new ArgumentNullException("serviceUrl");

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
                ArcGIS.Desktop.Framework.Dialogs.MessageBox.Show(e.ToString(), "MarkLogic", MessageBoxButton.OK, MessageBoxImage.Error);
            }
            finally
            {
                progressDlg.Hide();
            }
        }

        public async Task AddFeatureLayer(MapView mapView, string groupName, string serviceUrl, string layerName, string layerId)
        {
            if (mapView == null)
                throw new ArgumentNullException("mapView");
            if (groupName == null)
                throw new ArgumentNullException("groupName");
            if (serviceUrl == null)
                throw new ArgumentNullException("serviceUrl");
            if (layerName == null)
                throw new ArgumentNullException("layerName");
            if (layerId == null)
                throw new ArgumentNullException("layerId");

            var progressDlg = new ProgressDialog("Updating map...");
            try
            {
                progressDlg.Show();

                await QueuedTask.Run(() =>
                {   
                    // check if the feature layer already exists (e.g. save search -> replace)
                    var featureLayer = FindLayerByDataset<FeatureLayer>(mapView.Map, serviceUrl, layerId);
                    if (featureLayer == null)
                    {
                        // add the new feature layer 
                        using (var geoDb = new Geodatabase(new ServiceConnectionProperties(new Uri(serviceUrl))))
                        {
                            GroupLayer groupLayer = null;

                            // add the new group layer 
                            var uri = new Uri(serviceUrl);
                            groupLayer = (GroupLayer)LayerFactory.Instance.CreateLayer(uri, mapView.Map);
                            groupLayer.SetName(groupName);
                            foreach (var childLayer in groupLayer.GetLayersAsFlattenedList())
                            {
                                var match = MatchLayer(childLayer, serviceUrl, layerId);
                                if (match && childLayer.Name != layerName)
                                    childLayer.SetName(layerName);
                                childLayer.SetVisibility(match);
                            }
                        }
                    }
                    else
                    {
                        // force a refresh
                        //featureLayer.ClearDisplayCache();
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
                ArcGIS.Desktop.Framework.Dialogs.MessageBox.Show(e.ToString(), "MarkLogic", MessageBoxButton.OK, MessageBoxImage.Error);
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
