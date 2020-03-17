using MarkLogic.Esri.ArcGISPro.AddIn.Messaging;
using MarkLogic.Esri.ArcGISPro.AddIn.ViewModels.Messages;
using MarkLogic.Extensions.Koop;
using System;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Windows.Input;

namespace MarkLogic.Esri.ArcGISPro.AddIn.ViewModels
{
    public class SearchConnectionViewModel : ViewModelBase
    {
        public SearchConnectionViewModel(MessageBus messageBus)
        {
            MessageBus = messageBus ?? throw new ArgumentNullException("messageBus");
            ConnectionProfiles = AddInModule.Current.RegisteredConnectionProfiles;
            ServiceModels = new ObservableCollection<IServiceModel>();
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
                    MessageBus.Publish(new ConnectionProfileChangedMessage(value));
                }
            }
        }

        public bool HasSelectedProfile => SelectedConnectionProfile != null;

        public ObservableCollection<IServiceModel> ServiceModels { get; private set; }

        private ServerCommand _cmdGetServiceModels;
        public ICommand GetServiceModels => _cmdGetServiceModels ?? (_cmdGetServiceModels = new ServerCommand(
            async o =>
            {
                Debug.Assert(SelectedConnectionProfile != null);
                var conn = ConnectionService.Instance.Create(SelectedConnectionProfile);
                ServiceModels.Clear();
                foreach (var model in await KoopService.GetServiceModels(conn))
                    ServiceModels.Add(model);
                Connected = true;
            },
            o => HasSelectedProfile));

        private IServiceModel _selectedServiceModel;
        public IServiceModel SelectedServiceModel
        {
            get { return _selectedServiceModel; }
            set
            {
                if (SetProperty(ref _selectedServiceModel, value))
                {
                    NotifyPropertyChanged(nameof(HasSelectedServiceModel));
                    MessageBus.Publish(new ServiceModelChangedMessage(value));
                }
            }
        }

        public bool HasSelectedServiceModel => SelectedServiceModel != null;

        private bool _connected;
        public bool Connected
        {
            get { return _connected; }
            set { SetProperty(ref _connected, value); }
        }
    }
}
