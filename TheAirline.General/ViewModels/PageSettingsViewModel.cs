using System;
using System.Collections.ObjectModel;
using System.ComponentModel.Composition;
using System.Globalization;
using System.Windows.Input;
using Microsoft.Practices.Prism.Commands;
using Microsoft.Practices.Prism.Mvvm;
using Microsoft.Practices.Prism.Regions;
using TheAirline.General.Enums;
using WPFLocalizeExtension.Engine;

namespace TheAirline.General.ViewModels
{
    [Export]
    public class PageSettingsViewModel : BindableBase
    {
        private readonly LocalizeDictionary _dictionary = LocalizeDictionary.Instance;
        private readonly IRegionManager _regionManager;
        private readonly AppState _state;

        [ImportingConstructor]
        public PageSettingsViewModel(IRegionManager regionManager, AppState state)
        {
            _state = state;
            _regionManager = regionManager;

            SaveCommand = new DelegateCommand<object>(SaveSettings);
            BackCommand = new DelegateCommand<object>(GoBack);
        }

        public ScreenMode ScreenMode
        {
            get { return _state.Mode; }
            set { _state.Mode = value; }
        }

        public ObservableCollection<CultureInfo> Languages => _dictionary.MergedAvailableCultures;

        public string Language
        {
            get { return _state.Language; }
            set { _state.Language = value; }
        }

        public DelegateCommand<object> SaveCommand { get; set; }
        public DelegateCommand<object> BackCommand { get; set; }

        public ICommand ChangeLanguage => _dictionary.SetCultureCommand;

        private void GoBack(object obj)
        {
            _regionManager.RequestNavigate("MainContentRegion", new Uri("/PageStartMenu", UriKind.Relative));
        }

        private void SaveSettings(object obj)
        {
            _state.SaveState();
            _regionManager.RequestNavigate("MainContentRegion", new Uri("/PageStartMenu", UriKind.Relative));
        }
    }
}