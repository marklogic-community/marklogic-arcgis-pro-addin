using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MarkLogic.Extensions.Koop
{
    public class FeatureLayerItem
    {
        private JObject _json;

        public FeatureLayerItem(JObject json)
        {
            _json = json;
        }

        public string Id => _json["id"].Value<string>();

        public string Name => _json["name"].Value<string>();

        public string Description => _json["description"].Value<string>();
    }
}
