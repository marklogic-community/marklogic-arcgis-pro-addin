using ArcGIS.Core.Geometry;
using ArcGIS.Desktop.Framework;
using ArcGIS.Desktop.Framework.Contracts;
using ArcGIS.Desktop.Framework.Threading.Tasks;
using ArcGIS.Desktop.Mapping;
using ArcGIS.Desktop.Mapping.Events;
using MarkLogic.Client.Search.Query;
using MarkLogic.Esri.ArcGISPro.AddIn.Map;
using MarkLogic.Esri.ArcGISPro.AddIn.ViewModels;
using MarkLogic.Esri.ArcGISPro.AddIn.ViewModels.Messages;
using System;
using System.Diagnostics;
using System.Threading.Tasks;

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

        protected SearchDockPaneViewModel()
        {
            var messageBus = AddInModule.Current.MessageBus;
            messageBus.Subscribe<BeginSearchMessage>(async m => m.Query.Viewport = await GetCurrentViewport());

            ConnectionViewModel = new SearchConnectionViewModel(messageBus);
            ConnectionViewModel.ConnectionProfiles.CollectionChanged += (o, e) => ResolveSearchModelState();
            ConnectionViewModel.PropertyChanged += (o, e) => 
            {
                if (e.PropertyName == nameof(ConnectionViewModel.Connected)) 
                    ResolveSearchModelState(); 
            };
            QueryViewModel = new SearchQueryViewModel(messageBus);
            FacetsViewModel = new SearchFacetsViewModel(messageBus);
            OptionsViewModel = new SearchOptionsViewModel(messageBus);
            SymbologyViewModel = new SymbologyOptionsViewModel(messageBus);
            MapOverlay = new MapOverlayManager(messageBus);

            ActiveMapViewChangedEvent.Subscribe(e => ResolveSearchModelState());
            MapViewCameraChangedEvent.Subscribe(e =>
            {
                if (e.MapView == MapView.Active && ConnectionViewModel.HasSelectedServiceModel)
                    QueryViewModel.Search.Execute(null);
            });
        }

        public SearchConnectionViewModel ConnectionViewModel { get; private set; }

        public SearchQueryViewModel QueryViewModel { get; private set; }

        public SearchFacetsViewModel FacetsViewModel { get; private set; }

        public SearchOptionsViewModel OptionsViewModel { get; private set; }

        public SymbologyOptionsViewModel SymbologyViewModel { get; private set; }

        public MapOverlayManager MapOverlay { get; private set; }

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
            DockPane pane = FrameworkApplication.DockPaneManager.Find(SearchDockPaneViewModel.DockPaneId);
            pane?.Activate();
        }
    }
}
