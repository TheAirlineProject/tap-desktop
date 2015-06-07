using System.Data.Entity.Migrations;
using TheAirline.General.Enums;
using TheAirline.General.Models.Countries;
using TheAirline.Infrastructure.Db;

namespace TheAirline.Infrastructure.Migrations
{
    internal sealed class Configuration : DbMigrationsConfiguration<AirlineContext>
    {
        public Configuration()
        {
            AutomaticMigrationsEnabled = false;
        }

        protected override void Seed(AirlineContext context)
        {
            //  This method will be called after migrating to the latest version.

            //  You can use the DbSet<T>.AddOrUpdate() helper extension method 
            //  to avoid creating duplicate seed data. E.g.
            //
            //    context.People.AddOrUpdate(
            //      p => p.FullName,
            //      new Person { FullName = "Andrew Peters" },
            //      new Person { FullName = "Brice Lambson" },
            //      new Person { FullName = "Rowan Miller" }
            //    );
            //
            context.Settings.AddOrUpdate(s => s.Id,
                new General.Models.Settings {Mode = ScreenMode.Windowed});

            var africa = new Continent {Name = "Africa"};
            var asia = new Continent {Name = "Asia"};
            var australia = new Continent {Name = "Australia and Oceania"};
            var europe = new Continent {Name = "Europe"};
            var namerica = new Continent {Name = "North America"};
            var samerica = new Continent {Name = "South America"};

            context.Continents.AddOrUpdate(
                c => c.Name,
                new Continent {Name = "All Continents"},
                africa,
                asia,
                australia,
                europe,
                namerica,
                samerica);

            context.Regions.AddOrUpdate(
                r => r.Name,
                new Region {Name = "Australia and New Zealand", Continent = australia},
                new Region {Name = "Oceania", Continent = australia},
                new Region {Name = "Central Africa", Continent = africa},
                new Region {Name = "East Africa", Continent = africa},
                new Region {Name = "Southern Africa", Continent = africa},
                new Region {Name = "West Africa", Continent = africa},
                new Region {Name = "North Africa", Continent = africa},
                new Region {Name = "Central Asia", Continent = asia},
                new Region {Name = "East Asia", Continent = asia},
                new Region {Name = "Southeast Asia", Continent = asia},
                new Region {Name = "West Asia", Continent = asia},
                new Region {Name = "Northern Europe", Continent = europe},
                new Region {Name = "Eastern Europe", Continent = europe},
                new Region {Name = "Western Europe", Continent = europe},
                new Region {Name = "Southern Europe", Continent = europe},
                new Region {Name = "Caribbean", Continent = namerica},
                new Region {Name = "North America", Continent = namerica},
                new Region {Name = "Central America", Continent = namerica},
                new Region {Name = "South America", Continent = samerica});
        }
    }
}