using ArcGIS.Desktop.Framework;
using MarkLogic.Esri.ArcGISPro.AddIn.Commands;
using MarkLogic.Esri.ArcGISPro.AddIn.Messaging;
using MarkLogic.Esri.ArcGISPro.AddIn.ViewModels.Messages;
using MarkLogic.Extensions.Koop;
using System;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Threading.Tasks;
using System.Windows.Input;

namespace MarkLogic.Esri.ArcGISPro.AddIn.ViewModels
{
    public class SearchConnectionViewModel : ViewModelBase
    {
        public SearchConnectionViewModel(MessageBus messageBus)
        {
            MessageBus = messageBus ?? throw new ArgumentNullException("messageBus");
            MessageBus.Subscribe<GetServerSettingsMessage>(m =>
            {
                m.Profile = SelectedConnectionProfile;
                m.ServiceModel = SelectedServiceModel;
                m.Resolved = true;
            });
            ConnectionProfiles = AddInModule.Instance.RegisteredConnectionProfiles;
            ServiceModels = new ObservableCollection<ServiceModel>();
        }

        protected MessageBus MessageBus { get; private set; }

        // TODO: this needs to be from a "ConnectionProfileService" instead of AddInModule
        public ObservableCollection<ConnectionProfile> ConnectionProfiles { get; private set; }

        private ConnectionProfile _selectedConnProfile;
        public ConnectionProfile SelectedConnectionProfile
        {
            get { return _selectedConnProfile; }
            set 
            {
                if (SetProperty(ref _selectedConnProfile, value))
                {
                    NotifyPropertyChanged(nameof(HasSelectedProfile));
                    MessageBus.Publish(new ServerSettingsChangedMessage(value, SelectedServiceModel)).Wait();
                }
            }
        }

        public bool HasSelectedProfile => SelectedConnectionProfile != null;

        private RelayCommand _cmdResetConnectionProfile;
        public ICommand ResetConnectionProfile => _cmdResetConnectionProfile ?? (_cmdResetConnectionProfile = new RelayCommand(() => 
        {
            SelectedServiceModel = null;
            SelectedConnectionProfile = null;
            Connected = false;
        }));

        public ObservableCollection<ServiceModel> ServiceModels { get; private set; }

        private ServerCommand _cmdGetServiceModels;
        public ICommand GetServiceModels => _cmdGetServiceModels ?? (_cmdGetServiceModels = new ServerCommand(
            async o =>
            {
                Debug.Assert(SelectedConnectionProfile != null);
                Connecting = true;
                var conn = ConnectionService.Instance.Create(SelectedConnectionProfile);
                ServiceModels.Clear();
                foreach (var model in await KoopService.GetServiceModels(conn))
                    ServiceModels.Add(model);
                Connecting = false;
                Connected = true;
            },
            o => HasSelectedProfile,
            e => Connecting = false));

        private ServiceModel _selectedServiceModel;
        public ServiceModel SelectedServiceModel
        {
            get { return _selectedServiceModel; }
            set
            {
                if (SetProperty(ref _selectedServiceModel, value))
                {
                    NotifyPropertyChanged(nameof(HasSelectedServiceModel));
                    MessageBus.Publish(new ServerSettingsChangedMessage(SelectedConnectionProfile, value)).Wait();
                }
            }
        }

        public bool HasSelectedServiceModel => SelectedServiceModel != null;

        private bool _connecting;
        public bool Connecting
        {
            get { return _connecting; }
            set 
            { 
                SetProperty(ref _connecting, value);
                NotifyPropertyChanged(nameof(NotConnecting));
            }
        }

        public bool NotConnecting => !Connecting;

        private bool _connected;
        public bool Connected
        {
            get { return _connected; }
            set { SetProperty(ref _connected, value); }
        }
    }
}
