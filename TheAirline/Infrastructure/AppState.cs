using System;
using System.ComponentModel.Composition;
using TheAirline.Db;
using TheAirline.Infrastructure.Enums;
using WPFLocalizeExtension.Engine;

namespace TheAirline.Infrastructure
{
    [Export]
    [PartCreationPolicy(CreationPolicy.Shared)]
    public sealed class AppState : IDisposable
    {
        private readonly Models.General.Settings _settings;

        public AppState()
        {
            _settings = Context.Settings.Find(1);

            var dict = LocalizeDictionary.Instance;
            dict.IncludeInvariantCulture = false;
        }

        public string Language
        {
            get { return _settings.Language; }
            set { _settings.Language = value; }
        }

        public ScreenMode Mode
        {
            get { return _settings.Mode; }
            set { _settings.Mode = value; }
        }

        public AirlineContext Context { get; } = new AirlineContext();

        public void Dispose()
        {
            Context.SaveChanges();
            Context.Dispose();
        }

        public void SaveState()
        {
            Context.SaveChanges();
        }
    }
}