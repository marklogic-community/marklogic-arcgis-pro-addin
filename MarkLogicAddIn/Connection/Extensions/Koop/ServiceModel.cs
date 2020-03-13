using System.Collections.Generic;

namespace MarkLogic.Extensions.Koop
{
    public class ServiceModel : IServiceModel
    {
        internal ServiceModel(string id, string name, string description, IEnumerable<string> searchProfileNames)
        {
            Id = id;
            Name = name;
            Description = description;
            SearchProfileNames = new List<string>(searchProfileNames);
        }

        public string Id { get; private set; }

        public string Name { get; private set; }

        public string Description { get; private set; }

        public IEnumerable<string> SearchProfileNames { get; private set; }
    }
}
