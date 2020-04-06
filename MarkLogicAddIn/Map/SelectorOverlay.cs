using ArcGIS.Core.Geometry;
using ArcGIS.Desktop.Framework.Threading.Tasks;
using ArcGIS.Desktop.Mapping;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media;

namespace MarkLogic.Esri.ArcGISPro.AddIn.Map
{
    public class SelectorOverlay
    {
        private IDisposable _overlay;

        public SelectorOverlay()
        {
            Color = Colors.Black;
            LineWidth = 1.5;
        }

        public Color Color { get; set; }

        public double LineWidth { get; set; }

        public Task<bool> Select(MapView mapView, Envelope box)
        {
            if (mapView == null)
                return Task.FromResult(false);

            _overlay?.Dispose();
            if (box == null)
                return Task.FromResult(false); ;

            return QueuedTask.Run(() =>
            {
                var boxColor = ColorFactory.Instance.CreateRGBColor(Color.R, Color.G, Color.B);
                var polygon = PolygonBuilder.CreatePolygon(box);
                var symbol = SymbolFactory.Instance.ConstructPolygonSymbol(boxColor, SimpleFillStyle.Null, SymbolFactory.Instance.ConstructStroke(boxColor, LineWidth, SimpleLineStyle.Solid));

                _overlay = mapView.AddOverlay(polygon, symbol.MakeSymbolReference());
                return true;
            });
        }

        public void Clear()
        {
            _overlay?.Dispose();
        }
    }
}
