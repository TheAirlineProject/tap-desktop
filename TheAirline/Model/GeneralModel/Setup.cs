using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Media;
using System.Windows.Markup;
using System.Xml;
using TheAirline.Model.AirportModel;
using TheAirline.Model.AirlineModel;
using TheAirline.Model.AirlinerModel;
using TheAirline.Model.AirlinerModel.RouteModel;
using TheAirline.Model.GeneralModel.StatisticsModel;
using TheAirline.GraphicsModel.SkinsModel;

namespace TheAirline.Model.GeneralModel
{
    /*! Setup class.
     * This class is used for configuring the games environment.
     * The class needs no instantiation, because all methods are declared static.
     */
    public class Setup
    {
        /*! private static variable basePath.
         * stores the actual defined working directory.
         */
        private static string basePath = Environment.CurrentDirectory;

        /*! private static variable dataPath.
         * stores the path to the Data directory.
         */
        private static string dataPath = basePath + "\\data";

        /*! private static variable basePath.
         * stores the path to the Plugin directory.
         */
        private static string pluginPath = basePath + "\\plugin";

        /*! private static variable rnd.
         * holds a random number.
         */
        private static Random rnd = new Random();

        /*! public static method SetupGame().
         * Tries to create game´s environment and base configuration.
         */
        public static void SetupGame()
        {
            try
            {
                GeneralHelpers.CreateBigImageCanvas();
                ClearLists();

                LoadRegions();
                LoadCountries();
                LoadAirports();
                LoadAirportLogos();
                LoadAirportFacilities();
                LoadAirlineFacilities();
                LoadManufacturers();
                LoadAirliners();
                LoadAirlinerFacilities();

                SetupStatisticsTypes();

                CreateAdvertisementTypes();
                CreateTimeZones();
                CreateFeeTypes();
                CreateAirlines();
                CreateFlightFacilities();
                CreateLanguages();


                CreateSkins();
            }
            catch (Exception e)
            {
                string s = e.ToString();
            }



        }

        /*! private static method ClearLists().
         * Resets game´s environment.
         */
        private static void ClearLists()
        {
            AdvertisementTypes.Clear();
            TimeZones.Clear();
            Airports.Clear();
            AirportFacilities.Clear();
            AirlineFacilities.Clear();
            Manufacturers.Clear();
            Airliners.Clear();
            Airlines.Clear();
            AirlinerFacilities.Clear();
            RouteFacilities.Clear();
            StatisticsTypes.Clear();
            Regions.Clear();
            Countries.Clear();
            AirlinerTypes.Clear();
            Skins.Clear();
            FeeTypes.Clear();
            Languages.Clear();

        }

        /*! private static method CreateSkins().
         * Loads the available skins of the game from the subfolder skins.
         */
        private static void CreateSkins()
        {
            DirectoryInfo dir = new DirectoryInfo(dataPath + "\\skins");

            foreach (FileInfo file in dir.GetFiles("*.xml"))
            {
                LoadSkin(file.FullName);
            }
         

            SkinObject.GetInstance().setCurrentSkin(Skins.GetSkins()[0]);
        }

        /*! private static method LoadSkin(string file).
         * Helper method. Load and Analyses the given skin file.
         * \param filename as string.
         */
        private static void LoadSkin(string file)
        {
            XmlDocument doc = new XmlDocument();
            doc.Load(file);
            XmlElement root = doc.DocumentElement;
            string name = root.Attributes["name"].Value;

            XmlNodeList propertiesList = root.SelectNodes("//property");

            Skin skin = new Skin(name);

            foreach (XmlElement propertyElement in propertiesList)
            {
                string propertyName = propertyElement.Attributes["name"].Value;

                StringReader reader = new StringReader(propertyElement.InnerXml);
                XmlReader xmlReader = XmlReader.Create(reader);
                object o = XamlReader.Load(xmlReader);

                skin.addProperty(new KeyValuePair<string, object>(propertyName, o));
            }

            Skins.AddSkin(skin);

        }

