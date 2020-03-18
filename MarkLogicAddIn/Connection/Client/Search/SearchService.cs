using MarkLogic.Client.Search.Query;
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

        private bool RequiresPayload(SearchQuery query)
        {
            return query.AdditionalQueries.Count > 0;
        }

        private void PreparePayload(HttpRequestMessage msg, SearchQuery query)
        {
            var addtlQueries = new JObject();
            addtlQueries.Add("queries", new JArray(query.AdditionalQueries.Select(q => q.ToJson())));
            var payload = new JObject();
            payload.Add("query", addtlQueries);
            msg.Content = new StringContent(payload.ToString(), Encoding.UTF8, "application/json");
        }
        
        public async Task<SearchResults> Search(Connection connection, SearchQuery query, IServiceModel serviceModel, long start = 1)
        {
            var ub = new UriBuilder(connection.Profile.Uri) { Path = "v1/resources/geoSearchService" };

            var inputParams = new JObject();
            inputParams.Add("id", serviceModel.Id);
            inputParams.Add("request", new JArray("results", "facets", "values"));
            inputParams.Add("aggregateValues", true);

            var inputSearch = new JObject();
            inputSearch.Add("qtext", query.QueryText);
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
                viewport.Add("maxLatDivs", 50); // TODO: should be user configurable
                viewport.Add("maxLonDivs", 25);
                inputSearch.Add("viewport", viewport);
            }
            
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
                    var results = new SearchResults(responseContent);

                    // set selected facets
                    /*if (results.Facets.Count > 0)
                    {
                        var allFacetValues = results.Facets.Values.SelectMany(f => f.Values).ToArray();

                        // from external hint
                        foreach (var facetValue in query.FacetValues)
                        {
                            var facetValueToSelect = allFacetValues.Where(v => v.QueryString == facetValue).FirstOrDefault();
                            if (facetValueToSelect != null)
                                facetValueToSelect.Selected = true;
                        }

                        // from qtext
                        foreach (var facetValue in allFacetValues)
                        {
                            if (facetValue.ContainsQueryString(query.FullQuery))
                                facetValue.Selected = true;
                        }
                    }*/
                    return results;
                }
            }
        }

        public async Task<ValuesResults> Values(Connection connection, SearchQuery query, string searchOptions, string valuesName)
        {
            const uint limit = 1000000;

            var ub = new UriBuilder(connection.Profile.Uri) { Path = $"v1/values/{valuesName}" };
            ub.AddQueryParam("options", searchOptions);
            ub.AddQueryParam("limit", limit);
            //ub.AddQueryParam("q", query.FullQuery);

            var hasPayload = RequiresPayload(query);
            var msg = new HttpRequestMessage(hasPayload ? HttpMethod.Post : HttpMethod.Get, ub.Uri);
            if (hasPayload)
                PreparePayload(msg, query);

            using (msg)
            {
                using (var response = await connection.SendAsync(msg))
                {
                    response.EnsureSuccessStatusCode();
                    var responseContent = await response.Content.ReadAsStringAsync();
                    return new ValuesResults(responseContent);
                }
            }
        }

        public async Task<SuggestionResults> Suggest(Connection connection, string partialQ, string searchOptions, uint limit = 10)
        {
            var ub = new UriBuilder(connection.Profile.Uri) { Path = "v1/suggest" };
            ub.AddQueryParam("options", searchOptions);
            ub.AddQueryParam("partial-q", partialQ);
            ub.AddQueryParam("limit", limit);

            var msg = new HttpRequestMessage(HttpMethod.Get, ub.Uri);

            using (msg)
            {
                using (var response = await connection.SendAsync(msg))
                {
                    response.EnsureSuccessStatusCode();
                    var responseContent = await response.Content.ReadAsStringAsync();
                    return new SuggestionResults(responseContent);
                }
            }
        }

        public async Task<SearchOptions> ConfigQuery(Connection connection, string searchOptions)
        {
            var ub = new UriBuilder(connection.Profile.Uri) { Path = $"v1/config/query/{searchOptions}" };
            ub.AddQueryParam("format", "xml");

            var msg = new HttpRequestMessage(HttpMethod.Get, ub.Uri);

            using (msg)
            {
                using (var response = await connection.SendAsync(msg))
                {
                    response.EnsureSuccessStatusCode();
                    var responseContent = await response.Content.ReadAsStringAsync();
                    return new SearchOptions(responseContent);
                }
            }
        }
    }
}
