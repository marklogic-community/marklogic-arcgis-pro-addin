using MarkLogic.Esri.ArcGISPro.AddIn.Messaging;
using MarkLogic.Extensions.Koop;

namespace MarkLogic.Esri.ArcGISPro.AddIn.ViewModels.Messages
{
    public class ServiceModelChangedMessage : Message
    {
        public ServiceModelChangedMessage(IServiceModel model)
        {
            ServiceModel = model;
        }

        public IServiceModel ServiceModel { get; private set; }
    }
}
