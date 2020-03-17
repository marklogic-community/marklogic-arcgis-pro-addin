using MarkLogic.Client.Search;
using MarkLogic.Esri.ArcGISPro.AddIn.Messaging;
using System;

namespace MarkLogic.Esri.ArcGISPro.AddIn.ViewModels.Messages
{
    public class BeginSearchMessage : Message
    {
        public BeginSearchMessage(SearchQuery query)
        {
            Query = query ?? throw new ArgumentNullException("query");
        }

        public SearchQuery Query { get; private set; }
    }
}