        /*! Creates the Languages.
         * We need not only different languages.
         * In addition we need unit system conversions.
         */
        private static void CreateLanguages()
        {
            Language langUS = new Language("English (US)", "en-US");
            langUS.Unit = Language.UnitSystem.Imperial;
            langUS.addWord("km.", "miles");
            langUS.addWord("km/t", "mph");
            langUS.addWord("l/seat/km", "g/seat/m");
            langUS.addWord("ltr", "gal");

            Languages.AddLanguage(langUS);
            Language langUK = new Language("English (UK)", "en-GB");
            langUK.Unit = Language.UnitSystem.Metric;

            Languages.AddLanguage(langUK);

            GameObject.GetInstance().setLanguage(langUK);
        }
        /*! creates the Advertisement types
         */
        private static void CreateAdvertisementTypes()
        {
            
            AdvertisementTypes.AddAdvertisementType(new AdvertisementType(AdvertisementType.AirlineAdvertisementType.Internet, "No Advertisement", 0, 0));
            AdvertisementTypes.AddAdvertisementType(new AdvertisementType(AdvertisementType.AirlineAdvertisementType.Internet, "National",10000, 1));
            AdvertisementTypes.AddAdvertisementType(new AdvertisementType(AdvertisementType.AirlineAdvertisementType.Newspaper, "No Advertisement", 0, 0));
            AdvertisementTypes.AddAdvertisementType(new AdvertisementType(AdvertisementType.AirlineAdvertisementType.Newspaper, "National", 10000, 1));
            AdvertisementTypes.AddAdvertisementType(new AdvertisementType(AdvertisementType.AirlineAdvertisementType.Radio, "No Advertisement", 0, 0));
            AdvertisementTypes.AddAdvertisementType(new AdvertisementType(AdvertisementType.AirlineAdvertisementType.Radio, "National", 10000, 1));
            AdvertisementTypes.AddAdvertisementType(new AdvertisementType(AdvertisementType.AirlineAdvertisementType.TV, "No Advertisement", 0, 0));
            AdvertisementTypes.AddAdvertisementType(new AdvertisementType(AdvertisementType.AirlineAdvertisementType.TV, "National", 10000, 1));

        }
        /*! creates the time zones.
         */
        private static void CreateTimeZones()
        {
            TimeZones.AddTimeZone(new GameTimeZone("Baker Island Time", "BIT", new TimeSpan(-12, 0, 0)));
            TimeZones.AddTimeZone(new GameTimeZone("Samoa Standard Time", "SST", new TimeSpan(-11, 0, 0)));
            TimeZones.AddTimeZone(new GameTimeZone("Hawaiian Standard Time", "HAST", new TimeSpan(-10, 0, 0)));
            TimeZones.AddTimeZone(new GameTimeZone("Alaskan Standard Time", "MIT", new TimeSpan(-9, 0, 0)));
            TimeZones.AddTimeZone(new GameTimeZone("Pacific Standard Time", "PST", new TimeSpan(-8, 0, 0)));
            TimeZones.AddTimeZone(new GameTimeZone("Mountain Standard Time", "MST", new TimeSpan(-7, 0, 0)));
            TimeZones.AddTimeZone(new GameTimeZone("Central Standard Time", "CST", new TimeSpan(-6, 0, 0)));
            TimeZones.AddTimeZone(new GameTimeZone("Eastern Standard Time", "EST", new TimeSpan(-5, 0, 0)));
            TimeZones.AddTimeZone(new GameTimeZone("Venezuelan Standard Time", "VET", new TimeSpan(-4, 30, 0)));
            TimeZones.AddTimeZone(new GameTimeZone("Atlantic Standard Time", "AST", new TimeSpan(-4, 0, 0)));
            TimeZones.AddTimeZone(new GameTimeZone("Newfoundland Standard Time", "NST", new TimeSpan(-3, 30, 0)));
            TimeZones.AddTimeZone(new GameTimeZone("South America Standard Time", "SAST", new TimeSpan(-3, 0, 0)));
            TimeZones.AddTimeZone(new GameTimeZone("Mid-Atlantic Standard Time", "MAST", new TimeSpan(-2, 0, 0)));
            TimeZones.AddTimeZone(new GameTimeZone("Cape Verde Time", "CVT", new TimeSpan(-1, 0, 0)));
            TimeZones.AddTimeZone(new GameTimeZone("Greenwich Mean Time", "GMT", new TimeSpan(0, 0, 0)));
            TimeZones.AddTimeZone(new GameTimeZone("Central European Time", "CET", new TimeSpan(1, 0, 0)));
            TimeZones.AddTimeZone(new GameTimeZone("Eastern European Time", "EET", new TimeSpan(2, 0, 0)));
            TimeZones.AddTimeZone(new GameTimeZone("East Africa Time", "EAT", new TimeSpan(3, 0, 0)));
            TimeZones.AddTimeZone(new GameTimeZone("Iran Standard Time", "IRST", new TimeSpan(3, 30, 0)));
            TimeZones.AddTimeZone(new GameTimeZone("Gulf Standard Time", "GST", new TimeSpan(4, 0, 0)));
            TimeZones.AddTimeZone(new GameTimeZone("Afghanistan Time", "AFT", new TimeSpan(4, 30, 0)));
            TimeZones.AddTimeZone(new GameTimeZone("West Asia Standard Time", "WAST", new TimeSpan(5, 0, 0)));
            TimeZones.AddTimeZone(new GameTimeZone("Indian Standard Time", "IST", new TimeSpan(5, 30, 0)));
            TimeZones.AddTimeZone(new GameTimeZone("Asia Standard Time", "ASST", new TimeSpan(6, 0, 0)));
            TimeZones.AddTimeZone(new GameTimeZone("Central Asia Standard Time", "CEST", new TimeSpan(7, 0, 0)));
            TimeZones.AddTimeZone(new GameTimeZone("China Standard Time", "CST", new TimeSpan(8, 0, 0)));
            TimeZones.AddTimeZone(new GameTimeZone("Japan Standard Time", "JST", new TimeSpan(9, 0, 0)));
            TimeZones.AddTimeZone(new GameTimeZone("Australia Standard Time", "AST", new TimeSpan(9, 30, 0)));
            TimeZones.AddTimeZone(new GameTimeZone("Australian Eastern Standard Time", "AEST", new TimeSpan(10, 0, 0)));
            TimeZones.AddTimeZone(new GameTimeZone("Australian Central Standard Time", "ACST", new TimeSpan(10, 30, 0)));
            TimeZones.AddTimeZone(new GameTimeZone("Central Pacific Standard Time", "CPST", new TimeSpan(11, 0, 0)));
            TimeZones.AddTimeZone(new GameTimeZone("New Zealand Standard Time", "NZST", new TimeSpan(12, 0, 0)));
            TimeZones.AddTimeZone(new GameTimeZone("Tonga Standard Time", "TST", new TimeSpan(13, 0, 0)));



        }

