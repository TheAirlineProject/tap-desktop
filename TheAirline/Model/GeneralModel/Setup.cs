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
using TheAirline.Model.GeneralModel.Helpers;
using TheAirline.Model.PassengerModel;
using TheAirline.Model.GeneralModel.CountryModel;
using TheAirline.Model.GeneralModel.HolidaysModel;
using TheAirline.Model.GeneralModel.CountryModel.TownModel;
using TheAirline.Model.AirlineModel.SubsidiaryModel;
using TheAirline.Model.GeneralModel.HistoricEventModel;
using TheAirline.Model.GeneralModel.WeatherModel;
using System.Threading.Tasks;
using TheAirline.Model.PilotModel;
using System.Globalization;

namespace TheAirline.Model.GeneralModel
{
    /*! Setup class.
     * This class is used for configuring the games environment.
     * The class needs no instantiation, because all methods are declared static.
     */
    public class Setup
    {
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

                CreateTimeZones();
                SetupDifficultyLevels();
                SetupStatisticsTypes();

                LoadRegions();
                LoadCountries();
                LoadStates();
                LoadTemporaryCountries();
                LoadUnions();
                LoadAirports();
                LoadAirportLogos();
                LoadAirportMaps();
                LoadAirportFacilities();
                LoadAirlineFacilities();
                LoadManufacturers();
                LoadManufacturerLogos();
                LoadAirliners();
                LoadAirlinerFacilities();
                LoadFlightRestrictions();
                LoadInflationYears();
                LoadHolidays();
                LoadHistoricEvents();
                LoadWeatherAverages();

                CreateAdvertisementTypes();
                CreateFeeTypes();
                CreateFlightFacilities();
                CreateTrainingAircraftTypes();

                LoadStandardConfigurations();

                LoadAirlines();

