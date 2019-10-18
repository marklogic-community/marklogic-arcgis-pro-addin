using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MarkLogic.Extensions.Koop
{
    public class CreateFeatureLayerOptions
    {
        public string LayerName { get; set; }

        public string LayerDescription { get; set; }

        public string SearchOptions { get; set; }

        public string FeatureServiceName { get; set; }

        public string FeatureServiceSchema { get; set; }

        public string FeatureServiceView { get; set; }
    }
}
