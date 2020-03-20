﻿using MarkLogic.Esri.ArcGISPro.AddIn.Messaging;
using MarkLogic.Esri.ArcGISPro.AddIn.ViewModels.Messages;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows.Input;

namespace MarkLogic.Esri.ArcGISPro.AddIn.ViewModels
{
    public class SearchFacetsViewModel : ViewModelBase
    {
        public SearchFacetsViewModel(MessageBus messageBus)
        {
            MessageBus = messageBus ?? throw new ArgumentNullException("messageBus");
            MessageBus.Subscribe<BeginSearchMessage>(m => 
            {
                SelectedFacets = Facets.SelectMany(f => f.Values).Where(v => v.Selected).ToList(); // save selected facets
                SelectedFacets.ForEach(v => m.Query.AddFacetValue(v.FacetName, v.ValueName)); // add facets to query
            });
            MessageBus.Subscribe<EndSearchMessage>(m =>
            {
                Facets.Clear();
                foreach(var facet in m.Results.Facets.Values)
                {
                    var viewModel = new FacetViewModel(facet);
                    viewModel.Values.ToList().ForEach(fv => fv.Selected = SelectedFacets.Any(sf => sf.Equals(fv)));
                    Facets.Add(viewModel);
                }
                SelectedFacets = null; // clear previously saved
            });
            Facets = new ObservableCollection<FacetViewModel>();
            SelectedFacets = new List<FacetValueViewModel>();
        }

        protected MessageBus MessageBus { get; private set; }

        private List<FacetValueViewModel> SelectedFacets { get; set; }

        public ObservableCollection<FacetViewModel> Facets { get; private set; }

        private SearchCommand _cmdSelectFacet;
        public ICommand SelectFacet => _cmdSelectFacet ?? (_cmdSelectFacet = new SearchCommand(MessageBus));
    }
}