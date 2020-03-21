using MarkLogic.Esri.ArcGISPro.AddIn.Map;
using MarkLogic.Esri.ArcGISPro.AddIn.Messaging;
using System;

namespace MarkLogic.Esri.ArcGISPro.AddIn.ViewModels.Messages
{
    public class GetSymbologyMessage : Message
    {
        public GetSymbologyMessage(string valueName)
        {
            ValueName = valueName ?? throw new ArgumentNullException("valueName");
        }

        public string ValueName { get; private set; }

        public IPointSymbology Symbology { get; set; }
    }
}
