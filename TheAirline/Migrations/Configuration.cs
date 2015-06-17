using System.Data.Entity.Migrations;
using TheAirline.Db;
using TheAirline.Infrastructure.Enums;
using TheAirline.Migrations.Seeds;
using TheAirline.Models.General;

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

            context.Continents.AddOrUpdate(
                c => c.Id,
                Continents.All,
                Continents.Africa,
                Continents.Australia,
                Continents.Asia,
                Continents.Europe,
                Continents.NorthAmerica,
                Continents.SouthAmerica);

            context.Regions.AddOrUpdate(
                r => r.Id,
                Seeds.Regions.All,
                Seeds.Regions.Australia,
                Seeds.Regions.Oceania,
                Seeds.Regions.Caribbean,
                Seeds.Regions.CentralAfrica,
                Seeds.Regions.CentralAmerica,
                Seeds.Regions.CentralAsia,
                Seeds.Regions.EastAfrica,
                Seeds.Regions.EastAsia,
                Seeds.Regions.EasternEurope,
                Seeds.Regions.NorthAfrica,
                Seeds.Regions.NorthAmerica,
                Seeds.Regions.NorthernEurope,
                Seeds.Regions.SouthAmerica,
                Seeds.Regions.SoutheastAsia,
                Seeds.Regions.SouthernAfrica,
                Seeds.Regions.SouthernEurope,
                Seeds.Regions.WestAfrica,
                Seeds.Regions.WestAsia,
                Seeds.Regions.WesternEurope);

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