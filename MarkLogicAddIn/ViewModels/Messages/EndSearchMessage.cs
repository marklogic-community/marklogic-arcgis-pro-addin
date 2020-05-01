using MarkLogic.Client.Search;
using MarkLogic.Esri.ArcGISPro.AddIn.Messaging;
using System;

namespace MarkLogic.Esri.ArcGISPro.AddIn.ViewModels.Messages
{
    public class EndSearchMessage : Message
    {
        public EndSearchMessage(SearchResults results, bool isPaging = false)
        {
            Results = results ?? throw new ArgumentNullException("results");
            IsPaging = isPaging;
        }

        public SearchResults Results { get; private set; }

        public bool IsPaging { get; private set; }
    }
}
