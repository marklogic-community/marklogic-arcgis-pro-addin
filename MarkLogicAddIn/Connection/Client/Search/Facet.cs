using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MarkLogic.Client.Search
{
    public class Facet
    {
        public Facet(string name, string type, IEnumerable<FacetValue> values)
        {
            Name = !string.IsNullOrWhiteSpace(name) ? name : throw new ArgumentNullException("name");
            Type = type;
            Values = new List<FacetValue>(values);
        }

        public Facet(Facet facet) : this(facet.Name, facet.Type, facet.Values)
        {
        }

        public string Name { get; private set; }

        public string Type { get; private set; }

        public ICollection<FacetValue> Values { get; private set; }
    }
}
