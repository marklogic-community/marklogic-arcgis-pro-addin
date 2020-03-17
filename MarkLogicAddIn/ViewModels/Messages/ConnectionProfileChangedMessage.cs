using MarkLogic.Esri.ArcGISPro.AddIn.Messaging;

namespace MarkLogic.Esri.ArcGISPro.AddIn.ViewModels.Messages
{
    public class ConnectionProfileChangedMessage : Message
    {
        public ConnectionProfileChangedMessage(ConnectionProfile connProfile)
        {
            Profile = connProfile;
        }

        public ConnectionProfile Profile { get; private set; }
    }
}
