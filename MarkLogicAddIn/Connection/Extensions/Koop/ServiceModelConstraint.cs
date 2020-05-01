namespace MarkLogic.Extensions.Koop
{
    public class ServiceModelConstraint
    {
        public ServiceModelConstraint(string name, string description)
        {
            Name = name;
            Description = description;
        }

        public string Name { get; private set; }

        public string Description { get; private set; }
    }
}
