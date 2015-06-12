using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel.Composition;
using System.Linq;
using Microsoft.Practices.Prism.Commands;
using Microsoft.Practices.Prism.Mvvm;
using TheAirline.Infrastructure;
using TheAirline.Models.General.Countries;

namespace TheAirline.ViewModels.Game
{
    [Export]
    public sealed class PageStartDataViewModel : BindableBase
    {
        private readonly AppState _state;
        private readonly IQueryable<Region> _regions;

        [ImportingConstructor]
        public PageStartDataViewModel(AppState state)
        {
            _state = state;

            var continents = from continent in _state.Continents orderby continent.Id descending select continent;
            Continents = new ObservableCollection<Continent>(continents.ToList());

            _regions = from region in _state.Context.Regions select region;
            Regions = new ObservableCollection<Region>(_regions.ToList());

            MajorAirports = false;

            // Use Enumerable.Range to create lists for years and opponents.
            NumOpponents = Enumerable.Range(1, 51).ToList();

            var start = new DateTime(1960, 1, 1);
            var end = DateTime.Now;

            Years = Enumerable.Range(start.Year, (end.Year - start.Year) + 1).ToList();

            // DelegateCommand only works with Nullable objects hence the int?.
            ChangeRegions = new DelegateCommand<int?>(UpdateRegions);
        }

        private void UpdateRegions(int? obj)
        {
            if (obj == 0)
            {
                Regions.Clear();
                foreach (var region in _regions)
                {
                    Regions.Add(region);
                }
            }
            else
            {
                var continent = from cont in _state.Continents where cont.Id == obj select cont;

                Regions.Clear();
                var regions = continent.First()?.Regions;
                if (regions == null) return;
                foreach (var region in regions)
                {
                    Regions.Add(region);
                }
            }
        }

        public ObservableCollection<Continent> Continents { get; }

        public ObservableCollection<Region> Regions { get; set; }

        public DelegateCommand<int?> ChangeRegions { get; }

        public int ContinentId { get; set; }

        public int RegionId { get; set; }

        public bool MajorAirports { get; set; }

        public List<int> NumOpponents { get; }
        public int SelectedOpponents { get; set; }

        public List<int> Years { get; }
        public int Year { get; set; }
    }
}
