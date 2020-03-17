using MarkLogic.Client.Search;
using MarkLogic.Esri.ArcGISPro.AddIn.Messaging;
using MarkLogic.Esri.ArcGISPro.AddIn.ViewModels.Messages;
using MarkLogic.Extensions.Koop;
using System;
using System.Windows.Input;

namespace MarkLogic.Esri.ArcGISPro.AddIn.ViewModels
{
    public class SearchQueryViewModel : ViewModelBase
    {
        public SearchQueryViewModel(MessageBus messageBus)
        {
            MessageBus = messageBus ?? throw new ArgumentNullException("messageBus");
            MessageBus.Subscribe<ConnectionProfileChangedMessage>(m => ConnectionProfile = m.Profile);
            MessageBus.Subscribe<ServiceModelChangedMessage>(m => ServiceModel = m.ServiceModel);
            MessageBus.Subscribe<BeginSearchMessage>(m =>
            {
                IsSearching = true;
                StatusMessage = "Running query...";
                m.Query.QueryText = QueryText;
            });
            MessageBus.Subscribe<EndSearchMessage>(m =>
            {
                StatusMessage = $"Returned {m.Results.Total} results.";
                IsSearching = false;
            });
        }

        protected MessageBus MessageBus { get; private set; }

        protected ConnectionProfile ConnectionProfile { get; private set; }

        protected IServiceModel ServiceModel { get; private set; }

        private bool _isSearching;
        public bool IsSearching
        {
            get { return _isSearching; }
            set { SetProperty(ref _isSearching, value); }
        }

        private string _statusMsg;
        public string StatusMessage
        {
            get { return _statusMsg; }
            set { SetProperty(ref _statusMsg, value); }
        }

        private string _queryText;
        public string QueryText
        {
            get { return _queryText; }
            set { SetProperty(ref _queryText, value); }
        }

        private SearchCommand _cmdSearch;
        public ICommand Search => _cmdSearch ?? (_cmdSearch = new SearchCommand(MessageBus, e => IsSearching = false));
    }
}
