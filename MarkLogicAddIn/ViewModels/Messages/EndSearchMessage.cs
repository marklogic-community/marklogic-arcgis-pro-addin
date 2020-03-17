using MarkLogic.Client.Search;
using MarkLogic.Esri.ArcGISPro.AddIn.Messaging;
using System;

namespace MarkLogic.Esri.ArcGISPro.AddIn.ViewModels.Messages
{
    public class EndSearchMessage : Message
    {
        public EndSearchMessage(SearchResults results)
        {
            Results = results ?? throw new ArgumentNullException("results");
        }

        public SearchResults Results { get; private set; }
    }
}
