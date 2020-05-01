using System;

namespace MarkLogic.Client.Search
{
    public class FacetValue
    {
        public FacetValue(string facetName, string valueName, string value, long count)
        {
            FacetName = !string.IsNullOrWhiteSpace(facetName) ? facetName : throw new ArgumentNullException("facetName");
            ValueName = valueName;
            Value = value;
            Count = count;
        }

        public override bool Equals(object obj)
        {
            if (obj == null || (obj != null && !(obj is FacetValue)))
                return false;
            return GetConstraintTerm(obj as FacetValue) == GetConstraintTerm(this);
        }

        public override int GetHashCode() => GetConstraintTerm(this).GetHashCode();

        public string FacetName { get; private set; }

        public string ValueName { get; private set; }

        public string Value { get; private set; }

        public long Count { get; private set; }

        public static string GetConstraintTerm(string facetName, string valueName) => $"{facetName}:\"{valueName}\"";

        public static string GetConstraintTerm(FacetValue facetValue) => GetConstraintTerm(facetValue.FacetName, facetValue.ValueName);
    }
}
