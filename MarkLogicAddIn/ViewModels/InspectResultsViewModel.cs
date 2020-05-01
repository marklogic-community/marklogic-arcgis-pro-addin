using MarkLogic.Client.Search;
using MarkLogic.Client.Search.Query;
using MarkLogic.Esri.ArcGISPro.AddIn.Commands;
using MarkLogic.Esri.ArcGISPro.AddIn.Messaging;
using MarkLogic.Esri.ArcGISPro.AddIn.ViewModels.Messages;
using System.Windows.Input;

namespace MarkLogic.Esri.ArcGISPro.AddIn.ViewModels
{
    public class InspectResultsViewModel : ViewModelBase
    {
        private SearchResults _originalResults;

        public InspectResultsViewModel(MessageBus messageBus, GeospatialBox extent)
        {
            // create an isolated message bus
            InnerMessageBus = new MessageBus();
            InnerMessageBus.Subscribe<BuildSearchMessage>(async m =>
            {
                var query = new SearchQuery();
                await messageBus.Publish(new BuildSearchMessage(query)); // get current global query
                query.Viewport = extent;

                var start = m.Query.Start;
                m.Query.Apply(query);
                m.Query.Start = start;
            });
            InnerMessageBus.Subscribe<EndSearchMessage>(m =>
            {
                _originalResults = _originalResults ?? m.Results;
                NotifyPropertyChanged(nameof(Total));
            });

            // relay certain messages to outer bus so they can be resolved
            InnerMessageBus.Subscribe<GetServerSettingsMessage>(m => messageBus.Publish(m));

            FacetsViewModel = new SearchFacetsViewModel(InnerMessageBus);
            ResultsViewModel = new SearchResultsViewModel(InnerMessageBus);
            DocumentViewModel = new DocumentViewModel(InnerMessageBus);
        }

        private SearchCommand _cmdSearch;
        public ICommand Search => _cmdSearch ?? (_cmdSearch = new SearchCommand(InnerMessageBus, ReturnOptions.Results | ReturnOptions.Facets));

        protected MessageBus InnerMessageBus { get; private set; }

        public SearchFacetsViewModel FacetsViewModel { get; private set; }

        public SearchResultsViewModel ResultsViewModel { get; private set; }

        public DocumentViewModel DocumentViewModel { get; private set; }

        public long Total => _originalResults != null ? _originalResults.Total : 0;
    }
}
