using System;
using System.Collections.Generic;

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

        public string Name { get; private set; }

        public string Type { get; private set; }

        public ICollection<FacetValue> Values { get; private set; }
    }
}
