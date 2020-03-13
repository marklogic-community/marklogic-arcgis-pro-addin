using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using System.Net;
using System.Security;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Data;
using System.Windows.Input;
using ArcGIS.Core.CIM;
using ArcGIS.Core.Geometry;
using ArcGIS.Desktop.Framework;
using ArcGIS.Desktop.Framework.Contracts;
using ArcGIS.Desktop.Framework.Threading.Tasks;
using ArcGIS.Desktop.Mapping;
using ArcGIS.Desktop.Mapping.Events;
using MarkLogic.Client.Search;
using MarkLogic.Client.Search.Query;
using MarkLogic.Esri.ArcGISPro.AddIn.Feature;
using MarkLogic.Esri.ArcGISPro.AddIn.Map;
using MarkLogic.Extensions.Koop;

namespace MarkLogic.Esri.ArcGISPro.AddIn
{
    public class SearchDockPaneViewModel : DockPane
    {
        private const string _dockPaneID = "MarkLogic_Esri_ArcGISPro_AddIn_SearchDockPane";

        private Connection _conn;
        private SearchOptions _queryOptions;
        private ObservableCollection<object> _searchSuggestions;
        private SearchResults _searchResults;
        private ValuesResults _valuesResults;
        private string _prevQueryString = "";
        private SearchQuery _prevQuery = null;
        private HashSet<FacetValue> _selectedFacetValues = new HashSet<FacetValue>();
        private ICommand _cmdToRerun = null;

        private MapPointOverlay _pointOverlay = null;
        private MapPointOverlay PointOverlay => _pointOverlay ?? (_pointOverlay = new MapPointOverlay(MapView.Active));

        protected SearchDockPaneViewModel()
        {
            ActiveMapViewChangedEvent.Subscribe((e) => ResolveActiveMap(e.IncomingView));
            MapViewCameraChangedEvent.Subscribe((e) => OnCameraChanged(e));
            ConnectionProfiles.CollectionChanged += (o, e) => ResolveState();
            ResolveActiveMap(MapView.Active);
        }

        private void ResolveActiveMap(MapView mapView)
        {
            HasActiveMap = mapView != null;
            ResolveState();
        }

        private void ResolveState()
        {
            if (!HasActiveMap)
                State = SearchModelState.NoActiveMap;
            else if (ConnectionProfiles.Count == 0)
                State = SearchModelState.NoRegisteredServers;
            else
                State = _conn != null ? SearchModelState.HasConnection : SearchModelState.NoConnection;   
        }
        
        private bool EnsureHasConnection()
        {
            if (!HasConnectionProfile)
                return false;
            _conn = ConnectionService.Instance.Create(ConnectionProfile);
            ResolveState();
            return _conn != null;
        }

        private void Disconnect()
        {
            if (!HasConnectionProfile)
                return;
            _conn = null;
            ServiceModels.Clear();
            _reqAuth = false;
            ConnectionProfile = null;
            ResolveState();
        }

        private void SetCredentials(object credentials)
        {
            if (!(credentials is NetworkCredential))
                throw new ArgumentException("credentials must be NetworkCredential.");
            var creds = (NetworkCredential)credentials;
            creds.Domain = RequiresAuthorizationInfo.Domain;
            ConnectionService.Instance.SetCredentials(_connProfile, creds, RequiresAuthorizationInfo.Scheme);

            RequiresAuthorization = false;
            if (_cmdToRerun != null)
                _cmdToRerun.Execute(null);
        }

        private void ProcessAuthorizationException(AuthorizationRequiredException e, ICommand commandToRerun)
        {
            RequiresAuthorizationInfo = e.AuthInfo;
            _cmdToRerun = commandToRerun;
            RequiresAuthorization = true;
        }

        private async void Discover()
        {
            try
            {
                if (!EnsureHasConnection()) return;
                ServiceModels.Clear();
                foreach(var model in await KoopService.GetServiceModels(_conn)) {
                    ServiceModels.Add(model);
                }
            }
            catch (AuthorizationRequiredException e)
            {
                ProcessAuthorizationException(e, DiscoverCommand);
            }
            catch (Exception e)
            {
                ArcGIS.Desktop.Framework.Dialogs.MessageBox.Show(e.ToString(), "MarkLogic", MessageBoxButton.OK, MessageBoxImage.Error);
                Disconnect();
            }
        }

