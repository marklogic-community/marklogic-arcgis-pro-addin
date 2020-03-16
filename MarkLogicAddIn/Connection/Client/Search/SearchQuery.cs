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
        public SearchQuery()
        {
            FacetValues = new List<string>();
        }

        public SearchQuery(string query)
            : this()
        {
            QueryText = query;
        }
        
        public SearchQuery(string query, IEnumerable<FacetValue> facetValues) : this(query)
        {
            if (facetValues == null) return;
            foreach (var facetValue in facetValues)
            {
                FacetValues.Add(facetValue.QueryString);
            }
        }

        public string QueryText { get; set; }

        public ICollection<string> FacetValues { get; private set; }

        public bool IsFaceted { get { return FacetValues.Count > 0; } }

        public bool IsEmpty { get { return string.IsNullOrWhiteSpace(FullQuery); } }

        public string FullQuery
        {
            get
            {
                var q = QueryText ?? "";
                var facets = string.Join(" ", FacetValues);
                return string.Join(" ", q.Trim(), facets).Trim();
            }
        }

        private List<StructuredQuery> _additionalQueries;
        [Obsolete]
        public IList<StructuredQuery> AdditionalQueries => _additionalQueries ?? (_additionalQueries = new List<StructuredQuery>());

        public GeospatialBox Viewport { get; set; }
    }
}
