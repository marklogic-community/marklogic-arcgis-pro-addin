namespace MarkLogic.Extensions.Koop
{
    public enum GeometryType
    {
        Point,
        Polygon
    }

    public class ServiceModelLayer
    {
        public ServiceModelLayer(int layerId, string name, string description, GeometryType geometryType, string geoConstraint = null)
        {
            Id = layerId;
            Name = name;
            Description = description;
            GeometryType = geometryType;
            GeoConstraint = geoConstraint;
        }

        public int Id { get; private set; }

        public string Name { get; private set; }

        public string Description { get; private set; }

        public GeometryType GeometryType { get; private set; }

        public string GeoConstraint { get; private set; }
    }
}
