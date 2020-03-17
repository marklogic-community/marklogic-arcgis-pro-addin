using MarkLogic.Esri.ArcGISPro.AddIn.Messaging;
using MarkLogic.Esri.ArcGISPro.AddIn.ViewModels.Messages;
using System;

namespace MarkLogic.Esri.ArcGISPro.AddIn.ViewModels
{
    public class SearchFacetsViewModel : ViewModelBase
    {
        public SearchFacetsViewModel(MessageBus messageBus)
        {
            MessageBus = messageBus ?? throw new ArgumentNullException("messageBus");
            MessageBus.Subscribe<BeginSearchMessage>(m => 
            {
            });
            MessageBus.Subscribe<EndSearchMessage>(m =>
            {
            });
        }

        protected MessageBus MessageBus { get; private set; }
    }
}
