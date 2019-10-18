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
    public class SuggestionResults
    {
        private JObject _response;
        private JArray _suggestions;

        public SuggestionResults(string responseContent)
        {
            RawContent = responseContent;
            var json = JsonConvert.DeserializeObject(responseContent);
            Debug.Assert(json != null && json.GetType() == typeof(JObject));
            _response = (JObject)json;
            _suggestions = _response.Value<JArray>("suggestions");
        }

        public string RawContent { get; private set; }

        public IEnumerable<string> Suggestions => _suggestions.Values<string>();
    }
}
