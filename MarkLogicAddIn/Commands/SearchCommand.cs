using MarkLogic.Client.Search;
using MarkLogic.Esri.ArcGISPro.AddIn.Messaging;
using MarkLogic.Esri.ArcGISPro.AddIn.ViewModels.Messages;
using System;
using System.Diagnostics;
using System.Threading.Tasks;
using System.Windows.Input;

namespace MarkLogic.Esri.ArcGISPro.AddIn.Commands
{
    public class SearchCommand : ServerCommand
    {
        public SearchCommand(MessageBus messageBus, ReturnOptions returnOptions = SearchQuery.DefaultReturnOptions, bool broadcastSearch = true, Func<SearchQuery, Task> queryCallback = null, Func<SearchResults, Task> resultsCallback = null)
        {
            MessageBus = messageBus ?? throw new ArgumentNullException("messageBus");
            MessageBus.Subscribe<ServerSettingsChangedMessage>(m => CommandManager.InvalidateRequerySuggested());
            ExecuteCallback = new Func<object, Task>(ExecuteSearch);
            CanExecuteCallback = InternalCanExecuteCallback;
            ErrorCallback = InternalErrorCallback;
            ReturnOptions = returnOptions;
            BroadcastSearch = broadcastSearch;
            QueryCallback = queryCallback;
            ResultsCallback = resultsCallback;
        }

        protected virtual async void InternalErrorCallback(Exception e)
        {
            if (BroadcastSearch)
                await MessageBus.Publish(new SearchAbortedMessage());
        }

        protected virtual bool InternalCanExecuteCallback(object parameter)
        {
            var msg = new GetServerSettingsMessage();
            MessageBus.Publish(msg).Wait();
            return msg.Profile != null && msg.ServiceModel != null;
        }

        private MessageBus MessageBus { get; set; }

        protected ReturnOptions ReturnOptions { get; set; }

        protected bool BroadcastSearch { get; set; }

        protected Func<SearchQuery, Task> QueryCallback { get; set; }

        protected Func<SearchResults, Task> ResultsCallback { get; set; }

        protected virtual BuildSearchMessage OnBuildSearch() => new BuildSearchMessage(new SearchQuery() { ReturnOptions = ReturnOptions });

        protected virtual BeginSearchMessage OnBeginSearch() => new BeginSearchMessage(ReturnOptions);

        protected virtual EndSearchMessage OnEndSearch(SearchResults results) => new EndSearchMessage(results);

        private async Task ExecuteSearch(object parameter)
        {
            var serverMsg = await MessageBus.Publish(new GetServerSettingsMessage());
            Debug.Assert(serverMsg.Resolved && serverMsg.Profile != null && serverMsg.ServiceModel != null);
            var profile = serverMsg.Profile ?? throw new InvalidOperationException("Unable to resolve current connection profile.");
            var model = serverMsg.ServiceModel ?? throw new InvalidOperationException("Unable to resolve current service model.");

            // build query
            var buildMsg = OnBuildSearch();
            await MessageBus.Publish(buildMsg);
            if (QueryCallback != null)
                await QueryCallback(buildMsg.Query);

            // get results
            var conn = ConnectionService.Instance.Create(profile);
            if (BroadcastSearch)
                await MessageBus.Publish(OnBeginSearch());
            var results = await SearchService.Instance.Search(conn, buildMsg.Query, model);
            if (BroadcastSearch)
                await MessageBus.Publish(OnEndSearch(results));
            if (ResultsCallback != null)
                await ResultsCallback(results);
        }
    }
}
