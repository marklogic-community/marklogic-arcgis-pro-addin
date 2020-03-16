using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MarkLogic.Client.Search
{
    public class SearchResults
    {
        private JObject _response;
        private IDictionary<string, Facet> _facets = null;
        private List<SearchResult> _results = null;

        public SearchResults(string responseContent)
        {
            RawContent = responseContent;
            var json = JsonConvert.DeserializeObject(responseContent);
            Debug.Assert(json != null && json.GetType() == typeof(JObject));
            _response = (JObject)json;
        }

        private static IDictionary<string, Facet> ReadFacets(JObject response)
        {
            var map = new Dictionary<string, Facet>();
            var facets = response.Value<JObject>("facets");
            if (facets != null)
            {
                foreach (var facetName in facets.Properties().Select(p => p.Name))
                {
                    var facet = facets.Value<JObject>(facetName);
                    var facetValues = facet.Value<JArray>("facetValues").Select((fv) => new FacetValue(facetName, fv.Value<string>("name"), fv.Value<string>("value"), fv.Value<long>("count")));
                    map.Add(facetName, new Facet(facetName, facet.Value<string>("type"), facetValues));
                }
            }

            return map;
        }

        private static List<SearchResult> ReadResults(JObject response)
        {
            var list = new List<SearchResult>();
            var results = response.Value<JArray>("results");
            if (results != null && results.HasValues)
                list.AddRange(results.Values<JObject>().Select(o => new SearchResult(o)));
            return list;
        }

        public string RawContent { get; private set; }

        public long Total { get { return _response.Value<long>("total"); } }

        public int PageLength { get { return _response.Value<int>("page-length"); } }

        public long Start { get { return _response.Value<long>("start"); } }

        public IDictionary<string, Facet> Facets { get { return _facets ?? (_facets = ReadFacets(_response)); } }

        public IEnumerable<FacetValue> SelectedFacetValues => Facets.Values.SelectMany(f => f.Values).Where(v => v.Selected);

        public IEnumerable<SearchResult> Results => _results ?? (_results = ReadResults(_response));

        public bool IsFirstPage
        {
            get { return Start == 1; }
        }

        public bool IsLastPage
        {
            get { return (Start + PageLength) >= Total; }
        }

        public long PrevStart
        {
            get { return IsFirstPage ? Start : Start - PageLength; }
        }

        public long NextStart
        {
            get { return IsLastPage ? Start : Start + PageLength; }
        }

        public long CurrentPage
        {
            get { return (Start / PageLength) + 1; }
        }

        public long TotalPages
        {
            get { return Math.Max(Total / PageLength, 1); }
        }

        public IEnumerable<string> ValueNames => _response.Value<JObject>("values").Properties().Select(p => p.Name);

        public IEnumerable<ValuePoint> GetValuePoints(string valueName)
        {
            var points = _response.SelectToken("$.values." + valueName + ".points");
            if (points == null || points.Type != JTokenType.Array)
                return new ValuePoint[0];
            return (points as JArray).Select(obj => new ValuePoint()
            {
                Count = obj.Value<ulong>("count"),
                Latitude = obj.Value<double>("lat"),
                Longitude = obj.Value<double>("lon")
            });
        }

        public IEnumerable<ValuePointCluster> GetValuePointClusters(string valueName)
        {
            var pointClusters = _response.SelectToken("$.values." + valueName + ".pointClusters");
            if (pointClusters == null || pointClusters.Type != JTokenType.Array)
                return new ValuePointCluster[0];
            return (pointClusters as JArray).Select(obj => new ValuePointCluster()
            {
                Count = obj.Value<ulong>("count"),
                South = obj.Value<double>("s"),
                West = obj.Value<double>("w"),
                North = obj.Value<double>("n"),
                East = obj.Value<double>("e")
            });
        }
    }

    public class ValuePoint
    {
        public ulong Count { get; set; }

        public double Latitude { get; set; }

        public double Longitude { get; set; }
    }

    public class ValuePointCluster
    {
        public ulong Count { get; set; }

        public double South { get; set; }

        public double West { get; set; }

        public double North { get; set; }

        public double East { get; set; }

        public double Latitude => ((North - South) / 2) + South;

        public double Longitude => ((East - West) / 2) + West;
    }
}
