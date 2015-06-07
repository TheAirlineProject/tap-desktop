using System.Collections.ObjectModel;
using System.ComponentModel.Composition;
using System.Linq;
using Microsoft.Practices.Prism.Commands;
using Microsoft.Practices.Prism.Mvvm;
using TheAirline.General.Models.Countries;
using TheAirline.Infrastructure;
using TheAirline.Models.General.Countries;

namespace TheAirline.ViewModels.Game
{
    [Export]
    public class PageStartDataViewModel : BindableBase
    {
        private readonly AppState _state;

        [ImportingConstructor]
        public PageStartDataViewModel(AppState state)
        {
            _state = state;
            Regions = state.Regions;

            ChangeRegions = new DelegateCommand<int>(UpdateRegions);
        }

        private void UpdateRegions(int obj)
        {
            var continent = from cont in _state.Continents where cont.Id == obj select cont;
            Regions = new ObservableCollection<Region>(continent.SingleOrDefault()?.Regions);
        }

        public ObservableCollection<Continent> Continents => new ObservableCollection<Continent>(_state.Continents.ToList());

        public ObservableCollection<Region> Regions { get; set; }

        public DelegateCommand<int> ChangeRegions { get; set; }
    }
}
