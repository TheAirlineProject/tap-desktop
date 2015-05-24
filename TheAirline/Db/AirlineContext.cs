using System.Data.Entity;
using TheAirline.Models.General;
using TheAirline.Models.General.Countries;

namespace TheAirline.Db
{
    public class AirlineContext : DbContext
    {
        public DbSet<Settings> Settings { get; set; }
        public DbSet<Region> Regions { get; set; }
        public DbSet<Continent> Continents { get; set; }
    }
}
