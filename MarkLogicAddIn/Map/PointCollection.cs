using ArcGIS.Core.CIM;
using ArcGIS.Core.Geometry;
using ArcGIS.Desktop.Framework.Threading.Tasks;
using ArcGIS.Desktop.Mapping;
using MarkLogic.Client.Search;
using MarkLogic.Client.Search.Query;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace MarkLogic.Esri.ArcGISPro.AddIn.Map
{
    public class PointCollection : OverlayCollection
    {
        private class Element
        {
            public ValuePoint Value { get; set; }

            public MapPoint Location { get; set; }

            public IDisposable Overlay { get; set; }

            public Envelope Hitbox { get; set; }
        }

        private List<Element> _elements = new List<Element>();
        private CIMSymbolReference _symbolRef;

        public PointCollection(string valueName) : base(valueName)
        {
        }

        public void Clear()
        {
            _elements.ForEach(e => e.Overlay.Dispose());
            _elements.Clear();
        }

        private void InitSymbology(IPointSymbology symbology)
        {
            var pointColor = ColorFactory.Instance.CreateRGBColor(symbology.Color.R, symbology.Color.G, symbology.Color.B, symbology.Opacity);
            var pointSymbol = SymbolFactory.Instance.ConstructPointSymbol(pointColor, symbology.Size, symbology.Shape);
            (((pointSymbol.SymbolLayers[0] as CIMVectorMarker).MarkerGraphics[0].Symbol as CIMPolygonSymbol).SymbolLayers[0] as CIMSolidStroke).Width = 0;
            _symbolRef = pointSymbol.MakeSymbolReference();
        }

        public async Task<bool> ApplyResults(MapView mapView, SearchResults results, IPointSymbology symbology)
        {
            Clear();
            await QueuedTask.Run(() =>
            {
                InitSymbology(symbology);
                var spatialRef = SpatialReferences.WGS84; // TODO: this should be coming from response
                foreach (var point in results.GetValuePoints(ValueName))
                {
                    var location = MapPointBuilder.CreateMapPoint(point.Longitude, point.Latitude, spatialRef);
                    var overlay = mapView.AddOverlay(location, _symbolRef);
                    _elements.Add(new Element() { Value = point, Location = location, Overlay = overlay, Hitbox = CreateHitBox(mapView, location, symbology.Size / 2, spatialRef) });
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
                    element.Overlay.Dispose();
                    element.Overlay = mapView.AddOverlay(element.Location, _symbolRef);
                    element.Hitbox = CreateHitBox(mapView, element.Location, symbology.Size / 2, spatialRef);
                }
            });
            return true;
        }

        public bool TryGetValueExtent(MapPoint point, out GeospatialBox extent, out Envelope elementHitbox)
        {
            // TODO: is there a better way to wrap around a point?
            var xOffset = 0.000004;
            var yOffset = 0.000008;

            foreach (var elem in _elements)
            {
                if (GeometryEngine.Instance.Contains(elem.Hitbox, point))
                {
                    extent = new GeospatialBox() 
                    { 
                        West = elem.Location.X - xOffset, 
                        South = elem.Location.Y - yOffset, 
                        East = elem.Location.X + xOffset, 
                        North = elem.Location.Y + yOffset 
                    };
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
