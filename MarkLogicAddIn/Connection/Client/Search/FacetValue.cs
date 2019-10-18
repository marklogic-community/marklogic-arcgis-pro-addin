using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MarkLogic.Client.Search
{
    public class FacetValue
    {
        public FacetValue(FacetValue facetValue)
        {
            if (facetValue == null) throw new ArgumentNullException("facetValue");
            FacetName = facetValue.FacetName;
            ValueName = facetValue.ValueName;
            Value = facetValue.Value;
            Count = facetValue.Count;
        }

        public FacetValue(string facetName, string value) : this(facetName, "", value, -1)
        {
        }

        public FacetValue(string facetName, string valueName, string value, long count)
        {
            FacetName = !string.IsNullOrWhiteSpace(facetName) ? facetName : throw new ArgumentNullException("facetName");
            ValueName = valueName;
            Value = value;
            Count = count;
        }

        public override bool Equals(object obj)
        {
            if (obj == null)
                return false;
            if (!(obj is FacetValue))
                return false;
            return (obj as FacetValue).QueryString == this.QueryString;
        }

        public override int GetHashCode()
        {
            return QueryString.GetHashCode();
        }

        public string FacetName { get; private set; }

        public string ValueName { get; private set; }

        public string Value { get; private set; }

        public long Count { get; private set; }

        public bool Selected { get; set; }

        public string QueryString { get { return $"{FacetName}:\"{ValueName}\""; } }

        public bool ContainsQueryString(string str)
        {
            var qsnq = $"{FacetName}:{ValueName}";
            if (str.Contains(QueryString))
                return true;
            else if (str.Contains(qsnq))
                return true;
            else
                return false;
        }
    }
}