        private long ProcessResults(SearchResults results)
        {
            _searchResults = results;
            _searchResults.SelectedFacetValues.ToList().ForEach(v => _selectedFacetValues.Add(v));
            NotifyPropertyChanged(() => Facets);
            NotifyPropertyChanged(() => Results);
            NotifyPropertyChanged(() => CurrentPage);
            NotifyPropertyChanged(() => TotalPages);
            return _searchResults.Total;
        }

        private async Task<long> ProcessValues(SearchResults results)
        {
            SaveToNewLayerCommand.RaiseCanExecuteChanged();
            Debug.Assert(MapView.Active != null); // there should be a check if the current pane is a map
            await PointOverlay.SetPoints(results, results.ValueNames.FirstOrDefault());
            return results.Total;
        }

        private void AppendConstraint(object constraintName)
        {
            QueryString = QueryString + $" {constraintName.ToString()}:";
        }

        private Task<GeospatialBox> GetViewportBox(MapView mapView)
        {
            Debug.Assert(mapView != null);
            return QueuedTask.Run(() =>
            {
                var extent = mapView.Extent;
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

        private void OnCameraChanged(MapViewCameraChangedEventArgs e)
        {
            if (e.MapView == MapView.Active && HasSelectedServiceModel)
                Search();
        }

        private void ResetSearch()
        {
            PointOverlay.Clear(true);
            QueryString = "";
            _searchSuggestions.Clear();
            _selectedFacetValues.Clear();
            _searchResults = null;
            _valuesResults = null;
            NotifyPropertyChanged(() => Facets);
            NotifyPropertyChanged(() => Results);
            NotifyPropertyChanged(() => CurrentPage);
            NotifyPropertyChanged(() => TotalPages);

            SearchResultsDockPaneViewModel.ResetDocument();
        }

        private async void Search()
        {
            try
            {
                if (!EnsureHasConnection()) return;

                if (!HasSelectedServiceModel) return;

                IsSearching = true;

                // clear
                QueryResultsSummary = "";
                PointOverlay.Clear();
                _searchSuggestions.Clear();

                // build query
                if (_prevQueryString != QueryString)
                    _selectedFacetValues.Clear(); // different query; clear previously selected facets
                var query = new SearchQuery(QueryString, _selectedFacetValues);

                _prevQueryString = QueryString;
                _prevQuery = query;

                // prevent searching everything
                if (query.IsEmpty)
                {
                    ResetSearch();
                    return;
                }

                // run query
                QueryResultsSummary = "Running query...";

                var searchResults = await SearchService.Instance.Search(Connection, query, SelectedServiceModel);
                var resultsTotal = ProcessResults(searchResults);
                var objectsTotal = await ProcessValues(searchResults);

                QueryResultsSummary = $"Returned {resultsTotal} results and {objectsTotal} objects.";

                SearchResultsDockPaneViewModel.ShowResults(this);
            }
            catch (AuthorizationRequiredException e)
            {
                ProcessAuthorizationException(e, SearchCommand);
            }
            catch (Exception e)
            {
                ArcGIS.Desktop.Framework.Dialogs.MessageBox.Show(e.ToString(), "MarkLogic", MessageBoxButton.OK, MessageBoxImage.Error);
                QueryResultsSummary = "";
            }
            finally
            {   
                IsSearching = false;
                NotifyPropertyChanged(() => HasResults);
            }
        }

        private async void PageResults(bool forward)
        {
            try
            {
                Debug.Assert(_searchResults != null && _prevQuery != null);
                if (_searchResults == null || _prevQuery == null)
                    return;

                var start = forward ? _searchResults.NextStart : _searchResults.PrevStart;
                if (start == _searchResults.Start)
                    return; // already on first or last page

                IsSearching = true;
                var results = await SearchService.Instance.Search(_conn, _prevQuery, SelectedServiceModel, start);
                _searchResults = results; // replace

                NotifyPropertyChanged(() => Results);
                NotifyPropertyChanged(() => CurrentPage);
                NotifyPropertyChanged(() => TotalPages);
            }
            catch (AuthorizationRequiredException e)
            {
                ProcessAuthorizationException(e, PagePrevResultsCommand);
            }
            finally
            {
                IsSearching = false;
                NotifyPropertyChanged(() => HasResults);
            }
        }

        private async void Suggest()
        {
            try
            {
                if (!EnsureHasConnection()) return;

                if (!HasSelectedServiceModel) return;

                var partialQ = QueryString;
                /*var results = await SearchService.Instance.Suggest(_conn, partialQ, SelectedSearchServiceProfile.SearchOptions); TODO: replace
                _searchSuggestions.Clear();
                foreach (var  suggestion in results.Suggestions)
                    _searchSuggestions.Add(suggestion); TODO: replace */
            }
            catch (AuthorizationRequiredException e)
            {
                return; // suppress and don't do suggestions until connection is established
            }
            catch (Exception e)
            {
                ArcGIS.Desktop.Framework.Dialogs.MessageBox.Show(e.ToString(), "MarkLogic", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }


        private void SelectFacet(object facetValueObj)
        {
            if (facetValueObj == null)
                throw new ArgumentNullException("facetValueObj");
            if (!(facetValueObj is FacetValue))
                throw new ArgumentException("facetValueObj must be of type FacetValue", "facetValueObj");
            var facetValue = (FacetValue)facetValueObj;
            if (facetValue.Selected)
                _selectedFacetValues.Add(facetValue);
            else if (!facetValue.Selected && _selectedFacetValues.Contains(facetValue))
                _selectedFacetValues.Remove(facetValue);
            Search();
        }

        private async void SelectResult(object searchResultObj)
        {
            if (searchResultObj == null)
                throw new ArgumentNullException("searchResultObj");
            if (!(searchResultObj is SearchResult))
                throw new ArgumentException("searchResultObj must be of type SearchResult", "searchResultObj");
            try
            {
                /* TODO: replace
                var searchResult = (SearchResult)searchResultObj;
                SearchResultsDockPaneViewModel.ShowDocument(ConnectionProfile, searchResult.Uri, SelectedSearchServiceProfile.DocTransform);

                // retrieve document coordinates
                var query = new SearchQuery();
                query.AdditionalQueries.Add(new DocumentQuery(searchResult.Uri));
                var values = await SearchService.Instance.Values(Connection, query, SelectedSearchServiceProfile.SearchOptions, SelectedSearchServiceProfile.Values);
                if (values != null)
                {
                    var point = values.FirstOrDefault();
                    Debug.Assert(point != null);
                    await PointOverlay.SelectPoint(point.Long, point.Lat);
                }*/
            }
            catch(Exception e)
            {
                ArcGIS.Desktop.Framework.Dialogs.MessageBox.Show(e.ToString(), "MarkLogic", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        public bool IsFirstPage
        {
            get { return _searchResults != null && _prevQuery != null && _searchResults.IsFirstPage; }
        }

        public bool IsLastPage
        {
            get { return _searchResults != null && _prevQuery != null && _searchResults.IsLastPage; }
        }

        private bool SaveToNewLayerAllowed()
        {
            return _valuesResults != null && _valuesResults.Count > 0;
        }

        private async void SaveToNewLayer(object sopts)
        {
            try
            {
                if (!(sopts is ISaveSearchOptions))
                    throw new ArgumentException("Invalid save search options.");
                var saveOptions = (ISaveSearchOptions)sopts;

                IsSaving = true;

                if (saveOptions.Mode == SaveSearchMode.FeatureLayer)
                {
                    await FeatureLayerBuilder.Instance.CreateFeatureLayer(MapView.Active, saveOptions.LayerName, _valuesResults);
                }
                else if (saveOptions.Mode == SaveSearchMode.FeatureService)
                {
                    Debug.Assert(HasSelectedServiceModel);

                    var createOptions = new CreateFeatureLayerOptions()
                    {
                        LayerName = saveOptions.LayerName,
                        LayerDescription = saveOptions.LayerDescription/*,
                        FeatureServiceName = SelectedSearchServiceProfile.ServiceName,
                        FeatureServiceSchema = SelectedSearchServiceProfile.Schema,
                        FeatureServiceView = SelectedSearchServiceProfile.View,
                        SearchOptions = SelectedSearchServiceProfile.SearchOptions*/
                    };

                    string layerId = saveOptions.Replace ? saveOptions.ReplaceLayerId : null;
                    
                    var results = await KoopService.CreateFeatureLayer(_conn, _prevQuery, createOptions, layerId);
                    if (results.LayerUrl != null)
                    {
                        await FeatureLayerBuilder.Instance.AddFeatureLayer(
                            MapView.Active, 
                            createOptions.FeatureServiceName, 
                            results.ServiceUrl, 
                            createOptions.LayerName,
                            results.LayerId);
                    }
                }
                
                ResetSearch();
            }
            catch (Exception ex)
            {
                ArcGIS.Desktop.Framework.Dialogs.MessageBox.Show(ex.ToString(), "MarkLogic", MessageBoxButton.OK, MessageBoxImage.Error);
            }
            finally
            {
                IsSaving = false;
            }
        }

        /// <summary>
        /// Show the DockPane.
        /// </summary>
        internal static void Show()
        {
            DockPane pane = FrameworkApplication.DockPaneManager.Find(_dockPaneID);
            if (pane == null)
                return;

            pane.Activate();
        }

        private RelayCommand _cmdSetCredentials;
        public RelayCommand SetCredentialsCommand => _cmdSetCredentials ?? (_cmdSetCredentials = new RelayCommand(c => SetCredentials(c), () => true));

        private RelayCommand _cmdDiscover;
        public RelayCommand DiscoverCommand => _cmdDiscover ?? (_cmdDiscover = new RelayCommand(() => Discover(), () => ConnectionProfile != null));

        private RelayCommand _cmdDisconnect;
        public RelayCommand DisconnectCommand => _cmdDisconnect ?? (_cmdDisconnect = new RelayCommand(() => Disconnect(), () => HasConnectionProfile));

        private RelayCommand _cmdSuggest;
        public RelayCommand SuggestCommand => _cmdSuggest ?? (_cmdSuggest = new RelayCommand(() => Suggest(), () => true));

        private RelayCommand _cmdAppendConstraint;
        public RelayCommand AppendConstraintCommand => _cmdAppendConstraint ?? (_cmdAppendConstraint = new RelayCommand((constraintName) => AppendConstraint(constraintName), () => true));

        private RelayCommand _cmdSearch;
        public RelayCommand SearchCommand => _cmdSearch ?? (_cmdSearch = new RelayCommand(() => Search(), () => true));

        private RelayCommand _cmdSaveToNewLayer;
        public RelayCommand SaveToNewLayerCommand => _cmdSaveToNewLayer ?? (_cmdSaveToNewLayer = new RelayCommand((saveParams) => SaveToNewLayer(saveParams), () => SaveToNewLayerAllowed()));

        private RelayCommand _cmdSelectFacet;
        public RelayCommand SelectFacetCommand => _cmdSelectFacet ?? (_cmdSelectFacet = new RelayCommand(fv => SelectFacet(fv), () => true));

        private RelayCommand _cmdSelectResult;
        public RelayCommand SelectResultCommand => _cmdSelectResult ?? (_cmdSelectResult = new RelayCommand((r) => SelectResult(r), () => true));

        private RelayCommand _cmdPagePrevResults;
        public RelayCommand PagePrevResultsCommand => _cmdPagePrevResults ?? (_cmdPagePrevResults = new RelayCommand(() => PageResults(false), () => !IsFirstPage));

        private RelayCommand _cmdPageNextResults;
        public RelayCommand PageNextResultsCommand => _cmdPageNextResults ?? (_cmdPageNextResults = new RelayCommand(() => PageResults(true), () => !IsLastPage));

        private SearchModelState _state;
        public SearchModelState State
        {
            get { return _state; }
            private set { SetProperty(ref _state, value, () => State); }
        }

        private bool _hasActiveMap;
        public bool HasActiveMap
        {
            get { return _hasActiveMap; }
            private set
            {
                SetProperty(ref _hasActiveMap, value, () => HasActiveMap);
                NotifyPropertyChanged(() => NoActiveMap);
            }
        }

        public bool NoActiveMap { get { return !HasActiveMap; } }

        public ObservableCollection<ConnectionProfile> ConnectionProfiles => AddInModule.Current.RegisteredConnectionProfiles;

        private ConnectionProfile _connProfile;
        public ConnectionProfile ConnectionProfile
        {
            get { return _connProfile; }
            set
            {
                SetProperty(ref _connProfile, value, () => ConnectionProfile);
                NotifyPropertyChanged(() => NoConnectionProfile);
                NotifyPropertyChanged(() => HasConnectionProfile);
            }
        }

        public bool NoConnectionProfile
        {
            get { return _connProfile == null; }
        }

        public bool HasConnectionProfile
        {
            get { return _connProfile != null; }
        }

        public Connection Connection => _conn;

        private bool _reqAuth = false;
        public bool RequiresAuthorization
        {
            get { return _reqAuth; }
            protected set { SetProperty(ref _reqAuth, value, () => RequiresAuthorization); }
        }

        public AuthorizationRequiredInfo RequiresAuthorizationInfo { get; private set; }

        public ObservableCollection<IServiceModel> ServiceModels { get; } = new ObservableCollection<IServiceModel>();

        private IServiceModel _selectedServiceModel;
        public IServiceModel SelectedServiceModel
        {
            get => _selectedServiceModel;
            set
            {
                SetProperty(ref _selectedServiceModel, value, () => SelectedServiceModel);
                NotifyPropertyChanged(() => HasSelectedServiceModel);
                NotifyPropertyChanged(() => CanSearch);
            }
        }

        public bool HasSelectedServiceModel => SelectedServiceModel != null;

        public bool CanSearch
        {
            get { return IsNotSearching && HasSelectedServiceModel; }
        }

        private string _queryString;
        public string QueryString
        {
            get { return _queryString; }
            set { SetProperty(ref _queryString, value, () => QueryString); }
        }

        private string _queryResultsSummary;
        public string QueryResultsSummary
        {
            get { return _queryResultsSummary; }
            set { SetProperty(ref _queryResultsSummary, value, () => QueryResultsSummary); }
        }

        private bool _isSearching;
        public bool IsSearching
        {
            get { return _isSearching; }
            set
            {
                SetProperty(ref _isSearching, value, () => IsSearching);
                NotifyPropertyChanged(() => IsNotSearching);
                NotifyPropertyChanged(() => CanSearch);
            }
        }

        public bool IsNotSearching
        {
            get { return !IsSearching; }
        }

        public ObservableCollection<object> SearchSuggestions => _searchSuggestions ?? (_searchSuggestions = new ObservableCollection<object>());

        public IEnumerable<Facet> Facets
        {
            get { return _searchResults != null ? _searchResults.Facets.Values : new Facet[0]; }
        }

        public bool HasResults
        {
            get { return _searchResults != null; }
        }

        public IEnumerable<SearchResult> Results
        {
            get { return _searchResults != null ? _searchResults.Results : new SearchResult[0]; }
        }

        public long CurrentPage
        {
            get { return _searchResults != null ? _searchResults.CurrentPage : 0; }
        }

        public long TotalPages
        {
            get { return _searchResults != null ? _searchResults.TotalPages : 0; }
        }

        private bool _isSaving;
        public bool IsSaving
        {
            get { return _isSaving; }
            set { SetProperty(ref _isSaving, value, () => IsSaving); }
        }
    }

    public enum SearchModelState
    {
        NoActiveMap,
        NoRegisteredServers,
        NoConnection,
        HasConnection
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

    /// <summary>
    /// Button implementation to show the DockPane.
    /// </summary>
    internal class SearchDockPane_ShowButton : Button
    {
        protected override void OnClick()
        {
            SearchDockPaneViewModel.Show();
        }
    }
}
