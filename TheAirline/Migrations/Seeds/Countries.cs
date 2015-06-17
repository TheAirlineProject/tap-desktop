using TheAirline.Models.General.Countries;

namespace TheAirline.Migrations.Seeds
{
    public static class Countries
    {
        public static Country Un => new Country {Name = "United Nations", Region = Regions.All};

        public static Country Senegal
            =>
                new Country
                {
                    Name = "Senegal",
                    Region = Regions.WestAfrica,
                    TailNumberFormat = "6V-\\s3",
                    IsoName = "SN",
                    ShortName = "SEN"
                };

        public static Country Latvia
            =>
                new Country
                {
                    Name = "Latvia",
                    Region = Regions.NorthernEurope,
                    IsoName = "LV",
                    ShortName = "LVA",
                    TailNumberFormat = "YL-\\s3"
                };

        public static Country Lithuania
            =>
                new Country
                {
                    Name = "Lithuania",
                    Region = Regions.NorthernEurope,
                    ShortName = "LTU",
                    TailNumberFormat = "LY-\\s3",
                    IsoName = "LT"
                };
    }
}