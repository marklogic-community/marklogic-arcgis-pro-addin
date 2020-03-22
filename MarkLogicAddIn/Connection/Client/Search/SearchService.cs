using MarkLogic.Client.Search.Query;
using MarkLogic.Esri.ArcGISPro.AddIn;
using MarkLogic.Extensions.Koop;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace MarkLogic.Client.Search
{
    public class SearchService
    {
        public const int DefaultPageLength = 10;

        private static SearchService _instance;

        private SearchService()
        {
        }

        public static SearchService Instance => _instance ?? (_instance = new SearchService());

        public async Task<SearchResults> Search(Connection connection, SearchQuery query, IServiceModel serviceModel)
        {
            var ub = new UriBuilder(connection.Profile.Uri) { Path = "v1/resources/geoSearchService" };

            var paramRequests = new List<string>();
            if (query.ReturnOptions.HasFlag(ReturnOptions.Results)) paramRequests.Add("results");
            if (query.ReturnOptions.HasFlag(ReturnOptions.Facets)) paramRequests.Add("facets");
            if (query.ReturnOptions.HasFlag(ReturnOptions.Values)) paramRequests.Add("values");
            if (query.ReturnOptions.HasFlag(ReturnOptions.Suggest)) paramRequests.Add("suggest");

            var inputParams = new JObject();
            inputParams.Add("id", serviceModel.Id);
            inputParams.Add("request", new JArray(paramRequests));
            inputParams.Add("aggregateValues", query.AggregateValues);
            if (query.ValuesLimit > 0)
                inputParams.Add("valuesLimit", query.ValuesLimit);

            var inputSearch = new JObject();
            inputSearch.Add("qtext", query.QueryText);
            inputSearch.Add("start", query.Start);
            inputSearch.Add("pageLength", query.PageLength);
            if (query.FacetNames.Count > 0)
            {
                var facets = new JObject();
                foreach(var facetName in query.FacetNames)
                    facets.Add(facetName, new JArray(query.GetFacetValues(facetName).Cast<object>().ToArray()));
                inputSearch.Add("facets", facets);
            }
            if (query.Viewport != null)
            {
                var viewport = new JObject();
                var box = new JObject();
                box.Add("s", query.Viewport.South);
                box.Add("w", query.Viewport.West);
                box.Add("n", query.Viewport.North);
                box.Add("e", query.Viewport.East);
                viewport.Add("box", box);
                viewport.Add("maxLonDivs", query.MaxLonDivs);
                viewport.Add("maxLatDivs", query.MaxLatDivs);
                inputSearch.Add("viewport", viewport);
            }
            if (query.AdditionalQueries.Count > 0)
                inputSearch.Add("queries", new JArray(query.AdditionalQueries.Select(q => q.ToJson())));

            var input = new JObject();
            input.Add("params", inputParams);
            input.Add("search", inputSearch);

            var msg = new HttpRequestMessage(HttpMethod.Post, ub.Uri);
            msg.Content = new StringContent(input.ToString(), Encoding.UTF8, "application/json");
            
            using (msg)
            {
                using (var response = await connection.SendAsync(msg))
                {
                    response.EnsureSuccessStatusCode();
                    var responseContent = await response.Content.ReadAsStringAsync();
                    var results = new SearchResults(responseContent, query.ReturnOptions);
                    return results;
                }
            }
        }
    }
}
