using ArcGIS.Core.CIM;
using ArcGIS.Core.Geometry;
using ArcGIS.Desktop.Framework.Threading.Tasks;
using ArcGIS.Desktop.Mapping;
using MarkLogic.Client.Search;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace MarkLogic.Esri.ArcGISPro.AddIn.Map
{
    public class PointClusterCollection
    {
        private class Element
        {
            public ValuePointCluster Value { get; set; }

            public MapPoint Location { get; set; }

            public IDisposable PointOverlay { get; set; }

            public IDisposable TextOverlay { get; set; }

            public void Dispose()
            {
                PointOverlay.Dispose();
                TextOverlay.Dispose();
            }
        }

        private List<Element> _elements = new List<Element>();
        private CIMPointSymbol _pointSymbol;
        private CIMSymbolReference _textSymbolRef;
        private double _pointSize;

        public PointClusterCollection(string valueName)
        {
            ValueName = valueName ?? throw new ArgumentNullException("valueName");
        }

        public string ValueName { get; private set; }

        public void Clear()
        {
            _elements.ForEach(e =>
            {
                e.PointOverlay.Dispose();
                e.TextOverlay.Dispose();
            });
            _elements.Clear();
        }

        private void InitSymbology(IPointSymbology symbology)
        {
            _pointSize = symbology.Size;

            var pointColor = ColorFactory.Instance.CreateRGBColor(symbology.Color.R, symbology.Color.G, symbology.Color.B, symbology.Opacity);
            _pointSymbol = SymbolFactory.Instance.ConstructPointSymbol(pointColor, symbology.Size, symbology.Shape);
            (((_pointSymbol.SymbolLayers[0] as CIMVectorMarker).MarkerGraphics[0].Symbol as CIMPolygonSymbol).SymbolLayers[0] as CIMSolidStroke).Width = 0;

            var textSymbol = SymbolFactory.Instance.ConstructTextSymbol(ColorFactory.Instance.WhiteRGB, symbology.Size * 1.5, "Arial", "Bold");
            textSymbol.HorizontalAlignment = HorizontalAlignment.Center;
            textSymbol.VerticalAlignment = VerticalAlignment.Center;
            _textSymbolRef = textSymbol.MakeSymbolReference();
        }

        private Tuple<IDisposable, IDisposable> AddOverlay(MapView mapView, MapPoint location, ValuePointCluster value)
        {
            var text = value.Count.ToString();
            var textGraphic = new CIMTextGraphic() { Text = text, Symbol = _textSymbolRef, Shape = location };

            var baseSize = _pointSize * 2; // smallest size (slightly bigger than for single points)
            var multiplier = 1 + ((text.Length - 1) * 0.5); // increase size by 0.5 for every text digit
            _pointSymbol.SetSize(baseSize * multiplier);

            var pointElem = mapView.AddOverlay(location, _pointSymbol.MakeSymbolReference());
            var textElem = mapView.AddOverlay(textGraphic);

            return new Tuple<IDisposable, IDisposable>(pointElem, textElem);
        }

        public async Task<bool> ApplyResults(MapView mapView, SearchResults results, IPointSymbology symbology)
        {
            Clear();
            await QueuedTask.Run(() =>
            {
                InitSymbology(symbology);
                var spatialRef = SpatialReferences.WGS84; // TODO: this should be coming from response
                foreach (var pointCluster in results.GetValuePointClusters(ValueName))
                {
                    var location = MapPointBuilder.CreateMapPoint(pointCluster.Longitude, pointCluster.Latitude, spatialRef);
                    var overlays = AddOverlay(mapView, location, pointCluster);
                    _elements.Add(new Element() { Location = location, Value = pointCluster, PointOverlay = overlays.Item1, TextOverlay = overlays.Item2 });
                }
            });
            return _elements.Count > 0;
        }

        public async Task<bool> Redraw(MapView mapView, IPointSymbology symbology)
        {
            if (_elements.Count <= 0)
                return false;
            await QueuedTask.Run(() =>
            {
                InitSymbology(symbology);
                var spatialRef = SpatialReferences.WGS84; // TODO: this should be coming from response
                foreach (var element in _elements)
                {
                    element.PointOverlay.Dispose();
                    element.TextOverlay.Dispose();
                    var overlays = AddOverlay(mapView, element.Location, element.Value);
                    element.PointOverlay = overlays.Item1;
                    element.TextOverlay = overlays.Item2;
                }
            });
            return true;
        }
    }
}
