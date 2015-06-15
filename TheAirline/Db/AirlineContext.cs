using System.Data.Entity;
using TheAirline.Models.General;
using TheAirline.Models.General.Countries;
using Settings = TheAirline.Models.General.Settings;

namespace TheAirline.Db
{
    public sealed class AirlineContext : DbContext
    {
        public DbSet<Settings> Settings { get; set; }
        public DbSet<Region> Regions { get; set; }
        public DbSet<Continent> Continents { get; set; }
        public DbSet<Difficulty> Difficulties { get; set; }
        public DbSet<Player> Players { get; set; }
    }
}
