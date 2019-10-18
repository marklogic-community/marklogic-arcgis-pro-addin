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
    public class CreateFeatureLayerResults
    {
        private JObject _response;

        public CreateFeatureLayerResults(string responseContent)
        {
            RawContent = responseContent;
            var json = JsonConvert.DeserializeObject(responseContent);
            Debug.Assert(json != null && json.GetType() == typeof(JObject));
            _response = (JObject)json;
        }

        public string LayerUrl => _response["featureLayerUrl"].Value<string>();

        public string ServiceUrl => _response["featureServiceUrl"].Value<string>();

        public string LayerId => _response["layerId"].Value<string>();

        public string RawContent { get; private set; }
    }
}
