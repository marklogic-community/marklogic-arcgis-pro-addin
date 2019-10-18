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
    public class FeatureLayerResults
    {
        private JObject _response;

        public FeatureLayerResults(string responseContent)
        {
            RawContent = responseContent;
            var json = JsonConvert.DeserializeObject(responseContent);
            Debug.Assert(json != null && json.GetType() == typeof(JObject));
            _response = (JObject)json;
        }

        private static IEnumerable<FeatureLayerItem> ReadFeatureLayers(JObject response)
        {
            var list = new List<FeatureLayerItem>();
            var results = response.Value<JArray>("layers");
            if (results != null && results.HasValues)
                list.AddRange(results.Values<JObject>().Select(o => new FeatureLayerItem(o)));
            return list;
        }

        public string RawContent { get; private set; }

        private List<FeatureLayerItem> _featureLayerItems;
        public IEnumerable<FeatureLayerItem> FeatureLayers => _featureLayerItems ?? (_featureLayerItems = new List<FeatureLayerItem>(ReadFeatureLayers(_response)));
    }
}