        /*! loads the airline facilities.
         */
        private static void LoadAirlineFacilities()
        {
            XmlDocument doc = new XmlDocument();
            doc.Load(dataPath + "\\airlinefacilities.xml");
            XmlElement root = doc.DocumentElement;

            XmlNodeList facilitiesList = root.SelectNodes("//facility");

            foreach (XmlElement facility in facilitiesList)
            {
                string name = facility.Attributes["name"].Value;
                string shortname = facility.Attributes["shortname"].Value;
                double price = XmlConvert.ToDouble(facility.Attributes["price"].Value);

                XmlElement levelElement = (XmlElement)facility.SelectSingleNode("level");
                int service = Convert.ToInt32(levelElement.Attributes["service"].Value);
                int luxury = Convert.ToInt32(levelElement.Attributes["luxury"].Value);

                AirlineFacilities.AddFacility(new AirlineFacility(name, shortname, price, service, luxury));

            }
        }

        /*! loads the regions.
         */
        private static void LoadRegions()
        {
            XmlDocument doc = new XmlDocument();
            doc.Load(dataPath + "\\regions.xml");
            XmlElement root = doc.DocumentElement;

            XmlNodeList regionsList = root.SelectNodes("//region");
            foreach (XmlElement region in regionsList)
            {
                string name = region.Attributes["name"].Value;
                Regions.AddRegion(new Region(name));
            }
        }

        /*! loads the manufacturers.
         */
        private static void LoadManufacturers()
        {
            XmlDocument doc = new XmlDocument();
            doc.Load(dataPath + "\\manufacturers.xml");
            XmlElement root = doc.DocumentElement;

            XmlNodeList manufacturersList = root.SelectNodes("//manufacturer");
            foreach (XmlElement manufacturer in manufacturersList)
            {
                string name = manufacturer.Attributes["name"].Value;
                string shortname = manufacturer.Attributes["shortname"].Value;

                Country country = Countries.GetCountry(manufacturer.Attributes["country"].Value);

                Manufacturers.AddManufacturer(new Manufacturer(name, shortname, country));
            }
        }

