using MarkLogic.Client.Search;
using System.Collections.ObjectModel;
using System.Linq;

namespace MarkLogic.Esri.ArcGISPro.AddIn.ViewModels
{
    public class FacetViewModel
    {
        public FacetViewModel(Facet facet)
        {
            Name = facet.Name;
            Type = facet.Type;
            Values = new ObservableCollection<FacetValueViewModel>(facet.Values.Select(v => new FacetValueViewModel(v)));
        }

        public string Name { get; private set; }

        public string Type { get; private set; }

        public ObservableCollection<FacetValueViewModel> Values { get; private set; }
    }
}