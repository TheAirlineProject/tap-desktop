using System;
using System.ComponentModel.Composition;
using Prism.Commands;
using Prism.Mvvm;
using Prism.Regions;

namespace TheAirline.ViewModels.Game
{
    [Export]
    public class PageCreditsViewModel : BindableBase
    {
        private readonly IRegionManager _regionManager;

        [ImportingConstructor]
        public PageCreditsViewModel(IRegionManager regionManager)
        {
            
            _regionManager = regionManager;
            NavigateCommand = new DelegateCommand<Uri>(Navigate);
        }

        private void Navigate(Uri view)
        {
            _regionManager.RequestNavigate("MainContentRegion", view);
        }

        public DelegateCommand<Uri> NavigateCommand { get; }

        public Uri StartMenuUri => new Uri("/PageStartMenu", UriKind.Relative);
    }
}
