using System;
using System.ComponentModel.Composition;
using Microsoft.Practices.Prism.Commands;
using Microsoft.Practices.Prism.Mvvm;
using Microsoft.Practices.Prism.Regions;

namespace TheAirline.General.ViewModels
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
