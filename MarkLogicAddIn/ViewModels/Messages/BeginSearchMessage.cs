using MarkLogic.Client.Search;
using MarkLogic.Esri.ArcGISPro.AddIn.Messaging;

namespace MarkLogic.Esri.ArcGISPro.AddIn.ViewModels.Messages
{
    public class BeginSearchMessage : Message
    {
        public BeginSearchMessage(ReturnOptions returnOptions)
        {
            ReturnOptions = returnOptions;
        }

        public ReturnOptions ReturnOptions { get; private set; }
    }
}