        /*!loads the airliners.
         */
        private static void LoadAirliners()
        {
            XmlDocument doc = new XmlDocument();
            doc.Load(dataPath + "\\airliners.xml");
            XmlElement root = doc.DocumentElement;

            XmlNodeList airlinersList = root.SelectNodes("//airliner");

            foreach (XmlElement airliner in airlinersList)
            {
                string manufacturerName = airliner.Attributes["manufacturer"].Value;
                Manufacturer manufacturer = Manufacturers.GetManufacturer(manufacturerName);

                string name = airliner.Attributes["name"].Value;
                long price = Convert.ToInt64(airliner.Attributes["price"].Value);

                 XmlElement typeElement = (XmlElement)airliner.SelectSingleNode("type");
                AirlinerType.BodyType body = (AirlinerType.BodyType)Enum.Parse(typeof(AirlinerType.BodyType), typeElement.Attributes["body"].Value);
                AirlinerType.TypeRange rangeType = (AirlinerType.TypeRange)Enum.Parse(typeof(AirlinerType.TypeRange), typeElement.Attributes["rangetype"].Value);
                AirlinerType.EngineType engine = (AirlinerType.EngineType)Enum.Parse(typeof(AirlinerType.EngineType), typeElement.Attributes["engine"].Value);


                XmlElement specsElement = (XmlElement)airliner.SelectSingleNode("specs");
                double wingspan = XmlConvert.ToDouble(specsElement.Attributes["wingspan"].Value);
                double length = XmlConvert.ToDouble(specsElement.Attributes["length"].Value);
                long range = Convert.ToInt64(specsElement.Attributes["range"].Value);
                double speed = XmlConvert.ToDouble(specsElement.Attributes["speed"].Value);
                double fuel = XmlConvert.ToDouble(specsElement.Attributes["consumption"].Value);

                XmlElement capacityElement = (XmlElement)airliner.SelectSingleNode("capacity");
                int passengers = Convert.ToInt16(capacityElement.Attributes["passengers"].Value);
                int cockpitcrew = Convert.ToInt16(capacityElement.Attributes["cockpitcrew"].Value);
                int cabincrew = Convert.ToInt16(capacityElement.Attributes["cabincrew"].Value);
                int maxClasses = Convert.ToInt16(capacityElement.Attributes["maxclasses"].Value);
          
                XmlElement producedElement = (XmlElement)airliner.SelectSingleNode("produced");
                int from = Convert.ToInt16(producedElement.Attributes["from"].Value);
                int to = Convert.ToInt16(producedElement.Attributes["to"].Value);

                AirlinerTypes.AddType(new AirlinerType(manufacturer, name, passengers, cockpitcrew,cabincrew, speed, range, wingspan, length, fuel, price,maxClasses,body,rangeType,engine,new ProductionPeriod(from, to)));
            }
        }

        /*!loads the airports.
         */
        private static void LoadAirports()
        {
            XmlDocument doc = new XmlDocument();
            doc.Load(dataPath + "\\airports.xml");
            XmlElement root = doc.DocumentElement;

            XmlNodeList airportsList = root.SelectNodes("//airport");


            foreach (XmlElement airport in airportsList)
            {
                string name = airport.Attributes["name"].Value;
                string iata = airport.Attributes["iata"].Value;

                AirportProfile.AirportType type = (AirportProfile.AirportType)Enum.Parse(typeof(AirportProfile.AirportType), airport.Attributes["type"].Value);

                XmlElement townElement = (XmlElement)airport.SelectSingleNode("town");
                string town = townElement.Attributes["town"].Value;
                string country = townElement.Attributes["country"].Value;
                TimeSpan gmt = TimeSpan.Parse(townElement.Attributes["GMT"].Value);
                TimeSpan dst = TimeSpan.Parse(townElement.Attributes["DST"].Value);

                XmlElement latitudeElement = (XmlElement)airport.SelectSingleNode("coordinates/latitude");
                XmlElement longitudeElement = (XmlElement)airport.SelectSingleNode("coordinates/longitude");
                Coordinate latitude = Coordinate.Parse(latitudeElement.Attributes["value"].Value);
                Coordinate longitude = Coordinate.Parse(longitudeElement.Attributes["value"].Value);

                XmlElement sizeElement = (XmlElement)airport.SelectSingleNode("size");
                AirportProfile.AirportSize size = (AirportProfile.AirportSize)Enum.Parse(typeof(AirportProfile.AirportSize), sizeElement.Attributes["value"].Value);

                XmlElement gatesElement = (XmlElement)airport.SelectSingleNode("gates");
                int gates = Convert.ToInt32(gatesElement.Attributes["value"].Value);

                AirportProfile profile = new AirportProfile(name, iata, type, town, Countries.GetCountry(country), gmt, dst, new Coordinates(latitude, longitude), size, gates);
                Airports.AddAirport(new Airport(profile));
            }



        }

        /*!loads the airport facilities.
         */
        private static void LoadAirportFacilities()
        {
            XmlDocument doc = new XmlDocument();
            doc.Load(dataPath + "\\airportfacilities.xml");
            XmlElement root = doc.DocumentElement;

            XmlNodeList facilitiesList = root.SelectNodes("//facility");

            foreach (XmlElement facility in facilitiesList)
            {
                string name = facility.Attributes["name"].Value;
                string shortname = facility.Attributes["shortname"].Value;
                AirportFacility.FacilityType type =
      (AirportFacility.FacilityType)Enum.Parse(typeof(AirportFacility.FacilityType), facility.Attributes["type"].Value);
                int typeLevel = Convert.ToInt16(facility.Attributes["typelevel"].Value);

                double price = XmlConvert.ToDouble(facility.Attributes["price"].Value);

                XmlElement levelElement = (XmlElement)facility.SelectSingleNode("level");
                int service = Convert.ToInt32(levelElement.Attributes["service"].Value);
                int luxury = Convert.ToInt32(levelElement.Attributes["luxury"].Value);

                AirportFacilities.AddFacility(new AirportFacility(name, shortname, type, typeLevel, price, service, luxury));

            }
        }

