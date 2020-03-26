using MarkLogic.Esri.ArcGISPro.AddIn.Commands;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
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
        }

        public SaveSearchViewModel()
        {
        }

        public ObservableCollection<Layer> AvailableLayers { get; } = new ObservableCollection<Layer>();

        public ObservableCollection<SaveSearchConstraintViewModel> ConstraintsToSave { get; } = new ObservableCollection<SaveSearchConstraintViewModel>();

        private ServerCommand _cmdSave;
        public ICommand Save => _cmdSave ?? (_cmdSave = new ServerCommand(o =>
        {
            return Task.FromResult(0);
        }, o => ConstraintsToSave.Where(c => c.IncludeInSave).Count() > 0));
    }
}
