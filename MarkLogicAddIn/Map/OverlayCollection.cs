using ArcGIS.Core.Geometry;
using ArcGIS.Desktop.Mapping;
using System;

namespace MarkLogic.Esri.ArcGISPro.AddIn.Map
{
    public abstract class OverlayCollection
    {
        protected OverlayCollection(string valueName)
        {
            ValueName = valueName ?? throw new ArgumentNullException("valueName");
        }

        public string ValueName { get; private set; }

        protected Envelope CreateHitBox(MapView mapView, MapPoint location, double sizeInPoints, SpatialReference spatialRef)
        {
            var sizeInPixels = Drawing.PixelsFromPoints(sizeInPoints);
            var refPoint = mapView.MapToClient(location);
            var minPoint = mapView.ClientToMap(new System.Windows.Point(refPoint.X - sizeInPixels, refPoint.Y + sizeInPixels)); // remember, screen Y axis starts from top to bottom
            var maxPoint = mapView.ClientToMap(new System.Windows.Point(refPoint.X + sizeInPixels, refPoint.Y - sizeInPixels));
            return EnvelopeBuilder.CreateEnvelope(minPoint, maxPoint, spatialRef);
        }
    }
}
