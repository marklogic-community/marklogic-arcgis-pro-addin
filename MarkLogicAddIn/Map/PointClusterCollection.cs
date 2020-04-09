using ArcGIS.Core.CIM;
using ArcGIS.Core.Geometry;
using ArcGIS.Desktop.Framework.Threading.Tasks;
using ArcGIS.Desktop.Mapping;
using MarkLogic.Client.Search;
using MarkLogic.Client.Search.Query;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace MarkLogic.Esri.ArcGISPro.AddIn.Map
{
    public class PointClusterCollection : OverlayCollection
    {
        private class Element
        {
            public ValuePointCluster Value { get; set; }

            public MapPoint Location { get; set; }

            public IDisposable PointOverlay { get; set; }

            public IDisposable TextOverlay { get; set; }

            public Envelope Hitbox { get; set; }
        }

        private List<Element> _elements = new List<Element>();
        private CIMPointSymbol _pointSymbol;
        private CIMSymbolReference _textSymbolRef;
        private double _pointSize;

        public PointClusterCollection(string valueName) : base(valueName)
        {
        }

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

        private Tuple<IDisposable, IDisposable, Envelope> AddOverlay(MapView mapView, MapPoint location, ValuePointCluster value, SpatialReference spatialRef)
        {
            var text = value.Count.ToString();
            var textGraphic = new CIMTextGraphic() { Text = text, Symbol = _textSymbolRef, Shape = location };

            var baseSize = _pointSize * 2; // smallest size (slightly bigger than for single points)
            var multiplier = 1 + ((text.Length - 1) * 0.5); // increase size by 0.5 for every text digit
            var size = baseSize * multiplier;
            _pointSymbol.SetSize(size);

            var pointElem = mapView.AddOverlay(location, _pointSymbol.MakeSymbolReference());
            var textElem = mapView.AddOverlay(textGraphic);
            var hitbox = CreateHitBox(mapView, location, size / 2, spatialRef);

            return new Tuple<IDisposable, IDisposable, Envelope>(pointElem, textElem, hitbox);
        }

        public async Task<bool> ApplyResults(MapView mapView, SearchResults results, IPointSymbology symbology, CancellationToken ctsToken)
        {
            Clear();

            if (ctsToken.IsCancellationRequested)
                return false;

            await QueuedTask.Run(() =>
            {
                InitSymbology(symbology);
                var spatialRef = SpatialReferences.WGS84; // TODO: this should be coming from response
                foreach (var pointCluster in results.GetValuePointClusters(ValueName))
                {
                    if (ctsToken.IsCancellationRequested)
                        return;

                    var location = MapPointBuilder.CreateMapPoint(pointCluster.Longitude, pointCluster.Latitude, spatialRef);
                    var addItems = AddOverlay(mapView, location, pointCluster, spatialRef);
                    _elements.Add(new Element() { Location = location, Value = pointCluster, PointOverlay = addItems.Item1, TextOverlay = addItems.Item2, Hitbox = addItems.Item3 });
                }
            });
            return _elements.Count > 0;
        }

        public async Task<bool> Redraw(MapView mapView, IPointSymbology symbology, CancellationToken ctsToken)
        {
            if (_elements.Count <= 0)
                return false;

            await QueuedTask.Run(() =>
            {
                InitSymbology(symbology);
                var spatialRef = SpatialReferences.WGS84; // TODO: this should be coming from response
                foreach (var element in _elements)
                {
                    if (ctsToken.IsCancellationRequested)
                        return;

                    element.PointOverlay.Dispose();
                    element.TextOverlay.Dispose();
                    var addItems = AddOverlay(mapView, element.Location, element.Value, spatialRef);
                    element.PointOverlay = addItems.Item1;
                    element.TextOverlay = addItems.Item2;
                    element.Hitbox = addItems.Item3;
                }
            });

            return true;
        }

        public bool TryGetValueExtent(MapPoint point, out GeospatialBox extent, out Envelope elementHitbox)
        {
            foreach (var elem in _elements)
            {
                if (GeometryEngine.Instance.Contains(elem.Hitbox, point))
                {
                    extent = new GeospatialBox() { West = elem.Value.West, South = elem.Value.South, East = elem.Value.East, North = elem.Value.North };
                    elementHitbox = elem.Hitbox;
                    return true;
                }
            }
            extent = null;
            elementHitbox = null;
            return false;
        }
    }
}
