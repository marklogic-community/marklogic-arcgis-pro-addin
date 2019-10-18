using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MarkLogic.Extensions.Koop
{
    public class FeatureServerProfile
    {
        private List<SearchServiceProfile> _searchServices;

        public FeatureServerProfile(string name, string serviceUrl, IEnumerable<SearchServiceProfile> searchServices)
        {
            _searchServices = new List<SearchServiceProfile>();
            Name = name;
            ServiceUrl = serviceUrl;
            _searchServices.AddRange(searchServices);
        }

        public string Name { get; private set; }

        public string ServiceUrl { get; private set; }

        public int SearchServiceCount => _searchServices.Count;

        public IEnumerable<SearchServiceProfile> SearchServices => _searchServices;
    }
}
