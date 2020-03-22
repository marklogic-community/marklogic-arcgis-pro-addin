using MarkLogic.Esri.ArcGISPro.AddIn.Messaging;
using MarkLogic.Extensions.Koop;

namespace MarkLogic.Esri.ArcGISPro.AddIn.ViewModels.Messages
{
    public class GetServerSettingsMessage : Message
    {
        public GetServerSettingsMessage()
        {
        }

        public ConnectionProfile Profile { get; set; }

        public IServiceModel ServiceModel { get; set; }
    }
}