        /*!loads the countries.
         */
        private static void LoadCountries()
        {
            XmlDocument doc = new XmlDocument();
            doc.Load(dataPath + "\\countries.xml");
            XmlElement root = doc.DocumentElement;

            XmlNodeList countriesList = root.SelectNodes("//country");
            foreach (XmlElement element in countriesList)
            {
                string name = element.Attributes["name"].Value;
                string shortname = element.Attributes["shortname"].Value;
                Region region = Regions.GetRegion(element.Attributes["region"].Value);
                string tailformat = element.Attributes["tailformat"].Value;

                Country country = new Country(name, shortname, region, tailformat);
                country.Flag = dataPath + "\\graphics\\flags\\" + name + ".png";
                Countries.AddCountry(country);
            }

        }

        /*! load the airliner facilities.
         */
        private static void LoadAirlinerFacilities()
        {
        
            XmlDocument doc = new XmlDocument();
            doc.Load(dataPath + "\\airlinerfacilities.xml");
            XmlElement root = doc.DocumentElement;

            XmlNodeList facilitiesList = root.SelectNodes("//facility");
            foreach (XmlElement element in facilitiesList)
            {

                string name = element.Attributes["name"].Value;
                AirlinerFacility.FacilityType type = (AirlinerFacility.FacilityType)Enum.Parse(typeof(AirlinerFacility.FacilityType), element.Attributes["type"].Value);
                int fromyear = Convert.ToInt16(element.Attributes["fromyear"].Value);
             
                XmlElement levelElement = (XmlElement)element.SelectSingleNode("level");
                int service = Convert.ToInt32(levelElement.Attributes["service"].Value);

                XmlElement seatsElement = (XmlElement)element.SelectSingleNode("seats");
                double seatsPercent = XmlConvert.ToDouble(seatsElement.Attributes["percent"].Value);
                double seatsPrice = XmlConvert.ToDouble(seatsElement.Attributes["price"].Value);
                double seatuse = XmlConvert.ToDouble(seatsElement.Attributes["uses"].Value);


                AirlinerFacilities.AddFacility(new AirlinerFacility(type, name,fromyear, service, seatsPercent, seatsPrice, seatuse));


            }
        }

        /*! sets up the statistics types.
         */
        private static void SetupStatisticsTypes()
        {
            StatisticsTypes.AddStatisticsType(new StatisticsType("Arrivals", "Arrivals"));
            StatisticsTypes.AddStatisticsType(new StatisticsType("Departures", "Departures"));
            StatisticsTypes.AddStatisticsType(new StatisticsType("Passengers", "Passengers"));
            StatisticsTypes.AddStatisticsType(new StatisticsType("Passengers per flight", "Passengers%"));
        }

        /*! create a random airliner with a minimum range.
         */
        private static Airliner CreateAirliner(double minRange)
        {
             List<AirlinerType> types = AirlinerTypes.GetTypes().FindAll((delegate(AirlinerType t) { return t.Range >= minRange && t.Produced.From < GameObject.GetInstance().GameTime.Year && t.Produced.To > GameObject.GetInstance().GameTime.Year - 30; }));

            int typeNumber = rnd.Next(types.Count);
            AirlinerType type = types[typeNumber];

            int countryNumber = rnd.Next(Countries.GetCountries().Count);
            Country country = Countries.GetCountries()[countryNumber];

            int builtYear = rnd.Next(Math.Max(type.Produced.From, GameObject.GetInstance().GameTime.Year - 30), Math.Min(GameObject.GetInstance().GameTime.Year, type.Produced.To));


            Airliner airliner = new Airliner(type, country.TailNumbers.getNextTailNumber(), new DateTime(builtYear, 1, 1));

            int age = MathHelpers.CalculateAge(airliner.BuiltDate, GameObject.GetInstance().GameTime);

            long kmPerYear = rnd.Next(100000, 1000000);
            long km = kmPerYear * age;

            airliner.Flown = km;

            return airliner;
        }

