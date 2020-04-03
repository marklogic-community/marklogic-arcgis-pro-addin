﻿using MarkLogic.Client.Search;
using MarkLogic.Esri.ArcGISPro.AddIn.Feature;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace MarkLogic.Extensions.Koop
{
    public class KoopService
    {
        public static async Task<IEnumerable<ServiceModel>> GetServiceModels(Connection connection)
        {
            var ub = new UriBuilder(connection.Profile.Uri) { Path = "v1/resources/modelService", Query = "rs:filter=search" };
            using (var msg = new HttpRequestMessage(HttpMethod.Get, ub.Uri))
            {
                using (var response = await connection.SendAsync(msg))
                {
                    response.EnsureSuccessStatusCode();
                    var responseContent = await response.Content.ReadAsStringAsync();
                    try
                    {
                        var json = (JObject)JsonConvert.DeserializeObject(responseContent);
                        return json.Value<JObject>("models").PropertyValues().Select(m => {
                            var modelId = m.Value<string>("id");
                            var geoConstraints = new HashSet<string>(
                                m.SelectTokens("layers[*].geoConstraint")
                                    .Select(t => t.Value<string>()));

                            // TODO: uri assumes standard Koop route
                            var fsub = new UriBuilder(connection.Profile.Uri) { Path = $"marklogic/{modelId}/FeatureServer" };

                            return new ServiceModel(
                                fsub.Uri,
                                modelId,
                                m.Value<string>("name"),
                                m.Value<string>("description"),
                                geoConstraints.ToArray(),
                                m.SelectToken("search.docTransform", true).Value<string>(),
                                m.SelectToken("search.constraints", true).Values<JObject>()
                                    .Select(o => new ServiceModelConstraint(
                                        o.Value<string>("name"), 
                                        o.Value<string>("description"))),
                                m.Value<JArray>("layers").Values<JObject>().Select(o =>
                                {
                                    var geometryType = (GeometryType)Enum.Parse(typeof(GeometryType), o.Value<string>("geometryType"), true);
                                    return new ServiceModelLayer(
                                        o.Value<int>("id"), 
                                        o.Value<string>("name"), 
                                        o.Value<string>("description"),
                                        geometryType,
                                        o.Value<string>("geoConstraint"));
                                }));
                        });
                    }
                    catch(JsonException e)
                    {
                        throw new InvalidOperationException("An error occurred while trying to parse response JSON.", e);
                    }
                }
            }
        }

        private const string KoopFeatureLayerPath = "v1/resources/KoopFeatureLayer";

        public static async Task<FeatureLayerResults> GetFeatureLayers(Connection connection, string serviceName, bool? readOnly = null)
        {
            var ub = new UriBuilder(connection.Profile.Uri) { Path = KoopFeatureLayerPath };
            ub.AddQueryParam("rs:service", serviceName);

            if (readOnly.HasValue)
                ub.AddQueryParam("rs:readOnly", readOnly.Value);

            using (var msg = new HttpRequestMessage(HttpMethod.Get, ub.Uri))
            {
                using (var response = await connection.SendAsync(msg))
                {
                    response.EnsureSuccessStatusCode();
                    var responseContent = await response.Content.ReadAsStringAsync();
                    var results = new FeatureLayerResults(responseContent);
                    return results;
                }
            }
        }

        public static async Task<CreateFeatureLayerResults> CreateFeatureLayer(Connection connection, SearchQuery query, CreateFeatureLayerOptions createOptions, string layerId = null)
        {
            var isReplacing = !string.IsNullOrWhiteSpace(layerId);

            var ub = new UriBuilder(connection.Profile.Uri) { Path = KoopFeatureLayerPath };
            ub.AddQueryParam("rs:layerName", createOptions.LayerName);
            ub.AddQueryParam("rs:layerDescription", createOptions.LayerDescription);
            ub.AddQueryParam("rs:service", createOptions.FeatureServiceName);
            ub.AddQueryParam("rs:schema", createOptions.FeatureServiceSchema);
            ub.AddQueryParam("rs:view", createOptions.FeatureServiceView);
            
            var structuredQuery = "<cts:and-query xmlns:cts=\"http://marklogic.com/cts\"></cts:and-query>"; // default
            if (query.AdditionalQueries.Count > 0)
            {
                var addtlQueries = new JObject();
                addtlQueries.Add("queries", new JArray(query.AdditionalQueries.Select(q => q.ToJson())));
                var payload = new JObject();
                payload.Add("query", addtlQueries);
                structuredQuery = payload.ToString();
            }
            ub.AddQueryParam("rs:query", structuredQuery);
            ub.AddQueryParam("rs:qtext", query.QueryText);
            ub.AddQueryParam("rs:searchOptions", createOptions.SearchOptions);

            if (isReplacing)
                ub.AddQueryParam("rs:layerId", layerId);
            
            using (var msg = new HttpRequestMessage(HttpMethod.Put, ub.Uri))
            {
                using (var response = await connection.SendAsync(msg))
                {
                    response.EnsureSuccessStatusCode();
                    var responseContent = await response.Content.ReadAsStringAsync();
                    return new CreateFeatureLayerResults(responseContent);
                }
            }
        }
    }
}
