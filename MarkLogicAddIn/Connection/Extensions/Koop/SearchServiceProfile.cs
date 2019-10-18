using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MarkLogic.Extensions.Koop
{
    public class SearchServiceProfile
    {
        private JObject _json;
        
        public SearchServiceProfile(JObject json)
        {
            _json = json;
        }

        public string FlattenedName
        {
            get { return (HasName && HasServiceName) ? string.Concat(ServiceName, ": ", Name) : ""; }
        }
        
        public string Name => _json["name"].Value<string>();

        public bool HasName => !string.IsNullOrWhiteSpace(Name);

        public string ServiceName => _json["serviceName"].Value<string>();

        public string ServiceUrl => _json["serviceUrl"].Value<string>();

        public bool HasServiceName => !string.IsNullOrWhiteSpace(ServiceName);
        
        public string Schema => _json["schema"].Value<string>();

        public string View => _json["view"].Value<string>();

        public string SearchOptions => _json["options"].Value<string>();

        public string GeometryType => _json["geometryType"].Value<string>();

        public string GeoConstraint => _json["geoConstraint"].Value<string>();

        public string Values => _json["values"].Value<string>();

        public string DocTransform => _json["docTransform"].Value<string>();
        
        public bool HasDocTransform => !string.IsNullOrWhiteSpace(DocTransform);
    }
}
