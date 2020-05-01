using MarkLogic.Esri.ArcGISPro.AddIn.Messaging;
using System;

namespace MarkLogic.Esri.ArcGISPro.AddIn.ViewModels.Messages
{
    public class RedrawMessage : Message
    {
        public RedrawMessage(string valueName)
        {
            ValueName = valueName ?? throw new ArgumentNullException("valueName");
        }

        public string ValueName { get; private set; }
    }
}
