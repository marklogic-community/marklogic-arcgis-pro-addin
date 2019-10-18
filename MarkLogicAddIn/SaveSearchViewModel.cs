using MarkLogic.Esri.ArcGISPro.AddIn.Feature;
using MarkLogic.Extensions.Koop;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace MarkLogic.Esri.ArcGISPro.AddIn
{
    public enum SaveSearchMode
    {
        FeatureService,
        FeatureLayer
    }

    public interface ISaveSearchOptions
    {
        SaveSearchMode Mode { get; }

        string LayerName { get; }

        string LayerDescription { get; }

        bool Replace { get; }

        string ReplaceLayerId { get; }
    }
    
    public class SaveSearchViewModel : ViewModelBase, ISaveSearchOptions
    {
        public SaveSearchViewModel()
        {
            // defaults
            Mode = SaveSearchMode.FeatureService;
            LayerName = "New Layer";
        }

        private async void LoadReplacableLayers()
        {
            try
            {
                ReplacableLayers.Clear();

                Debug.Assert(Connection != null);
                if (Connection == null)
                    return;

                var results = await KoopService.GetFeatureLayers(Connection, ServiceName, false);
                foreach (var item in results.FeatureLayers)
                    ReplacableLayers.Add(item);
            }
            catch(Exception e)
            {
                ArcGIS.Desktop.Framework.Dialogs.MessageBox.Show(e.ToString(), "MarkLogic", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        protected override bool SetProperty<T>(ref T storage, T value, [CallerMemberName] string propertyName = "")
        {
            if (base.SetProperty(ref storage, value, propertyName))
            {
                OnPropertyChanged("IsFormValid");
                return true;
            }
            else
                return false;
        }

        public Connection Connection { get; set; }

        private ObservableCollection<FeatureLayerItem> _replacableLayers = new ObservableCollection<FeatureLayerItem>();
        public ObservableCollection<FeatureLayerItem> ReplacableLayers => _replacableLayers;

        private FeatureLayerItem _layerToReplace;
        public FeatureLayerItem LayerToReplace
        {
            get { return _layerToReplace; }
            set
            {
                SetProperty(ref _layerToReplace, value);
                if (value != null)
                {
                    LayerName = value.Name;
                    LayerDescription = value.Description;
                }
            }
        }

        public bool Replace => LayerToReplace != null;

        public string ReplaceLayerId => LayerToReplace?.Id;

        private string _serviceName;
        public string ServiceName
        {
            get { return _serviceName; }
            set
            {
                SetProperty(ref _serviceName, value);
                LoadReplacableLayers();
            }
        }

        private SaveSearchMode _mode;
        public SaveSearchMode Mode
        {
            get { return _mode; }
            set { SetProperty(ref _mode, value); }
        }
        
        private string _layerName;
        public string LayerName
        {
            get { return _layerName; }
            set { SetProperty(ref _layerName, value); }
        }

        private string _layerDescription;
        public string LayerDescription
        {
            get { return _layerDescription; }
            set { SetProperty(ref _layerDescription, value); }
        }
        
        public bool IsFormValid
        {
            get
            {
                var valid = false;

                if (Mode == SaveSearchMode.FeatureLayer)
                {
                    valid = !string.IsNullOrWhiteSpace(LayerName);
                }
                else if (Mode == SaveSearchMode.FeatureService)
                {
                    valid = !string.IsNullOrWhiteSpace(LayerName);
                }

                return valid;
            }
        }
    }
}