        /*! create some game airliners.
         */
        public static void CreateAirliners()
        {

            int number = AirlinerTypes.GetTypes().FindAll((delegate(AirlinerType t) { return t.Produced.From <= GameObject.GetInstance().GameTime.Year && t.Produced.To >= GameObject.GetInstance().GameTime.Year - 30; })).Count * 25;
            for (int i = 0; i < number; i++)
            {


                Airliners.AddAirliner(CreateAirliner(0));

            }

        }

        /*! create some game airlines.
         */
        private static void CreateAirlines()
        {
            Airlines.AddAirline(new Airline(new AirlineProfile("Air Vegas", "6V", "LightBlue", Countries.GetCountry("United States"), "Michael Smidth")));
            Airlines.AddAirline(new Airline(new AirlineProfile("German Wings", "GER", "DarkRed", Countries.GetCountry("Germany"), "Franz Ötzel")));
            Airlines.AddAirline(new Airline(new AirlineProfile("Key Airlines", "KWY", "Black", Countries.GetCountry("United States"), "Peter Hanson")));
            Airlines.AddAirline(new Airline(new AirlineProfile("AccessAir", "ZA", "DarkGreen", Countries.GetCountry("United States"), "Benjamin Watson")));
            Airlines.AddAirline(new Airline(new AirlineProfile("Big Sky Airlines", "CQ", "DarkBlue", Countries.GetCountry("United States"), "George Clark")));
            Airlines.AddAirline(new Airline(new AirlineProfile("Caledonian Airways", "KG", "Purple", Countries.GetCountry("United Kingdom"), "Thomas Owen")));
            Airlines.AddAirline(new Airline(new AirlineProfile("Australian Airlines", "AO", "Orange", Countries.GetCountry("Australia"), "Clive MacPherson")));
            Airlines.AddAirline(new Airline(new AirlineProfile("Adam Air", "KI", "LightGreen", Countries.GetCountry("Indonesia"), "Adam Adhitya Suherman")));
            Airlines.AddAirline(new Airline(new AirlineProfile("Nationwide Airlines", "CE", "Yellow", Countries.GetCountry("South Africa"), "Vernon Bricknell")));
            Airlines.AddAirline(new Airline(new AirlineProfile("Dinar Líneas Aéreas", "D7", "LightBlue", Countries.GetCountry("Argentina"), "Manuel Santosa")));


            GameObject.GetInstance().HumanAirline = Airlines.GetAirline("KWY");

            CreateAirlineLogos();
            
           

        }

        /*! sets up test game.
         */
        public static void SetupTestGame(int opponents)
        {
            RemoveAirlines(opponents);

            foreach (Airline airline in Airlines.GetAirlines())
            {
                airline.Money = GameObject.GetInstance().StartMoney;
                if (!airline.IsHuman) CreateComputerRoutes(airline);
            }

            // chs, 2011-21-10 added for the possibility of creating a new airline
            //sets all the facilities at an airport to none for all airlines
            foreach (Airport airport in Airports.GetAirports())
            {
                foreach (Airline airline in Airlines.GetAirlines())
                {
                    foreach (AirportFacility.FacilityType type in Enum.GetValues(typeof(AirportFacility.FacilityType)))
                    {
                        AirportFacility noneFacility = AirportFacilities.GetFacilities(type).Find((delegate(AirportFacility facility) { return facility.TypeLevel == 0; }));

                        airport.setAirportFacility(airline, noneFacility);

                    }
                }
            }
        }

        /*! removes some random airlines from the list bases on number of opponents.
         */
        private static void RemoveAirlines(int opponnents)
        {
            int count = Airlines.GetAirlines().FindAll((delegate(Airline a) { return !a.IsHuman; })).Count;

            for (int i = 0; i < count - opponnents; i++)
            {
                List<Airline> airlines = Airlines.GetAirlines().FindAll((delegate(Airline a) { return !a.IsHuman; }));

                Airlines.RemoveAirline(airlines[rnd.Next(airlines.Count)]);
            }
        }

