using ArcGIS.Desktop.Mapping;
using MarkLogic.Client.Search;
using MarkLogic.Esri.ArcGISPro.AddIn.Commands;
using MarkLogic.Esri.ArcGISPro.AddIn.Messaging;
using MarkLogic.Esri.ArcGISPro.AddIn.ViewModels.Messages;
using MarkLogic.Extensions.Koop;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

namespace MarkLogic.Esri.ArcGISPro.AddIn.ViewModels
{
    public class SaveSearchViewModel : ViewModelBase
    {
        public class Layer
        {
            public int Id { get; set; }

            public string Name { get; set; }

            public string Description { get; set; }
        }

        public SaveSearchViewModel(MessageBus messageBus, SearchResults results, bool enableAddNew)
        {
            MessageBus = messageBus ?? throw new ArgumentNullException("messageBus");
            var msg = new GetServerSettingsMessage();
            MessageBus.Publish(msg).Wait();
            ConnectionProfile = msg.Profile;
            ServiceModel = msg.ServiceModel;

            // available layers
            EnableAddNew = enableAddNew;
            if (EnableAddNew)
                AvailableLayers.Add(new Layer() { Id = SaveSearchConstraintViewModel.AddNewLayerId, Name = "(new layer)" });
            ServiceModel.Layers.ToList().ForEach(l => AvailableLayers.Add(new Layer() { Id = l.Id, Name = l.Name, Description = l.Description }));

            FeatureService = ServiceModel.FeatureService.AbsoluteUri;
            results.ValueNames.ToList().ForEach(name => ConstraintsToSave.Add(
                new SaveSearchConstraintViewModel(name, AvailableLayers) { IncludeInSave = true, TargetLayerId = SaveSearchConstraintViewModel.AddNewLayerId }));
        }

        protected MessageBus MessageBus { get; private set; }

        protected bool EnableAddNew { get; private set; }

        protected ConnectionProfile ConnectionProfile { get; private set; }

        protected ServiceModel ServiceModel { get; private set; }

        private string _featureService;
        public string FeatureService
        {
            get { return _featureService; }
            set { SetProperty(ref _featureService, value); }
        }

        public ObservableCollection<Layer> AvailableLayers { get; } = new ObservableCollection<Layer>();

        public ObservableCollection<SaveSearchConstraintViewModel> ConstraintsToSave { get; } = new ObservableCollection<SaveSearchConstraintViewModel>();

        public bool CanSave => ConstraintsToSave.Where(c => c.IncludeInSave).Count() > 0 && ConstraintsToSave.Where(c => c.IncludeInSave).All(c => c.Valid);

        private ServerCommand _cmdSave;
        public ICommand Save => _cmdSave ?? (_cmdSave = new ServerCommand(async o =>
        {
            var query = new SearchQuery();
            await MessageBus.Publish(new BuildSearchMessage(query));

            var saveRequest = new SaveSearchRequest(ServiceModel, query);
            ConstraintsToSave.Where(c => c.IncludeInSave)
                .ToList()
                .ForEach(c =>
                {
                    int sourceLayerId = ServiceModel.Layers.Where(l => l.GeoConstraint == c.ConstraintName).Select(l => l.Id).FirstOrDefault();
                    int? targetLayerId = c.TargetLayerId == SaveSearchConstraintViewModel.AddNewLayerId ? (int?)null : c.TargetLayerId;
                    saveRequest.Layers.Add(new SaveSearchRequest.TargetLayer()
                    {
                        SourceLayerId = sourceLayerId,
                        TargetLayerId = targetLayerId,
                        Name = c.LayerName,
                        Description = c.LayerDescription
                    });
                });

            // if add new is disabled, verify that we aren't calling for the creation of any new layers
            if (!EnableAddNew)
                Debug.Assert(ConstraintsToSave.All(c => c.TargetLayerId != SaveSearchConstraintViewModel.AddNewLayerId));

            var conn = ConnectionService.Instance.Create(ConnectionProfile);
            var saveResults = await SearchService.Instance.SaveSearch(conn, saveRequest);

            await MessageBus.Publish(new SearchSavedMessage(saveResults, ServiceModel));
                
        }, o => CanSave));

    }
}
