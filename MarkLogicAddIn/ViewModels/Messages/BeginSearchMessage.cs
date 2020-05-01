using MarkLogic.Client.Search;
using MarkLogic.Esri.ArcGISPro.AddIn.Messaging;

namespace MarkLogic.Esri.ArcGISPro.AddIn.ViewModels.Messages
{
    public class BeginSearchMessage : Message
    {
        public BeginSearchMessage(ReturnOptions returnOptions, bool isPaging = false)
        {
            ReturnOptions = returnOptions;
            IsPaging = isPaging;
        }

        public ReturnOptions ReturnOptions { get; private set; }

        public bool IsPaging { get; private set; }
    }
}
