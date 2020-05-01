using MarkLogic.Extensions.Koop;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MarkLogic.Client.Search
{
    public class SaveSearchRequest
    {
        public class TargetLayer
        {
            public int SourceLayerId { get; set; }

            public int? TargetLayerId { get; set; }

            public string Name { get; set; }

            public string Description { get; set; }
        }

        private List<TargetLayer> _layers = new List<TargetLayer>();

        public SaveSearchRequest(ServiceModel serviceModel, SearchQuery query)
        {
            ServiceModel = serviceModel;
            Query = query;
        }

        public ServiceModel ServiceModel { get; private set; }

        public SearchQuery Query { get; private set; }

        public IList<TargetLayer> Layers => _layers;
    }
}
