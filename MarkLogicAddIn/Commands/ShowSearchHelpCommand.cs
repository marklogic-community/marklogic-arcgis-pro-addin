using MarkLogic.Esri.ArcGISPro.AddIn.Controls;
using MarkLogic.Esri.ArcGISPro.AddIn.Messaging;
using MarkLogic.Esri.ArcGISPro.AddIn.ViewModels.Messages;
using System;
using System.Windows.Input;

namespace MarkLogic.Esri.ArcGISPro.AddIn.Commands
{
    public class ShowSearchHelpCommand : ICommand
    {
        public ShowSearchHelpCommand(MessageBus messageBus)
        {
            MessageBus = messageBus ?? throw new ArgumentNullException("messageBus");
        }

        protected MessageBus MessageBus { get; set; }

        public event EventHandler CanExecuteChanged
        {
            add { CommandManager.RequerySuggested += value; }
            remove { CommandManager.RequerySuggested -= value; }
        }

        public bool CanExecute(object parameter)
        {
            var msg = new GetServerSettingsMessage();
            MessageBus.Publish(msg).Wait();
            return msg.ServiceModel != null;
        }

        public void Execute(object parameter)
        {
            if (!CanExecute(parameter))
                return;

            var viewModel = new SearchHelpViewModel(MessageBus);
            var window = new SearchHelpWindow() { DataContext = viewModel };
            window.Show();
        }
    }
}
