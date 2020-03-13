using ArcGIS.Core.CIM;
using ArcGIS.Core.Geometry;
using ArcGIS.Desktop.Framework.Threading.Tasks;
using ArcGIS.Desktop.Mapping;
using MarkLogic.Client.Search;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MarkLogic.Esri.ArcGISPro.AddIn.Map
{
    public class MapPointOverlay
    {
        private List<IDisposable> _overlayElements = new List<IDisposable>();
        private CIMSymbolReference _symbol = null;
        private CIMSymbolReference _focusSymbol = null;
        private IDisposable _focusElement = null;
        private MapPoint _focusPoint = null;

        public MapPointOverlay(MapView mapView)
        {
            MapView = mapView ?? throw new ArgumentNullException("mapView");
        }

        public MapView MapView { get; private set; }

        public void Clear(bool clearFocus = false)
        {
            foreach (var overlayElem in _overlayElements)
                overlayElem.Dispose();
            _overlayElements.Clear();
            if (_focusElement != null)
                _focusElement.Dispose();
            if (clearFocus)
                _focusPoint = null;
        }

        public async Task<bool> SetPoints(SearchResults results, string valuesName)
        {
            var mapView = MapView.Active;
            if (mapView == null) return false;

            Clear();

            await QueuedTask.Run(() =>
            {
                if (_symbol == null)
                    _symbol = SymbolFactory.Instance.ConstructPointSymbol(ColorFactory.Instance.RedRGB, 10.0, SimpleMarkerStyle.Circle).MakeSymbolReference();

                var _symbolCluster = SymbolFactory.Instance.ConstructPointSymbol(ColorFactory.Instance.BlueRGB, 20.0, SimpleMarkerStyle.Circle).MakeSymbolReference();

                foreach (var valuePoint in results.GetValuePoints(valuesName))
                {
                    var mapPoint = MapPointBuilder.CreateMapPoint(valuePoint.Longitude, valuePoint.Latitude, SpatialReferences.WGS84);
                    var overlayElem = MapView.AddOverlay(mapPoint, _symbol);
                    _overlayElements.Add(overlayElem);
                }

                foreach (var valuePointCluster in results.GetValuePointClusters(valuesName))
                {
                    var mapPoint = MapPointBuilder.CreateMapPoint(valuePointCluster.Longitude, valuePointCluster.Latitude, SpatialReferences.WGS84);
                    var overlayElem = MapView.AddOverlay(mapPoint, _symbolCluster);
                    _overlayElements.Add(overlayElem);
                }
            });

            /*if (_focusPoint != null)
            {
                _focusElement.Dispose();
                await SelectPoint(_focusPoint.X, _focusPoint.Y);
            }*/

            return true;
        }

        public async Task<bool> SelectPoint(double _long, double _lat)
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
        }
    }
}
