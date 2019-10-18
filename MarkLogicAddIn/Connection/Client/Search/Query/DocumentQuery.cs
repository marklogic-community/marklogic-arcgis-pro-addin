using Newtonsoft.Json.Linq;
using System.Collections.Generic;
using System.Linq;

namespace MarkLogic.Client.Search.Query
{
    public class DocumentQuery : StructuredQuery
    {
        public DocumentQuery()
        {
        }

        public DocumentQuery(params string[] uris)
        {
            foreach (var uri in uris)
                Uris.Add(uri);
        }

        private List<string> _uris;
        public IList<string> Uris => _uris ?? (_uris = new List<string>());

        public override JObject ToJson()
        {
            var queryJson = new JObject();
            queryJson.Add("uri", new JArray(Uris.ToArray()));
            var json = new JObject();
            json.Add("document-query", queryJson);
            return json;
        }
    }
}
