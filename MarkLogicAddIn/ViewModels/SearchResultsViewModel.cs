using MarkLogic.Client.Search;
using MarkLogic.Esri.ArcGISPro.AddIn.Commands;
using MarkLogic.Esri.ArcGISPro.AddIn.Messaging;
using MarkLogic.Esri.ArcGISPro.AddIn.ViewModels;
using MarkLogic.Esri.ArcGISPro.AddIn.ViewModels.Messages;
using System;
using System.Collections.ObjectModel;
using System.Windows.Input;

namespace MarkLogic.Esri.ArcGISPro.AddIn
{
    public class SearchResultsViewModel : ViewModelBase
    {
        public SearchResultsViewModel(MessageBus messageBus)
        {
            MessageBus = messageBus ?? throw new ArgumentNullException("messageBus");
            MessageBus.Subscribe<BeginSearchMessage>(m =>
            {
                if (m.ReturnOptions.HasFlag(ReturnOptions.Results))
                    Results.Clear();
                IsFirstPage = IsLastPage = false;
                PrevStart = NextStart = CurrentPage = TotalPages = 0;
            });
            MessageBus.Subscribe<EndSearchMessage>(m =>
            {
                if (m.Results.ReturnOptions.HasFlag(ReturnOptions.Results))
                {
                    foreach (var result in m.Results.DocumentResults)
                        Results.Add(result);
                    IsFirstPage = m.Results.IsFirstPage;
                    IsLastPage = m.Results.IsLastPage;
                    PrevStart = m.Results.PrevStart;
                    NextStart = m.Results.NextStart;
                    CurrentPage = m.Results.CurrentPage;
                    TotalPages = m.Results.TotalPages;
                }
            });
        }

        protected MessageBus MessageBus { get; private set; }

        public ObservableCollection<SearchResult> Results { get; } = new ObservableCollection<SearchResult>();

        public bool IsFirstPage { get; private set; }

        public bool IsLastPage { get; private set; }

        public long PrevStart { get; private set; }

        public long NextStart { get; private set; }

        private long _currentPage;
        public long CurrentPage
        {
            get { return _currentPage; }
            set { SetProperty(ref _currentPage, value); }
        }

        private long _totalPages;
        public long TotalPages
        {
            get { return _totalPages; }
            set { SetProperty(ref _totalPages, value); }
        }

        private SearchCommand _cmdSelectResult;
        public ICommand SelectResult => _cmdSelectResult ?? (_cmdSelectResult = new SearchCommand(MessageBus));

        private PageSearchCommand _cmdPagePrev;
        public ICommand PagePrev => _cmdPagePrev ?? (_cmdPagePrev = new PageSearchCommand(MessageBus, () => PrevStart, o => !IsFirstPage));

        private PageSearchCommand _cmdPageNext;
        public ICommand PageNext => _cmdPageNext ?? (_cmdPageNext = new PageSearchCommand(MessageBus, () => NextStart, o => !IsLastPage));
    }
}