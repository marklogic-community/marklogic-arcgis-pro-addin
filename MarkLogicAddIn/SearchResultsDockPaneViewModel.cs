using System.Collections.ObjectModel;
using ArcGIS.Desktop.Framework;
using ArcGIS.Desktop.Framework.Contracts;
using ArcGIS.Desktop.Framework.Controls;
using MarkLogic.Client.Search;
using MarkLogic.Esri.ArcGISPro.AddIn.ViewModels.Messages;

namespace MarkLogic.Esri.ArcGISPro.AddIn
{
    internal class SearchResultsDockPaneViewModel : DockPane
    {
        public const string DockPaneId = "MarkLogic_Esri_ArcGISPro_AddIn_SearchResultsDockPane";

        protected SearchResultsDockPaneViewModel()
        {
            var module = AddInModule.Instance;
            var messageBus = module.MessageBus;
            messageBus.Subscribe<EndSearchMessage>(m =>
            {
                if (!m.Results.ReturnOptions.HasFlag(ReturnOptions.Results))
                    return;
                // show results pane
                Activate();
                SelectedTabIndex = 0;
            });
            messageBus.Subscribe<ViewDocumentMessage>(m =>
            {
                // show document pane
                Activate();
                SelectedTabIndex = 1;
            });

            Tabs.Add(new TabControl() { Text = "Search Results", Tooltip = "Search Results" });
            Tabs.Add(new TabControl() { Text = "Document", Tooltip = "Document" });
            _selectedTabIndex = 0;

            ResultsViewModel = module.GetMainViewModel<SearchResultsViewModel>();
            DocumentViewModel = module.GetMainViewModel<DocumentViewModel>();
        }

        private ObservableCollection<TabControl> _tabs;
        public ObservableCollection<TabControl> Tabs => _tabs ?? (_tabs = new ObservableCollection<TabControl>());

        private int _selectedTabIndex;
        public int SelectedTabIndex
        {
            get { return _selectedTabIndex; }
            set
            {
                SetProperty(ref _selectedTabIndex, value, () => SelectedTabIndex);
                NotifyPropertyChanged(() => CurrentPage);
                NotifyPropertyChanged(() => IsResultsTabSelected);
                NotifyPropertyChanged(() => IsDocumentTabSelected);
            }
        }

        public object CurrentPage => SelectedTabIndex == 0 ? (object)ResultsViewModel : (object)DocumentViewModel;

        public bool IsResultsTabSelected => SelectedTabIndex == 0;

        public bool IsDocumentTabSelected => SelectedTabIndex == 1;

        public SearchResultsViewModel ResultsViewModel { get; private set; }

        public DocumentViewModel DocumentViewModel { get; private set; }
    }

    internal class SearchResultsDockPane_ShowButton : Button
    {
        protected override void OnClick()
        {
            var pane = FrameworkApplication.DockPaneManager.Find(SearchResultsDockPaneViewModel.DockPaneId);
            pane?.Activate();
        }
    }
}
