using MarkLogic.Client.Search;
using MarkLogic.Esri.ArcGISPro.AddIn.Messaging;
using MarkLogic.Esri.ArcGISPro.AddIn.ViewModels.Messages;
using MarkLogic.Extensions.Koop;
using System;
using System.Threading.Tasks;

namespace MarkLogic.Esri.ArcGISPro.AddIn
{
    public class SearchCommand : ServerCommand
    {
        public SearchCommand(MessageBus messageBus, Action<Exception> error = null, ReturnOptions returnOptions = SearchQuery.DefaultReturnOptions)
        {
            MessageBus = messageBus ?? throw new ArgumentNullException("messageBus");
            MessageBus.Subscribe<ConnectionProfileChangedMessage>(m => 
            {
                ConnectionProfile = m.Profile;
                CanExecuteChanged?.Invoke(this, new EventArgs());
            });
            MessageBus.Subscribe<ServiceModelChangedMessage>(m =>
            {
                ServiceModel = m.ServiceModel;
                CanExecuteChanged?.Invoke(this, new EventArgs());
            });

            ExecuteCallback = new Func<object, Task>(ExecuteSearch);
            CanExecuteCallback = o => ConnectionProfile != null && ServiceModel != null;
            ErrorCallback = error;
            ReturnOptions = returnOptions;
        }

        public override event EventHandler CanExecuteChanged;

        private MessageBus MessageBus { get; set; }

        private ConnectionProfile ConnectionProfile { get; set; }

        private IServiceModel ServiceModel { get; set; }

        private ReturnOptions ReturnOptions { get; set; }

        private async Task ExecuteSearch(object parameter)
        {
            // build query
            var query = new SearchQuery() { ReturnOptions = ReturnOptions };
            await MessageBus.Publish(new BuildSearchMessage(query));

            // get results
            var conn = ConnectionService.Instance.Create(ConnectionProfile);
            await MessageBus.Publish(new BeginSearchMessage(ReturnOptions));
            var results = await SearchService.Instance.Search(conn, query, ServiceModel);
            await MessageBus.Publish(new EndSearchMessage(results));
        }
    }
}