        /*! creates some airliners and routes for a computer airline.
         */
        private static void CreateComputerRoutes(Airline airline)
        {
            Airport airport = null;

            Boolean isFree = false;

            Region region = airline.Profile.Country.Region;

            List<Airport> airports = Airports.GetAirports(region).FindAll((delegate(Airport a) { return a.Profile.Size == AirportProfile.AirportSize.Very_small || a.Profile.Size == AirportProfile.AirportSize.Smallest; }));

            while (!isFree)
            {
                airport = airports[rnd.Next(airports.Count)];
                isFree = airport.Gates.getFreeGates() > 1;
            }

            airport.Gates.rentGate(airline);
            airport.Gates.rentGate(airline);

            List<AirportFacility> facilities = AirportFacilities.GetFacilities(AirportFacility.FacilityType.Service);

            AirportFacility facility = facilities.Find((delegate(AirportFacility f) { return f.TypeLevel == 1; }));

            airport.setAirportFacility(GameObject.GetInstance().HumanAirline, facility);


            airports = Airports.GetAirports(region);

            double dist = 0;

            isFree = false;

            airport = null;

            Boolean isInArea = false;


            var query = from a in Airliners.GetAirlinersForSale()
                        select a.Type.Range;

            double maxDistance = query.Max();

            while (dist < 50 || dist > maxDistance || !isFree || !isInArea)
            {
                airport = airports[rnd.Next(airports.Count)];

                dist = MathHelpers.GetDistance(airport.Profile.Coordinates, airline.Airports[0].Profile.Coordinates);

                isFree = airport.Gates.getFreeGates() > 0;

                isInArea = (airport.Profile.Type != AirportProfile.AirportType.Domestic && airline.Airports[0].Profile.Type != AirportProfile.AirportType.Domestic) || (airport.Profile.Country == airline.Airports[0].Profile.Country);
            }

            airport.Gates.rentGate(airline);

            double price = PassengerHelpers.GetPassengerPrice(airport, airline.Airports[0]);

            Guid id = Guid.NewGuid();

            Route route = new Route(id.ToString(), airport, airline.Airports[0], price, airline.getNextFlightCode(), airline.getNextFlightCode());

            foreach (AirlinerClass.ClassType type in Enum.GetValues(typeof(AirlinerClass.ClassType)))
            {
                route.getRouteAirlinerClass(type).CabinCrew = 2;
                route.getRouteAirlinerClass(type).DrinksFacility = RouteFacilities.GetFacilities(RouteFacility.FacilityType.Drinks)[rnd.Next(RouteFacilities.GetFacilities(RouteFacility.FacilityType.Drinks).Count)];// RouteFacilities.GetBasicFacility(RouteFacility.FacilityType.Drinks);
                route.getRouteAirlinerClass(type).FoodFacility = RouteFacilities.GetFacilities(RouteFacility.FacilityType.Food)[rnd.Next(RouteFacilities.GetFacilities(RouteFacility.FacilityType.Food).Count)];//RouteFacilities.GetBasicFacility(RouteFacility.FacilityType.Food);
            }
            airline.addRoute(route);

            airport.Gates.getEmptyGate(airline).Route = route;
            airline.Airports[0].Gates.getEmptyGate(airline).Route = route;

            double distance = MathHelpers.GetDistance(route.Destination1.Profile.Coordinates, route.Destination2.Profile.Coordinates);

            AirlinerType.TypeRange rangeType = GeneralHelpers.ConvertDistanceToRangeType(dist);
          
            List<Airliner> airliners = Airliners.GetAirlinersForSale().FindAll((delegate(Airliner a) { return a.getPrice() < airline.Money && dist<a.Type.Range &&  rangeType == a.Type.RangeType; }));

            if (airliners.Count == 0)
            {
                airliners = Airliners.GetAirlinersForSale().FindAll((delegate(Airliner a) { return distance < a.Type.Range; }));
                airliners.Sort(delegate(Airliner a1, Airliner a2) { return a1.getPrice().CompareTo(a2.getPrice()); });

                airliners = airliners.GetRange(0, 1);

                double amount = airliners[0].getPrice() - airline.Money + 20000000;


                Loan loan = new Loan(GameObject.GetInstance().GameTime, amount, 120, GeneralHelpers.GetAirlineLoanRate(airline));

                double payment = loan.getMonthlyPayment();

                airline.addLoan(loan);
                airline.addInvoice(new Invoice(loan.Date, Invoice.InvoiceType.Loans, loan.Amount));

            }
            Airliner airliner = airliners[rnd.Next(airliners.Count)];


            if (Countries.GetCountryFromTailNumber(airliner.TailNumber).Name != airline.Profile.Country.Name)
                airliner.TailNumber = airline.Profile.Country.TailNumbers.getNextTailNumber();


      
            FleetAirliner fAirliner = new FleetAirliner(FleetAirliner.PurchasedType.Bought, airline, airliner, airliner.TailNumber, airline.Airports[0]);

            RouteAirliner rAirliner = new RouteAirliner(fAirliner, route);

            airline.addInvoice(new Invoice(GameObject.GetInstance().GameTime, Invoice.InvoiceType.Purchases, -airliner.getPrice()));

            fAirliner.RouteAirliner = rAirliner;

            airline.Fleet.Add(fAirliner);

            rAirliner.Status = RouteAirliner.AirlinerStatus.To_route_start;

        }

