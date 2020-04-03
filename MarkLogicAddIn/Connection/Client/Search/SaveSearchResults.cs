using System;
using System.Collections.Generic;
using System.Linq;

namespace MarkLogic.Client.Search
{
    public class SaveSearchResults
    {
        public class Layer
        {
            public Layer(int id, string geoConstraint, Uri uri)
            {
                Id = id;
                GeoConstraint = geoConstraint;
                Uri = uri;
            }

            public int Id { get; private set; }

            public string GeoConstraint { get; private set; }

            public Uri Uri { get; private set; }
        }

        private readonly List<Layer> _layers;

        public SaveSearchResults(string modelId, Uri featureServiceUri, IDictionary<string, int> layers)
        {
            ServiceModelId = modelId;
            FeatureService = featureServiceUri;
            _layers = new List<Layer>();
            layers.ToList().ForEach(l =>
            {
                var ub = new UriBuilder(FeatureService);
                ub.Path = ub.Path.TrimEnd('/') + '/' + l.Value.ToString();
                _layers.Add(new Layer(l.Value, l.Key, ub.Uri));
            });
        }

        public string ServiceModelId { get; private set; }

        public Uri FeatureService { get; private set; }

        public IEnumerable<Layer> FeatureLayers => _layers;
    }
}
