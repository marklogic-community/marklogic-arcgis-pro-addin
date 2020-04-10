using MarkLogic.Client.Search;
using MarkLogic.Esri.ArcGISPro.AddIn.Messaging;
using MarkLogic.Extensions.Koop;

namespace MarkLogic.Esri.ArcGISPro.AddIn.ViewModels.Messages
{
    public class SearchSavedMessage : Message
    {
        public SearchSavedMessage(SaveSearchResults results, ServiceModel serviceModel)
        {
            Results = results;
            ServiceModel = serviceModel;
        }

        public SaveSearchResults Results { get; private set; }

        public ServiceModel ServiceModel { get; private set; }
    }
}
