using MarkLogic.Esri.ArcGISPro.AddIn.Messaging;
using MarkLogic.Extensions.Koop;

namespace MarkLogic.Esri.ArcGISPro.AddIn.ViewModels.Messages
{
    public class ServerSettingsChangedMessage : Message
    {
        public ServerSettingsChangedMessage(ConnectionProfile connProfile, IServiceModel serviceModel)
        {
            Profile = connProfile;
            ServiceModel = serviceModel;
        }

        public ConnectionProfile Profile { get; private set; }

        public IServiceModel ServiceModel { get; private set; }
    }
}
