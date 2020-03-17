using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Input;
using ArcGIS.Desktop.Framework;
using ArcGIS.Desktop.Framework.Contracts;
using System.Threading.Tasks;
using System.Collections.ObjectModel;
using MarkLogic.Esri.ArcGISPro.AddIn.Settings;
using System.Windows;
using MarkLogic.Esri.ArcGISPro.AddIn.Messaging;

namespace MarkLogic.Esri.ArcGISPro.AddIn
{
    internal class AddInModule : Module
    {
        public const string ModuleId = "MarkLogic_Esri_ArcGISPro_AddIn_Module";

        public static AddInModule Current => (AddInModule)FrameworkApplication.FindModule(ModuleId);

        protected override bool Initialize()
        {
            LoadConnectionProfiles();
            return base.Initialize();
        }

        protected override bool CanUnload()
        {
            return true;
        }

        private MessageBus _messageBus;
        public MessageBus MessageBus => _messageBus ?? (_messageBus = new MessageBus());

        private ObservableCollection<ConnectionProfile> _registeredConnectionProfiles;
        public ObservableCollection<ConnectionProfile> RegisteredConnectionProfiles => _registeredConnectionProfiles ?? (_registeredConnectionProfiles = new ObservableCollection<ConnectionProfile>());

        private void LoadConnectionProfiles()
        {
            try
            {
                var settings = AppSettings.Default;
                if (settings.ConnectionProfiles != null)
                {
                    foreach (var item in settings.ConnectionProfiles.Items)
                        RegisteredConnectionProfiles.Add(item);
                }
            }
            catch(Exception e)
            {
                ArcGIS.Desktop.Framework.Dialogs.MessageBox.Show(e.ToString(), "MarkLogic", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        public void SaveConnectionProfiles(IEnumerable<ConnectionProfile> connProfiles)
        {
            try
            {
                var settings = AppSettings.Default;
                settings.ConnectionProfiles = new ConnectionProfileList() { Items = connProfiles.ToArray() };
                settings.Save();

                RegisteredConnectionProfiles.Clear();
                foreach (var cp in connProfiles)
                    RegisteredConnectionProfiles.Add(cp);
            }
            catch (Exception e)
            {
                ArcGIS.Desktop.Framework.Dialogs.MessageBox.Show(e.ToString(), "MarkLogic", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
    }
}
