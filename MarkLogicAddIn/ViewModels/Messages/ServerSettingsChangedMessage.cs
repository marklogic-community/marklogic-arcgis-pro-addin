using MarkLogic.Esri.ArcGISPro.AddIn.Messaging;
using MarkLogic.Extensions.Koop;

namespace MarkLogic.Esri.ArcGISPro.AddIn.ViewModels.Messages
{
    public class ServerSettingsChangedMessage : Message
    {
        public ServerSettingsChangedMessage(ConnectionProfile connProfile, ServiceModel serviceModel)
        {
            Profile = connProfile;
            ServiceModel = serviceModel;
        }

        public ConnectionProfile Profile { get; private set; }

        public ServiceModel ServiceModel { get; private set; }
    }
}
