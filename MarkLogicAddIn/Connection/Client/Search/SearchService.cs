using MarkLogic.Client.Search.Query;
using MarkLogic.Esri.ArcGISPro.AddIn;
using MarkLogic.Extensions.Koop;
using Newtonsoft.Json;
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

        private JObject CreateSearchInput(SearchQuery query)
        {
            var inputSearch = new JObject();
            inputSearch.Add("qtext", query.QueryText);
            inputSearch.Add("start", query.Start);
            inputSearch.Add("pageLength", query.PageLength);
            if (query.FacetNames.Count > 0)
            {
                var facets = new JObject();
                foreach (var facetName in query.FacetNames)
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

            return inputSearch;
        }

        public async Task<SearchResults> Search(Connection connection, SearchQuery query, ServiceModel serviceModel)
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

            var input = new JObject();
            input.Add("params", inputParams);
            input.Add("search", CreateSearchInput(query));

            var msg = new HttpRequestMessage(HttpMethod.Post, ub.Uri);
            msg.Content = new StringContent(input.ToString(), Encoding.UTF8, "application/json");
            
            using (msg)
            {
                using (var response = await connection.SendAsync(msg))
                {
                    response.EnsureSuccessStatusCode();
                    var responseContent = await response.Content.ReadAsStringAsync();
                    var results = new SearchResults(responseContent, query);
                    return results;
                }
            }
        }

        public async Task<SaveSearchResults> SaveSearch(Connection connection, SaveSearchRequest request)
        {
            var ub = new UriBuilder(connection.Profile.Uri) { Path = "v1/resources/geoSearchService" };

            var inputParams = new JObject();
            inputParams.Add("id", request.ServiceModel.Id);

            var paramsLayers = new JObject();
            foreach (var layer in request.Layers)
            {
                var layerObj = new JObject();
                if (layer.TargetLayerId == null)
                    layerObj.Add("layerId", "new");
                else
                    layerObj.Add("layerId", layer.TargetLayerId);
                layerObj.Add("name", layer.Name);
                layerObj.Add("description", layer.Description);

                paramsLayers.Add(layer.SourceLayerId.ToString(), layerObj);
            }
            if (paramsLayers.Count > 0)
                inputParams.Add("layers", paramsLayers);
            
            var input = new JObject();
            input.Add("params", inputParams);
            input.Add("search", CreateSearchInput(request.Query));

            var msg = new HttpRequestMessage(HttpMethod.Put, ub.Uri);
            msg.Content = new StringContent(input.ToString(), Encoding.UTF8, "application/json");

            using (msg)
            {
                using (var response = await connection.SendAsync(msg))
                {
                    response.EnsureSuccessStatusCode();
                    var responseContent = await response.Content.ReadAsStringAsync();
                    try
                    {
                        var json = (JObject)JsonConvert.DeserializeObject(responseContent);
                        var modelId = json.Value<string>("id");

                        var layers = new List<SaveSearchResults.Layer>();
                        var layersObj = json.Value<JObject>("layers");
                        layersObj.Properties().ToList().ForEach(p =>
                        {
                            var propObj = p.Value;
                            layers.Add(new SaveSearchResults.Layer(propObj.Value<int>("layerId"), propObj.Value<string>("name"), p.Name));
                        });
                        
                        // TODO: uri assumes standard Koop route
                        var fsub = new UriBuilder(connection.Profile.Uri) { Path = $"marklogic/{modelId}/FeatureServer" };

                        return new SaveSearchResults(modelId, fsub.Uri, layers);
                    }
                    catch (JsonException e)
                    {
                        throw new InvalidOperationException("An error occurred while trying to parse response JSON.", e);
                    }
                }
            }
        }
    }
}
