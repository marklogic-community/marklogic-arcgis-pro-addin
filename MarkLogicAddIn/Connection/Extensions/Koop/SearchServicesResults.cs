using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MarkLogic.Extensions.Koop
{
    public class SearchServicesResults
    {
        private List<FeatureServerProfile> _featureServers;
        private List<SearchServiceProfile> _searchServices;
        private JObject _response;

        public SearchServicesResults(string responseContent)
        {
            RawContent = responseContent;
            var json = JsonConvert.DeserializeObject(responseContent);
            Debug.Assert(json != null && json.GetType() == typeof(JObject));
            _response = (JObject)json;

            _searchServices = new List<SearchServiceProfile>();
            var results = _response.Value<JArray>("searchServices");
            if (results != null && results.HasValues)
                _searchServices.AddRange(results.Values<JObject>().Select(o => new SearchServiceProfile(o)));

            _featureServers = new List<FeatureServerProfile>();
            foreach (var serviceName in _searchServices.Select(s => s.ServiceName).Distinct())
                _featureServers.Add(new FeatureServerProfile(
                    serviceName, 
                    _searchServices.Where(s => s.ServiceName == serviceName).First().ServiceUrl,
                    _searchServices.Where(s => s.ServiceName == serviceName)));
        }
        
        public string RawContent { get; private set; }

        public IEnumerable<SearchServiceProfile> AllSearchServices => _searchServices;

        public IEnumerable<FeatureServerProfile> FeatureServers => _featureServers;
    }
}
