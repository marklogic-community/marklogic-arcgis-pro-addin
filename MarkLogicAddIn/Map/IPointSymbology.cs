using ArcGIS.Desktop.Mapping;
using System;
using System.Windows.Media;

namespace MarkLogic.Esri.ArcGISPro.AddIn.Map
{
    public interface IPointSymbology : IEquatable<IPointSymbology>
    {
        Color Color { get; set; }

        SimpleMarkerStyle Shape { get; set; }

        double Size { get; set; }

        int Opacity { get; set; }
    }
}
