using MarkLogic.Esri.ArcGISPro.AddIn.Messaging;
using MarkLogic.Esri.ArcGISPro.AddIn.ViewModels;
using MarkLogic.Esri.ArcGISPro.AddIn.ViewModels.Messages;
using MarkLogic.Extensions.Koop;
using System.Collections.ObjectModel;

namespace MarkLogic.Esri.ArcGISPro.AddIn
{
    public class SearchHelpViewModel : ViewModelBase
    {
        public SearchHelpViewModel(MessageBus messageBus)
        {
            var msg = new GetServerSettingsMessage();
            messageBus.Publish(msg).Wait();
            if (msg.Resolved && msg.ServiceModel != null)
            {
                foreach (var constraint in msg.ServiceModel.Constraints)
                    Constraints.Add(constraint);
            }
        }

        public ObservableCollection<ServiceModelConstraint> Constraints { get; } = new ObservableCollection<ServiceModelConstraint>();

        public bool HasConstraints => Constraints.Count > 0;

        public bool NoConstraints => Constraints.Count == 0;
    }
}
