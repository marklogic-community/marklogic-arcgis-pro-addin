using ArcGIS.Core.Geometry;
using MarkLogic.Client.Search.Query;
using MarkLogic.Esri.ArcGISPro.AddIn.Messaging;

namespace MarkLogic.Esri.ArcGISPro.AddIn.ViewModels.Messages
{
    public class SelectMapLocationMessage : Message
    {
        public SelectMapLocationMessage(MapPoint location)
        {
            Location = location;
        }

        public MapPoint Location { get; private set; }

        public GeospatialBox Extent { get; set; }
    }
}
