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
    public class MapOverlayManager
    {
        private List<IDisposable> _overlayElements = new List<IDisposable>();
        /*
        private CIMSymbolReference _focusSymbol = null;
        private IDisposable _focusElement = null;
        private MapPoint _focusPoint = null;
        */

        public MapOverlayManager(MapView mapView)
        {
            MapView = mapView ?? throw new ArgumentNullException("mapView");
        }

        public MapView MapView { get; private set; }

        public void Clear(bool clearFocus = false)
        {
            foreach (var overlayElem in _overlayElements)
                overlayElem.Dispose();
            _overlayElements.Clear();
            /*if (_focusElement != null)
                _focusElement.Dispose();
            if (clearFocus)
                _focusPoint = null;*/
        }

        // TODO: these should be exposed as user configurable options
        public double DefaultPointSize => 5.0;

        public CIMColor DefaultPointColor => ColorFactory.Instance.CreateRGBColor(255, 0, 0, 60);
        
        public SimpleMarkerStyle DefaultPointMarkerStyle => SimpleMarkerStyle.Circle;

        public async Task<bool> ApplyResults(SearchResults results)
        {
            var mapView = MapView.Active;
            if (mapView == null) return false;

            Clear();

            await QueuedTask.Run(() =>
            {
                var pointSize = DefaultPointSize;
                var pointColor = DefaultPointColor;
                var pointMarkerStyle = DefaultPointMarkerStyle;

                var pointSymbol = SymbolFactory.Instance.ConstructPointSymbol(pointColor, pointSize, pointMarkerStyle);
                (((pointSymbol.SymbolLayers[0] as CIMVectorMarker).MarkerGraphics[0].Symbol as CIMPolygonSymbol).SymbolLayers[0] as CIMSolidStroke).Width = 0;
                var pointSymbolRef = pointSymbol.MakeSymbolReference();

                var pointClusterSymbol = SymbolFactory.Instance.ConstructPointSymbol(pointColor, pointSize, pointMarkerStyle);
                (((pointClusterSymbol.SymbolLayers[0] as CIMVectorMarker).MarkerGraphics[0].Symbol as CIMPolygonSymbol).SymbolLayers[0] as CIMSolidStroke).Width = 0;

                var pointClusterTextSymbol = SymbolFactory.Instance.ConstructTextSymbol(ColorFactory.Instance.WhiteRGB, pointSize * 1.5, "Arial", "Bold");
                pointClusterTextSymbol.HorizontalAlignment = HorizontalAlignment.Center;
                pointClusterTextSymbol.VerticalAlignment = VerticalAlignment.Center;
                var pointClusterTextSymbolRef = pointClusterTextSymbol.MakeSymbolReference();

                var spatialRef = SpatialReferences.WGS84; // TODO: this should be coming from response

                foreach (var valueName in results.ValueNames)
                {
                    foreach (var point in results.GetValuePoints(valueName))
                    {
                        var mapPoint = MapPointBuilder.CreateMapPoint(point.Longitude, point.Latitude, spatialRef);
                        _overlayElements.Add(MapView.AddOverlay(mapPoint, pointSymbolRef));
                    }
                    
                    foreach (var pointCluster in results.GetValuePointClusters(valueName))
                    {
                        var text = pointCluster.Count.ToString();
                        var mapPoint = MapPointBuilder.CreateMapPoint(pointCluster.Longitude, pointCluster.Latitude, spatialRef);
                        var textGraphic = new CIMTextGraphic() { Text = text, Symbol = pointClusterTextSymbolRef, Shape = mapPoint };

                        var baseSize = pointSize * 2; // smallest size (slightly bigger than for single points)
                        var multiplier = 1 + ((text.Length - 1) * 0.5); // increase size by 0.5 for every text digit
                        pointClusterSymbol.SetSize(baseSize * multiplier);

                        _overlayElements.Add(MapView.AddOverlay(mapPoint, pointClusterSymbol.MakeSymbolReference()));
                        _overlayElements.Add(MapView.AddOverlay(textGraphic));

                        //_overlayElements.Add(MapView.AddOverlay(
                        //    EnvelopeBuilder.CreateEnvelope(pointCluster.West * 2.0, pointCluster.South * 2.0, pointCluster.East * 2.0, pointCluster.North * 2.0, spatialRef), 
                        //    extentSymbolRef));
                    }
                }
            });

            /*if (_focusPoint != null)
            {
                _focusElement.Dispose();
                await SelectPoint(_focusPoint.X, _focusPoint.Y);
            }*/

            return true;
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
