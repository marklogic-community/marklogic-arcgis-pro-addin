using ArcGIS.Desktop.Mapping;
using MarkLogic.Client.Search;
using MarkLogic.Esri.ArcGISPro.AddIn.Messaging;
using MarkLogic.Esri.ArcGISPro.AddIn.ViewModels.Messages;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;

#if DEBUG
using System.Windows.Controls;
using System.Windows.Media;
#endif

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
#if DEBUG
        private MapViewOverlayControl _mapCtlPerf;
        private TextBlock _ctlPerf;
        private TimeSpan _perfClear;
#endif

        public MapOverlayManager(MessageBus messageBus)
        {
            MessageBus = messageBus ?? throw new ArgumentNullException("messageBus");
            MessageBus.Subscribe<BeginSearchMessage>(m =>
            {
                if (m.ReturnOptions.HasFlag(ReturnOptions.Values))
                {
#if DEBUG
                    var watch = Stopwatch.StartNew();
#endif
                    Clear();
#if DEBUG
                    watch.Stop();
                    _perfClear = watch.Elapsed;
#endif
                }
            });
            MessageBus.Subscribe<EndSearchMessage>(async m =>
            {
                if (m.Results.ReturnOptions.HasFlag(ReturnOptions.Values))
                {
#if DEBUG
                    var watch = Stopwatch.StartNew();
#endif
                    await ApplyResults(m.Results);
#if DEBUG
                    watch.Stop();
                    UpdatePerfOverlay(_perfClear, watch.Elapsed, false);
#endif
                }
            });
            MessageBus.Subscribe<RedrawMessage>(async m =>
            {
#if DEBUG
                var watch = Stopwatch.StartNew();
#endif
                await Redraw(m.ValueName);
#if DEBUG
                watch.Stop();
                UpdatePerfOverlay(_perfClear, watch.Elapsed, true);
#endif
            });
            MessageBus.Subscribe<SearchSavedMessage>(m => Clear());
        }

        private MessageBus MessageBus { get; set; }

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

#if DEBUG
        private void UpdatePerfOverlay(TimeSpan perfClear, TimeSpan perfApply, bool isRedraw)
        {
            if (MapView.Active == null)
                return;
            if (_mapCtlPerf == null && _ctlPerf == null)
            {
                _ctlPerf = new TextBlock();
                _ctlPerf.Background = Brushes.Transparent;
                _ctlPerf.TextWrapping = System.Windows.TextWrapping.Wrap;
                _mapCtlPerf = new MapViewOverlayControl(_ctlPerf, true, true, true, OverlayControlRelativePosition.BottomLeft);
                MapView.Active.AddOverlayControl(_mapCtlPerf);
            }

            _ctlPerf.Text = $"Overlays cleared: {perfClear.ToString("mm':'ss':'fff")}\n{(isRedraw ? "Overlay redrawn" : "New overlays added")}: {perfApply.ToString("mm':'ss':'fff")}";
        }
#endif

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
