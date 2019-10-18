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

namespace MarkLogic.Esri.ArcGISPro.AddIn
{
    internal class AddInModule : Module
    {
        private static AddInModule _this = null;

        /// <summary>
        /// Retrieve the singleton instance to this module here
        /// </summary>
        public static AddInModule Current
        {
            get
            {
                return _this ?? (_this = (AddInModule)FrameworkApplication.FindModule("MarkLogic_Esri_ArcGISPro_AddIn_Module"));
            }
        }

        protected override bool Initialize()
        {
            LoadConnectionProfiles();
            return base.Initialize();
        }

        private ObservableCollection<ConnectionProfile> _registeredConnectionProfiles = new ObservableCollection<ConnectionProfile>();
        public ObservableCollection<ConnectionProfile> RegisteredConnectionProfiles => _registeredConnectionProfiles;

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

        #region Overrides
        /// <summary>
        /// Called by Framework when ArcGIS Pro is closing
        /// </summary>
        /// <returns>False to prevent Pro from closing, otherwise True</returns>
        protected override bool CanUnload()
        {
            //TODO - add your business logic
            //return false to ~cancel~ Application close
            return true;
        }

        #endregion Overrides
    }
}
