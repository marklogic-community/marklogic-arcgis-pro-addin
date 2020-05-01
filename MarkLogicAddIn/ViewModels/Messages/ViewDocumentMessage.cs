using MarkLogic.Esri.ArcGISPro.AddIn.Messaging;

namespace MarkLogic.Esri.ArcGISPro.AddIn.ViewModels.Messages
{
    public class ViewDocumentMessage : Message
    {
        public ViewDocumentMessage(string documentUri)
        {
            DocumentUri = documentUri;
        }

        public string DocumentUri { get; private set; }
    }
}
