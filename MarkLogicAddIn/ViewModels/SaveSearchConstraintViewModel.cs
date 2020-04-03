using System.Collections.Generic;
using System.Linq;

namespace MarkLogic.Esri.ArcGISPro.AddIn.ViewModels
{
    public class SaveSearchConstraintViewModel : ViewModelBase
    {
        public const int AddNewLayerId = -1;

        private readonly IList<SaveSearchViewModel.Layer> _availableLayers;

        public SaveSearchConstraintViewModel(string constraintName, IList<SaveSearchViewModel.Layer> availableLayers)
        {
            ConstraintName = constraintName;
            _availableLayers = availableLayers;
        }

        public bool Valid => TargetLayerId >= AddNewLayerId && !string.IsNullOrWhiteSpace(LayerName);

        private bool _includeInSave;
        public bool IncludeInSave
        {
            get { return _includeInSave; }
            set 
            { 
                if (SetProperty(ref _includeInSave, value) && !value)
                {
                    TargetLayerId = AddNewLayerId;
                    LayerName = LayerDescription = "";
                }
            }
        }

        private string _constraintName;
        public string ConstraintName
        {
            get { return _constraintName; }
            set { SetProperty(ref _constraintName, value); }
        }

        private int _targetLayerId;
        public int TargetLayerId
        {
            get { return _targetLayerId; }
            set
            {
                if (SetProperty(ref _targetLayerId, value)) {
                    var layer = _targetLayerId == AddNewLayerId ? null : _availableLayers.Where(l => l.Id == value).FirstOrDefault();
                    LayerName = layer?.Name;
                    LayerDescription = layer?.Description;
                }
            }
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
    }
}
