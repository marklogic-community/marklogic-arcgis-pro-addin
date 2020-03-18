using MarkLogic.Client.Search;
using MarkLogic.Esri.ArcGISPro.AddIn.Messaging;
using MarkLogic.Esri.ArcGISPro.AddIn.ViewModels.Messages;
using MarkLogic.Extensions.Koop;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MarkLogic.Esri.ArcGISPro.AddIn
{
    public class SearchCommand : ServerCommand
    {
        public SearchCommand(MessageBus messageBus, Action<object> preSearch = null, Action<Exception> error = null)
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
            PreSearchCallback = preSearch;
            ErrorCallback = error;
        }

        public override event EventHandler CanExecuteChanged;

        private MessageBus MessageBus { get; set; }

        private ConnectionProfile ConnectionProfile { get; set; }

        private IServiceModel ServiceModel { get; set; }

        private Action<object> PreSearchCallback { get; set; }

        private async Task ExecuteSearch(object parameter)
        {
            PreSearchCallback?.Invoke(parameter);

            // build query
            var query = new SearchQuery();
            await MessageBus.Publish(new BeginSearchMessage(query));

            // get results
            var conn = ConnectionService.Instance.Create(ConnectionProfile);
            var results = await SearchService.Instance.Search(conn, query, ServiceModel);
            await MessageBus.Publish(new EndSearchMessage(results));
        }
    }
}
