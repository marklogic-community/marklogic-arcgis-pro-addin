using MarkLogic.Client.Search;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

namespace MarkLogic.Esri.ArcGISPro.AddIn
{
    public class ResultsPanelViewModel : ViewModelBase
    {
        private static readonly string[] _props = { "Results", "CurrentPage", "TotalPages", "SelectResultCommand", "PagePrevResultsCommand", "PageNextResultsCommand" };

        public ResultsPanelViewModel()
        {
        }

        private SearchDockPaneViewModel _searchViewModel;
        public void Set(SearchDockPaneViewModel searchViewModel)
        {
            SetProperty(ref _searchViewModel, searchViewModel);
            searchViewModel.PropertyChanged += SearchViewModel_PropertyChanged;
            foreach(var prop in _props)
                OnPropertyChanged(prop);
        }

        private void SearchViewModel_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (_props.Contains(e.PropertyName))
                OnPropertyChanged(e.PropertyName);
        }

        public IEnumerable<SearchResult> Results => _searchViewModel != null ? _searchViewModel.Results : null;

        public long CurrentPage => _searchViewModel != null ? _searchViewModel.CurrentPage : 0;

        public long TotalPages => _searchViewModel != null ? _searchViewModel.TotalPages : 0;

        public ICommand SelectResultCommand => _searchViewModel != null ? _searchViewModel.SelectResultCommand : null;

        public ICommand PagePrevResultsCommand => _searchViewModel != null ? _searchViewModel.PagePrevResultsCommand : null;

        public ICommand PageNextResultsCommand => _searchViewModel != null ? _searchViewModel.PageNextResultsCommand : null;
    }
}