                Skins.Init();
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
            TemporaryCountries.Clear();
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
            States.Clear();
            Unions.Clear();
            AirlinerTypes.Clear();
            Skins.Clear();
            FeeTypes.Clear();
            Alliances.Clear();
            FlightRestrictions.Clear();
            Inflations.Clear();
            Holidays.Clear();
            Configurations.Clear();
            HistoricEvents.Clear();
            WeatherAverages.Clear();
            DifficultyLevels.Clear();
            Pilots.Clear();
            Instructors.Clear();
            TrainingAircraftTypes.Clear();
        }
        /*! creates some pilots
         */
        private static void CreatePilots()
        {
            if (GameObject.GetInstance().PilotsEnabled)
            {
                List<Town> towns = Towns.GetTowns();

                int pilotsPool = 100;

                for (int i = 0; i < pilotsPool; i++)
                {

                    Town town = towns[rnd.Next(towns.Count)];
                    DateTime birthdate = MathHelpers.GetRandomDate(GameObject.GetInstance().GameTime.AddYears(-55), GameObject.GetInstance().GameTime.AddYears(-23));
                    PilotProfile profile = new PilotProfile(Names.GetInstance().getRandomFirstName(), Names.GetInstance().getRandomLastName(), birthdate, town);

                    Dictionary<Pilot.PilotRating, int> rankings = new Dictionary<Pilot.PilotRating, int>();
                    rankings.Add(Pilot.PilotRating.A, 10);
                    rankings.Add(Pilot.PilotRating.B, 20);
                    rankings.Add(Pilot.PilotRating.C, 40);
                    rankings.Add(Pilot.PilotRating.D, 20);
                    rankings.Add(Pilot.PilotRating.E, 10);

                    Pilot.PilotRating ranking = AIHelpers.GetRandomItem<Pilot.PilotRating>(rankings);

                    int fromYear = Math.Min(GameObject.GetInstance().GameTime.Year-1, birthdate.AddYears(23).Year);
                    int toYear = Math.Min(GameObject.GetInstance().GameTime.Year, birthdate.AddYears(55).Year);

                    DateTime educationTime = MathHelpers.GetRandomDate(birthdate.AddYears(23), new DateTime(toYear,1,1));
                    Pilot pilot = new Pilot(profile, educationTime, ranking);

                    Pilots.AddPilot(pilot);
                }

                int instructorsPool = 100;

                 for (int i = 0; i < instructorsPool; i++)
                {
                        Town town = towns[rnd.Next(towns.Count)];
                        DateTime birthdate = MathHelpers.GetRandomDate(GameObject.GetInstance().GameTime.AddYears(-55), GameObject.GetInstance().GameTime.AddYears(-23));
                        PilotProfile profile = new PilotProfile(Names.GetInstance().getRandomFirstName(), Names.GetInstance().getRandomLastName(), birthdate, town);

                        Dictionary<Pilot.PilotRating, int> rankings = new Dictionary<Pilot.PilotRating, int>();
                        rankings.Add(Pilot.PilotRating.A, 10);
                        rankings.Add(Pilot.PilotRating.B, 20);
                        rankings.Add(Pilot.PilotRating.C, 40);
                        rankings.Add(Pilot.PilotRating.D, 20);
                        rankings.Add(Pilot.PilotRating.E, 10);

                        Pilot.PilotRating ranking = AIHelpers.GetRandomItem<Pilot.PilotRating>(rankings);

                        Instructor instructor = new Instructor(profile, ranking);

                        Instructors.AddInstructor(instructor);
                    }
            }
        }
        /*! creates the training aircraft types
         */
        private static void CreateTrainingAircraftTypes()
        {
            TrainingAircraftTypes.AddAircraftType(new TrainingAircraftType("Cessna 172", 26705,2));
            TrainingAircraftTypes.AddAircraftType(new TrainingAircraftType("Beechcraft King Air 350", 129520,12));
        
        }
        /*! creates the Advertisement types
         */
        private static void CreateAdvertisementTypes()
        {
            AdvertisementTypes.AddAdvertisementType(new AdvertisementType(AdvertisementType.AirlineAdvertisementType.Internet, "No Advertisement", 0, 0));
            AdvertisementTypes.AddAdvertisementType(new AdvertisementType(AdvertisementType.AirlineAdvertisementType.Internet, "Local", 5000, 1));
            AdvertisementTypes.AddAdvertisementType(new AdvertisementType(AdvertisementType.AirlineAdvertisementType.Internet, "National", 10000, 2));
            AdvertisementTypes.AddAdvertisementType(new AdvertisementType(AdvertisementType.AirlineAdvertisementType.Internet, "Global", 25000, 3));
            AdvertisementTypes.AddAdvertisementType(new AdvertisementType(AdvertisementType.AirlineAdvertisementType.Newspaper, "No Advertisement", 0, 0));
            AdvertisementTypes.AddAdvertisementType(new AdvertisementType(AdvertisementType.AirlineAdvertisementType.Newspaper, "Local", 5000, 1));
            AdvertisementTypes.AddAdvertisementType(new AdvertisementType(AdvertisementType.AirlineAdvertisementType.Newspaper, "National", 10000, 2));
            AdvertisementTypes.AddAdvertisementType(new AdvertisementType(AdvertisementType.AirlineAdvertisementType.Newspaper, "Global", 25000, 3));
            AdvertisementTypes.AddAdvertisementType(new AdvertisementType(AdvertisementType.AirlineAdvertisementType.Radio, "No Advertisement", 0, 0));
            AdvertisementTypes.AddAdvertisementType(new AdvertisementType(AdvertisementType.AirlineAdvertisementType.Radio, "Local", 5000, 1));
            AdvertisementTypes.AddAdvertisementType(new AdvertisementType(AdvertisementType.AirlineAdvertisementType.Radio, "National", 10000, 2));
            AdvertisementTypes.AddAdvertisementType(new AdvertisementType(AdvertisementType.AirlineAdvertisementType.Radio, "Global", 25000, 3));
            AdvertisementTypes.AddAdvertisementType(new AdvertisementType(AdvertisementType.AirlineAdvertisementType.TV, "No Advertisement", 0, 0));
            AdvertisementTypes.AddAdvertisementType(new AdvertisementType(AdvertisementType.AirlineAdvertisementType.TV, "Local", 5000, 1));
            AdvertisementTypes.AddAdvertisementType(new AdvertisementType(AdvertisementType.AirlineAdvertisementType.TV, "National", 10000, 2));
            AdvertisementTypes.AddAdvertisementType(new AdvertisementType(AdvertisementType.AirlineAdvertisementType.TV, "Global", 25000, 3));
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
            TimeZones.AddTimeZone(new GameTimeZone("Venezuelan Standard Time", "VET", new TimeSpan(-4, -30, 0)));
            TimeZones.AddTimeZone(new GameTimeZone("Atlantic Standard Time", "AST", new TimeSpan(-4, 0, 0)));
            TimeZones.AddTimeZone(new GameTimeZone("Newfoundland Standard Time", "NST", new TimeSpan(-3, -30, 0)));
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
            TimeZones.AddTimeZone(new GameTimeZone("Nepal Standard Time", "NPT", new TimeSpan(5, 45, 0)));
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
        /*!loads the standard configurations
         */
        private static void LoadStandardConfigurations()
        {
            XmlDocument doc = new XmlDocument();
            doc.Load(AppSettings.getDataPath() + "\\standardconfigurations.xml");
            XmlElement root = doc.DocumentElement;

            try
            {
                XmlNodeList configurationsList = root.SelectNodes("//configuration");

                foreach (XmlElement element in configurationsList)
                {
                    string type = element.Attributes["type"].Value;

                    if (type == "Route")
                        Configurations.AddConfiguration(LoadRouteClassesConfiguration(element));
                    if (type == "Airliner")
                        Configurations.AddConfiguration(LoadAirlinerConfiguration(element));
                }
            }
            catch (Exception e)
            {

                string s = e.ToString();
            }

        }
        /*! loads and returns an airliner configuration
        */
        private static AirlinerConfiguration LoadAirlinerConfiguration(XmlElement element)
        {
            string name = element.Attributes["name"].Value;
            string id = element.Attributes["id"].Value;
            int minimumSeats = Convert.ToInt16(element.Attributes["minimumseats"].Value);

            XmlNodeList classesList = element.SelectNodes("classes/class");

            AirlinerConfiguration configuration = new AirlinerConfiguration(name, minimumSeats, true);
            configuration.ID = id;

            foreach (XmlElement classElement in classesList)
            {
                int seating = Convert.ToInt16(classElement.Attributes["seating"].Value);
                int regularseating = Convert.ToInt16(classElement.Attributes["regularseating"].Value);
                AirlinerClass.ClassType classType = (AirlinerClass.ClassType)Enum.Parse(typeof(AirlinerClass.ClassType), classElement.Attributes["type"].Value);

                AirlinerClassConfiguration classConf = new AirlinerClassConfiguration(classType, seating, regularseating);
                foreach (AirlinerFacility.FacilityType facType in Enum.GetValues(typeof(AirlinerFacility.FacilityType)))
                {
                    string facUid = classElement.Attributes[facType.ToString()].Value;

                    classConf.addFacility(AirlinerFacilities.GetFacility(facType, facUid));
                }

                configuration.addClassConfiguration(classConf);
            }

            return configuration;
        }
        /*! loads and returns a route classes configuration
         */
        private static RouteClassesConfiguration LoadRouteClassesConfiguration(XmlElement element)
        {
            string name = element.Attributes["name"].Value;
            string id = element.Attributes["id"].Value;

            XmlNodeList classesList = element.SelectNodes("classes/class");

            RouteClassesConfiguration configuration = new RouteClassesConfiguration(name, true);
            configuration.ID = id;

            foreach (XmlElement classElement in classesList)
            {
                AirlinerClass.ClassType type = (AirlinerClass.ClassType)Enum.Parse(typeof(AirlinerClass.ClassType), classElement.Attributes["type"].Value);

                RouteClassConfiguration classConfiguration = new RouteClassConfiguration(type);

                foreach (RouteFacility.FacilityType facilityType in Enum.GetValues(typeof(RouteFacility.FacilityType)))
                {
                    string value = classElement.Attributes[facilityType.ToString()].Value;
                    RouteFacility facility = RouteFacilities.GetFacility(value);

                    classConfiguration.addFacility(facility);
                }
                configuration.addClass(classConfiguration);


            }

            return configuration;
        }
        /*! loads the historic events
         */
        private static void LoadHistoricEvents()
        {
            DirectoryInfo dir = new DirectoryInfo(AppSettings.getDataPath() + "\\addons\\historicevents");

            foreach (FileInfo file in dir.GetFiles("*.xml"))
            {
                LoadHistoricEvent(file.FullName);
            }

        }
        /*! loads a historic event
         */
        private static void LoadHistoricEvent(string filename)
        {
            XmlDocument doc = new XmlDocument();
            doc.Load(filename);
            XmlElement root = doc.DocumentElement;

            string name = root.Attributes["name"].Value;
            string text = root.Attributes["text"].Value;
            DateTime eventDate = Convert.ToDateTime(root.Attributes["date"].Value);

            HistoricEvent historicEvent = new HistoricEvent(name, text, eventDate);

            XmlNodeList influencesList = root.SelectNodes("influences/influence");

            foreach (XmlElement influenceElement in influencesList)
            {
                HistoricEventInfluence.InfluenceType type = (HistoricEventInfluence.InfluenceType)Enum.Parse(typeof(HistoricEventInfluence.InfluenceType), influenceElement.Attributes["type"].Value);
                double value = Convert.ToDouble(influenceElement.Attributes["value"].Value);
                DateTime endDate = Convert.ToDateTime(influenceElement.Attributes["enddate"].Value);

                historicEvent.addInfluence(new HistoricEventInfluence(type, value, endDate));
            }

            HistoricEvents.AddHistoricEvent(historicEvent);


        }
        /*! loads the holidays
         */
        private static void LoadHolidays()
        {
            string id = " ";
            try
            {

                XmlDocument doc = new XmlDocument();
                doc.Load(AppSettings.getDataPath() + "\\holidays.xml");
                XmlElement root = doc.DocumentElement;

                XmlNodeList holidaysList = root.SelectNodes("//holiday");

                foreach (XmlElement element in holidaysList)
                {


                    string uid = element.Attributes["uid"].Value;
                    string name = element.Attributes["name"].Value;

                    id = name;

                    Holiday.HolidayType type = (Holiday.HolidayType)Enum.Parse(typeof(Holiday.HolidayType), element.Attributes["type"].Value);
                    Holiday.TravelType traveltype = (Holiday.TravelType)Enum.Parse(typeof(Holiday.TravelType), element.Attributes["holidaytype"].Value);

                    List<Country> countries = new List<Country>();

                    XmlNodeList countriesList = element.SelectNodes("observers/observer");

                    foreach (XmlElement eCountry in countriesList)
                        countries.Add(Countries.GetCountry(eCountry.Attributes["country"].Value));

                    XmlElement dateElement = (XmlElement)element.SelectSingleNode("observationdate");

                    if (type == Holiday.HolidayType.Fixed_Date)
                    {

                        int month = Convert.ToInt16(dateElement.Attributes["month"].Value);
                        int day = Convert.ToInt16(dateElement.Attributes["day"].Value);

                        DateTime date = new DateTime(1900, month, day);

                        foreach (Country country in countries)
                        {
                            Holiday holiday = new Holiday(root.Name, uid, type, name, traveltype, country);
                            holiday.Date = date;

                            Holidays.AddHoliday(holiday);
                        }

                    }
                    if (type == Holiday.HolidayType.Fixed_Month)
                    {
                        int month = Convert.ToInt16(dateElement.Attributes["month"].Value);

                        foreach (Country country in countries)
                        {
                            Holiday holiday = new Holiday(root.Name, uid, type, name, traveltype, country);
                            holiday.Month = month;

                            Holidays.AddHoliday(holiday);
                        }
                    }
                    if (type == Holiday.HolidayType.Fixed_Week)
                    {
                        int week = Convert.ToInt16(dateElement.Attributes["week"].Value);

                        foreach (Country country in countries)
                        {
                            Holiday holiday = new Holiday(root.Name, uid, type, name, traveltype, country);
                            holiday.Week = week;

                            Holidays.AddHoliday(holiday);
                        }

                    }
                    if (type == Holiday.HolidayType.Non_Fixed_Date)
                    {
                        int month = Convert.ToInt16(dateElement.Attributes["month"].Value);
                        int week = Convert.ToInt16(dateElement.Attributes["week"].Value);
                        DayOfWeek day = (DayOfWeek)Enum.Parse(typeof(DayOfWeek), dateElement.Attributes["dayofweek"].Value);

                        foreach (Country country in countries)
                        {
                            Holiday holiday = new Holiday(root.Name, uid, type, name, traveltype, country);
                            holiday.Month = month;
                            holiday.Week = week;
                            holiday.Day = day;

                            Holidays.AddHoliday(holiday);
                        }
                    }

                    if (element.SelectSingleNode("translations") != null)
                        Translator.GetInstance().addTranslation(root.Name, element.Attributes["uid"].Value, element.SelectSingleNode("translations"));


                }
            }
            catch (Exception e)
            {
                string s = e.ToString();
                s = id;
            }
        }
        /*! loads the inflation years
         */
        private static void LoadInflationYears()
        {
            XmlDocument doc = new XmlDocument();
            doc.Load(AppSettings.getDataPath() + "\\inflations.xml");
            XmlElement root = doc.DocumentElement;

            XmlNodeList inflationsList = root.SelectNodes("//inflation");

            foreach (XmlElement element in inflationsList)
            {
                int year = Convert.ToInt16(element.Attributes["year"].Value);
                double fuelprice = Convert.ToDouble(element.Attributes["fuelprice"].Value);
                double inflation = Convert.ToDouble(element.Attributes["inflation"].Value);
                double modifier = Convert.ToDouble(element.Attributes["pricemodifier"].Value);

                Inflations.AddInflationYear(new Inflation(year, fuelprice, inflation, modifier));

            }


        }
        /*! loads the airline facilities.
         */
        private static void LoadAirlineFacilities()
        {
            XmlDocument doc = new XmlDocument();
            doc.Load(AppSettings.getDataPath() + "\\airlinefacilities.xml");
            XmlElement root = doc.DocumentElement;

            XmlNodeList facilitiesList = root.SelectNodes("//airlinefacility");

            foreach (XmlElement element in facilitiesList)
            {
                string section = root.Name;
                string uid = element.Attributes["uid"].Value;
                double price = XmlConvert.ToDouble(element.Attributes["price"].Value);
                double monthlycost = XmlConvert.ToDouble(element.Attributes["monthlycost"].Value);
                int fromyear = XmlConvert.ToInt16(element.Attributes["fromyear"].Value);

                XmlElement levelElement = (XmlElement)element.SelectSingleNode("level");
                int service = Convert.ToInt32(levelElement.Attributes["service"].Value);
                int luxury = Convert.ToInt32(levelElement.Attributes["luxury"].Value);

                AirlineFacilities.AddFacility(new AirlineFacility(section, uid, price, monthlycost, fromyear, service, luxury));

                if (element.SelectSingleNode("translations") != null)
                    Translator.GetInstance().addTranslation(root.Name, element.Attributes["uid"].Value, element.SelectSingleNode("translations"));
            }
        }

        /*! loads the regions.
         */
        private static void LoadRegions()
        {
            XmlDocument doc = new XmlDocument();
            doc.Load(AppSettings.getDataPath() + "\\regions.xml");
            XmlElement root = doc.DocumentElement;

            XmlNodeList regionsList = root.SelectNodes("//region");
            foreach (XmlElement element in regionsList)
            {
                string section = root.Name;
                string uid = element.Attributes["uid"].Value;

                Regions.AddRegion(new Region(section, uid));

                if (element.SelectSingleNode("translations") != null)
                    Translator.GetInstance().addTranslation(root.Name, element.Attributes["uid"].Value, element.SelectSingleNode("translations"));
            }
        }

        /*! loads the manufacturers.
         */
        private static void LoadManufacturers()
        {
            XmlDocument doc = new XmlDocument();
            doc.Load(AppSettings.getDataPath() + "\\manufacturers.xml");
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

            try
            {
                DirectoryInfo dir = new DirectoryInfo(AppSettings.getDataPath() + "\\addons\\airliners");

                foreach (FileInfo file in dir.GetFiles("*.xml"))
                {

                    LoadAirliners(file.FullName);

                }
            }
            catch (Exception e)
            {
                string s = e.ToString();
            }
        }

        private static void LoadAirliners(string file)
        {
            string dir = AppSettings.getDataPath() + "\\graphics\\airlinerimages\\";
            string id = "";
            try
            {
                XmlDocument doc = new XmlDocument();
                doc.Load(file);
                XmlElement root = doc.DocumentElement;

                XmlNodeList airlinersList = root.SelectNodes("//airliner");

                foreach (XmlElement airliner in airlinersList)
                {
                    AirlinerType.TypeOfAirliner airlinerType = airliner.HasAttribute("type") ? (AirlinerType.TypeOfAirliner)Enum.Parse(typeof(AirlinerType.TypeOfAirliner), airliner.Attributes["type"].Value) : AirlinerType.TypeOfAirliner.Passenger;

                    string manufacturerName = airliner.Attributes["manufacturer"].Value;
                    Manufacturer manufacturer = Manufacturers.GetManufacturer(manufacturerName);

                    string name = airliner.Attributes["name"].Value;
                    long price = Convert.ToInt64(airliner.Attributes["price"].Value);



                    id = name;

                    XmlElement typeElement = (XmlElement)airliner.SelectSingleNode("type");
                    AirlinerType.BodyType body = (AirlinerType.BodyType)Enum.Parse(typeof(AirlinerType.BodyType), typeElement.Attributes["body"].Value);
                    AirlinerType.TypeRange rangeType = (AirlinerType.TypeRange)Enum.Parse(typeof(AirlinerType.TypeRange), typeElement.Attributes["rangetype"].Value);
                    AirlinerType.EngineType engine = (AirlinerType.EngineType)Enum.Parse(typeof(AirlinerType.EngineType), typeElement.Attributes["engine"].Value);

                    XmlElement specsElement = (XmlElement)airliner.SelectSingleNode("specs");
                    double wingspan = XmlConvert.ToDouble(specsElement.Attributes["wingspan"].Value);
                    double length = XmlConvert.ToDouble(specsElement.Attributes["length"].Value);
                    long range = Convert.ToInt64(specsElement.Attributes["range"].Value);
                    double speed = XmlConvert.ToDouble(specsElement.Attributes["speed"].Value);
                    long fuelcapacity = XmlConvert.ToInt64(specsElement.Attributes["fuelcapacity"].Value);
                    double fuel = XmlConvert.ToDouble(specsElement.Attributes["consumption"].Value);
                    long runwaylenght = XmlConvert.ToInt64(specsElement.Attributes["runwaylengthrequired"].Value);



                    XmlElement capacityElement = (XmlElement)airliner.SelectSingleNode("capacity");

                    XmlElement producedElement = (XmlElement)airliner.SelectSingleNode("produced");
                    int fromYear = Convert.ToInt16(producedElement.Attributes["from"].Value);
                    int toYear = Convert.ToInt16(producedElement.Attributes["to"].Value);

                    DateTime from = new DateTime(fromYear, 1, 2);
                    DateTime to = new DateTime(toYear, 12, 31);

                    AirlinerType type = null;

                    if (airlinerType == AirlinerType.TypeOfAirliner.Passenger)
                    {

                        int passengers = Convert.ToInt16(capacityElement.Attributes["passengers"].Value);
                        int cockpitcrew = Convert.ToInt16(capacityElement.Attributes["cockpitcrew"].Value);
                        int cabincrew = Convert.ToInt16(capacityElement.Attributes["cabincrew"].Value);
                        int maxClasses = Convert.ToInt16(capacityElement.Attributes["maxclasses"].Value);
                        type = new AirlinerPassengerType(manufacturer, name, passengers, cockpitcrew, cabincrew, speed, range, wingspan, length, fuel, price, maxClasses, runwaylenght, fuelcapacity, body, rangeType, engine, new Period(from, to));

                    }
                    if (airlinerType == AirlinerType.TypeOfAirliner.Cargo)
                    {
                        int cockpitcrew = Convert.ToInt16(capacityElement.Attributes["cockpitcrew"].Value);
                        double cargo = Convert.ToDouble(capacityElement.Attributes["cargo"].Value);
                        type = new AirlinerCargoType(manufacturer, name, cockpitcrew, cargo, speed, range, wingspan, length, fuel, price, runwaylenght, fuelcapacity, body, rangeType, engine, new Period(from, to));
                    }

                    if (airliner.HasAttribute("image"))
                        type.Image = dir + airliner.Attributes["image"].Value + ".png";

                    if (type != null && airlinerType == AirlinerType.TypeOfAirliner.Passenger)
                        AirlinerTypes.AddType(type);
                }
            }
            catch (Exception e)
            {

                string s = id;
                s = e.ToString();
            }
        }

        /*!loads the airports.
         */
        private static void LoadAirports()
        {
            //LoadAirports(AppSettings.getDataPath() + "\\airports.xml");
            try
            {
                DirectoryInfo dir = new DirectoryInfo(AppSettings.getDataPath() + "\\addons\\airports");

                foreach (FileInfo file in dir.GetFiles("*.xml"))
                {
                    LoadAirports(file.FullName);

                }
            }
            catch (Exception e)
            {
                string s = e.ToString();
            }
            Airports.LargestAirports = Airports.GetAirports(a => a.Profile.Size == GeneralHelpers.Size.Largest).Count;
            Airports.VeryLargeAirports = Airports.GetAirports(a => a.Profile.Size == GeneralHelpers.Size.Very_large).Count;
            Airports.LargeAirports = Airports.GetAirports(a => a.Profile.Size == GeneralHelpers.Size.Large).Count;
            Airports.MediumAirports = Airports.GetAirports(a => a.Profile.Size == GeneralHelpers.Size.Medium).Count;
            Airports.SmallAirports = Airports.GetAirports(a => a.Profile.Size == GeneralHelpers.Size.Small).Count;
            Airports.VerySmallAirports = Airports.GetAirports(a => a.Profile.Size == GeneralHelpers.Size.Very_small).Count;
            Airports.SmallestAirports = Airports.GetAirports(a => a.Profile.Size == GeneralHelpers.Size.Smallest).Count;

        }
        private static void LoadAirports(string file)
        {
            string id = "";
            try
            {
                XmlDocument doc = new XmlDocument();
                doc.Load(file);
                XmlElement root = doc.DocumentElement;

                XmlNodeList airportsList = root.SelectNodes("//airport");

                foreach (XmlElement airportElement in airportsList)
                {
                    string name = airportElement.Attributes["name"].Value;
                    string icao = airportElement.Attributes["icao"].Value;
                    string iata = airportElement.Attributes["iata"].Value;

                    id = name;

                    AirportProfile.AirportType type = (AirportProfile.AirportType)Enum.Parse(typeof(AirportProfile.AirportType), airportElement.Attributes["type"].Value);
                    Weather.Season season = (Weather.Season)Enum.Parse(typeof(Weather.Season), airportElement.Attributes["season"].Value);

                    XmlElement periodElement = (XmlElement)airportElement.SelectSingleNode("period");

                    Period airportPeriod;
                    if (periodElement != null)
                    {
                        DateTime airportFrom = Convert.ToDateTime(periodElement.Attributes["from"].Value);
                        DateTime airportTo = Convert.ToDateTime(periodElement.Attributes["to"].Value);

                        airportPeriod = new Period(airportFrom, airportTo);
                    }
                    else
                        airportPeriod = new Period(new DateTime(1959, 12, 31), new DateTime(2199, 12, 31));

                    XmlElement townElement = (XmlElement)airportElement.SelectSingleNode("town");
                    string town = townElement.Attributes["town"].Value;
                    string country = townElement.Attributes["country"].Value;
                    TimeSpan gmt = TimeSpan.Parse(townElement.Attributes["GMT"].Value);
                    TimeSpan dst = TimeSpan.Parse(townElement.Attributes["DST"].Value);

                    XmlElement latitudeElement = (XmlElement)airportElement.SelectSingleNode("coordinates/latitude");
                    XmlElement longitudeElement = (XmlElement)airportElement.SelectSingleNode("coordinates/longitude");
                    Coordinate latitude = Coordinate.Parse(latitudeElement.Attributes["value"].Value);
                    Coordinate longitude = Coordinate.Parse(longitudeElement.Attributes["value"].Value);

                    XmlElement sizeElement = (XmlElement)airportElement.SelectSingleNode("size");
                    GeneralHelpers.Size size = (GeneralHelpers.Size)Enum.Parse(typeof(GeneralHelpers.Size), sizeElement.Attributes["value"].Value);
                    int pax = sizeElement.HasAttribute("pax") ? Convert.ToInt32(sizeElement.Attributes["pax"].Value) : 0;

                    Town eTown = null;
                    if (town.Contains(","))
                    {
                        State state = States.GetState(Countries.GetCountry(country), town.Split(',')[1].Trim());

                        if (state == null)
                            eTown = new Town(town.Split(',')[0], Countries.GetCountry(country));
                        else
                            eTown = new Town(town.Split(',')[0], Countries.GetCountry(country), state);
                    }
                    else
                        eTown = new Town(town, Countries.GetCountry(country));

                    AirportProfile profile = new AirportProfile(name, iata, icao, type, airportPeriod, eTown, gmt, dst, new Coordinates(latitude, longitude), size, size, pax, season);

                    Airport airport = new Airport(profile);

                    XmlNodeList terminalList = airportElement.SelectNodes("terminals/terminal");

                    foreach (XmlElement terminalNode in terminalList)
                    {
                        string terminalName = terminalNode.Attributes["name"].Value;
                        int terminalGates = XmlConvert.ToInt32(terminalNode.Attributes["gates"].Value);

                        airport.Terminals.addTerminal(new Terminal(airport, null, terminalName, terminalGates, new DateTime(1950, 1, 1)));
                    }

                    XmlNodeList runwaysList = airportElement.SelectNodes("runways/runway");

                    foreach (XmlElement runwayNode in runwaysList)
                    {
                        string runwayName = runwayNode.Attributes["name"].Value;
                        long runwayLength = XmlConvert.ToInt32(runwayNode.Attributes["length"].Value);
                        Runway.SurfaceType surface = (Runway.SurfaceType)Enum.Parse(typeof(Runway.SurfaceType), runwayNode.Attributes["surface"].Value);

                        airport.Runways.Add(new Runway(runwayName, runwayLength, surface, new DateTime(1900, 1, 1), true));

                    }
                   
                    Airports.AddAirport(airport);

                }
            }
            catch (Exception e)
            {
                string i = id;
                string s = e.ToString();
            }
        }

        /*!loads the airport facilities.
         */
        private static void LoadAirportFacilities()
        {
            XmlDocument doc = new XmlDocument();
            doc.Load(AppSettings.getDataPath() + "\\airportfacilities.xml");
            XmlElement root = doc.DocumentElement;

            XmlNodeList facilitiesList = root.SelectNodes("//airportfacility");

            foreach (XmlElement element in facilitiesList)
            {
                string section = root.Name;
                string uid = element.Attributes["uid"].Value;
                string shortname = element.Attributes["shortname"].Value;
                AirportFacility.FacilityType type =
      (AirportFacility.FacilityType)Enum.Parse(typeof(AirportFacility.FacilityType), element.Attributes["type"].Value);
                int typeLevel = Convert.ToInt16(element.Attributes["typelevel"].Value);

                double price = XmlConvert.ToDouble(element.Attributes["price"].Value);
                int buildingDays = XmlConvert.ToInt32(element.Attributes["buildingdays"].Value);

                XmlElement levelElement = (XmlElement)element.SelectSingleNode("level");
                int service = Convert.ToInt32(levelElement.Attributes["service"].Value);
                int luxury = Convert.ToInt32(levelElement.Attributes["luxury"].Value);

                AirportFacility facility = new AirportFacility(section, uid, shortname, type, buildingDays, typeLevel, price, service, luxury);

                AirportFacilities.AddFacility(facility);

                XmlElement employeesElement = (XmlElement)element.SelectSingleNode("employees");

                AirportFacility.EmployeeTypes employeestype = (AirportFacility.EmployeeTypes)Enum.Parse(typeof(AirportFacility.EmployeeTypes), employeesElement.Attributes["type"].Value);
                int numberofemployees = Convert.ToInt16(employeesElement.Attributes["numberofemployees"].Value);

                facility.EmployeeType = employeestype;
                facility.NumberOfEmployees = numberofemployees;


                if (element.SelectSingleNode("translations") != null)
                    Translator.GetInstance().addTranslation(root.Name, element.Attributes["uid"].Value, element.SelectSingleNode("translations"));
            }
        }
        /*!loads the states
         */
        private static void LoadStates()
        {
            XmlDocument doc = new XmlDocument();
            doc.Load(AppSettings.getDataPath() + "\\states.xml");
            XmlElement root = doc.DocumentElement;

            XmlNodeList statesList = root.SelectNodes("//state");
            foreach (XmlElement element in statesList)
            {
                Country country = Countries.GetCountry(element.Attributes["country"].Value);
                string name = element.Attributes["name"].Value;
                string shortname = element.Attributes["shortname"].Value;

                State state = new State(country, name, shortname);
                state.Flag = AppSettings.getDataPath() + "\\graphics\\flags\\states\\" + element.Attributes["flag"].Value + ".png";

                States.AddState(state);

                if (!File.Exists(state.Flag))
                {
                    name = "";
                }

            }
        }
        /*!loads the countries.
         */
        private static void LoadCountries()
        {
            List<XmlElement> territoryElements = new List<XmlElement>();

            XmlDocument doc = new XmlDocument();
            doc.Load(AppSettings.getDataPath() + "\\countries.xml");
            XmlElement root = doc.DocumentElement;

            XmlNodeList countriesList = root.SelectNodes("//country");
            foreach (XmlElement element in countriesList)
            {
                XmlElement territoryElement = (XmlElement)element.SelectSingleNode("territoryof");

                if (territoryElement != null)
                {
                    territoryElements.Add(element);
                }
                else
                {
                    string section = root.Name;
                    string uid = element.Attributes["uid"].Value;
                    string shortname = element.Attributes["shortname"].Value;
                    string flag = element.Attributes["flag"].Value;
                    Region region = Regions.GetRegion(element.Attributes["region"].Value);
                    string tailformat = element.Attributes["tailformat"].Value;

                    Country country = new Country(section, uid, shortname, region, tailformat);

                    country.Flag = AppSettings.getDataPath() + "\\graphics\\flags\\" + flag + ".png";
                    Countries.AddCountry(country);

                    XmlNodeList currenciesList = element.SelectNodes("currency");

                    foreach (XmlElement currencyElement in currenciesList)
                    {
                        string currencySymbol = currencyElement.Attributes["format"].Value; ;
                        double currencyRate =  Convert.ToDouble(currencyElement.Attributes["rate"].Value);

                        DateTime currencyFromDate = new DateTime(1900,1,1);
                        DateTime currencyToDate = new DateTime(2199,12,31);

                        if (currencyElement.HasAttribute("from"))
                            currencyFromDate = Convert.ToDateTime(currencyElement.Attributes["from"].Value);
                        
                        if (currencyElement.HasAttribute("to"))
                            currencyToDate = Convert.ToDateTime(currencyElement.Attributes["to"].Value);

                        country.addCurrency(new CountryCurrency(currencyFromDate, currencyToDate, currencySymbol, currencyRate));
                    }
                    

                    if (element.SelectSingleNode("translations") != null)
                        Translator.GetInstance().addTranslation(root.Name, element.Attributes["uid"].Value, element.SelectSingleNode("translations"));

                
                }
            }
            //reads all countries which is a territory for another
            foreach (XmlElement element in territoryElements)
            {
                string section = root.Name;
                string uid = element.Attributes["uid"].Value;
                string shortname = element.Attributes["shortname"].Value;
                string flag = element.Attributes["flag"].Value;
                Region region = Regions.GetRegion(element.Attributes["region"].Value);
                string tailformat = element.Attributes["tailformat"].Value;

                XmlElement territoryElement = (XmlElement)element.SelectSingleNode("territoryof");

                Country territoryOf = Countries.GetCountry(territoryElement.Attributes["uid"].Value);

                Country country = new TerritoryCountry(section, uid, shortname, region, tailformat, territoryOf);

                country.Flag = AppSettings.getDataPath() + "\\graphics\\flags\\" + flag + ".png";
                Countries.AddCountry(country);

                if (element.SelectSingleNode("translations") != null)
                    Translator.GetInstance().addTranslation(root.Name, element.Attributes["uid"].Value, element.SelectSingleNode("translations"));



            }

        }
        /*! loads the temporary countries
         */
        private static void LoadTemporaryCountries()
        {
            XmlDocument doc = new XmlDocument();
            doc.Load(AppSettings.getDataPath() + "\\temporary countries.xml");
            XmlElement root = doc.DocumentElement;

            XmlNodeList countriesList = root.SelectNodes("//country");
            foreach (XmlElement element in countriesList)
            {

                string section = root.Name;
                string uid = element.Attributes["uid"].Value;
                string shortname = element.Attributes["shortname"].Value;
                string flag = element.Attributes["flag"].Value;
                Region region = Regions.GetRegion(element.Attributes["region"].Value);
                string tailformat = element.Attributes["tailformat"].Value;
                TemporaryCountry.TemporaryType tempType = (TemporaryCountry.TemporaryType)Enum.Parse(typeof(TemporaryCountry.TemporaryType), element.Attributes["type"].Value);

                XmlElement periodElement = (XmlElement)element.SelectSingleNode("period");
                DateTime startDate = Convert.ToDateTime(periodElement.Attributes["start"].Value, new CultureInfo("en-US", false));
                DateTime endDate = Convert.ToDateTime(periodElement.Attributes["end"].Value, new CultureInfo("en-US", false));

                Country country = new Country(section, uid, shortname, region, tailformat);

                if (element.SelectSingleNode("translations") != null)
                    Translator.GetInstance().addTranslation(root.Name, element.Attributes["uid"].Value, element.SelectSingleNode("translations"));


                XmlElement historyElement = (XmlElement)element.SelectSingleNode("history");

                TemporaryCountry tCountry = new TemporaryCountry(tempType, country, startDate, endDate);

                if (tempType == TemporaryCountry.TemporaryType.ManyToOne)
                {
                    Country before = Countries.GetCountry(historyElement.Attributes["before"].Value);
                    Country after = Countries.GetCountry(historyElement.Attributes["after"].Value);

                    tCountry.CountryBefore = before;
                    tCountry.CountryAfter = after;

                }
                if (tempType == TemporaryCountry.TemporaryType.OneToMany)
                {
                    XmlNodeList tempCountriesList = historyElement.SelectNodes("tempcountries/tempcountry");

                    foreach (XmlElement tempCountryElement in tempCountriesList)
                    {
                        Country tempCountry = Countries.GetCountry(tempCountryElement.Attributes["id"].Value);
                        DateTime cStartDate = Convert.ToDateTime(tempCountryElement.Attributes["start"].Value);
                        DateTime cEndDate = Convert.ToDateTime(tempCountryElement.Attributes["end"].Value);

                        tCountry.Countries.Add(new OneToManyCountry(tempCountry, cStartDate, cEndDate));
                    }

                }


                tCountry.Flag = AppSettings.getDataPath() + "\\graphics\\flags\\" + flag + ".png";

                TemporaryCountries.AddCountry(tCountry);


            }
        }
        /*! load the unions
         */
        private static void LoadUnions()
        {
            XmlDocument doc = new XmlDocument();
            doc.Load(AppSettings.getDataPath() + "\\unions.xml");
            XmlElement root = doc.DocumentElement;

            XmlNodeList unionsList = root.SelectNodes("//union");
            foreach (XmlElement element in unionsList)
            {
                try
                {
                    string section = root.Name;
                    string uid = element.Attributes["uid"].Value;
                    string shortname = element.Attributes["shortname"].Value;
                    string flag = element.Attributes["flag"].Value;


                    XmlElement periodElement = (XmlElement)element.SelectSingleNode("period");
                    DateTime creationDate = Convert.ToDateTime(periodElement.Attributes["creation"].Value);
                    DateTime obsoleteDate = Convert.ToDateTime(periodElement.Attributes["obsolete"].Value);

                    Union union = new Union(section, uid, shortname, creationDate, obsoleteDate);

                    XmlNodeList membersList = element.SelectNodes("members/member");

                    foreach (XmlElement memberNode in membersList)
                    {
                        Country country = Countries.GetCountry(memberNode.Attributes["country"].Value);
                        DateTime fromDate = Convert.ToDateTime(memberNode.Attributes["memberfrom"].Value);
                        DateTime toDate = Convert.ToDateTime(memberNode.Attributes["memberto"].Value);

                        union.addMember(new UnionMember(country, fromDate, toDate));
                    }


                    union.Flag = AppSettings.getDataPath() + "\\graphics\\flags\\" + flag + ".png";
                    Unions.AddUnion(union);

                    if (element.SelectSingleNode("translations") != null)
                        Translator.GetInstance().addTranslation(root.Name, element.Attributes["uid"].Value, element.SelectSingleNode("translations"));
                }
                catch (Exception e)
                {
                    throw new Exception("Error on reading unions");
                }
            }

        }
        /*! load the airliner facilities.
         */
        private static void LoadAirlinerFacilities()
        {
            XmlDocument doc = new XmlDocument();
            doc.Load(AppSettings.getDataPath() + "\\airlinerfacilities.xml");
            XmlElement root = doc.DocumentElement;

            XmlNodeList facilitiesList = root.SelectNodes("//airlinerfacility");
            foreach (XmlElement element in facilitiesList)
            {
                string section = root.Name;
                string uid = element.Attributes["uid"].Value;
                AirlinerFacility.FacilityType type = (AirlinerFacility.FacilityType)Enum.Parse(typeof(AirlinerFacility.FacilityType), element.Attributes["type"].Value);
                int fromyear = Convert.ToInt16(element.Attributes["fromyear"].Value);

                XmlElement levelElement = (XmlElement)element.SelectSingleNode("level");
                int service = Convert.ToInt32(levelElement.Attributes["service"].Value);

                XmlElement seatsElement = (XmlElement)element.SelectSingleNode("seats");
                double seatsPercent = XmlConvert.ToDouble(seatsElement.Attributes["percent"].Value);
                double seatsPrice = XmlConvert.ToDouble(seatsElement.Attributes["price"].Value);
                double seatuse = XmlConvert.ToDouble(seatsElement.Attributes["uses"].Value);

                AirlinerFacilities.AddFacility(new AirlinerFacility(section, uid, type, fromyear, service, seatsPercent, seatsPrice, seatuse));

                if (element.SelectSingleNode("translations") != null)
                    Translator.GetInstance().addTranslation(root.Name, element.Attributes["uid"].Value, element.SelectSingleNode("translations"));
            }
        }
        /*! loads all weather averages
         */
        private static void LoadWeatherAverages()
        {
            DirectoryInfo dir = new DirectoryInfo(AppSettings.getDataPath() + "\\addons\\weather");

            foreach (FileInfo file in dir.GetFiles("*.xml"))
            {
                LoadWeatherAverages(file.FullName);
            }
        }
        /*! loads a weather averages
       */
        private static void LoadWeatherAverages(string path)
        {
            XmlDocument doc = new XmlDocument();
            doc.Load(path);
            XmlElement root = doc.DocumentElement;

            Country country = null;
            Airport airport = null;
            Town town = null;

            string type = root.Attributes["type"].Value;
            string value = root.Attributes["value"].Value;

            if (type == "country")
                country = Countries.GetCountry(value);

            if (type == "town")
                town = Towns.GetTown(value);

            if (type == "airport")
                airport = Airports.GetAirport(value);


            XmlNodeList monthsList = root.SelectNodes("months/month");

            foreach (XmlElement monthElement in monthsList)
            {

                int month = Convert.ToInt16(monthElement.Attributes["month"].Value);
                int precipitation = Convert.ToInt16(monthElement.Attributes["precipitation"].Value);

                XmlElement tempElement = (XmlElement)monthElement.SelectSingleNode("temp");
                double minTemp = Convert.ToDouble(tempElement.Attributes["min"].Value);
                double maxTemp = Convert.ToDouble(tempElement.Attributes["max"].Value);

                XmlElement windElement = (XmlElement)monthElement.SelectSingleNode("wind");
                Weather.eWindSpeed minWind = (Weather.eWindSpeed)Enum.Parse(typeof(Weather.eWindSpeed), windElement.Attributes["min"].Value);
                Weather.eWindSpeed maxWind = (Weather.eWindSpeed)Enum.Parse(typeof(Weather.eWindSpeed), windElement.Attributes["max"].Value);

                if (country != null)
                    WeatherAverages.AddWeatherAverage(new WeatherAverage(month, minTemp, maxTemp, precipitation, minWind, maxWind, country));

                if (town != null)
                    WeatherAverages.AddWeatherAverage(new WeatherAverage(month, minTemp, maxTemp, precipitation, minWind, maxWind, town));

                if (airport != null)
                    WeatherAverages.AddWeatherAverage(new WeatherAverage(month, minTemp, maxTemp, precipitation, minWind, maxWind, airport));



            }

        }

        /*loads the airlines
         */
        private static void LoadAirlines()
        {
            string f = "";
            try
            {
                DirectoryInfo dir = new DirectoryInfo(AppSettings.getDataPath() + "\\addons\\airlines");

                foreach (FileInfo file in dir.GetFiles("*.xml"))
                {
                    f = file.Name;
                    LoadAirline(file.FullName);
                }
                GameObject.GetInstance().HumanAirline = Airlines.GetAllAirlines()[0];
                GameObject.GetInstance().MainAirline = GameObject.GetInstance().HumanAirline;

                CreateAirlineLogos();

            }
            catch (Exception e)
            {
                string s = e.ToString();
            }

        }
        /*loads an airline
         */
        private static void LoadAirline(string path)
        {
            XmlDocument doc = new XmlDocument();
            doc.Load(path);
            XmlElement root = doc.DocumentElement;

            XmlElement profileElement = (XmlElement)root.SelectSingleNode("profile");

            string name = profileElement.Attributes["name"].Value;
            string iata = profileElement.Attributes["iata"].Value;
            string color = profileElement.Attributes["color"].Value;
            Country country = Countries.GetCountry(profileElement.Attributes["country"].Value);
            string ceo = profileElement.Attributes["CEO"].Value;
            Airline.AirlineMentality mentality = (Airline.AirlineMentality)Enum.Parse(typeof(Airline.AirlineMentality), profileElement.Attributes["mentality"].Value);
            Airline.AirlineFocus market = (Airline.AirlineFocus)Enum.Parse(typeof(Airline.AirlineFocus), profileElement.Attributes["market"].Value);

            XmlElement narrativeElement = (XmlElement)profileElement.SelectSingleNode("narrative");

            string narrative = "";
            if (narrativeElement != null)
            {
                narrative = narrativeElement.Attributes["narrative"].Value;
            }


            Boolean isReal = true;
            int founded = 1950;
            int folded = 2199;

            XmlElement infoElement = (XmlElement)root.SelectSingleNode("info");
            if (infoElement != null)
            {
                isReal = Convert.ToBoolean(infoElement.Attributes["real"].Value);
                founded = Convert.ToInt16(infoElement.Attributes["from"].Value);
                folded = Convert.ToInt16(infoElement.Attributes["to"].Value);
            }

            Airline airline = new Airline(new AirlineProfile(name, iata, color, country, ceo, isReal, founded, folded), mentality, market);
            if (profileElement.HasAttribute("preferedairport"))
            {
                Airport preferedAirport = Airports.GetAirport(profileElement.Attributes["preferedairport"].Value);
                airline.Profile.PreferedAirport = preferedAirport;
            }



            XmlNodeList subsidiariesList = root.SelectNodes("subsidiaries/subsidiary");
            if (subsidiariesList != null)
            {
                foreach (XmlElement subsidiaryElement in subsidiariesList)
                {
                    string subName = subsidiaryElement.Attributes["name"].Value;
                    string subIATA = subsidiaryElement.Attributes["IATA"].Value;
                    Airport subAirport = Airports.GetAirport(subsidiaryElement.Attributes["homebase"].Value);
                    Airline.AirlineMentality subMentality = (Airline.AirlineMentality)Enum.Parse(typeof(Airline.AirlineMentality), subsidiaryElement.Attributes["mentality"].Value);
                    Airline.AirlineFocus subMarket = (Airline.AirlineFocus)Enum.Parse(typeof(Airline.AirlineFocus), subsidiaryElement.Attributes["market"].Value);
                    string subLogo = AppSettings.getDataPath() + "\\graphics\\airlinelogos\\" + subsidiaryElement.Attributes["logo"].Value + ".png";

                    airline.FutureAirlines.Add(new FutureSubsidiaryAirline(subName, subIATA, subAirport, subMentality, subMarket, subLogo));
                }
            }

            XmlElement startDataElement = (XmlElement)root.SelectSingleNode("startdata");
            if (startDataElement != null)
            {
                AirlineStartData startData = new AirlineStartData(airline);

                XmlNodeList routesList = startDataElement.SelectNodes("routes/route");
                foreach (XmlElement routeElement in routesList)
                {
                    string dest1 = routeElement.Attributes["destination1"].Value;
                    string dest2 = routeElement.Attributes["destination2"].Value;
                    int opened = Convert.ToInt16(routeElement.Attributes["opened"].Value);
                    int closed = Convert.ToInt16(routeElement.Attributes["closed"].Value);

                    startData.addRoute(new StartDataRoute(dest1, dest2, opened, closed));
                }

                XmlNodeList airlinersList = startDataElement.SelectNodes("airliners/airliner");

                foreach (XmlElement airlinerElement in airlinersList)
                {
                    string type = airlinerElement.Attributes["type"].Value;
                    int early = Convert.ToInt16(airlinerElement.Attributes["early"].Value);
                    int late = Convert.ToInt16(airlinerElement.Attributes["late"].Value);

                    startData.addAirliners(new StartDataAirliners(type, early, late));
                }

                XmlNodeList randomRoutesList = startDataElement.SelectNodes("random_routes/routes");

                foreach (XmlElement routeElement in randomRoutesList)
                {
                    string origin = routeElement.Attributes["origin"].Value;
                    int destinations = Convert.ToInt16(routeElement.Attributes["destinations"].Value);
                    GeneralHelpers.Size minimumsize = (GeneralHelpers.Size)Enum.Parse(typeof(GeneralHelpers.Size), routeElement.Attributes["minimumsize"].Value);

                    StartDataRoutes routes = new StartDataRoutes(origin, destinations, minimumsize);

                    XmlNodeList countriesList = routeElement.SelectNodes("countries/country");

                    foreach (XmlElement countryElement in countriesList)
                    {
                        Country routesCountry = Countries.GetCountry(countryElement.Attributes["value"].Value);
                        routes.addCountry(routesCountry);
                    }
                    startData.addOriginRoutes(routes);
                }


                AirlineStartDatas.AddStartData(startData);
            }

            airline.Profile.Narrative = narrative;


            Airlines.AddAirline(airline);

        }
        /*loads the flight restrictions
       */
        private static void LoadFlightRestrictions()
        {

            XmlDocument doc = new XmlDocument();
            doc.Load(AppSettings.getDataPath() + "\\flightrestrictions.xml");
            XmlElement root = doc.DocumentElement;

            XmlNodeList restrictionsList = root.SelectNodes("//restriction");
            foreach (XmlElement element in restrictionsList)
            {
                FlightRestriction.RestrictionType type = (FlightRestriction.RestrictionType)Enum.Parse(typeof(FlightRestriction.RestrictionType), element.Attributes["type"].Value);

                DateTime startDate = Convert.ToDateTime(element.Attributes["start"].Value);
                DateTime endDate = Convert.ToDateTime(element.Attributes["end"].Value);

                XmlElement countriesElement = (XmlElement)element.SelectSingleNode("countries");

                BaseUnit from, to;

                if (countriesElement.Attributes["fromtype"].Value == "C")
                    from = Countries.GetCountry(countriesElement.Attributes["from"].Value);
                else
                    from = Unions.GetUnion(countriesElement.Attributes["from"].Value);

                if (countriesElement.Attributes["totype"].Value == "C")
                    to = Countries.GetCountry(countriesElement.Attributes["to"].Value);
                else
                    to = Unions.GetUnion(countriesElement.Attributes["to"].Value);

                FlightRestrictions.AddRestriction(new FlightRestriction(type, startDate, endDate, from, to));

            }
        }
        /*! sets up the difficulty levels
         */
        private static void SetupDifficultyLevels()
        {

            DifficultyLevels.AddDifficultyLevel(new DifficultyLevel("Easy", 1.5, 0.75, 1.5, 1, 1.25));
            DifficultyLevels.AddDifficultyLevel(new DifficultyLevel("Normal", 1, 1, 1.2, 1.1, 1));
            DifficultyLevels.AddDifficultyLevel(new DifficultyLevel("Hard", 0.5, 1.25, 1, 1.2, 0.75));

        }
        /*! sets up the statistics types.
         */
        private static void SetupStatisticsTypes()
        {
            StatisticsTypes.AddStatisticsType(new StatisticsType("Arrivals", "Arrivals"));
            StatisticsTypes.AddStatisticsType(new StatisticsType("Departures", "Departures"));
            StatisticsTypes.AddStatisticsType(new StatisticsType("Passengers", "Passengers"));
            StatisticsTypes.AddStatisticsType(new StatisticsType("Passengers per flight", "Passengers%"));
            StatisticsTypes.AddStatisticsType(new StatisticsType("Passenger Capacity", "Capacity"));
            StatisticsTypes.AddStatisticsType(new StatisticsType("Airliner Income", "Airliner_Income"));
            StatisticsTypes.AddStatisticsType(new StatisticsType("On-Time flights", "On-Time"));
            StatisticsTypes.AddStatisticsType(new StatisticsType("Flights On-Time", "On-Time%"));
            StatisticsTypes.AddStatisticsType(new StatisticsType("Cancellations", "Cancellations"));
            StatisticsTypes.AddStatisticsType(new StatisticsType("Cancellation Percent", "Cancellation%"));
        }


        /*! sets up test game.
         */
        public static void SetupTestGame(int opponents)
        {
            CreatePilots();

            RemoveAirlines(opponents);

            //sets all the facilities at an airport to none for all airlines
            Parallel.ForEach(Airports.GetAllAirports(), airport =>
            {
                foreach (Airline airline in Airlines.GetAllAirlines())
                {
                    foreach (AirportFacility.FacilityType type in Enum.GetValues(typeof(AirportFacility.FacilityType)))
                    {
                        AirportFacility noneFacility = AirportFacilities.GetFacilities(type).Find((delegate(AirportFacility facility) { return facility.TypeLevel == 0; }));

                        airport.addAirportFacility(airline, noneFacility, GameObject.GetInstance().GameTime);
                    }
                }
                AirportHelpers.CreateAirportWeather(airport);
            });

            foreach (Airline airline in Airlines.GetAllAirlines())
            {
                airline.Money = GameObject.GetInstance().StartMoney;
                airline.StartMoney = airline.Money;
                airline.Fees = new AirlineFees();
                airline.addAirlinePolicy(new AirlinePolicy("Cancellation Minutes", 150));

                if (!airline.IsHuman)
                    CreateComputerRoutes(airline);
            }
        }

        /*! removes some random airlines from the list bases on number of opponents.
         */
        private static void RemoveAirlines(int opponnents)
        {
            int year = GameObject.GetInstance().GameTime.Year;

            var notAvailableAirlines = Airlines.GetAirlines(a => !(a.Profile.Founded <= year && a.Profile.Folded > year));

            foreach (Airline airline in notAvailableAirlines)
                Airlines.RemoveAirline(airline);
    
            Airline lot = Airlines.GetAirline("DA");
            int count = Airlines.GetAirlines(a => !a.IsHuman && a.Profile.Founded <= year && a.Profile.Folded > year).Count;
   
            for (int i = 0; i < count - opponnents; i++)
            {
                List<Airline> airlines = Airlines.GetAirlines(a => !a.IsHuman && a.Profile.Founded <= year && a.Profile.Folded > year); 

                Airlines.RemoveAirline(airlines[rnd.Next(airlines.Count)]);
            }

           
            if (Airlines.GetAllAirlines().Contains(lot))
                Airlines.RemoveAirline(lot);
            //if (!Airlines.GetAllAirlines().Contains(lot))
            //      Airlines.AddAirline(lot);

        }
        //finds the home base for a computer airline
        private static Airport FindComputerHomeBase(Airline airline)
        {
            if (airline.Profile.PreferedAirport != null && GeneralHelpers.IsAirportActive(airline.Profile.PreferedAirport) && airline.Profile.PreferedAirport.Terminals.getFreeGates() > 1)
            {
                return airline.Profile.PreferedAirport;
            }
            else
            {
                List<Airport> airports = Airports.GetAirports(airline.Profile.Country).FindAll(a => a.Terminals.getFreeGates() > 1);

                if (airports.Count < 4)
                    airports = Airports.GetAirports(airline.Profile.Country.Region).FindAll(a => a.Terminals.getFreeGates() > 1);

                Dictionary<Airport, int> list = new Dictionary<Airport, int>();
                airports.ForEach(a => list.Add(a, ((int)a.Profile.Size) * AirportHelpers.GetAirportsNearAirport(a).Count));

                return AIHelpers.GetRandomItem(list);
            }


        }
        /*! creates some airliners and routes for a computer airline.
         */
        private static void CreateComputerRoutes(Airline airline)
        {
            Airport airportHomeBase = FindComputerHomeBase(airline);

            AirportFacility serviceFacility = AirportFacilities.GetFacilities(AirportFacility.FacilityType.Service).Find(f => f.TypeLevel == 1);
            AirportFacility checkinFacility = AirportFacilities.GetFacilities(AirportFacility.FacilityType.CheckIn).Find(f => f.TypeLevel == 1);

            airportHomeBase.addAirportFacility(airline, serviceFacility, GameObject.GetInstance().GameTime);
            airportHomeBase.addAirportFacility(airline, checkinFacility, GameObject.GetInstance().GameTime);

            AirlineStartData startData = AirlineStartDatas.GetAirlineStartData(airline);

            //creates the start data for an airline
            if (startData != null)
            {
                airportHomeBase.Terminals.rentGate(airline);
                airportHomeBase.Terminals.rentGate(airline);

                CreateAirlineStartData(airline, startData);
            }
            else
            {
                List<Airport> airportDestinations = AIHelpers.GetDestinationAirports(airline, airportHomeBase);

                KeyValuePair<Airliner, Boolean>? airliner = null;
                Airport airportDestination = null;

                int counter = 0;

                while (airportDestination == null || airliner == null || !airliner.HasValue)
                {
                    airportDestination = airportDestinations[counter];

                    airliner = AIHelpers.GetAirlinerForRoute(airline, airportHomeBase, airportDestination);

                    counter++;
                }

                if (airportDestination == null || !airliner.HasValue)
                {

                    CreateComputerRoutes(airline);

                }
                else
                {
                    airportHomeBase.Terminals.rentGate(airline);
                    airportHomeBase.Terminals.rentGate(airline);


                    airportDestination.Terminals.rentGate(airline);
                    airportDestination.addAirportFacility(airline, checkinFacility, GameObject.GetInstance().GameTime);


                    double price = PassengerHelpers.GetPassengerPrice(airportDestination, airline.Airports[0]);

                    Guid id = Guid.NewGuid();

                    Route route = new Route(id.ToString(), airportDestination, airline.Airports[0], price);

                    FleetAirliner fAirliner = AirlineHelpers.BuyAirliner(airline, airliner.Value.Key, airportHomeBase);
                    fAirliner.addRoute(route);
                    fAirliner.Status = FleetAirliner.AirlinerStatus.To_route_start;

                    AIHelpers.CreateAirlinerClasses(fAirliner);

                    route.LastUpdated = GameObject.GetInstance().GameTime;

                    RouteClassesConfiguration configuration = AIHelpers.GetRouteConfiguration(route);

                    foreach (RouteClassConfiguration classConfiguration in configuration.getClasses())
                    {
                        route.getRouteAirlinerClass(classConfiguration.Type).FarePrice = price * GeneralHelpers.ClassToPriceFactor(classConfiguration.Type);

                        foreach (RouteFacility rFacility in classConfiguration.getFacilities())
                            route.getRouteAirlinerClass(classConfiguration.Type).addFacility(rFacility);
                    }

                    airline.addRoute(route);

                    airportDestination.Terminals.getEmptyGate(airline).HasRoute = true;
                    airline.Airports[0].Terminals.getEmptyGate(airline).HasRoute = true;

                    AIHelpers.CreateRouteTimeTable(route, fAirliner);


                }
            }
        }
        /*!creates the start data for an airline
         */
        private static void CreateAirlineStartData(Airline airline, AirlineStartData startData)
        {
            AirportFacility checkinFacility = AirportFacilities.GetFacilities(AirportFacility.FacilityType.CheckIn).Find(f => f.TypeLevel == 1);
            
            int difficultyFactor = GameObject.GetInstance().Difficulty.AILevel > 1 ? 2 : 1; //level easy
     
            var startroutes = startData.Routes.FindAll(r => r.Opened <= GameObject.GetInstance().GameTime.Year && r.Closed >= GameObject.GetInstance().GameTime.Year);
             
            //creates the routes
            foreach (StartDataRoute startRoute in startroutes.GetRange(0,startroutes.Count / difficultyFactor))
            {
                Airport dest1 = Airports.GetAirport(startRoute.Destination1);
                Airport dest2 = Airports.GetAirport(startRoute.Destination2);

                if (dest1.getAirportFacility(airline, AirportFacility.FacilityType.CheckIn).TypeLevel == 0)
                    dest1.addAirportFacility(airline, checkinFacility, GameObject.GetInstance().GameTime);

                if (dest2.getAirportFacility(airline, AirportFacility.FacilityType.CheckIn).TypeLevel == 0)
                    dest2.addAirportFacility(airline, checkinFacility, GameObject.GetInstance().GameTime);

                if (dest1.Terminals.getEmptyGate(airline) == null)
                    dest1.Terminals.rentGate(airline);

                if (dest2.Terminals.getEmptyGate(airline) == null)
                    dest2.Terminals.rentGate(airline);

                dest1.Terminals.getEmptyGate(airline).HasRoute = true;
                dest2.Terminals.getEmptyGate(airline).HasRoute = true;

                double price = PassengerHelpers.GetPassengerPrice(dest1, dest2);

                Guid id = Guid.NewGuid();

                Route route = new Route(id.ToString(), dest1, dest2, price);

                KeyValuePair<Airliner, Boolean>? airliner = AIHelpers.GetAirlinerForRoute(airline, dest2, dest1);

                FleetAirliner fAirliner = AirlineHelpers.AddAirliner(airline, airliner.Value.Key, airline.Airports[0]);
                fAirliner.addRoute(route);
                fAirliner.Status = FleetAirliner.AirlinerStatus.To_route_start;

                AIHelpers.CreateAirlinerClasses(fAirliner);

                route.LastUpdated = GameObject.GetInstance().GameTime;

                RouteClassesConfiguration configuration = AIHelpers.GetRouteConfiguration(route);

                foreach (RouteClassConfiguration classConfiguration in configuration.getClasses())
                {
                    route.getRouteAirlinerClass(classConfiguration.Type).FarePrice = price * GeneralHelpers.ClassToPriceFactor(classConfiguration.Type);

                    foreach (RouteFacility rFacility in classConfiguration.getFacilities())
                        route.getRouteAirlinerClass(classConfiguration.Type).addFacility(rFacility);
                }

                airline.addRoute(route);

                AIHelpers.CreateRouteTimeTable(route, fAirliner);


            }
         
            //adds the airliners
            foreach (StartDataAirliners airliners in startData.Airliners.GetRange(0,startData.Airliners.Count / difficultyFactor))
            {
                AirlinerType type = AirlinerTypes.GetType(airliners.Type);

                int totalSpan = 2010 - 1960;
                int yearSpan = GameObject.GetInstance().GameTime.Year - 1960;
                double valueSpan = Convert.ToDouble(airliners.AirlinersLate - airliners.AirlinersEarly);
                double span = valueSpan / Convert.ToDouble(totalSpan);

                int numbers = Convert.ToInt16(span * yearSpan);

                if (type.Produced.From <= GameObject.GetInstance().GameTime)
                {
                    for (int i = 0; i < Math.Max(numbers, airliners.AirlinersEarly); i++)
                    {
                        int countryNumber = rnd.Next(Countries.GetCountries().Count() - 1);
                        Country country = Countries.GetCountries()[countryNumber];

                        int builtYear = rnd.Next(type.Produced.From.Year, Math.Min(GameObject.GetInstance().GameTime.Year - 1, type.Produced.To.Year));

                        Airliner airliner = new Airliner(type, country.TailNumbers.getNextTailNumber(), new DateTime(builtYear, 1, 1));

                        int age = MathHelpers.CalculateAge(airliner.BuiltDate, GameObject.GetInstance().GameTime);

                        long kmPerYear = rnd.Next(100000, 1000000);
                        long km = kmPerYear * age;

                        airliner.Flown = km;

                        Airliners.AddAirliner(airliner);

                        FleetAirliner fAirliner = AirlineHelpers.AddAirliner(airline, airliner, airline.Airports[0]);
                        fAirliner.Status = FleetAirliner.AirlinerStatus.Stopped;

                        AIHelpers.CreateAirlinerClasses(fAirliner);
                    }
                }

            }
            //the origin routes
            foreach (StartDataRoutes routes in startData.OriginRoutes.GetRange(0,startData.OriginRoutes.Count / difficultyFactor))
            {
                Airport origin = Airports.GetAirport(routes.Origin);

                for (int i = 0; i < Math.Min(routes.Destinations, origin.Terminals.getFreeGates()); i++)
                {
                    if (origin.getAirportFacility(airline, AirportFacility.FacilityType.CheckIn).TypeLevel == 0)
                        origin.addAirportFacility(airline, checkinFacility, GameObject.GetInstance().GameTime);

                    if (origin.Terminals.getEmptyGate(airline) == null)
                        origin.Terminals.rentGate(airline);

                    Airport destination = GetStartDataRoutesDestination(routes);

                    if (destination.getAirportFacility(airline, AirportFacility.FacilityType.CheckIn).TypeLevel == 0)
                        destination.addAirportFacility(airline, checkinFacility, GameObject.GetInstance().GameTime);

                    if (destination.Terminals.getEmptyGate(airline) == null)
                        destination.Terminals.rentGate(airline);

                    origin.Terminals.getEmptyGate(airline).HasRoute = true;
                    destination.Terminals.getEmptyGate(airline).HasRoute = true;

                    double price = PassengerHelpers.GetPassengerPrice(origin, destination);

                    Guid id = Guid.NewGuid();

                    Route route = new Route(id.ToString(), origin, destination, price);

                    KeyValuePair<Airliner, Boolean>? airliner = AIHelpers.GetAirlinerForRoute(airline, origin, destination);

                    FleetAirliner fAirliner = AirlineHelpers.AddAirliner(airline, airliner.Value.Key, airline.Airports[0]);
                    fAirliner.addRoute(route);
                    fAirliner.Status = FleetAirliner.AirlinerStatus.To_route_start;

                    AIHelpers.CreateAirlinerClasses(fAirliner);

                    route.LastUpdated = GameObject.GetInstance().GameTime;

                    RouteClassesConfiguration configuration = AIHelpers.GetRouteConfiguration(route);

                    foreach (RouteClassConfiguration classConfiguration in configuration.getClasses())
                    {
                        route.getRouteAirlinerClass(classConfiguration.Type).FarePrice = price * GeneralHelpers.ClassToPriceFactor(classConfiguration.Type);

                        foreach (RouteFacility rFacility in classConfiguration.getFacilities())
                            route.getRouteAirlinerClass(classConfiguration.Type).addFacility(rFacility);
                    }

                    airline.addRoute(route);

                    AIHelpers.CreateRouteTimeTable(route, fAirliner);

                }
            }
        }
        //returns a random destination for an origin start routes
        private static Airport GetStartDataRoutesDestination(StartDataRoutes routes)
        {
            List<Airport> airports = Airports.GetAirports(a => routes.Countries.Contains(a.Profile.Country) && a != Airports.GetAirport(routes.Origin) && ((int)a.Profile.Size) >= ((int)routes.MinimumSize) && a.Terminals.getFreeGates() > 0);

            return airports[rnd.Next(airports.Count)];
        }
        /*! loads the maps for the airports
        */
        private static void LoadAirportMaps()
        {
            DirectoryInfo dir = new DirectoryInfo(AppSettings.getDataPath() + "\\graphics\\airportmaps");

            foreach (FileInfo file in dir.GetFiles("*.png"))
            {
                string code = file.Name.Split('.')[0].ToUpper();
                Airport airport = Airports.GetAirport(code);

                if (airport != null)
                    airport.Profile.Map = file.FullName;
                else
                    code = "x";
            }
        }
        /*! loads the logos for the airports.
         */
        private static void LoadAirportLogos()
        {
            DirectoryInfo dir = new DirectoryInfo(AppSettings.getDataPath() + "\\graphics\\airportlogos");

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
        /*! loads the manufacturer logos
         */
        private static void LoadManufacturerLogos()
        {
            DirectoryInfo dir = new DirectoryInfo(AppSettings.getDataPath() + "\\graphics\\manufacturerlogos");

            foreach (FileInfo file in dir.GetFiles("*.png"))
            {
                string name = file.Name.Split('.')[0];
                Manufacturer manufacturer = Manufacturers.GetManufacturer(name);

                if (manufacturer != null)
                    manufacturer.Logo = file.FullName;
                else
                    name = "x";
            }
        }

        /*! creates the logos for the game airlines.
         */
        private static void CreateAirlineLogos()
        {
            foreach (Airline airline in Airlines.GetAllAirlines())
            {
                if (File.Exists(AppSettings.getDataPath() + "\\graphics\\airlinelogos\\" + airline.Profile.IATACode + ".png"))
                    airline.Profile.Logo = AppSettings.getDataPath() + "\\graphics\\airlinelogos\\" + airline.Profile.IATACode + ".png";
                else
                    airline.Profile.Logo = AppSettings.getDataPath() + "\\graphics\\airlinelogos\\default.png";
            }
        }

        /*! private stativ method CreateFlightFacilities.
         * creates the in flight facilities.
         */
        private static void CreateFlightFacilities()
        {
            RouteFacilities.AddFacility(new RouteFacility("100", RouteFacility.FacilityType.Food, "None", -50, RouteFacility.ExpenseType.Fixed, 0, null));
            RouteFacilities.AddFacility(new RouteFacility("101", RouteFacility.FacilityType.Food, "Buyable Snacks", 10, RouteFacility.ExpenseType.Random, 0.10, FeeTypes.GetType("Snacks")));
            RouteFacilities.AddFacility(new RouteFacility("102", RouteFacility.FacilityType.Food, "Free Snacks", 15, RouteFacility.ExpenseType.Fixed, 0.10, null));
            RouteFacilities.AddFacility(new RouteFacility("103", RouteFacility.FacilityType.Food, "Buyable Meal", 25, RouteFacility.ExpenseType.Random, 0.50, FeeTypes.GetType("Meal")));
            RouteFacilities.AddFacility(new RouteFacility("104", RouteFacility.FacilityType.Food, "Basic Meal", 50, RouteFacility.ExpenseType.Fixed, 0.50, null));
            RouteFacilities.AddFacility(new RouteFacility("105", RouteFacility.FacilityType.Food, "Full Dinner", 100, RouteFacility.ExpenseType.Fixed, 2, null));
            RouteFacilities.AddFacility(new RouteFacility("106", RouteFacility.FacilityType.Drinks, "None", -50, RouteFacility.ExpenseType.Fixed, 0, null));
            RouteFacilities.AddFacility(new RouteFacility("107", RouteFacility.FacilityType.Drinks, "Buyable", 25, RouteFacility.ExpenseType.Random, 0.10, FeeTypes.GetType("Drinks")));
            RouteFacilities.AddFacility(new RouteFacility("108", RouteFacility.FacilityType.Drinks, "Free", 80, RouteFacility.ExpenseType.Fixed, 0.20, null));
            RouteFacilities.AddFacility(new RouteFacility("109", RouteFacility.FacilityType.Alcoholic_Drinks, "None", 0, RouteFacility.ExpenseType.Fixed, 0, null));
            RouteFacilities.AddFacility(new RouteFacility("110", RouteFacility.FacilityType.Alcoholic_Drinks, "Buyable", 40, RouteFacility.ExpenseType.Random, 0.05, FeeTypes.GetType("Alcholic Drinks")));
            RouteFacilities.AddFacility(new RouteFacility("111", RouteFacility.FacilityType.Alcoholic_Drinks, "Free", 100, RouteFacility.ExpenseType.Fixed, 0.75, null));
            RouteFacilities.AddFacility(new RouteFacility("112", RouteFacility.FacilityType.WiFi, "None", 0, RouteFacility.ExpenseType.Fixed, 0, null));
            RouteFacilities.AddFacility(new RouteFacility("113", RouteFacility.FacilityType.WiFi, "Buyable", 40, RouteFacility.ExpenseType.Random, 0.5, FeeTypes.GetType("WiFi"), AirlineFacilities.GetFacility("107")));
            RouteFacilities.AddFacility(new RouteFacility("114", RouteFacility.FacilityType.WiFi, "Free", 100, RouteFacility.ExpenseType.Fixed, 0.5, null, AirlineFacilities.GetFacility("107")));
            RouteFacilities.AddFacility(new RouteFacility("115", RouteFacility.FacilityType.Magazines, "None", 0, RouteFacility.ExpenseType.Fixed, 0, null));
            RouteFacilities.AddFacility(new RouteFacility("116", RouteFacility.FacilityType.Magazines, "Available", 40, RouteFacility.ExpenseType.Fixed, 0, null, AirlineFacilities.GetFacility("101")));
            RouteFacilities.AddFacility(new RouteFacility("117", RouteFacility.FacilityType.Newspapers, "None", 0, RouteFacility.ExpenseType.Fixed, 0, null));
            RouteFacilities.AddFacility(new RouteFacility("118", RouteFacility.FacilityType.Newspapers, "Available", 35, RouteFacility.ExpenseType.Fixed, 0, null, AirlineFacilities.GetFacility("102")));
        }

        /*! creates the Fee types.
         */
        private static void CreateFeeTypes()
        {
            //wages
            FeeTypes.AddType(new FeeType(FeeType.eFeeType.Wage, "Cockpit Wage", 4.11, 3.75, 12.75, 100));
            FeeTypes.AddType(new FeeType(FeeType.eFeeType.Wage, "Maintenance Wage", 3.95, 3.0, 4.25, 100));
            FeeTypes.AddType(new FeeType(FeeType.eFeeType.Wage, "Support Wage", 2.65, 1, 3, 100));
            FeeTypes.AddType(new FeeType(FeeType.eFeeType.Wage, "Cabin Wage", 1.9, 1, 4, 100));

            //food and drinks
            FeeTypes.AddType(new FeeType(FeeType.eFeeType.FoodDrinks, "Alcholic Drinks", 0.75, 0.5, 1.1, 75));
            FeeTypes.AddType(new FeeType(FeeType.eFeeType.FoodDrinks, "Drinks", 0.2, 0.1, 0.8, 75));
            FeeTypes.AddType(new FeeType(FeeType.eFeeType.FoodDrinks, "Snacks", 0.35, 0.25, 0.5, 70));
            FeeTypes.AddType(new FeeType(FeeType.eFeeType.FoodDrinks, "Meal", 1.40, 1.25, 2, 50));

            //fees
            FeeTypes.AddType(new FeeType(FeeType.eFeeType.Fee, "1 Bag", 0, 0, 5, 95));
            FeeTypes.AddType(new FeeType(FeeType.eFeeType.Fee, "2 Bags", 0, 0, 5.25, 25));
            FeeTypes.AddType(new FeeType(FeeType.eFeeType.Fee, "3+ Bags", 0, 0, 6, 2));
            FeeTypes.AddType(new FeeType(FeeType.eFeeType.Fee, "Pets", 0, 0, 18, 1));
            FeeTypes.AddType(new FeeType(FeeType.eFeeType.Fee, "WiFi", 1.4, 1.4, 6.25, 25, 2007));

            //discounts
            FeeTypes.AddType(new FeeType(FeeType.eFeeType.Discount, "Employee Discount", 0, 0, 100, 1));
            FeeTypes.AddType(new FeeType(FeeType.eFeeType.Discount, "Government Discount", 0, 0, 100, 3));
            FeeTypes.AddType(new FeeType(FeeType.eFeeType.Discount, "Military Discount", 0, 0, 100, 1));
        }
    }
}
