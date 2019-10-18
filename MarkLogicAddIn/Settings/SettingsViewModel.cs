using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ArcGIS.Desktop.Framework;
using ArcGIS.Desktop.Framework.Contracts;
using ArcGIS.Desktop.Core;
using System.Collections.ObjectModel;

namespace MarkLogic.Esri.ArcGISPro.AddIn.Settings
{
    internal class SettingsViewModel : Page
    {
        private ObservableCollection<ConnectionProfile> _connectionProfiles;

        public SettingsViewModel()
        {
            _connectionProfiles = new ObservableCollection<ConnectionProfile>();
            _connectionProfiles.CollectionChanged += (o, e) => { IsModified = true; };
        }

        public ObservableCollection<ConnectionProfile> ConnectionProfiles => _connectionProfiles;
        
        /// <summary>
        /// Invoked when the OK or apply button on the property sheet has been clicked.
        /// </summary>
        /// <returns>A task that represents the work queued to execute in the ThreadPool.</returns>
        /// <remarks>This function is only called if the page has set its IsModified flag to true.</remarks>
        protected override Task CommitAsync()
        {
            AddInModule.Current.SaveConnectionProfiles(ConnectionProfiles);
            return Task.FromResult(0);
        }

        /// <summary>
        /// Called when the page loads because to has become visible.
        /// </summary>
        /// <returns>A task that represents the work queued to execute in the ThreadPool.</returns>
        protected override Task InitializeAsync()
        {
            foreach (var connProfile in AddInModule.Current.RegisteredConnectionProfiles)
            {
                ConnectionProfiles.Add(connProfile.Clone() as ConnectionProfile);
            }
            return Task.FromResult(0);
        }

        /// <summary>
        /// Called when the page is destroyed.
        /// </summary>
        protected override void Uninitialize()
        {
        }

        private RelayCommand _cmdAddServer;
        public RelayCommand AddServerCommand => _cmdAddServer ?? (_cmdAddServer = new RelayCommand((cp) => AddServer(cp), () => true));

        private RelayCommand _cmdEditServer;
        public RelayCommand EditServerCommand => _cmdEditServer ?? (_cmdEditServer = new RelayCommand((cp) => EditServer(cp), () => true));

        private RelayCommand _cmdDeleteServer;
        public RelayCommand DeleteServerCommand => _cmdDeleteServer ?? (_cmdDeleteServer = new RelayCommand((cp) => DeleteServer(cp), () => true));

        private void AddServer(object cp)
        {
            if (!(cp is ConnectionProfile))
                throw new ArgumentException("connProfile is not of type ConnectionProfile.", "connProfile");
            var connProfile = (ConnectionProfile)cp;
            ConnectionProfiles.Add(connProfile);
        }

        private void EditServer(object cp)
        {
            if (!(cp is ConnectionProfile))
                throw new ArgumentException("connProfile is not of type ConnectionProfile.", "connProfile");
            var connProfile = (ConnectionProfile)cp;
            ConnectionProfiles[ConnectionProfiles.IndexOf(connProfile)] = connProfile;
        }

        private void DeleteServer(object cp)
        {
            if (!(cp is ConnectionProfile))
                throw new ArgumentException("connProfile is not of type ConnectionProfile.", "connProfile");
            var connProfile = (ConnectionProfile)cp;
            ConnectionProfiles.Remove(connProfile);
        }
    }
}
