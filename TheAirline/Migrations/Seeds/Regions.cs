using TheAirline.Models.General.Countries;

namespace TheAirline.Migrations.Seeds
{
    public static class Regions
    {
        public static Region All => new Region {Name = "All Regions", Continent = Continents.All};

        public static Region Australia
            => new Region {Name = "Australia and New Zealand", Continent = Continents.Australia, FuelIndex = 0.98};

        public static Region Oceania
            => new Region {Name = "Oceania", Continent = Continents.Australia, FuelIndex = 0.98};

        public static Region CentralAfrica
            => new Region {Name = "Central Africa", Continent = Continents.Africa, FuelIndex = 0.97};

        public static Region EastAfrica
            => new Region {Name = "East Africa", Continent = Continents.Africa, FuelIndex = 0.97};

        public static Region SouthernAfrica
            => new Region {Name = "Southern Africa", Continent = Continents.Africa, FuelIndex = 0.97};

        public static Region WestAfrica
            => new Region {Name = "West Africa", Continent = Continents.Africa, FuelIndex = 0.97};

        public static Region NorthAfrica
            => new Region {Name = "North Africa", Continent = Continents.Africa, FuelIndex = 0.97};

        public static Region CentralAsia
            => new Region {Name = "Central Asia", Continent = Continents.Asia, FuelIndex = 0.98};

        public static Region EastAsia => new Region {Name = "East Asia", Continent = Continents.Asia, FuelIndex = 0.98};

        public static Region SoutheastAsia
            => new Region {Name = "Southeast Asia", Continent = Continents.Asia, FuelIndex = 0.98};

        public static Region WestAsia => new Region {Name = "West Asia", Continent = Continents.Asia, FuelIndex = 0.98};

        public static Region NorthernEurope
            => new Region {Name = "Northern Europe", Continent = Continents.Europe, FuelIndex = 1.025};

        public static Region EasternEurope
            => new Region {Name = "Eastern Europe", Continent = Continents.Europe, FuelIndex = 1.025};

        public static Region WesternEurope
            => new Region {Name = "Western Europe", Continent = Continents.Europe, FuelIndex = 1.025};

        public static Region SouthernEurope
            => new Region {Name = "Southern Europe", Continent = Continents.Europe, FuelIndex = 1.025};

        public static Region Caribbean
            => new Region {Name = "Caribbean", Continent = Continents.NorthAmerica, FuelIndex = 1.005};

        public static Region NorthAmerica
            => new Region {Name = "North America", Continent = Continents.NorthAmerica, FuelIndex = 1.005};

        public static Region CentralAmerica
            => new Region {Name = "Central America", Continent = Continents.NorthAmerica, FuelIndex = 1.04};

        public static Region SouthAmerica
            => new Region {Name = "South America", Continent = Continents.SouthAmerica, FuelIndex = 1.04};
    }
}