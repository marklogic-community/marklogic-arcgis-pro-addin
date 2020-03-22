using MarkLogic.Esri.ArcGISPro.AddIn.Commands;
using MarkLogic.Esri.ArcGISPro.AddIn.Messaging;
using MarkLogic.Esri.ArcGISPro.AddIn.ViewModels.Messages;
using System;
using System.Windows.Input;

namespace MarkLogic.Esri.ArcGISPro.AddIn.ViewModels
{
    public class SearchOptionsViewModel : ViewModelBase
    {
        public SearchOptionsViewModel(MessageBus messageBus)
        {
            MessageBus = messageBus ?? throw new ArgumentNullException("messageBus");
            MessageBus.Subscribe<BuildSearchMessage>(m =>
            {
                m.Query.ValuesLimit = LimitValues ? MaxValues : 0;
                m.Query.AggregateValues = ClusterResults;
                m.Query.MaxLonDivs = ClusterDivisions;
                m.Query.MaxLatDivs = Math.Max(ClusterDivisions / 2, 1);
            });

            // set defaults TODO: load defaults from config
            LimitValues = false;
            ClusterResults = true;
            ClusterDivisions = 50;
        }

        protected MessageBus MessageBus { get; private set; }

        private bool _limitValues;
        public bool LimitValues
        {
            get { return _limitValues; }
            set { SetProperty(ref _limitValues, value); }
        }

        private long _maxValues;
        public long MaxValues
        {
            get { return _maxValues; }
            set { SetProperty(ref _maxValues, value); }
        }

        private bool _clusterResults;
        public bool ClusterResults
        {
            get { return _clusterResults; }
            set { SetProperty(ref _clusterResults, value); }
        }

        private uint _clusterDivisions;
        public uint ClusterDivisions
        {
            get { return _clusterDivisions; }
            set { SetProperty(ref _clusterDivisions, value); }
        }

        private SearchCommand _cmdApplyOptions;
        public ICommand ApplyOptions => _cmdApplyOptions ?? (_cmdApplyOptions = new SearchCommand(MessageBus));
    }
}
