using System;
using System.ComponentModel.Composition;
using TheAirline.Db;
using TheAirline.Infrastructure.Enums;

namespace TheAirline.Infrastructure
{
    [Export]
    [PartCreationPolicy(CreationPolicy.Shared)]
    public class AppState : IDisposable
    {
        private readonly AirlineContext _context = new AirlineContext();
        private readonly Models.General.Settings _settings;

        public AppState()
        {
            _settings = _context.Settings.Find(1);
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

        public void SaveState()
        {
            _context.SaveChanges();
        }

        public void Dispose()
        {
            _context.SaveChanges();
            _context.Dispose();
        }
    }
}
