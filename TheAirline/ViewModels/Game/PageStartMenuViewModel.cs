using System;
using Microsoft.Practices.Prism.Commands;
using System.ComponentModel.Composition;
using Microsoft.Practices.Prism.Regions;
using Microsoft.Practices.Prism.Mvvm;

namespace TheAirline.ViewModels.Game
{
    [Export]
    public class PageStartMenuViewModel : BindableBase
    {
        private readonly IRegionManager _regionManager;

        [ImportingConstructor]
        public PageStartMenuViewModel(IRegionManager regionManager)
        {
            _regionManager = regionManager;
            NavigateCommand = new DelegateCommand<Uri>(Navigate);
        }

        private void Navigate(Uri view)
        {
            _regionManager.RequestNavigate("MainContentRegion", view);
        }

        public DelegateCommand<Uri> NavigateCommand { get; }

        public Uri NewGameUri => new Uri("/PageStartGameData", UriKind.Relative);
    }
}
