using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel.Composition;
using System.Data.Entity;
using System.Linq;
using TheAirline.Db;
using TheAirline.General.Enums;
using TheAirline.General.Models;
using TheAirline.General.Models.Countries;
using WPFLocalizeExtension.Engine;

namespace TheAirline.Infrastructure
{
    [Export]
    [PartCreationPolicy(CreationPolicy.Shared)]
    public class AppState : IDisposable
    {
        private readonly AirlineContext _context = new AirlineContext();
        private readonly List<Region> _regions;
        private readonly Settings _settings;

        public AppState()
        {
            _settings = _context.Settings.Find(1);
            _regions = _context.Regions.ToList();

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

        public DbSet<Continent> Continents => _context.Continents;
        public ObservableCollection<Region> Regions => new ObservableCollection<Region>(_regions);

        public void Dispose()
        {
            _context.SaveChanges();
            _context.Dispose();
        }

        public void SaveState()
        {
            _context.SaveChanges();
        }
    }
}