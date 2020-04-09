using ArcGIS.Core.Geometry;
using ArcGIS.Desktop.Framework;
using ArcGIS.Desktop.Framework.Contracts;
using ArcGIS.Desktop.Framework.Threading.Tasks;
using ArcGIS.Desktop.Mapping;
using ArcGIS.Desktop.Mapping.Events;
using MarkLogic.Client.Search;
using MarkLogic.Client.Search.Query;
using MarkLogic.Esri.ArcGISPro.AddIn.Commands;
using MarkLogic.Esri.ArcGISPro.AddIn.Map;
using MarkLogic.Esri.ArcGISPro.AddIn.Messaging;
using MarkLogic.Esri.ArcGISPro.AddIn.ViewModels;
using MarkLogic.Esri.ArcGISPro.AddIn.ViewModels.Messages;
using System;
using System.Diagnostics;
using System.Threading.Tasks;
using System.Windows.Input;

namespace MarkLogic.Esri.ArcGISPro.AddIn
{
    public enum SearchModelState
    {
        NoActiveMap,
        NoRegisteredServers,
        NoConnection,
        HasConnection
    }

    public class SearchDockPaneViewModel : DockPane
    {
        public const string DockPaneId = "MarkLogic_Esri_ArcGISPro_AddIn_SearchDockPane";

        private SearchResults _lastSearchResults;

        protected SearchDockPaneViewModel()
        {
            var module = AddInModule.Instance;
            MessageBus = module.MessageBus;
            MessageBus.Subscribe<BuildSearchMessage>(async m => m.Query.Viewport = await GetCurrentViewport());
            MessageBus.Subscribe<EndSearchMessage>(m => 
            {
                _lastSearchResults = m.Results.ReturnOptions.HasFlag(ReturnOptions.Values) ? m.Results : null;
                if (_lastSearchResults != null)
                {
                    var pane = FrameworkApplication.DockPaneManager.Find(SearchResultsDockPaneViewModel.DockPaneId);
                    pane?.Activate();
                }
            });

            ConnectionViewModel = module.GetMainViewModel<SearchConnectionViewModel>();
            QueryViewModel = module.GetMainViewModel<SearchQueryViewModel>();
            FacetsViewModel = module.GetMainViewModel<SearchFacetsViewModel>();
            OptionsViewModel = module.GetMainViewModel<SearchOptionsViewModel>();
            SymbologyViewModel = module.GetMainViewModel<SymbologyOptionsViewModel>();
            MapOverlay = new MapOverlayManager(module.MessageBus);

            ConnectionViewModel.ConnectionProfiles.CollectionChanged += (o, e) => ResolveSearchModelState();
            ConnectionViewModel.PropertyChanged += (o, e) => 
            {
                if (e.PropertyName == nameof(ConnectionViewModel.Connected)) 
                    ResolveSearchModelState(); 
            };
            
            ActiveMapViewChangedEvent.Subscribe(e => ResolveSearchModelState());
            MapViewCameraChangedEvent.Subscribe(e =>
            {
                if (e.MapView == MapView.Active && ConnectionViewModel.HasSelectedServiceModel && !QueryViewModel.IsSearching)
                    QueryViewModel.Search.Execute(null);
            });

            ResolveSearchModelState();
        }

        private MessageBus MessageBus { get; set; }

        public SearchConnectionViewModel ConnectionViewModel { get; private set; }

        public SearchQueryViewModel QueryViewModel { get; private set; }

        public SearchFacetsViewModel FacetsViewModel { get; private set; }

        public SearchOptionsViewModel OptionsViewModel { get; private set; }

        public SymbologyOptionsViewModel SymbologyViewModel { get; private set; }

        public MapOverlayManager MapOverlay { get; private set; }

        public ShowSearchHelpCommand _cmdShowSearchHelp;
        public ICommand ShowSearchHelp => _cmdShowSearchHelp ?? (_cmdShowSearchHelp = new ShowSearchHelpCommand(MessageBus));

        public ShowSaveSearchCommand _cmdShowSaveSearch;
        public ICommand ShowSaveSearch => _cmdShowSaveSearch ?? (_cmdShowSaveSearch = new ShowSaveSearchCommand(MessageBus, () => _lastSearchResults));

        public RelayCommand _cmdInspectResults;
        public ICommand InspectResults => _cmdInspectResults ?? (_cmdInspectResults = new RelayCommand(
            () => FrameworkApplication.SetCurrentToolAsync("MarkLogic_Esri_ArcGISPro_AddIn_Map_Tool_InspectResultsMapTool"),
            () => _lastSearchResults != null && MapView.Active != null));

        public SearchModelState State { get; private set; }

        private void ResolveSearchModelState()
        {
            if (MapView.Active == null)
                State = SearchModelState.NoActiveMap;
            else if (ConnectionViewModel.ConnectionProfiles.Count == 0)
                State = SearchModelState.NoRegisteredServers;
            else if (!ConnectionViewModel.Connected)
                State = SearchModelState.NoConnection;
            else
                State = SearchModelState.HasConnection;
            NotifyPropertyChanged(nameof(State));
        }

        private Task<GeospatialBox> GetCurrentViewport()
        {
            Debug.Assert(MapView.Active != null);
            if (MapView.Active == null)
                return Task.FromResult(GeospatialBox.FullExtent);

            return QueuedTask.Run(() =>
            {
                var extent = MapView.Active.Extent;
                var extentEnvelope = EnvelopeBuilder.CreateEnvelope(extent.XMin, extent.YMin, extent.XMax, extent.YMax, extent.SpatialReference);
                ProjectionTransformation pxForm = ProjectionTransformation.Create(extent.SpatialReference, SpatialReferences.WGS84);
                var wgsEnvelope = GeometryEngine.Instance.ProjectEx(extentEnvelope, pxForm) as Envelope;

                return new GeospatialBox()
                {
                    North = Math.Min(wgsEnvelope.YMax, 90.0), // north up to +90 deg
                    South = Math.Max(wgsEnvelope.YMin, -90.0), // south up to -90 deg
                    West = Math.Max(wgsEnvelope.XMin, -180.0), // west up to -180 deg
                    East = Math.Min(wgsEnvelope.XMax, 180.0) // east up to +180 deg
                };
            });
        }
    }

    internal class SearchDockPane_ShowButton : Button
    {
        protected override void OnClick()
        {
            var pane = FrameworkApplication.DockPaneManager.Find(SearchDockPaneViewModel.DockPaneId);
            pane?.Activate();
        }
    }
}
