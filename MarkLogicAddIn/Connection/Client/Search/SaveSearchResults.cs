using System;
using System.Collections.Generic;
using System.Linq;

namespace MarkLogic.Client.Search
{
    public class SaveSearchResults
    {
        public class Layer
        {
            public Layer(int id, string name, string geoConstraint, Uri uri = null)
            {
                Id = id;
                Name = name;
                GeoConstraint = geoConstraint;
                Uri = uri;
            }

            public int Id { get; private set; }

            public string Name { get; private set; }

            public string GeoConstraint { get; private set; }

            public Uri Uri { get; private set; }
        }

        private readonly List<Layer> _layers;

        public SaveSearchResults(string modelId, Uri featureServiceUri, IEnumerable<Layer> layers)
        {
            ServiceModelId = modelId;
            FeatureService = featureServiceUri;
            _layers = new List<Layer>();
            foreach(var layer in layers)
            {
                var ub = new UriBuilder(FeatureService);
                ub.Path = ub.Path.TrimEnd('/') + '/' + layer.Id.ToString();
                _layers.Add(new Layer(layer.Id, layer.Name, layer.GeoConstraint, ub.Uri));
            };
        }

        public string ServiceModelId { get; private set; }

        public Uri FeatureService { get; private set; }

        public IEnumerable<Layer> FeatureLayers => _layers;
    }
}
