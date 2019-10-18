using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MarkLogic.Client.Search
{
    public class SearchResult
    {
        private JObject _json;
        private List<MatchComponent> _matches = null;

        public SearchResult(JObject json)
        {
            _json = json;
        }

        private static List<MatchComponent> ReadMatches(JArray matches)
        {
            var list = new List<MatchComponent>();
            if (matches.HasValues && matches.Count > 0)
            {
                foreach (var match in matches)
                {
                    var path = match["path"].Value<string>();
                    foreach(var token in match["match-text"])
                    {
                        if (token.Type == JTokenType.String)
                            list.Add(new MatchComponent() { Path = path, IsHighlight = false, Text = token.Value<string>() });
                        else if (token.Type == JTokenType.Object)
                            list.Add(new MatchComponent() { Path = path, IsHighlight = true, Text = token["highlight"].Value<string>() });
                    }
                }
            }
            return list;
        }

        public string Uri => _json["uri"].Value<string>();

        public int Index => _json["index"].Value<int>();

        public IEnumerable<MatchComponent> Matches => _matches ?? (_matches = ReadMatches((JArray)_json["matches"]));

        public string MatchesFullText => string.Concat(Matches.Select(m => m.Text));
    }

    public class MatchComponent
    {
        public string Path { get; set; }

        public bool IsHighlight { get; set; }

        public string Text { get; set; }
    }
}
