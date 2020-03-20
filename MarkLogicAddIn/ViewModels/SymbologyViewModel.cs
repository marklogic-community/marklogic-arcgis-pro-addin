using MarkLogic.Esri.ArcGISPro.AddIn.Messaging;
using MarkLogic.Esri.ArcGISPro.AddIn.ViewModels.Messages;
using System;
using System.Collections.ObjectModel;

namespace MarkLogic.Esri.ArcGISPro.AddIn.ViewModels
{
    public class SymbologyViewModel : ViewModelBase
    {
        public SymbologyViewModel(MessageBus messageBus)
        {
            MessageBus = messageBus ?? throw new ArgumentNullException("messageBus");
            MessageBus.Subscribe<ServiceModelChangedMessage>(m =>
            {
                Items.Clear();
                if (m.ServiceModel != null)
                {
                    foreach (var valueName in m.ServiceModel.ValueNames)
                        Items.Add(new SymbologyItemViewModel(MessageBus, valueName));
                }
            });
            Items = new ObservableCollection<SymbologyItemViewModel>();
        }

        protected MessageBus MessageBus { get; private set; }

        public ObservableCollection<SymbologyItemViewModel> Items { get; }
    }
}