        /*! loads the logos for the airports.
         */
        private static void LoadAirportLogos()
        {
            DirectoryInfo dir = new DirectoryInfo(dataPath + "\\graphics\\airportlogos");

           foreach (FileInfo file in dir.GetFiles("*.png"))
            {
                string code = file.Name.Split('.')[0].ToUpper();
                Airport airport = Airports.GetAirport(code);

                if (airport != null)
                    airport.Profile.Logo = file.FullName;
                else
                    code = "x";
            }

        }

        /*! loads the logos for the game airlines.
         */
        private static void CreateAirlineLogos()
        {
            foreach (Airline airline in Airlines.GetAirlines())
                airline.Profile.Logo = dataPath + "\\graphics\\airlinelogos\\" + airline.Profile.IATACode + ".png";
        }

        /*! private stativ method CreateFlightFacilities.
         * creates the in flight facilities.
         */
        private static void CreateFlightFacilities()
        {
            RouteFacilities.AddFacility(new RouteFacility(RouteFacility.FacilityType.Food, "No Food", 1, -50, RouteFacility.ExpenseType.Fixed, 0,null));
            RouteFacilities.AddFacility(new RouteFacility(RouteFacility.FacilityType.Food, "Buyable Snacks", 1, 10, RouteFacility.ExpenseType.Random, 0,FeeTypes.GetType("Snacks")));
            RouteFacilities.AddFacility(new RouteFacility(RouteFacility.FacilityType.Food, "Buyable Meal", 1, 25, RouteFacility.ExpenseType.Random, 0, FeeTypes.GetType("Meal")));
            RouteFacilities.AddFacility(new RouteFacility(RouteFacility.FacilityType.Food, "Basic Meal", 2, 50, RouteFacility.ExpenseType.Fixed, 20,null));
            RouteFacilities.AddFacility(new RouteFacility(RouteFacility.FacilityType.Food, "Full Dinner", 3, 100, RouteFacility.ExpenseType.Fixed, 40,null));
            RouteFacilities.AddFacility(new RouteFacility(RouteFacility.FacilityType.Drinks, "No Drinks", 1, -50, RouteFacility.ExpenseType.Fixed, 0,null));
            RouteFacilities.AddFacility(new RouteFacility(RouteFacility.FacilityType.Drinks, "Buyable Drinks", 1, 25, RouteFacility.ExpenseType.Random, 0, FeeTypes.GetType("Drinks")));
            RouteFacilities.AddFacility(new RouteFacility(RouteFacility.FacilityType.Drinks, "Free Drinks", 3, 100, RouteFacility.ExpenseType.Fixed, 30,null));
        }

        /*! creates the Fee types.
         */
        private static void CreateFeeTypes()
        {
           
            FeeTypes.AddType(new FeeType(FeeType.eFeeType.Wage, "Cabin kilometer rate", 0.8, 0.2, 2, 100));
            FeeTypes.AddType(new FeeType(FeeType.eFeeType.Wage, "Cockpit kilometer rate", 2, 1, 4, 100));
            FeeTypes.AddType(new FeeType(FeeType.eFeeType.Wage, "Cockpit wage", 100, 20, 200, 100));
            FeeTypes.AddType(new FeeType(FeeType.eFeeType.Wage, "Cabin wage", 50, 10, 100, 100));
            FeeTypes.AddType(new FeeType(FeeType.eFeeType.FoodDrinks, "Drinks", 2, 0.5, 6, 75));
            FeeTypes.AddType(new FeeType(FeeType.eFeeType.FoodDrinks, "Snacks", 3, 2, 4, 70));
            FeeTypes.AddType(new FeeType(FeeType.eFeeType.FoodDrinks, "Meal", 5, 4, 6, 50));
            FeeTypes.AddType(new FeeType(FeeType.eFeeType.Fee, "1 Bag", 0, 0, 30, 95));
            FeeTypes.AddType(new FeeType(FeeType.eFeeType.Fee, "2 Bags", 10, 0, 45, 25));
            FeeTypes.AddType(new FeeType(FeeType.eFeeType.Fee, "3+ Bags", 20, 0, 55, 2));
            FeeTypes.AddType(new FeeType(FeeType.eFeeType.Fee, "Pets", 0, 0, 150, 1));
		}

        /*! public static method getBasePath.
         * returns the path to the working directory.
         * \return working directory path as string.
         */
        public static string getBasePath()
        {
            return basePath;
        }

        /*! public static method getDataPath.
         * returns the path to the Data directory.
         * \return Data directory path as string.
         */
        public static string getDataPath()
        {
            return dataPath;
        }

        /*! public static method getPluginPath.
         * returns the path to the Plugin directory.
         * \return Plugin directory path as string.
         */
        public static string getPluginPath()
        {
            return pluginPath;
        }
    }


}
