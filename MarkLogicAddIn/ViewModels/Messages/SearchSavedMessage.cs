using MarkLogic.Client.Search;
using MarkLogic.Esri.ArcGISPro.AddIn.Messaging;

namespace MarkLogic.Esri.ArcGISPro.AddIn.ViewModels.Messages
{
    public class SearchSavedMessage : Message
    {
        public SearchSavedMessage(SaveSearchResults results)
        {
            Results = results;
        }

        public SaveSearchResults Results { get; private set; }
    }
}
