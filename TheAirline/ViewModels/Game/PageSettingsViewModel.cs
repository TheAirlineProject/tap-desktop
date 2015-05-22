using System.ComponentModel.Composition;
using Microsoft.Practices.Prism.Mvvm;
using TheAirline.Db;
using TheAirline.Infrastructure.Enums;
using TheAirline.Models.General;

namespace TheAirline.ViewModels.Game
{
    [Export]
    public class PageSettingsViewModel : BindableBase
    {
        private readonly Settings _settings;

        public PageSettingsViewModel()
        {
            AirlineContext context = new AirlineContext();
            _settings = context.Settings.Find(1);
        }

        public ScreenMode ScreenMode
        {
            get { return _settings.Mode; }
            set { _settings.Mode = value; }
        }
    }
}
