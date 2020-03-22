using ArcGIS.Desktop.Framework;
using MarkLogic.Client.Search;
using MarkLogic.Esri.ArcGISPro.AddIn.ViewModels;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

namespace MarkLogic.Esri.ArcGISPro.AddIn
{
    public class SearchInfoModel : ViewModelBase
    {
        private SearchDockPaneViewModel _searchModel;

        public SearchInfoModel()
        {
        }

        public void Initialize(SearchOptions queryOptions, SearchDockPaneViewModel searchModel)
        {
            _searchModel = searchModel;

            QueryOptions = queryOptions;
            Constraints = new ObservableCollection<Constraint>();
            foreach (var constraint in queryOptions.Constraints)
                Constraints.Add(constraint);
        }

        public SearchOptions QueryOptions { get; private set; }

        public ObservableCollection<Constraint> Constraints { get; private set; }

        public bool HasConstraints => Constraints.Count > 0;

        public bool NoConstraints => Constraints.Count == 0;

        private RelayCommand _cmdAddConstraint;
        public RelayCommand AddConstraintCommand => _cmdAddConstraint ?? (_cmdAddConstraint = new RelayCommand(c => AddConstraint(c), () => true));

        private void AddConstraint(object constraint)
        {
            //_searchModel.QueryString += constraint.ToString() + ":";
            //_searchModel.SuggestCommand.Execute(null);
        }
    }
}
