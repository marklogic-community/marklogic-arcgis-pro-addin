using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using ArcGIS.Desktop.Framework;
using ArcGIS.Desktop.Framework.Contracts;
using ArcGIS.Desktop.Framework.Controls;
using MarkLogic.Client.Document;

namespace MarkLogic.Esri.ArcGISPro.AddIn
{
    internal class SearchResultsDockPaneViewModel : DockPane
    {
        public const string DockPaneId = "MarkLogic_Esri_ArcGISPro_AddIn_SearchResultsDockPane";

        protected SearchResultsDockPaneViewModel()
        {
            var module = AddInModule.Instance;

            Tabs.Add(new TabControl() { Text = "Search Results", Tooltip = "Search Results" });
            Tabs.Add(new TabControl() { Text = "Document", Tooltip = "Document" });

            ResultsViewModel = module.GetMainViewModel<SearchResultsViewModel>();
            DocumentViewModel = new DocumentPanelViewModel();
            _selectedTabIndex = 0;
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

        public DocumentPanelViewModel DocumentViewModel { get; private set; }

        /*internal static void ShowResults(SearchDockPaneViewModel searchViewModel)
        {
            var pane = Show();
            if (pane == null)
                return;
            pane.SelectedTabIndex = 0;
            pane.ResultsViewModel.Set(searchViewModel);
        }

        internal static void ShowDocument(ConnectionProfile connProfile = null, string documentUri = null, string docTransform = null)
        {
            var pane = Show();
            if (pane == null)
                return;
            pane.SelectedTabIndex = 1;
            if (connProfile != null && !string.IsNullOrWhiteSpace(documentUri))
                pane.DocumentViewModel.FetchDocument(connProfile, documentUri, docTransform);
        }

        internal static void ResetDocument()
        {
            var pane = FrameworkApplication.DockPaneManager.Find(DockPaneId) as SearchResultsDockPaneViewModel;
            if (pane == null)
                return;
            pane.DocumentViewModel.Reset();
        }*/
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
