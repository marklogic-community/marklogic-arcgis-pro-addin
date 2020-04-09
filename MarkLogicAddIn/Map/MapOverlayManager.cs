using ArcGIS.Core.Geometry;
using ArcGIS.Desktop.Mapping;
using MarkLogic.Client.Search;
using MarkLogic.Client.Search.Query;
using MarkLogic.Esri.ArcGISPro.AddIn.Messaging;
using MarkLogic.Esri.ArcGISPro.AddIn.ViewModels.Messages;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading;
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

        private readonly Dictionary<string, OverlayGroup> _overlayGroupMap = new Dictionary<string, OverlayGroup>();
        private readonly object _overlayGroupMapLock = new object();
        private CancellationTokenSource _cts = new CancellationTokenSource();
        private readonly object _ctsLock = new object();
        private readonly SelectorOverlay _selector = new SelectorOverlay();
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
            MessageBus.Subscribe<SelectMapLocationMessage>(async m =>
            {
                var extent = await HitTest(m.Location);
                if (extent != null)
                    m.Extent = extent;
                m.Resolved = true; // resolve even when nothing hits, i.e. deselect
            });
        }

        private MessageBus MessageBus { get; set; }

        private void CancelRender()
        {
            lock (_ctsLock)
            {
                _cts?.Cancel();
            }
        }

        private CancellationToken GetCancelRenderToken()
        {
            lock(_ctsLock)
            {
                _cts = new CancellationTokenSource();
                return _cts.Token;
            }
        }

        public void Clear()
        {
            Debug.WriteLine("MapOverlayManager: Clear");
            CancelRender();
            Monitor.Enter(_overlayGroupMapLock);
            try
            { 
                foreach (var group in _overlayGroupMap.Values)
                {
                    group.Points.Clear();
                    group.PointClusters.Clear();
                }
                _overlayGroupMap.Clear();
                _selector.Clear();
            }
            finally
            {
                Monitor.Exit(_overlayGroupMapLock);
            }
        }

        public async Task<bool> ApplyResults(SearchResults results)
        { 
            Debug.WriteLine("MapOverlayManager: ApplyResults");

            var mapView = MapView.Active;
            if (mapView == null)
                return false;

            var ctsToken = GetCancelRenderToken();
            Monitor.Enter(_overlayGroupMapLock);
            try
            {
                var tasks = new List<Task<bool>>();
                foreach (var valueName in results.ValueNames)
                {
                    if (ctsToken.IsCancellationRequested)
                        return false;

                    var msg = await MessageBus.Publish(new GetSymbologyMessage(valueName));
                    Debug.Assert(msg.Resolved && msg.Symbology != null);

                    if (ctsToken.IsCancellationRequested)
                        return false;

                    var group = new OverlayGroup(valueName);
                    tasks.Add(group.Points.ApplyResults(mapView, results, msg.Symbology, ctsToken));
                    tasks.Add(group.PointClusters.ApplyResults(mapView, results, msg.Symbology, ctsToken));
                    _overlayGroupMap.Add(valueName, group);
                }
                var retVals = await Task.WhenAll(tasks);
                return retVals.All(r => r == true);
            }
            finally
            {
                Monitor.Exit(_overlayGroupMapLock);
                lock (_ctsLock)
                {
                    _cts.Dispose();
                    _cts = null;
                }
            }
        }

        public async Task<bool> Redraw(string valueName)
        {
            Debug.WriteLine("MapOverlayManager: Redraw");

            var mapView = MapView.Active;
            if (mapView == null) 
                return false;

            var ctsToken = GetCancelRenderToken();
            Monitor.Enter(_overlayGroupMapLock);
            try
            {
                var msg = await MessageBus.Publish(new GetSymbologyMessage(valueName));
                Debug.Assert(msg.Resolved && msg.Symbology != null);

                if (ctsToken.IsCancellationRequested)
                    return false;

                var hasGroup = _overlayGroupMap.TryGetValue(valueName, out OverlayGroup group);
                Debug.Assert(hasGroup);
                if (!hasGroup)
                    return false;

                if (ctsToken.IsCancellationRequested)
                    return false;

                var pointsTask = group.Points.Redraw(mapView, msg.Symbology, ctsToken);
                var pointClustersTask = group.PointClusters.Redraw(mapView, msg.Symbology, ctsToken);
                var retVals = await Task.WhenAll(pointsTask, pointClustersTask);
                return retVals.All(r => r == true);
            }
            finally
            {
                Monitor.Exit(_overlayGroupMapLock);
                lock (_ctsLock)
                {
                    _cts.Dispose();
                    _cts = null;
                }
            }
        }

        public async Task<GeospatialBox> HitTest(MapPoint location)
        {
            var mapView = MapView.Active;
            if (mapView == null)
                return null;

            _selector.Clear();
            foreach (var group in _overlayGroupMap.Values.Reverse()) // start from the last geo constraint added
            {
                GeospatialBox extent;
                Envelope elementHitbox;
                var hit = group.PointClusters.TryGetValueExtent(location, out extent, out elementHitbox);
                if (!hit)
                    hit = group.Points.TryGetValueExtent(location, out extent, out elementHitbox);

                if (hit)
                {
                    await _selector.Select(mapView, elementHitbox);
                    return extent;
                }
            }

            return null; // no hits
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
    }
}
