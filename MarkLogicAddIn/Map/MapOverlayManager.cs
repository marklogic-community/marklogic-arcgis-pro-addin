using ArcGIS.Core.CIM;
using ArcGIS.Core.Geometry;
using ArcGIS.Desktop.Framework.Threading.Tasks;
using ArcGIS.Desktop.Mapping;
using MarkLogic.Client.Search;
using MarkLogic.Client.Search.Query;
using MarkLogic.Esri.ArcGISPro.AddIn.Messaging;
using MarkLogic.Esri.ArcGISPro.AddIn.ViewModels.Messages;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;

namespace MarkLogic.Esri.ArcGISPro.AddIn.Map
{
    public class MapOverlayManager
    {
        private class OverlayGroup
        {
            public OverlayGroup(string valueName)
            {
                Points = new PointCollection(valueName);
                PointClusters = new PointClusterCollection(valueName);
            }

            public PointCollection Points { get; set; }

            public PointClusterCollection PointClusters { get; set; }
        }

        private Dictionary<string, OverlayGroup> _overlayGroupMap = new Dictionary<string, OverlayGroup>();
        
        public MapOverlayManager(MessageBus messageBus)
        {
            MessageBus = messageBus ?? throw new ArgumentNullException("messageBus");
            MessageBus.Subscribe<BeginSearchMessage>(async m => 
            {
                Clear();
                m.Query.Viewport = await GetCurrentViewport();
            });
            MessageBus.Subscribe<EndSearchMessage>(async m => {
                await ApplyResults(m.Results);
            });
            MessageBus.Subscribe<RedrawMessage>(async m =>
            {
                await Redraw(m.ValueName);
            });
        }

        private MessageBus MessageBus { get; set; }

        private Task<GeospatialBox> GetCurrentViewport()
        {
            Debug.Assert(MapView.Active != null);
            return QueuedTask.Run(() =>
            {
                var extent = MapView.Active.Extent;
                var extentEnvelope = EnvelopeBuilder.CreateEnvelope(extent.XMin, extent.YMin, extent.XMax, extent.YMax, extent.SpatialReference);
                ProjectionTransformation pxForm = ProjectionTransformation.Create(extent.SpatialReference, SpatialReferences.WGS84);
                var wgsEnvelope = GeometryEngine.Instance.ProjectEx(extentEnvelope, pxForm) as Envelope;

                return new GeospatialBox()
                {
                    North = Math.Min(wgsEnvelope.YMax, 90.0), // north up to +90 deg
                    South = Math.Max(wgsEnvelope.YMin, -90.0), // south up to -90 deg
                    West = Math.Max(wgsEnvelope.XMin, -180.0), // west up to -180 deg
                    East = Math.Min(wgsEnvelope.XMax, 180.0) // east up to +180 deg
                };
            });
        }

        public void Clear()
        {
            foreach(var group in _overlayGroupMap.Values)
            {
                group.Points.Clear();
                group.PointClusters.Clear();
            }
            _overlayGroupMap.Clear();
        }

        public async Task<bool> ApplyResults(SearchResults results)
        {
            var mapView = MapView.Active;
            if (mapView == null)
                return false;

            var tasks = new List<Task<bool>>();
            foreach(var valueName in results.ValueNames)
            {
                var msg = new GetSymbologyMessage(valueName);
                await MessageBus.Publish(msg);
                Debug.Assert(msg.Resolved && msg.Symbology != null);

                var group = new OverlayGroup(valueName);
                tasks.Add(group.Points.ApplyResults(mapView, results, msg.Symbology));
                tasks.Add(group.PointClusters.ApplyResults(mapView, results, msg.Symbology));
                _overlayGroupMap.Add(valueName, group);
            }
            var retVals = await Task.WhenAll(tasks);
            return retVals.All(r => r == true);
        }

        public async Task<bool> Redraw(string valueName)
        {
            var mapView = MapView.Active;
            if (mapView == null) 
                return false;

            var msg = new GetSymbologyMessage(valueName);
            await MessageBus.Publish(msg);
            Debug.Assert(msg.Resolved && msg.Symbology != null);
            
            var hasGroup = _overlayGroupMap.TryGetValue(valueName, out OverlayGroup group);
            Debug.Assert(hasGroup);
            if (!hasGroup)
                return false;

            var pointsTask = group.Points.Redraw(mapView, msg.Symbology);
            var pointClustersTask = group.PointClusters.Redraw(mapView, msg.Symbology);
            var retVals = await Task.WhenAll(pointsTask, pointClustersTask);
            return retVals.All(r => r == true);
        }

        /*public async Task<bool> SelectPoint(double _long, double _lat)
        {
            var mapView = MapView.Active;
            if (mapView == null) return false;
            
            await QueuedTask.Run(() => 
            {
                if (_focusSymbol == null)
                    _focusSymbol = SymbolFactory.Instance.ConstructPointSymbol(ColorFactory.Instance.BlueRGB, 12.0, SimpleMarkerStyle.Circle).MakeSymbolReference();

                if (_focusElement != null)
                    _focusElement.Dispose();

                _focusPoint = MapPointBuilder.CreateMapPoint(_long, _lat, SpatialReferences.WGS84);
                _focusElement = this.MapView.AddOverlay(_focusPoint, _focusSymbol);
                return _focusPoint;
            });

            return true;
        }*/
    }
}
