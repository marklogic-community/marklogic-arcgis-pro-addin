using MarkLogic.Client.Search;

namespace MarkLogic.Esri.ArcGISPro.AddIn.ViewModels
{
    public class FacetValueViewModel
    {
        public FacetValueViewModel(FacetValue facetValue)
        {
            FacetName = facetValue.FacetName;
            ValueName = facetValue.ValueName;
            Value = facetValue.Value;
            Count = facetValue.Count;
        }

        public override bool Equals(object obj)
        {
            if (obj == null || (obj != null && !(obj is FacetValueViewModel)))
                return false;
            else
            {
                var fv = obj as FacetValueViewModel;
                return FacetValue.GetConstraintTerm(FacetName, ValueName) == FacetValue.GetConstraintTerm(fv.FacetName, fv.ValueName);
            }
        }

        public override int GetHashCode()
        {
            return FacetValue.GetConstraintTerm(FacetName, ValueName).GetHashCode();
        }

        public string FacetName { get; private set; }

        public string ValueName { get; private set; }

        public string Value { get; private set; }

        public long Count { get; private set; }

        public bool Selected { get; set; }
    }
}
