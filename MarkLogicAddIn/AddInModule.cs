using System;
using System.Collections.Generic;
using System.Linq;
using ArcGIS.Desktop.Framework;
using ArcGIS.Desktop.Framework.Contracts;
using System.Collections.ObjectModel;
using MarkLogic.Esri.ArcGISPro.AddIn.Settings;
using System.Windows;
using MarkLogic.Esri.ArcGISPro.AddIn.Messaging;
using MarkLogic.Esri.ArcGISPro.AddIn.ViewModels;

namespace MarkLogic.Esri.ArcGISPro.AddIn
{
    internal class AddInModule : Module
    {
        public const string ModuleId = "MarkLogic_Esri_ArcGISPro_AddIn_Module";

        public static AddInModule Instance => (AddInModule)FrameworkApplication.FindModule(ModuleId);

        protected override bool Initialize()
        {
            LoadConnectionProfiles();
            InitializeMainViewModels();
            return base.Initialize();
        }

        protected override bool CanUnload()
        {
            return true;
        }

        public MessageBus MessageBus { get; } = new MessageBus();

        private Dictionary<Type, ViewModels.ViewModelBase> MainViewModels { get; } = new Dictionary<Type, ViewModels.ViewModelBase>();

        private void InitializeMainViewModels()
        {
            ViewModels.ViewModelBase[] viewModels = {
                new SearchConnectionViewModel(MessageBus),
                new SearchQueryViewModel(MessageBus),
                new SearchFacetsViewModel(MessageBus),
                new SearchOptionsViewModel(MessageBus),
                new SearchResultsViewModel(MessageBus),
                new SymbologyOptionsViewModel(MessageBus)
            };
            foreach (var viewModel in viewModels)
                MainViewModels.Add(viewModel.GetType(), viewModel);
        }

        public T GetMainViewModel<T>() where T : ViewModels.ViewModelBase
        {
            if (!MainViewModels.TryGetValue(typeof(T), out ViewModels.ViewModelBase viewModel))
                throw new InvalidOperationException($"Unable to locate main view model for type {typeof(T).FullName}");
            return viewModel as T;
        }

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
