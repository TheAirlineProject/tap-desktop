using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel.Composition;
using System.Linq;
using Microsoft.Practices.Prism.Commands;
using Microsoft.Practices.Prism.Mvvm;
using TheAirline.Infrastructure;
using TheAirline.Models.General;
using TheAirline.Models.General.Countries;

namespace TheAirline.ViewModels.Game
{
    [Export]
    public sealed class PageStartDataViewModel : BindableBase
    {
        private readonly Region _allRegions;
        private readonly IQueryable<Region> _regions;
        private readonly AppState _state;
        private Region _selectedRegion;

        [ImportingConstructor]
        public PageStartDataViewModel(AppState state)
        {
            _state = state;

            _allRegions = new Region {Name = "All Regions", Id = 100};

            var continents = from continent in _state.Context.Continents
                orderby continent.Id descending
                select continent;
            Continents = new ObservableCollection<Continent>(continents.ToList());

            _regions = from region in _state.Context.Regions select region;
            Regions = new ObservableCollection<Region>(_regions.ToList());
            Regions.Insert(0, _allRegions);
            SelectedRegion = _allRegions;

            MajorAirports = false;
            RandomOpponents = true;
            RealData = false;
            GameTurn = true;
            PausedOnStart = false;
            SameRegion = true;

            // Use Enumerable.Range to create lists for years and opponents.
            NumOpponents = Enumerable.Range(1, 51).ToList();

            var start = new DateTime(1960, 1, 1);
            var end = DateTime.Now;

            Years = Enumerable.Range(start.Year, (end.Year - start.Year) + 1).OrderByDescending(i => i).ToList();

            var difficulties = from difficulty in _state.Context.Difficulties select difficulty;
            Difficulties = new ObservableCollection<Difficulty>(difficulties.ToList());

            // DelegateCommand only works with Nullable objects hence the int?.
            ChangeRegions = new DelegateCommand<Continent>(UpdateRegions);
        }

        public ObservableCollection<Continent> Continents { get; }
        public ObservableCollection<Difficulty> Difficulties { get; }
        public ObservableCollection<Region> Regions { get; }
        public DelegateCommand<Continent> ChangeRegions { get; }
        public Continent Continent { get; set; }

        public Region SelectedRegion
        {
            get { return _selectedRegion; }
            set { SetProperty(ref _selectedRegion, value); }
        }

        public bool MajorAirports { get; set; }
        public List<int> NumOpponents { get; }
        public int SelectedOpponents { get; set; }
        public List<int> Years { get; }
        public int Year { get; set; }
        public bool RandomOpponents { get; set; }
        public bool RealData { get; set; }
        public bool GameTurn { get; set; }
        public bool PausedOnStart { get; set; }
        public bool SameRegion { get; set; }

        private void UpdateRegions(Continent obj)
        {
            if (obj.Id == 7)
            {
                Regions.Clear();
                Regions.Add(_allRegions);
                foreach (var region in _regions.ToList())
                {
                    Regions.Add(region);
                }
            }
            else
            {
                var continent = from cont in _state.Context.Continents where cont.Id == obj.Id select cont;

                Regions.Clear();
                var regions = continent.First()?.Regions;
                if (regions == null) return;
                Regions.Add(_allRegions);
                foreach (var region in regions)
                {
                    Regions.Add(region);
                }
            }

            SelectedRegion = _allRegions;
        }
    }
}