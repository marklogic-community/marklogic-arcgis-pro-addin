using System.Collections.Generic;

namespace MarkLogic.Extensions.Koop
{
    public class ServiceModel : IServiceModel
    {
        internal ServiceModel(string id, string name, string description)
        {
            Id = id;
            Name = name;
            Description = description;
        }

        public string Id { get; private set; }

        public string Name { get; private set; }

        public string Description { get; private set; }
    }
}
