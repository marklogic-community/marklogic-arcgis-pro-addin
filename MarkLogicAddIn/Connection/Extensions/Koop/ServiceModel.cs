using System.Collections.Generic;

namespace MarkLogic.Extensions.Koop
{
    public class ServiceModel : IServiceModel
    {
        private string[] _valueNames;

        internal ServiceModel(string id, string name, string description, string[] valueNames, string docTransform)
        {
            Id = id;
            Name = name;
            Description = description;
            _valueNames = valueNames;
            DocTransform = docTransform;
        }

        public string Id { get; private set; }

        public string Name { get; private set; }

        public string Description { get; private set; }

        public IEnumerable<string> ValueNames => _valueNames;

        public string DocTransform { get; private set; }
    }
}
