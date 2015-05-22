using System.Data.Entity;
using TheAirline.Models.General;

namespace TheAirline.Db
{
    public class AirlineContext : DbContext
    {
        public DbSet<Settings> Settings { get; set; }
    }
}
