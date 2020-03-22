using MarkLogic.Client.Search.Query;
using System;
using System.Collections.Generic;

namespace MarkLogic.Client.Search
{
    [Flags]
    public enum ReturnOptions
    {
        Results = 1,
        Facets = 2,
        Values = 4,
        Suggest = 8
    }

    public class SearchQuery
    {
        public const ReturnOptions DefaultReturnOptions = ReturnOptions.Results | ReturnOptions.Facets | ReturnOptions.Values;

        private Dictionary<string, HashSet<string>> _facets = new Dictionary<string, HashSet<string>>();

        public SearchQuery()
        {
            // defaults
            ReturnOptions = DefaultReturnOptions;
            Start = 1;
            PageLength = 10;
            AggregateValues = true;
            ValuesLimit = 0;
            MaxLonDivs = MaxLatDivs = 100;
        }

        public ReturnOptions ReturnOptions { get; set; }

        public long Start { get; set; }

        public int PageLength { get; set; }

        public string QueryText { get; set; }

        public void AddFacetValue(string facetName, string valueName)
        {
            if (!_facets.TryGetValue(facetName, out HashSet<string> facetValues))
            {
                facetValues = new HashSet<string>();
                _facets.Add(facetName, facetValues);
            }
            facetValues.Add(valueName);
        }

        public ICollection<string> FacetNames => _facets.Keys;

        public IEnumerable<string> GetFacetValues(string facetName)
        {
            return _facets.TryGetValue(facetName, out HashSet<string> facetValues) ? facetValues : new string[0] as IEnumerable<string>;
        }

        public bool AggregateValues { get; set; }

        public long ValuesLimit { get; set; }

        public GeospatialBox Viewport { get; set; }

        private uint _maxLonDivs;
        public uint MaxLonDivs
        {
            get { return _maxLonDivs; }
            set { _maxLonDivs = (value >= 1 && value <= 100) ? value : throw new ArgumentOutOfRangeException("value"); }
        }

        private uint _maxLatDivs;
        public uint MaxLatDivs
        {
            get { return _maxLatDivs; }
            set { _maxLatDivs = (value >= 1 && value <= 100) ? value : throw new ArgumentOutOfRangeException("value"); }
        }

        private List<StructuredQuery> _additionalQueries;
        public IList<StructuredQuery> AdditionalQueries => _additionalQueries ?? (_additionalQueries = new List<StructuredQuery>());
    }
}
