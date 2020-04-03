using MarkLogic.Client.Search;
using MarkLogic.Esri.ArcGISPro.AddIn.Controls;
using MarkLogic.Esri.ArcGISPro.AddIn.Messaging;
using MarkLogic.Esri.ArcGISPro.AddIn.ViewModels;
using System;
using System.Windows.Input;

namespace MarkLogic.Esri.ArcGISPro.AddIn.Commands
{
    public class ShowSaveSearchCommand : ICommand
    {
        public ShowSaveSearchCommand(MessageBus messageBus, Func<SearchResults> getSearchResults)
        {
            MessageBus = messageBus ?? throw new ArgumentNullException("messageBus");
            GetSearchResults = getSearchResults ?? throw new ArgumentNullException("getSearchResults");
        }

        protected MessageBus MessageBus { get; set; }

        protected Func<SearchResults> GetSearchResults { get; set; }

        public event EventHandler CanExecuteChanged
        {
            add { CommandManager.RequerySuggested += value; }
            remove { CommandManager.RequerySuggested -= value; }
        }

        public bool CanExecute(object parameter)
        {
            return GetSearchResults() != null;
        }

        public void Execute(object parameter)
        {
            if (!CanExecute(parameter))
                return;

            var viewModel = new SaveSearchViewModel(MessageBus, GetSearchResults());
            var window = new SaveSearchWindow() { DataContext = viewModel };
            window.ShowDialog();
        }
    }
}
