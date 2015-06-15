using System.Data.Entity.Migrations;
using TheAirline.Db;
using TheAirline.Infrastructure.Enums;
using TheAirline.Models.General;
using TheAirline.Models.General.Countries;

namespace TheAirline.Migrations
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
                new Models.General.Settings {Mode = ScreenMode.Windowed});

            var africa = new Continent {Name = "Africa"};
            var asia = new Continent {Name = "Asia"};
            var australia = new Continent {Name = "Australia and Oceania"};
            var europe = new Continent {Name = "Europe"};
            var namerica = new Continent {Name = "North America"};
            var samerica = new Continent {Name = "South America"};

            context.Continents.AddOrUpdate(
                c => c.Id,
                new Continent {Name = "All Continents"},
                africa,
                asia,
                australia,
                europe,
                namerica,
                samerica);

            context.Regions.AddOrUpdate(
                r => r.Id,
                new Region {Name = "Australia and New Zealand", Continent = australia, FuelIndex = 0.98},
                new Region {Name = "Oceania", Continent = australia, FuelIndex = 0.98},
                new Region {Name = "Central Africa", Continent = africa, FuelIndex = 0.97},
                new Region {Name = "East Africa", Continent = africa, FuelIndex = 0.97},
                new Region {Name = "Southern Africa", Continent = africa, FuelIndex = 0.97},
                new Region {Name = "West Africa", Continent = africa, FuelIndex = 0.97},
                new Region {Name = "North Africa", Continent = africa, FuelIndex = 0.97},
                new Region {Name = "Central Asia", Continent = asia, FuelIndex = 0.98},
                new Region {Name = "East Asia", Continent = asia, FuelIndex = 0.98},
                new Region {Name = "Southeast Asia", Continent = asia, FuelIndex = 0.98},
                new Region {Name = "West Asia", Continent = asia, FuelIndex = 0.98},
                new Region {Name = "Northern Europe", Continent = europe, FuelIndex = 1.025},
                new Region {Name = "Eastern Europe", Continent = europe, FuelIndex = 1.025},
                new Region {Name = "Western Europe", Continent = europe, FuelIndex = 1.025},
                new Region {Name = "Southern Europe", Continent = europe, FuelIndex = 1.025},
                new Region {Name = "Caribbean", Continent = namerica, FuelIndex = 1.005},
                new Region {Name = "North America", Continent = namerica, FuelIndex = 1.005},
                new Region {Name = "Central America", Continent = namerica, FuelIndex = 1.04},
                new Region {Name = "South America", Continent = samerica, FuelIndex = 1.04});

            context.Difficulties.AddOrUpdate(
                d => d.Id,
                new Difficulty
                {
                    Name = "Easy",
                    MoneyLevel = 1.5,
                    LoanLevel = 0.75,
                    PassengersLevel = 1.5,
                    PriceLevel = 1,
                    AiLevel = 1.25,
                    StartDataLevel = 5
                },
                new Difficulty
                {
                    Name = "Normal",
                    MoneyLevel = 1,
                    LoanLevel = 1,
                    PassengersLevel = 1.2,
                    PriceLevel = 1.1,
                    AiLevel = 1,
                    StartDataLevel = 2
                },
                new Difficulty
                {
                    Name = "Hard",
                    MoneyLevel = 0.5,
                    LoanLevel = 1.25,
                    PassengersLevel = 1,
                    PriceLevel = 1.2,
                    AiLevel = 0.75,
                    StartDataLevel = 1
                });
        }
    }
}