using MarkLogic.Client.Search;
using MarkLogic.Esri.ArcGISPro.AddIn.Messaging;
using MarkLogic.Esri.ArcGISPro.AddIn.ViewModels.Messages;
using System;

namespace MarkLogic.Esri.ArcGISPro.AddIn.Commands
{
    public class PageSearchCommand : SearchCommand
    {
        public PageSearchCommand(MessageBus messageBus, Func<long> startOn, Func<object, bool> canPage, Action<Exception> error = null)
            : base(messageBus, ReturnOptions.Results)
        {
            StartOn = startOn;
            CanPage = canPage;
        }

        private Func<long> StartOn { get; set; }

        private Func<object, bool> CanPage { get; set; }

        protected override bool InternalCanExecuteCallback(object parameter)
        {
            return base.InternalCanExecuteCallback(parameter) && CanPage(parameter);
        }

        protected override BuildSearchMessage OnBuildSearch()
        {
            var msg = base.OnBuildSearch();
            msg.Query.Start = StartOn();
            return msg;
        }

        protected override BeginSearchMessage OnBeginSearch()
        {
            return new BeginSearchMessage(ReturnOptions, true);
        }

        protected override EndSearchMessage OnEndSearch(SearchResults results)
        {
            return new EndSearchMessage(results, true);
        }
    }
}
