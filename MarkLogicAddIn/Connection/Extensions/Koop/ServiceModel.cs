using System.Collections.Generic;

namespace MarkLogic.Extensions.Koop
{
    public class ServiceModel
    {
        internal ServiceModel(string id, string name, string description, IEnumerable<string> valueNames, string docTransform, IEnumerable<ServiceModelConstraint> constraints)
        {
            Id = id;
            Name = name;
            Description = description;
            ValueNames = new List<string>(valueNames);
            DocTransform = docTransform;
            Constraints = new List<ServiceModelConstraint>(constraints);
        }

        public string Id { get; private set; }

        public string Name { get; private set; }

        public string Description { get; private set; }

        public IEnumerable<string> ValueNames { get; private set; }

        public string DocTransform { get; private set; }

        public IEnumerable<ServiceModelConstraint> Constraints { get; private set; }
    }
}
