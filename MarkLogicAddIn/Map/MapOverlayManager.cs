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
using System.Threading.Tasks;

namespace MarkLogic.Esri.ArcGISPro.AddIn.Map
{
    public class MapOverlayManager
    {
        private List<IDisposable> _overlayElements = new List<IDisposable>();

        public MapOverlayManager(MessageBus messageBus)
        {
            MessageBus = messageBus ?? throw new ArgumentNullException("messageBus");
            MessageBus.Subscribe<BeginSearchMessage>(async m => 
            {
                Clear();
                m.Query.Viewport = await GetCurrentViewport();
            });
            MessageBus.Subscribe<EndSearchMessage>(m => ApplyResults(m.Results));
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
            foreach (var overlayElem in _overlayElements)
                overlayElem.Dispose();
            _overlayElements.Clear();
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
                        _overlayElements.Add(mapView.AddOverlay(mapPoint, pointSymbolRef));
                    }
                    
                    foreach (var pointCluster in results.GetValuePointClusters(valueName))
                    {
                        var text = pointCluster.Count.ToString();
                        var mapPoint = MapPointBuilder.CreateMapPoint(pointCluster.Longitude, pointCluster.Latitude, spatialRef);
                        var textGraphic = new CIMTextGraphic() { Text = text, Symbol = pointClusterTextSymbolRef, Shape = mapPoint };

                        var baseSize = pointSize * 2; // smallest size (slightly bigger than for single points)
                        var multiplier = 1 + ((text.Length - 1) * 0.5); // increase size by 0.5 for every text digit
                        pointClusterSymbol.SetSize(baseSize * multiplier);

                        _overlayElements.Add(mapView.AddOverlay(mapPoint, pointClusterSymbol.MakeSymbolReference()));
                        _overlayElements.Add(mapView.AddOverlay(textGraphic));

                        //_overlayElements.Add(MapView.AddOverlay(
                        //    EnvelopeBuilder.CreateEnvelope(pointCluster.West * 2.0, pointCluster.South * 2.0, pointCluster.East * 2.0, pointCluster.North * 2.0, spatialRef), 
                        //    extentSymbolRef));
                    }
                }
            });

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
