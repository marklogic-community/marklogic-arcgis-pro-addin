using MarkLogic.Client.Search.Query;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MarkLogic.Client.Search
{
    public class SearchQuery
    {
        private Dictionary<string, HashSet<string>> _facets;

        public SearchQuery()
        {
            _facets = new Dictionary<string, HashSet<string>>();
        }

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

        public GeospatialBox Viewport { get; set; }

        private List<StructuredQuery> _additionalQueries;
        public IList<StructuredQuery> AdditionalQueries => _additionalQueries ?? (_additionalQueries = new List<StructuredQuery>());
    }
}
