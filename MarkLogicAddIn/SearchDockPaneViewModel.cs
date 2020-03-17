using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;
using ArcGIS.Desktop.Framework;
using ArcGIS.Desktop.Framework.Contracts;
using ArcGIS.Desktop.Mapping;
using ArcGIS.Desktop.Mapping.Events;
using MarkLogic.Esri.ArcGISPro.AddIn.Map;
using MarkLogic.Esri.ArcGISPro.AddIn.ViewModels;

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
            ConnectionViewModel = new SearchConnectionViewModel(messageBus);
            ConnectionViewModel.ConnectionProfiles.CollectionChanged += (o, e) => ResolveSearchModelState();
            ConnectionViewModel.PropertyChanged += (o, e) => 
            {
                if (e.PropertyName == nameof(ConnectionViewModel.Connected)) 
                    ResolveSearchModelState(); 
            };
            QueryViewModel = new SearchQueryViewModel(messageBus);
            FacetsViewModel = new SearchFacetsViewModel(messageBus);
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
    }

    public class EnumToVisibilityConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object property, CultureInfo culture)
        {
            if (value != null && value.GetType().IsEnum)
                return Enum.Equals(value, property) ? Visibility.Visible : Visibility.Collapsed;
            else
                return DependencyProperty.UnsetValue;
        }

        public object ConvertBack(object value, Type targetType, object property, CultureInfo culture)
        {
            if (value is Visibility && (Visibility)value == Visibility.Visible)
                return property;
            else
                return DependencyProperty.UnsetValue;
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
