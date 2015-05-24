using System;
using System.Collections.ObjectModel;
using System.ComponentModel.Composition;
using System.Globalization;
using System.Windows.Input;
using Microsoft.Practices.Prism.Commands;
using Microsoft.Practices.Prism.Mvvm;
using Microsoft.Practices.Prism.Regions;
using TheAirline.Db;
using TheAirline.Infrastructure.Enums;
using TheAirline.Models.General;
using WPFLocalizeExtension.Engine;

namespace TheAirline.ViewModels.Game
{
    [Export]
    public class PageSettingsViewModel : BindableBase
    {
        private readonly AirlineContext _context;
        private readonly LocalizeDictionary _dictionary = LocalizeDictionary.Instance;
        private readonly IRegionManager _regionManager;
        private readonly Settings _settings;

        [ImportingConstructor]
        public PageSettingsViewModel(IRegionManager regionManager)
        {
            _regionManager = regionManager;
            _context = new AirlineContext();

            SaveCommand = new DelegateCommand<object>(SaveSettings);
            BackCommand = new DelegateCommand<object>(GoBack);

            _settings = _context.Settings.Find(1);
        }

        public ScreenMode ScreenMode
        {
            get { return _settings.Mode; }
            set { _settings.Mode = value; }
        }

        public ObservableCollection<CultureInfo> Languages => _dictionary.MergedAvailableCultures;

        public string Language
        {
            get { return _settings.Language; }
            set { _settings.Language = value; }
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
            _context.SaveChanges();
            _regionManager.RequestNavigate("MainContentRegion", new Uri("/PageStartMenu", UriKind.Relative));
        }
    }
}