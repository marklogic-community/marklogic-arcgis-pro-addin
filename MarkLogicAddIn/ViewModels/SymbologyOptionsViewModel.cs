using MarkLogic.Esri.ArcGISPro.AddIn.Messaging;
using MarkLogic.Esri.ArcGISPro.AddIn.ViewModels.Messages;
using System;
using System.Collections.ObjectModel;

namespace MarkLogic.Esri.ArcGISPro.AddIn.ViewModels
{
    public class SymbologyOptionsViewModel : ViewModelBase
    {
        public SymbologyOptionsViewModel(MessageBus messageBus)
        {
            MessageBus = messageBus ?? throw new ArgumentNullException("messageBus");
            MessageBus.Subscribe<ServerSettingsChangedMessage>(m =>
            {
                Items.Clear();
                if (m.ServiceModel != null)
                {
                    foreach (var valueName in m.ServiceModel.ValueNames)
                        Items.Add(new PointSymbologyOptionsViewModel(MessageBus, valueName));
                }
            });
            Items = new ObservableCollection<PointSymbologyOptionsViewModel>();
        }

        protected MessageBus MessageBus { get; private set; }

        public ObservableCollection<PointSymbologyOptionsViewModel> Items { get; }
    }
}
