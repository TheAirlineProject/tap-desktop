using TheAirline.Models.General.Countries;

namespace TheAirline.Migrations.Seeds
{
    public static class Continents
    {
        public static Continent All => new Continent {Name = "All Continents"};
        public static Continent Asia => new Continent {Name = "Asia"};
        public static Continent Africa => new Continent {Name = "Africa"};
        public static Continent Australia => new Continent {Name = "Australia and Oceania"};
        public static Continent Europe => new Continent {Name = "Europe"};
        public static Continent NorthAmerica => new Continent {Name = "North America"};
        public static Continent SouthAmerica => new Continent {Name = "South America"};
    }
}
