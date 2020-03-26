using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MarkLogic.Esri.ArcGISPro.AddIn.ViewModels
{
    public class SaveSearchConstraintViewModel : ViewModelBase
    {
        public const int AddNewLayerId = -1;

        public SaveSearchConstraintViewModel(string constraintName)
        {
            ConstraintName = constraintName;
        }

        private bool _includeInSave;
        public bool IncludeInSave
        {
            get { return _includeInSave; }
            set { SetProperty(ref _includeInSave, value); }
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
            set { SetProperty(ref _targetLayerId, value); }
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
