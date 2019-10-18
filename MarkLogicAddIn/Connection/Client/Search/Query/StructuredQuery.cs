using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MarkLogic.Client.Search.Query
{
    public abstract class StructuredQuery
    {
        public abstract JObject ToJson();
    }
}
