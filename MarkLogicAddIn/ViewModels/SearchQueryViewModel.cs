using MarkLogic.Client.Search;
using MarkLogic.Esri.ArcGISPro.AddIn.Commands;
using MarkLogic.Esri.ArcGISPro.AddIn.Messaging;
using MarkLogic.Esri.ArcGISPro.AddIn.ViewModels.Messages;
using System;
using System.Collections.ObjectModel;
using System.Windows.Input;

namespace MarkLogic.Esri.ArcGISPro.AddIn.ViewModels
{
    public class SearchQueryViewModel : ViewModelBase
    {
        public SearchQueryViewModel(MessageBus messageBus)
        {
            MessageBus = messageBus ?? throw new ArgumentNullException("messageBus");
            MessageBus.Subscribe<ServerSettingsChangedMessage>(m =>
            {
                if (m.ServiceModel == null)
                {
                    QueryText = "";
                    StatusMessage = "";
                }
            });
            MessageBus.Subscribe<BuildSearchMessage>(m =>
            {
                m.Query.QueryText = QueryText;
            });
            MessageBus.Subscribe<BeginSearchMessage>(m =>
            {
                if (m.ReturnOptions.HasFlag(ReturnOptions.Results) && !m.IsPaging)
                {
                    StatusMessage = "Searching...";
                    IsSearching = true;
                }
                if (m.ReturnOptions.HasFlag(ReturnOptions.Suggest))
                    Suggestions.Clear();

            });
            MessageBus.Subscribe<EndSearchMessage>(m =>
            {
                if (m.Results.ReturnOptions.HasFlag(ReturnOptions.Results) && !m.IsPaging)
                {
                    StatusMessage = $"Matched {m.Results.Total,1:n0} documents and {m.Results.TotalObjects,1:n0} distinct objects.";
                    IsSearching = false;
                }
                if (m.Results.ReturnOptions.HasFlag(ReturnOptions.Suggest))
                {
                    foreach (var suggestion in m.Results.QuerySuggestions)
                        Suggestions.Add(suggestion);
                }
            });
            MessageBus.Subscribe<SearchAbortedMessage>(m =>
            {
                IsSearching = false;
                StatusMessage = "Search aborted.";
            });
        }

        protected MessageBus MessageBus { get; private set; }

        private bool _isSearching;
        public bool IsSearching
        {
            get { return _isSearching; }
            set 
            { 
                SetProperty(ref _isSearching, value);
                NotifyPropertyChanged(nameof(NotSearching));
            }
        }

        public bool NotSearching => !IsSearching;

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
        public ICommand Search => _cmdSearch ?? (_cmdSearch = new SearchCommand(MessageBus));

        public ObservableCollection<string> Suggestions { get; } = new ObservableCollection<string>();

        private SearchCommand _cmdSuggest;
        public ICommand Suggest => _cmdSuggest ?? (_cmdSuggest = new SearchCommand(MessageBus, ReturnOptions.Suggest) { RequeryOnExecute = false });
    }
}
