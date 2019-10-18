using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json.Linq;

namespace MarkLogic.Client.Search.Query
{
    public abstract class ConstraintQuery : StructuredQuery
    {
        public string ConstraintName { get; set; }

        protected void SerializeConstraintJson(JObject json)
        {
            json.Add("constraint-name", new JValue(ConstraintName));
        }
    }
}
