using ArcGIS.Desktop.Mapping;
using MarkLogic.Esri.ArcGISPro.AddIn.Messaging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media;

namespace MarkLogic.Esri.ArcGISPro.AddIn.ViewModels.Messages
{
    public class GetSymbologyMessage : Message
    {
        public GetSymbologyMessage(string valueName)
        {
            ValueName = valueName ?? throw new ArgumentNullException("valueName");
        }

        public string ValueName { get; private set; }

        public Color Color { get; set; }

        public SimpleMarkerStyle Shape { get; set; }

        public double Size { get; set; }

        public int Opacity { get; set; }
    }
}
