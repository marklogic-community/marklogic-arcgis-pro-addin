using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json.Linq;

namespace MarkLogic.Client.Search.Query
{
    public class GeospatialConstraintQuery : ConstraintQuery
    {
        private List<GeospatialBox> _boxes;
        public IList<GeospatialBox> Boxes => _boxes ?? (_boxes = new List<GeospatialBox>());

        public override JObject ToJson()
        {
            var constraintJson = new JObject();
            SerializeConstraintJson(constraintJson);
            if (Boxes.Count > 0)
                constraintJson.Add("box", new JArray(Boxes.Select(b => b.ToJson())));
            var json = new JObject();
            json.Add("geospatial-constraint-query", constraintJson);
            return json;
        }
    }

    public class GeospatialBox
    {
        public double South { get; set; }

        public double West { get; set; }

        public double North { get; set; }

        public double East { get; set; }

        public JObject ToJson()
        {
            var json = new JObject();
            json.Add("south", new JValue(South));
            json.Add("west", new JValue(West));
            json.Add("north", new JValue(North));
            json.Add("east", new JValue(East));
            return json;
        }
    }
}
