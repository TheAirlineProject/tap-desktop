using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Xml;
using NLog;
using TheAirline.Helpers;
using TheAirline.Infrastructure.Enums;
using TheAirline.Models.Airliners;
using TheAirline.Models.Airlines;
using TheAirline.Models.Airlines.AirlineCooperation;
using TheAirline.Models.Airlines.Subsidiary;
using TheAirline.Models.Airports;
using TheAirline.Models.General;
using TheAirline.Models.General.Countries;
using TheAirline.Models.General.Countries.Towns;
using TheAirline.Models.General.Environment;
using TheAirline.Models.General.Finances;
using TheAirline.Models.General.HistoricEvents;
using TheAirline.Models.General.Holidays;
using TheAirline.Models.General.Scenarios;
using TheAirline.Models.General.Statistics;
using TheAirline.Models.Passengers;
using TheAirline.Models.Pilots;
using TheAirline.Models.Routes;

namespace TheAirline.Infrastructure
{
    /*! Setup class.
     * This class is used for configuring the games environment.
     * The class needs no instantiation, because all methods are declared static.
     */

    public class Setup
    {
        #region Static Fields

        private static readonly Random Rnd = new Random();
        private static readonly Logger Logger = LogManager.GetCurrentClassLogger();

        #endregion

        #region Public Methods and Operators

        public static Airline LoadAirline(string path)
        {
            var doc = new XmlDocument();
            doc.Load(path);
            XmlElement root = doc.DocumentElement;

            var profileElement = (XmlElement) root?.SelectSingleNode("profile");

            if (profileElement != null)
            {
                string name = profileElement.Attributes["name"].Value;
                string iata = profileElement.Attributes["iata"].Value;
                string color = profileElement.Attributes["color"].Value;

                string logoImage = profileElement.HasAttribute("logo") ? profileElement.Attributes["logo"].Value : "";

                string sCountries = profileElement.Attributes["country"].Value;

                var countries = sCountries.Split(';').Select(Countries.GetCountry).ToList();

                //Country country = Countries.GetCountry(profileElement.Attributes["country"].Value);
                string ceo = profileElement.Attributes["CEO"].Value;
                var mentality =
                    (Airline.AirlineMentality)
                        Enum.Parse(typeof (Airline.AirlineMentality), profileElement.Attributes["mentality"].Value);
                var market =
                    (AirlineFocus)
                        Enum.Parse(typeof (AirlineFocus), profileElement.Attributes["market"].Value);

                var routeFocus = Route.RouteType.Passenger;

                if (profileElement.HasAttribute("routefocus"))
                {
                    routeFocus =
                        (Route.RouteType) Enum.Parse(typeof (Route.RouteType), profileElement.Attributes["routefocus"].Value);
                }

                var narrativeElement = (XmlElement) profileElement.SelectSingleNode("narrative");

                string narrative = "";
                if (narrativeElement != null)
                {
                    narrative = narrativeElement.Attributes["narrative"].Value;
                }

                bool isReal = true;
                int founded = 1950;
                int folded = 2199;

                var infoElement = (XmlElement) root.SelectSingleNode("info");
                if (infoElement != null)
                {
                    isReal = Convert.ToBoolean(infoElement.Attributes["real"].Value);
                    founded = Convert.ToInt16(infoElement.Attributes["from"].Value);
                    folded = Convert.ToInt16(infoElement.Attributes["to"].Value);
                }

                var license = Airline.AirlineLicense.Domestic;

                if (market == AirlineFocus.Global)
                {
                    license = mentality == Airline.AirlineMentality.Aggressive ? Airline.AirlineLicense.LongHaul : Airline.AirlineLicense.ShortHaul;
                }

                if (market == AirlineFocus.Regional)
                {
                    license = Airline.AirlineLicense.Regional;
                }

                var airline = new Airline(
                    new AirlineProfile(name, iata, color, ceo, isReal, founded, folded),
                    mentality,
                    market,
                    license,
                    routeFocus) {Profile = {Countries = countries}};
                airline.Profile.Country = airline.Profile.Countries[0];
                airline.Profile.LogoName = logoImage;

                var preferedsElement = (XmlElement) root.SelectSingleNode("prefereds");

                if (preferedsElement != null)
                {
                    if (preferedsElement.HasAttribute("aircrafts"))
                    {
                        string[] preferedAircrafts = preferedsElement.Attributes["aircrafts"].Value.Split(',');

                        foreach (string preferedAircraft in preferedAircrafts)
                        {
                            AirlinerType pAircraft = AirlinerTypes.GetType(preferedAircraft);
                            airline.Profile.AddPreferedAircraft(pAircraft);
                        }
                    }
                    if (preferedsElement.HasAttribute("primarypurchasing"))
                    {
                        var primarypurchasing =
                            (AirlineProfile.PreferedPurchasing)
                                Enum.Parse(
                                    typeof (AirlineProfile.PreferedPurchasing),
                                    preferedsElement.Attributes["primarypurchasing"].Value);
                        airline.Profile.PrimaryPurchasing = primarypurchasing;
                    }
                }

                XmlNodeList logosList = profileElement.SelectNodes("logos/logo");

                if (logosList != null)
                    foreach (XmlElement logoElement in logosList)
                    {
                        int logoFromYear = Convert.ToInt16(logoElement.Attributes["from"].Value);
                        int logoToYear = Convert.ToInt16(logoElement.Attributes["to"].Value);
                        string logoPath = AppSettings.GetDataPath() + "\\graphics\\airlinelogos\\multilogos\\"
                                          + logoElement.Attributes["path"].Value + ".png";

                        airline.Profile.AddLogo(new AirlineLogo(logoFromYear, logoToYear, logoPath));
                    }

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
                        var subMentality =
                            (Airline.AirlineMentality)
                                Enum.Parse(
                                    typeof (Airline.AirlineMentality),
                                    subsidiaryElement.Attributes["mentality"].Value);
                        var subMarket =
                            (AirlineFocus)
                                Enum.Parse(typeof (AirlineFocus), subsidiaryElement.Attributes["market"].Value);

                        string subLogo = AppSettings.GetDataPath() + "\\graphics\\airlinelogos\\"
                                         + subsidiaryElement.Attributes["logo"].Value + ".png";

                        var airlineRouteFocus = Route.RouteType.Passenger;

                        if (subsidiaryElement.HasAttribute("routefocus"))
                        {
                            airlineRouteFocus =
                                (Route.RouteType)
                                    Enum.Parse(typeof (Route.RouteType), subsidiaryElement.Attributes["routefocus"].Value);
                        }

                        airline.FutureAirlines.Add(
                            new FutureSubsidiaryAirline(
                                subName,
                                subIATA,
                                subAirport,
                                subMentality,
                                subMarket,
                                airlineRouteFocus,
                                subLogo));
                    }
                }

                var startDataElement = (XmlElement) root.SelectSingleNode("startdata");
                if (startDataElement != null)
                {
                    var startData = new AirlineStartData(airline);

                    XmlNodeList routesList = startDataElement.SelectNodes("routes/route");
                    if (routesList != null)
                        foreach (XmlElement routeElement in routesList)
                        {
                            string dest1 = routeElement.Attributes["destination1"].Value;
                            string dest2 = routeElement.Attributes["destination2"].Value;
                            int opened = Convert.ToInt16(routeElement.Attributes["opened"].Value);
                            int closed = Convert.ToInt16(routeElement.Attributes["closed"].Value);

                            Route.RouteType routetype = airline.AirlineRouteFocus;

                            if (routeElement.HasAttribute("routetype"))
                            {
                                routetype =
                                    (Route.RouteType)
                                        Enum.Parse(typeof (Route.RouteType), routeElement.Attributes["routetype"].Value);
                            }

                            var sdr = new StartDataRoute(dest1, dest2, opened, closed, routetype);
                            startData.AddRoute(sdr);

                            if (routeElement.HasAttribute("airliner"))
                            {
                                AirlinerType airlinerType = AirlinerTypes.GetType(routeElement.Attributes["airliner"].Value);
                                sdr.Type = airlinerType;
                            }
                        }

                    XmlNodeList airlinersList = startDataElement.SelectNodes("airliners/airliner");

                    foreach (XmlElement airlinerElement in airlinersList)
                    {
                        string type = airlinerElement.Attributes["type"].Value;
                        int early = Convert.ToInt16(airlinerElement.Attributes["early"].Value);
                        int late = Convert.ToInt16(airlinerElement.Attributes["late"].Value);

                        startData.AddAirliners(new StartDataAirliners(type, early, late));
                    }

                    XmlNodeList randomRoutesList = startDataElement.SelectNodes("random_routes/routes");

                    foreach (XmlElement routeElement in randomRoutesList)
                    {
                        string origin = routeElement.Attributes["origin"].Value;
                        int destinations = Convert.ToInt16(routeElement.Attributes["destinations"].Value);
                        var minimumsize =
                            (GeneralHelpers.Size)
                                Enum.Parse(typeof (GeneralHelpers.Size), routeElement.Attributes["minimumsize"].Value);

                        Route.RouteType routetype = airline.AirlineRouteFocus;

                        if (routeElement.HasAttribute("routetype"))
                        {
                            routetype =
                                (Route.RouteType)
                                    Enum.Parse(typeof (Route.RouteType), routeElement.Attributes["routetype"].Value);
                        }

                        var routes = new StartDataRoutes(origin, destinations, minimumsize, routetype);

                        XmlNodeList countriesList = routeElement.SelectNodes("countries/country");

                        foreach (XmlElement countryElement in countriesList)
                        {
                            Country routesCountry = Countries.GetCountry(countryElement.Attributes["value"].Value);
                            routes.AddCountry(routesCountry);
                        }
                        startData.AddOriginRoutes(routes);
                    }

                    AirlineStartDatas.AddStartData(startData);
                }

                airline.Profile.Narrative = narrative;

                Airlines.AddAirline(airline);

                return airline;
            }
            return null;
        }

        public static void LoadAirports()
        {
            //LoadAirports(AppSettings.getDataPath() + "\\airports.xml");
            try
            {
                var dir = new DirectoryInfo(AppSettings.GetDataPath() + "\\addons\\airports");

                foreach (FileInfo file in dir.GetFiles("*.xml"))
                {
                    LoadAirports(file.FullName);
                }
            }
            catch (Exception e)
            {
                var file = new StreamWriter(
                    AppSettings.GetCommonApplicationDataPath() + "\\theairlinestartup.log",
                    true);
                file.WriteLine("Airport failing");
                file.WriteLine(e.ToString());
                file.WriteLine(e.StackTrace);
                file.Close();
            }
            Airports.LargestAirports = Airports.GetAirports(a => a.Profile.Size == GeneralHelpers.Size.Largest).Count;
            Airports.VeryLargeAirports =
                Airports.GetAirports(a => a.Profile.Size == GeneralHelpers.Size.VeryLarge).Count;
            Airports.LargeAirports = Airports.GetAirports(a => a.Profile.Size == GeneralHelpers.Size.Large).Count;
            Airports.MediumAirports = Airports.GetAirports(a => a.Profile.Size == GeneralHelpers.Size.Medium).Count;
            Airports.SmallAirports = Airports.GetAirports(a => a.Profile.Size == GeneralHelpers.Size.Small).Count;
            Airports.VerySmallAirports =
                Airports.GetAirports(a => a.Profile.Size == GeneralHelpers.Size.VerySmall).Count;
            Airports.SmallestAirports = Airports.GetAirports(a => a.Profile.Size == GeneralHelpers.Size.Smallest).Count;

            foreach (GeneralHelpers.Size size in Enum.GetValues(typeof (GeneralHelpers.Size)))
            {
                if (!Airports.CargoAirportsSizes.ContainsKey(size))
                {
                    GeneralHelpers.Size size1 = size;
                    Airports.CargoAirportsSizes.Add(size, Airports.GetAirports(a => a.Profile.Cargo == size1).Count);
                }
            }
        }

        public static void LoadConfigurations()
        {
            Configurations.Clear();

            LoadStandardConfigurations();
        }

        public static void SetupAlliances()
        {
            var alliances = new List<Alliance>(Alliances.GetAlliances());
            foreach (Alliance alliance in alliances)
            {
                int activeMembers =
                    alliance.Members.Count(
                        m =>
                        Airlines.ContainsAirline(m.Airline) && !m.Airline.IsHuman
                        && m.JoinedDate <= GameObject.GetInstance().GameTime
                        && m.ExitedDate > GameObject.GetInstance().GameTime);

                if (activeMembers > 1)
                {
                    var members = new List<AllianceMember>(alliance.Members);

                    foreach (
                        AllianceMember member in
                            members.Where(
                                m =>
                                !Airlines.GetAllAirlines().Contains(m.Airline)
                                || m.JoinedDate > GameObject.GetInstance().GameTime
                                || GameObject.GetInstance().GameTime > m.ExitedDate))
                    {
                        alliance.RemoveMember(member);
                    }
                }
                else
                {
                    while (alliance.Members.Count > 0)
                    {
                        alliance.RemoveMember(alliance.Members[0]);
                    }

                    Alliances.RemoveAlliance(alliance);
                }
            }
        }

        /*! reads the basic xmls for loading of game
         */

        /*! public static method SetupGame().
         * Tries to create game´s environment and base configuration.
         */

        public static void SetupGame()
        {
            try
            {
                ReadSettingsFile();
                GeneralHelpers.CreateBigImageCanvas();
                ClearLists();

                CreateTimeZones();
                SetupDifficultyLevels();
                SetupStatisticsTypes();
                CreateHubTypes();

                LoadRegions();
                LoadCountries();
                LoadStates();
                LoadTemporaryCountries();
                LoadUnions();

                CreateContinents();

                LoadAirports();
                LoadAirportLogos();
                LoadAirportMaps();
                LoadAirportFacilities();
                LoadCooperations();
                LoadMajorDestinations();

                LoadAirlineFacilities();

                LoadManufacturers();
                LoadManufacturerLogos();
                LoadAirliners();
                LoadAirlinerImages();
                LoadAirlinerFacilities();
                LoadEngineTypes();
                LoadFlightRestrictions();
                LoadInflationYears();
                LoadHolidays();
                LoadHistoricEvents();
                LoadRandomEvents();
                LoadSpecialContracts();
                LoadWeatherAverages();

                CreateAdvertisementTypes();
                CreateFeeTypes();
                CreateFlightFacilities();
                CreateTrainingAircraftTypes();
                CreatePilotRatings();

                LoadStandardConfigurations();
                LoadAirlinerTypeConfigurations();

                LoadAirlines();
                LoadAlliances();

                LoadScenarios();
            }
            catch (Exception e)
            {
                //var file = new StreamWriter(
                //    AppSettings.GetCommonApplicationDataPath() + "\\theairlinestartup.log",
                //    true);
                //file.WriteLine("Game start failing");
                //file.WriteLine(e.ToString());
                //file.WriteLine(e.StackTrace);
                //file.Close();

                Logger.Error("Failed to setup game", e);
            }


            IEnumerable<Airport> noRunways = Airports.GetAllAirports().Where(a => a.Runways.Count == 0);

            foreach (Airport airport in noRunways)
                Console.WriteLine(@"{0}, {1} in {2} has no runways", airport.Profile.Name, airport.Profile.IATACode, airport.Profile.Country.Name);

            Console.WriteLine(@"Airports: " + Airports.GetAllAirports().Count);
            Console.WriteLine(@"Airlines: " + Airlines.GetAllAirlines().Count);

            Airport lbb = Airports.GetAirport("LBB");
            Airport dfw = Airports.GetAirport("DFW");

            Console.WriteLine(@"Distance: " + MathHelpers.GetDistance(lbb, dfw));

            /*
            System.IO.StreamWriter aFile = new System.IO.StreamWriter("c:\\bbm\\airports.csv");

            string lines = "Name;IATA;ICAO;Type;Season;Town;Country;GMT;DST;Latitude;Longitude;Size;Pax;Cargosize;Cargo;Terminals[Name%Gates];Runways[Name%Surface%Lenght]";
            
            aFile.WriteLine(lines);

            foreach (Airport airport in Airports.GetAllAirports())
            {
                string airportLine = airport.Profile.Name;
                airportLine += ";" + airport.Profile.IATACode;
                airportLine += ";" + airport.Profile.ICAOCode;
                airportLine += ";" + airport.Profile.Type.ToString();
                airportLine += ";" + airport.Profile.Season.ToString(); 
                airportLine += ";" + airport.Profile.Town.Name;
                airportLine += ";" + airport.Profile.Country.Name;
                airportLine += ";" + airport.Profile.OffsetGMT.ToString();
                airportLine += ";" + airport.Profile.OffsetDST.ToString();
                airportLine += ";" + airport.Profile.Coordinates.Latitude.ToString();
                airportLine += ";" + airport.Profile.Coordinates.Longitude.ToString();
                airportLine += ";" + airport.Profile.Size.ToString();
                airportLine += ";" + airport.Profile.Pax.ToString();
                airportLine += ";" + airport.Profile.Cargo.ToString();
                airportLine += ";" + airport.Profile.CargoVolume.ToString();

                string runwaysLine="";
                string terminalsLine="";

                foreach (Terminal terminal in airport.Terminals.AirportTerminals)
                {
                    terminalsLine += "[" + terminal.Name + "%" + terminal.Gates.NumberOfGates.ToString() + "]";
                }

                foreach (Runway runway in airport.Runways)
                {
                    runwaysLine +=  "[" + runway.Name + "%" + runway.Surface.ToString() + "%" + runway.Length + "]";
                }

                airportLine += ";" + terminalsLine;
                airportLine += ";" + runwaysLine;

                aFile.WriteLine(airportLine);
            
                    
                    
                    //string.Format("{0};{1};{2};{3};{4};{5};{6};{7};{8};{9};{10};{11};{12};{13};{14};{15};{16}", airport.Profile.Name, airport.Profile.IATACode, airport.Profile.ICAOCode, airport.Profile.Type.ToString(), airport.Profile.Season.ToString(),airport.Profile.Town.Name,airport.Profile.Town.Country.Name,airport.Profile.OffsetGMT.ToString(),airport.Profile.OffsetDST.ToString());

               
            }
            aFile.Close();
        */
            /*
                var townGroups =
                    from a in Airports.GetAllAirports() where a.Profile.Town.Name.Trim().Length == 0
                    group a by a.Profile.Country.Region into g
                    select new { Region = g.Key, Airports = g };

                foreach (var g in townGroups)
                {
                    Console.WriteLine(g.Region.Name);
                    foreach (var a in g.Airports)
                    {
                        Console.WriteLine("      {0} ({1}), {2}",a.Profile.Name,a.Profile.IATACode,a.Profile.Country.Name);
                    }
                } */

            /*
            var aircraftFamilies = AirlinerTypes.GetAllTypes().Select(a => a.AirlinerFamily).Distinct();

            foreach (string family in aircraftFamilies)
                Console.WriteLine(family);

            /*
            var airlineRegionGroups =
                from a in Airlines.GetAllAirlines()
                group a by a.Profile.Country.Region into g
                select new { Region = g.Key, Airports = g };

            foreach (var g in airlineRegionGroups)
            {
                Console.WriteLine("{0} has {1} airports", g.Region.Name,g.Airports.Count());
                
             
            }


            var airlines = Airlines.GetAllAirlines().FindAll(a => a.Profile.PreferedAirport == null);
            var airlineLogos = Airlines.GetAllAirlines().FindAll(a => a.Profile.Logos.Count==0);

            foreach (Airline noLogoAirline in airlineLogos)
                Console.WriteLine(noLogoAirline.Profile.Name);

            var noRunwayAirports = Airports.GetAirports(a => a.Runways.Count == 0);
            var cargoAirliners = AirlinerTypes.GetAllTypes().FindAll(a => a.TypeAirliner == AirlinerType.TypeOfAirliner.Cargo);

            foreach (Airport airport in noRunwayAirports)
                Console.WriteLine(airport.Profile.Name);

            */
        }

        public static void SetupLoadedGame()
        {
            Countries.Clear();
            Regions.Clear();
            Unions.Clear();
            Continents.Clear();

            LoadRegions();
            LoadCountries();
            LoadStates();
            LoadTemporaryCountries();
            LoadUnions();

            CreateContinents();
        }

        public static void SetupMainGame(List<Airline> opponents, int totalOpponents)
        {
            int year = GameObject.GetInstance().GameTime.Year;

            List<Airline> notAvailableAirlines =
                Airlines.GetAirlines(a => !(a.Profile.Founded <= year && a.Profile.Folded > year));

            foreach (Airline airline in notAvailableAirlines)
            {
                Airlines.RemoveAirline(airline);
            }

            var airlines =
                new List<Airline>(
                    Airlines.GetAirlines(
                        a =>
                        !a.IsHuman && !opponents.Contains(a) && a.Profile.Founded <= year && a.Profile.Folded > year));

            airlines = MathHelpers.Shuffle(airlines);

            for (int i = 0; i < airlines.Count + opponents.Count - totalOpponents; i++)
            {
                Airlines.RemoveAirline(airlines[i]);
            }

            SetupMainGame();
        }

        public static void SetupMainGame(int opponents, bool sameRegion)
        {
            RemoveAirlines(opponents, sameRegion);

            SetupMainGame();
        }

        public static void SetupMergers()
        {
            AirlineMergers.Clear();
            LoadAirlineMergers();

            var mergers = new List<AirlineMerger>(AirlineMergers.GetAirlineMergers());

            foreach (AirlineMerger merger in mergers)
            {
                if (!Airlines.ContainsAirline(merger.Airline1) || !Airlines.ContainsAirline(merger.Airline2)
                    || merger.Airline2.IsHuman || merger.Airline1.IsHuman)
                {
                    AirlineMergers.RemoveAirlineMerger(merger);
                }
            }
        }

        #endregion

        #region Methods

        public static void ClearLists()
        {
            AdvertisementTypes.Clear();
            TimeZones.Clear();
            SpecialContractTypes.Clear();
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
            Continents.Clear();
            Regions.Clear();
            Countries.Clear();
            States.Clear();
            Unions.Clear();
            AirlinerTypes.Clear();
            EngineTypes.Clear();
            FeeTypes.Clear();
            PilotRatings.Clear();
            Alliances.Clear();
            FlightRestrictions.Clear();
            Inflations.Clear();
            Holidays.Clear();
            Configurations.Clear();
            HistoricEvents.Clear();
            RandomEvents.Clear();
            WeatherAverages.Clear();
            DifficultyLevels.Clear();
            Pilots.Clear();
            Instructors.Clear();
            TrainingAircraftTypes.Clear();
            Scenarios.Clear();
            HubTypes.Clear();
            CooperationTypes.Clear();
        }

        /*! reads the settings file if existing
         */

        /*! creates the Advertisement types
         */

        public static void CreateAdvertisementTypes()
        {
            AdvertisementTypes.AddAdvertisementType(
                new AdvertisementType(AdvertisementType.AirlineAdvertisementType.Internet, "No Advertisement", 0, 0));
            AdvertisementTypes.AddAdvertisementType(
                new AdvertisementType(AdvertisementType.AirlineAdvertisementType.Internet, "Local", 5000, 1));
            AdvertisementTypes.AddAdvertisementType(
                new AdvertisementType(AdvertisementType.AirlineAdvertisementType.Internet, "National", 10000, 2));
            AdvertisementTypes.AddAdvertisementType(
                new AdvertisementType(AdvertisementType.AirlineAdvertisementType.Internet, "Global", 25000, 3));
            AdvertisementTypes.AddAdvertisementType(
                new AdvertisementType(AdvertisementType.AirlineAdvertisementType.Newspaper, "No Advertisement", 0, 0));
            AdvertisementTypes.AddAdvertisementType(
                new AdvertisementType(AdvertisementType.AirlineAdvertisementType.Newspaper, "Local", 5000, 1));
            AdvertisementTypes.AddAdvertisementType(
                new AdvertisementType(AdvertisementType.AirlineAdvertisementType.Newspaper, "National", 10000, 2));
            AdvertisementTypes.AddAdvertisementType(
                new AdvertisementType(AdvertisementType.AirlineAdvertisementType.Newspaper, "Global", 25000, 3));
            AdvertisementTypes.AddAdvertisementType(
                new AdvertisementType(AdvertisementType.AirlineAdvertisementType.Radio, "No Advertisement", 0, 0));
            AdvertisementTypes.AddAdvertisementType(
                new AdvertisementType(AdvertisementType.AirlineAdvertisementType.Radio, "Local", 5000, 1));
            AdvertisementTypes.AddAdvertisementType(
                new AdvertisementType(AdvertisementType.AirlineAdvertisementType.Radio, "National", 10000, 2));
            AdvertisementTypes.AddAdvertisementType(
                new AdvertisementType(AdvertisementType.AirlineAdvertisementType.Radio, "Global", 25000, 3));
            AdvertisementTypes.AddAdvertisementType(
                new AdvertisementType(AdvertisementType.AirlineAdvertisementType.Tv, "No Advertisement", 0, 0));
            AdvertisementTypes.AddAdvertisementType(
                new AdvertisementType(AdvertisementType.AirlineAdvertisementType.Tv, "Local", 5000, 1));
            AdvertisementTypes.AddAdvertisementType(
                new AdvertisementType(AdvertisementType.AirlineAdvertisementType.Tv, "National", 10000, 2));
            AdvertisementTypes.AddAdvertisementType(
                new AdvertisementType(AdvertisementType.AirlineAdvertisementType.Tv, "Global", 25000, 3));
        }

        private static void CreateAirlineLogos()
        {
            foreach (Airline airline in Airlines.GetAllAirlines())
            {
                string logoName = airline.Profile.IATACode;
                if (airline.Profile.LogoName.Length > 0)
                {
                    logoName = airline.Profile.LogoName;
                }

                airline.Profile.AddLogo(
                    File.Exists(AppSettings.GetDataPath() + "\\graphics\\airlinelogos\\" + logoName + ".png")
                        ? new AirlineLogo(AppSettings.GetDataPath() + "\\graphics\\airlinelogos\\" + logoName + ".png")
                        : new AirlineLogo(AppSettings.GetDataPath() + "\\graphics\\airlinelogos\\default.png"));
            }
        }

        private static void CreateAirlineStartData(Airline airline, AirlineStartData startData)
        {
            AirportFacility checkinFacility =
                AirportFacilities.GetFacilities(AirportFacility.FacilityType.CheckIn).Find(f => f.TypeLevel == 1);
            AirportFacility cargoTerminal =
                AirportFacilities.GetFacilities(AirportFacility.FacilityType.Cargo).Find(f => f.TypeLevel > 0);

            //  int difficultyFactor = GameObject.GetInstance().Difficulty.AILevel > 1 ? 2 : 1; //level easy

            int startDataFactor = Convert.ToInt16(GameObject.GetInstance().Difficulty.StartDataLevel);
            List<StartDataRoute> startroutes =
                startData.Routes.FindAll(
                    r =>
                    r.Opened <= GameObject.GetInstance().GameTime.Year
                    && r.Closed >= GameObject.GetInstance().GameTime.Year);

            Terminal.TerminalType terminaltype = airline.AirlineRouteFocus == Route.RouteType.Cargo ? Terminal.TerminalType.Cargo : Terminal.TerminalType.Passenger;
            //creates the routes
            List<StartDataRoute> sRoutes = startroutes.GetRange(0, startroutes.Count/startDataFactor);
            Parallel.ForEach(
                sRoutes,
                startRoute =>
                    {
                        Airport dest1 = Airports.GetAirport(startRoute.Destination1);
                        Airport dest2 = Airports.GetAirport(startRoute.Destination2);

                        if (dest1 != null && dest2 != null)
                        {
                            if (dest1.GetAirportFacility(airline, AirportFacility.FacilityType.Cargo).TypeLevel == 0
                                && dest1.GetAirportFacility(null, AirportFacility.FacilityType.Cargo).TypeLevel == 0
                                && airline.AirlineRouteFocus == Route.RouteType.Cargo)
                            {
                                dest1.AddAirportFacility(airline, cargoTerminal, GameObject.GetInstance().GameTime);
                            }

                            if (dest2.GetAirportFacility(airline, AirportFacility.FacilityType.Cargo).TypeLevel == 0
                                && dest2.GetAirportFacility(null, AirportFacility.FacilityType.Cargo).TypeLevel == 0
                                && airline.AirlineRouteFocus == Route.RouteType.Cargo)
                            {
                                dest2.AddAirportFacility(airline, cargoTerminal, GameObject.GetInstance().GameTime);
                            }

                            if (!AirportHelpers.HasFreeGates(dest1, airline, terminaltype))
                            {
                                AirportHelpers.RentGates(dest1, airline, AirportContract.ContractType.LowService, terminaltype);
                            }

                            if (!AirportHelpers.HasFreeGates(dest2, airline, terminaltype))
                            {
                                AirportHelpers.RentGates(dest2, airline, AirportContract.ContractType.LowService, terminaltype);
                            }

                            Guid id = Guid.NewGuid();

                            Route route = null;

                            double price = PassengerHelpers.GetPassengerPrice(dest1, dest2);

                            if (startRoute.RouteType == Route.RouteType.Mixed
                                || startRoute.RouteType == Route.RouteType.Passenger)
                            {
                                route = new PassengerRoute(
                                    id.ToString(),
                                    dest1,
                                    dest2,
                                    GameObject.GetInstance().GameTime,
                                    price);
                            }

                            if (startRoute.RouteType == Route.RouteType.Cargo)
                            {
                                route = new CargoRoute(
                                    id.ToString(),
                                    dest1,
                                    dest2,
                                    GameObject.GetInstance().GameTime,
                                    PassengerHelpers.GetCargoPrice(dest1, dest2));
                            }

                            KeyValuePair<Airliner, bool>? airliner = null;
                            if (startRoute.Type != null)
                            {
                                double distance = MathHelpers.GetDistance(dest1, dest2);

                                if (startRoute.Type.Range > distance)
                                {
                                    airliner =
                                        new KeyValuePair<Airliner, bool>(
                                            Airliners.GetAirlinersForSale(a => a.Type == startRoute.Type).FirstOrDefault(),
                                            true);

                                    if (airliner.Value.Key == null)
                                    {
                                        id = Guid.NewGuid();
                                        var nAirliner = new Airliner(
                                            id.ToString(),
                                            startRoute.Type,
                                            airline.Profile.Country.TailNumbers.GetNextTailNumber(),
                                            GameObject.GetInstance().GameTime);
                                        Airliners.AddAirliner(nAirliner);

                                        nAirliner.ClearAirlinerClasses();

                                        AirlinerHelpers.CreateAirlinerClasses(nAirliner);

                                        airliner = new KeyValuePair<Airliner, bool>(nAirliner, true);
                                    }
                                }
                            }

                            bool leaseAircraft = airline.Profile.PrimaryPurchasing
                                                    == AirlineProfile.PreferedPurchasing.Leasing;

                            if (airliner == null)
                            {
                                airliner = AIHelpers.GetAirlinerForRoute(
                                    airline,
                                    dest2,
                                    dest1,
                                    leaseAircraft,
                                    startRoute.RouteType,
                                    true);

                                if (airliner == null
                                    && airline.Profile.PrimaryPurchasing == AirlineProfile.PreferedPurchasing.Random)
                                {
                                    AIHelpers.GetAirlinerForRoute(
                                        airline,
                                        dest2,
                                        dest1,
                                        true,
                                        startRoute.RouteType,
                                        true);
                                }
                            }

                            if (airliner != null)
                            {
                                FleetAirliner fAirliner = AirlineHelpers.AddAirliner(
                                    airline,
                                    airliner.Value.Key,
                                    airline.Airports[0],
                                    leaseAircraft);
                                fAirliner.AddRoute(route);
                                fAirliner.Status = FleetAirliner.AirlinerStatus.ToRouteStart;
                                AirlineHelpers.HireAirlinerPilots(fAirliner);

                                if (route != null)
                                {
                                    route.LastUpdated = GameObject.GetInstance().GameTime;

                                    if (startRoute.RouteType == Route.RouteType.Mixed
                                        || startRoute.RouteType == Route.RouteType.Passenger)
                                    {
                                        AirlinerHelpers.CreateAirlinerClasses(fAirliner.Airliner);

                                        RouteClassesConfiguration configuration =
                                            AIHelpers.GetRouteConfiguration((PassengerRoute) route);

                                        foreach (RouteClassConfiguration classConfiguration in configuration.GetClasses())
                                        {
                                            ((PassengerRoute) route).GetRouteAirlinerClass(classConfiguration.Type).FarePrice =
                                                price*GeneralHelpers.ClassToPriceFactor(classConfiguration.Type);

                                            foreach (RouteFacility rFacility in classConfiguration.GetFacilities())
                                            {
                                                ((PassengerRoute) route).GetRouteAirlinerClass(classConfiguration.Type)
                                                                        .AddFacility(rFacility);
                                            }
                                        }

                                        AIHelpers.CreateRouteTimeTable(route, fAirliner);
                                    }
                                    if (startRoute.RouteType == Route.RouteType.Cargo)
                                    {
                                        AIHelpers.CreateCargoRouteTimeTable(route, fAirliner);
                                    }
                                }
                            }
                            airline.AddRoute(route);
                        }
                    });
            //adds the airliners
            Parallel.ForEach(
                startData.Airliners,
                airliners =>
                    {
                        AirlinerType type = AirlinerTypes.GetType(airliners.Type); //B747-200

                        const int totalSpan = 2010 - 1960;
                        int yearSpan = GameObject.GetInstance().GameTime.Year - 1960;
                        double valueSpan = Convert.ToDouble(airliners.AirlinersLate - airliners.AirlinersEarly);
                        double span = valueSpan/Convert.ToDouble(totalSpan);

                        int numbers = Math.Max(1, Convert.ToInt16(span*yearSpan)/startDataFactor);

                        if (type == null)
                        {
                            string tAirline = airline.Profile.Name;
                            string typeNull = airliners.Type;

                            Console.WriteLine(tAirline + @" " + typeNull);
                        }
                        if (type != null && type.Produced.From <= GameObject.GetInstance().GameTime)
                        {
                            for (int i = 0; i < Math.Max(numbers, airliners.AirlinersEarly); i++)
                            {
                                Guid id = Guid.NewGuid();

                                int countryNumber = Rnd.Next(Countries.GetCountries().Count() - 1);
                                Country country = Countries.GetCountries()[countryNumber];

                                int builtYear = Rnd.Next(
                                    type.Produced.From.Year,
                                    Math.Min(GameObject.GetInstance().GameTime.Year - 1, type.Produced.To.Year));

                                var airliner = new Airliner(
                                    id.ToString(),
                                    type,
                                    country.TailNumbers.GetNextTailNumber(),
                                    new DateTime(builtYear, 1, 1));

                                int age = MathHelpers.CalculateAge(airliner.BuiltDate, GameObject.GetInstance().GameTime);

                                long kmPerYear = Rnd.Next(100000, 1000000);
                                long km = kmPerYear*age;

                                airliner.Flown = km;

                                airliner.EngineType = EngineTypes.GetStandardEngineType(airliner.Type, airliner.BuiltDate.Year);

                                Airliners.AddAirliner(airliner);

                                bool leaseAircraft = airline.Profile.PrimaryPurchasing
                                                        == AirlineProfile.PreferedPurchasing.Leasing;

                                FleetAirliner fAirliner = AirlineHelpers.AddAirliner(
                                    airline,
                                    airliner,
                                    airline.Airports[0],
                                    leaseAircraft);
                                fAirliner.Status = FleetAirliner.AirlinerStatus.Stopped;
                                AirlineHelpers.HireAirlinerPilots(fAirliner);

                                AirlinerHelpers.CreateAirlinerClasses(fAirliner.Airliner);
                            }
                        }
                    });

            //the origin routes
            Parallel.ForEach(
                startData.OriginRoutes,
                routes =>
                    {
                        Airport origin = Airports.GetAirport(routes.Origin);

                        if (origin != null)
                        {
                            Terminal.TerminalType terminalType = airline.AirlineRouteFocus == Route.RouteType.Cargo ? Terminal.TerminalType.Cargo : Terminal.TerminalType.Passenger;

                            for (int i = 0;
                                 i < Math.Min(routes.Destinations/startDataFactor, origin.Terminals.GetFreeGates(terminalType));
                                 i++)
                            {
                                //if (origin.getAirportFacility(airline, AirportFacility.FacilityType.CheckIn).TypeLevel == 0)
                                //origin.addAirportFacility(airline, checkinFacility, GameObject.GetInstance().GameTime);

                                if (!AirportHelpers.HasFreeGates(origin, airline, terminalType))
                                {
                                    AirportHelpers.RentGates(origin, airline, AirportContract.ContractType.LowService, terminalType);
                                }

                                Airport destination = GetStartDataRoutesDestination(routes);

                                //if (destination.getAirportFacility(airline, AirportFacility.FacilityType.CheckIn).TypeLevel == 0)
                                //destination.addAirportFacility(airline, checkinFacility, GameObject.GetInstance().GameTime);

                                if (!AirportHelpers.HasFreeGates(destination, airline, terminalType))
                                {
                                    AirportHelpers.RentGates(destination, airline, AirportContract.ContractType.LowService, terminalType);
                                }

                                Guid id = Guid.NewGuid();

                                Route route = null;

                                double price = PassengerHelpers.GetPassengerPrice(origin, destination);

                                if (routes.RouteType == Route.RouteType.Mixed
                                    || routes.RouteType == Route.RouteType.Passenger)
                                {
                                    route = new PassengerRoute(
                                        id.ToString(),
                                        origin,
                                        destination,
                                        GameObject.GetInstance().GameTime,
                                        price);
                                }

                                if (routes.RouteType == Route.RouteType.Cargo)
                                {
                                    route = new CargoRoute(
                                        id.ToString(),
                                        origin,
                                        destination,
                                        GameObject.GetInstance().GameTime,
                                        PassengerHelpers.GetCargoPrice(origin, destination));
                                }

                                bool leaseAircraft = airline.Profile.PrimaryPurchasing
                                                        == AirlineProfile.PreferedPurchasing.Leasing;

                                KeyValuePair<Airliner, bool>? airliner = AIHelpers.GetAirlinerForRoute(
                                    airline,
                                    origin,
                                    destination,
                                    leaseAircraft,
                                    routes.RouteType,
                                    true);

                                if (airliner == null
                                    && airline.Profile.PrimaryPurchasing == AirlineProfile.PreferedPurchasing.Random)
                                {
                                    airliner = AIHelpers.GetAirlinerForRoute(
                                        airline,
                                        origin,
                                        destination,
                                        true,
                                        routes.RouteType,
                                        true);
                                }

                                double distance = MathHelpers.GetDistance(origin, destination);

                                if (airliner != null)
                                {
                                    FleetAirliner fAirliner = AirlineHelpers.AddAirliner(
                                        airline,
                                        airliner.Value.Key,
                                        airline.Airports[0],
                                        leaseAircraft);
                                    fAirliner.AddRoute(route);
                                    fAirliner.Status = FleetAirliner.AirlinerStatus.ToRouteStart;
                                    AirlineHelpers.HireAirlinerPilots(fAirliner);

                                    if (route != null)
                                    {
                                        route.LastUpdated = GameObject.GetInstance().GameTime;

                                        if (routes.RouteType == Route.RouteType.Passenger
                                            || routes.RouteType == Route.RouteType.Mixed)
                                        {
                                            AirlinerHelpers.CreateAirlinerClasses(fAirliner.Airliner);

                                            RouteClassesConfiguration configuration =
                                                AIHelpers.GetRouteConfiguration((PassengerRoute) route);

                                            foreach (RouteClassConfiguration classConfiguration in configuration.GetClasses())
                                            {
                                                ((PassengerRoute) route).GetRouteAirlinerClass(classConfiguration.Type).FarePrice
                                                    = price*GeneralHelpers.ClassToPriceFactor(classConfiguration.Type);

                                                foreach (RouteFacility rFacility in classConfiguration.GetFacilities())
                                                {
                                                    ((PassengerRoute) route).GetRouteAirlinerClass(classConfiguration.Type)
                                                                            .AddFacility(rFacility);
                                                }
                                            }

                                            AIHelpers.CreateRouteTimeTable(route, fAirliner);
                                        }
                                        if (routes.RouteType == Route.RouteType.Cargo)
                                        {
                                            AIHelpers.CreateCargoRouteTimeTable(route, fAirliner);
                                        }

                                        airline.AddRoute(route);
                                    }
                                }
                            }
                        }
                    });
        }

        private static void CreateComputerRoutes(Airline airline)
        {
            bool leaseAircraft = airline.Profile.PrimaryPurchasing == AirlineProfile.PreferedPurchasing.Leasing;

            Airport airportHomeBase = FindComputerHomeBase(airline);

            AirportFacility serviceFacility =
                AirportFacilities.GetFacilities(AirportFacility.FacilityType.Service).Find(f => f.TypeLevel == 1);
            AirportFacility checkinFacility =
                AirportFacilities.GetFacilities(AirportFacility.FacilityType.CheckIn).Find(f => f.TypeLevel == 1);
            AirportFacility cargoTerminal =
                AirportFacilities.GetFacilities(AirportFacility.FacilityType.Cargo).Find(f => f.TypeLevel > 0);

            airportHomeBase.AddAirportFacility(airline, serviceFacility, GameObject.GetInstance().GameTime);
            airportHomeBase.AddAirportFacility(airline, checkinFacility, GameObject.GetInstance().GameTime);

            if (airline.AirlineRouteFocus == Route.RouteType.Cargo)
            {
                airportHomeBase.AddAirportFacility(airline, cargoTerminal, GameObject.GetInstance().GameTime);
            }

            AirlineStartData startData = AirlineStartDatas.GetAirlineStartData(airline);

            //creates the start data for an airline
            if (startData != null)
            {
                AirportHelpers.RentGates(airportHomeBase, airline, AirportContract.ContractType.Full);

                CreateAirlineStartData(airline, startData);
            }
            else
            {
                List<Airport> airportDestinations = AIHelpers.GetDestinationAirports(airline, airportHomeBase);

                if (airportDestinations.Count == 0)
                {
                    airportDestinations =
                        Airports.GetAirports(
                            a =>
                            a.Profile.Country.Region == airportHomeBase.Profile.Country.Region
                            && a != airportHomeBase);
                }

                KeyValuePair<Airliner, bool>? airliner = null;
                Airport airportDestination = null;

                int counter = 0;

                while ((airportDestination == null || airliner == null || !airliner.HasValue)
                       && airportDestinations.Count > counter)
                {
                    airportDestination = airportDestinations[counter];

                    airliner = AIHelpers.GetAirlinerForRoute(
                        airline,
                        airportHomeBase,
                        airportDestination,
                        leaseAircraft,
                        airline.AirlineRouteFocus,
                        true);

                    counter++;
                }

                if (airportDestination == null || !airliner.HasValue)
                {
                    CreateComputerRoutes(airline);
                }
                else
                {
                    AirportHelpers.AddAirlineContract(
                        new AirportContract(
                            airline,
                            airportHomeBase,
                            AirportContract.ContractType.Full,
                            airline.AirlineRouteFocus == Route.RouteType.Cargo ? Terminal.TerminalType.Cargo : Terminal.TerminalType.Passenger,
                            GameObject.GetInstance().GameTime,
                            2,
                            25,
                            0,
                            true));

                    AirportHelpers.RentGates(airportDestination, airline, AirportContract.ContractType.LowService);
                    //airportDestination.addAirportFacility(airline, checkinFacility, GameObject.GetInstance().GameTime);

                    Guid id = Guid.NewGuid();

                    double price = PassengerHelpers.GetPassengerPrice(airportDestination, airline.Airports[0]);

                    Route route = null;
                    if (airline.AirlineRouteFocus == Route.RouteType.Passenger)
                    {
                        route = new PassengerRoute(
                            id.ToString(),
                            airportDestination,
                            airline.Airports[0],
                            GameObject.GetInstance().GameTime,
                            price);

                        RouteClassesConfiguration configuration = AIHelpers.GetRouteConfiguration((PassengerRoute) route);

                        foreach (RouteClassConfiguration classConfiguration in configuration.GetClasses())
                        {
                            ((PassengerRoute) route).GetRouteAirlinerClass(classConfiguration.Type).FarePrice = price
                                                                                                                *GeneralHelpers
                                                                                                                     .ClassToPriceFactor
                                                                                                                     (
                                                                                                                         classConfiguration
                                                                                                                             .Type);

                            foreach (RouteFacility rFacility in classConfiguration.GetFacilities())
                            {
                                ((PassengerRoute) route).GetRouteAirlinerClass(classConfiguration.Type)
                                                        .AddFacility(rFacility);
                            }
                        }
                    }
                    if (airline.AirlineRouteFocus == Route.RouteType.Helicopter)
                    {
                        route = new HelicopterRoute(
                            id.ToString(),
                            airportDestination,
                            airline.Airports[0],
                            GameObject.GetInstance().GameTime,
                            price);

                        RouteClassesConfiguration configuration = AIHelpers.GetRouteConfiguration((HelicopterRoute) route);

                        foreach (RouteClassConfiguration classConfiguration in configuration.GetClasses())
                        {
                            ((HelicopterRoute) route).GetRouteAirlinerClass(classConfiguration.Type).FarePrice = price
                                                                                                                 *GeneralHelpers
                                                                                                                      .ClassToPriceFactor
                                                                                                                      (
                                                                                                                          classConfiguration
                                                                                                                              .Type);

                            foreach (RouteFacility rFacility in classConfiguration.GetFacilities())
                            {
                                ((HelicopterRoute) route).GetRouteAirlinerClass(classConfiguration.Type)
                                                         .AddFacility(rFacility);
                            }
                        }
                    }
                    if (airline.AirlineRouteFocus == Route.RouteType.Cargo)
                    {
                        route = new CargoRoute(
                            id.ToString(),
                            airportDestination,
                            airline.Airports[0],
                            GameObject.GetInstance().GameTime,
                            PassengerHelpers.GetCargoPrice(airportDestination, airline.Airports[0]));

                        airportDestination.AddAirportFacility(airline, cargoTerminal, GameObject.GetInstance().GameTime);
                    }
                    if (leaseAircraft)
                    {
                        AirlineHelpers.AddAirlineInvoice(
                            airline,
                            GameObject.GetInstance().GameTime,
                            Invoice.InvoiceType.Rents,
                            -airliner.Value.Key.LeasingPrice*2);
                    }
                    else
                    {
                        AirlineHelpers.AddAirlineInvoice(
                            airline,
                            GameObject.GetInstance().GameTime,
                            Invoice.InvoiceType.Purchases,
                            -airliner.Value.Key.GetPrice());
                    }

                    var fAirliner =
                        new FleetAirliner(
                            leaseAircraft ? FleetAirliner.PurchasedType.Leased : FleetAirliner.PurchasedType.Bought,
                            GameObject.GetInstance().GameTime,
                            airline,
                            airliner.Value.Key,
                            airportHomeBase) {Status = FleetAirliner.AirlinerStatus.ToRouteStart};
                    fAirliner.AddRoute(route);
                    AirlinerHelpers.CreateAirlinerClasses(fAirliner.Airliner);
                    AirlineHelpers.HireAirlinerPilots(fAirliner);

                    airline.AddAirliner(fAirliner);

                    airline.AddRoute(route);
                    if (route != null)
                    {
                        route.LastUpdated = GameObject.GetInstance().GameTime;

                        if (route.Type == Route.RouteType.Passenger || route.Type == Route.RouteType.Mixed)
                        {
                            AIHelpers.CreateRouteTimeTable(route, fAirliner);
                        }
                        if (route.Type == Route.RouteType.Cargo)
                        {
                            AIHelpers.CreateCargoRouteTimeTable(route, fAirliner);
                        }
                    }
                }
            }
        }

        /*! creates the time zones.
         */

        /*!creates the continents
         */

        private static void CreateContinents()
        {
            var africa = new Continent("101", "Africa");
            africa.AddRegion(Regions.GetRegion("101"));
            africa.AddRegion(Regions.GetRegion("102"));
            africa.AddRegion(Regions.GetRegion("103"));
            africa.AddRegion(Regions.GetRegion("104"));
            africa.AddRegion(Regions.GetRegion("105"));
            Continents.AddContinent(africa);

            var asia = new Continent("102", "Asia");
            asia.AddRegion(Regions.GetRegion("106"));
            asia.AddRegion(Regions.GetRegion("107"));
            asia.AddRegion(Regions.GetRegion("108"));
            asia.AddRegion(Regions.GetRegion("109"));
            asia.AddRegion(Regions.GetRegion("110"));
            Continents.AddContinent(asia);

            var australia = new Continent("103", "Australia and Oceania");
            australia.AddRegion(Regions.GetRegion("111"));
            australia.AddRegion(Regions.GetRegion("112"));
            Continents.AddContinent(australia);

            var europe = new Continent("104", "Europe");
            europe.AddRegion(Regions.GetRegion("113"));
            europe.AddRegion(Regions.GetRegion("114"));
            europe.AddRegion(Regions.GetRegion("115"));
            europe.AddRegion(Regions.GetRegion("116"));
            Continents.AddContinent(europe);

            var northAmerica = new Continent("105", "North America");
            northAmerica.AddRegion(Regions.GetRegion("117"));
            northAmerica.AddRegion(Regions.GetRegion("118"));
            northAmerica.AddRegion(Regions.GetRegion("119"));
            Continents.AddContinent(northAmerica);

            var southAmerica = new Continent("106", "South America");
            southAmerica.AddRegion(Regions.GetRegion("120"));
            Continents.AddContinent(southAmerica);
        }

        private static void CreateFeeTypes()
        {
            //wages
            FeeTypes.AddType(new FeeType(FeeType.EFeeType.Wage, "Cockpit Wage", 4.11, 3.75, 12.75, 100));
            FeeTypes.AddType(new FeeType(FeeType.EFeeType.Wage, "Maintenance Wage", 3.95, 3.0, 4.25, 100));
            FeeTypes.AddType(new FeeType(FeeType.EFeeType.Wage, "Support Wage", 2.65, 1, 3, 100));
            FeeTypes.AddType(new FeeType(FeeType.EFeeType.Wage, "Cabin Wage", 1.9, 1, 4, 100));
            FeeTypes.AddType(new FeeType(FeeType.EFeeType.Wage, "Instructor Base Salary", 267.00, 200, 300, 100));
            FeeTypes.AddType(new FeeType(FeeType.EFeeType.Wage, "Pilot Base Salary", 133.53, 100, 150, 100));

            //food and drinks
            FeeTypes.AddType(new FeeType(FeeType.EFeeType.FoodDrinks, "Alcholic Drinks", 0.75, 0.5, 1.1, 75));
            FeeTypes.AddType(new FeeType(FeeType.EFeeType.FoodDrinks, "Drinks", 0.2, 0.1, 0.8, 75));
            FeeTypes.AddType(new FeeType(FeeType.EFeeType.FoodDrinks, "Snacks", 0.35, 0.25, 0.5, 70));
            FeeTypes.AddType(new FeeType(FeeType.EFeeType.FoodDrinks, "Meal", 1.40, 1.25, 2, 50));
            FeeTypes.AddType(new FeeType(FeeType.EFeeType.FoodDrinks, "WiFi", 1.4, 0, 6.25, 25, 2007));

            //fees
            FeeTypes.AddType(new FeeType(FeeType.EFeeType.Fee, "1 Bag", 0, 0, 5, 95));
            FeeTypes.AddType(new FeeType(FeeType.EFeeType.Fee, "2 Bags", 0, 0, 5.25, 25));
            FeeTypes.AddType(new FeeType(FeeType.EFeeType.Fee, "3+ Bags", 0, 0, 6, 2));
            FeeTypes.AddType(new FeeType(FeeType.EFeeType.Fee, "Pets", 0, 0, 18, 1));

            //discounts
            FeeTypes.AddType(new FeeType(FeeType.EFeeType.Discount, "Employee Discount", 0, 0, 100, 1));
            FeeTypes.AddType(new FeeType(FeeType.EFeeType.Discount, "Government Discount", 0, 0, 100, 3));
            FeeTypes.AddType(new FeeType(FeeType.EFeeType.Discount, "Military Discount", 0, 0, 100, 1));
        }

        private static void CreateFlightFacilities()
        {
            RouteFacilities.AddFacility(
                new RouteFacility(
                    "100",
                    RouteFacility.FacilityType.Food,
                    "None",
                    -50,
                    RouteFacility.ExpenseType.Fixed,
                    0,
                    null));
            RouteFacilities.AddFacility(
                new RouteFacility(
                    "101",
                    RouteFacility.FacilityType.Food,
                    "Buyable Snacks",
                    10,
                    RouteFacility.ExpenseType.Random,
                    0.10,
                    FeeTypes.GetType("Snacks")));
            RouteFacilities.AddFacility(
                new RouteFacility(
                    "102",
                    RouteFacility.FacilityType.Food,
                    "Free Snacks",
                    15,
                    RouteFacility.ExpenseType.Fixed,
                    0.10,
                    null));
            RouteFacilities.AddFacility(
                new RouteFacility(
                    "103",
                    RouteFacility.FacilityType.Food,
                    "Buyable Meal",
                    25,
                    RouteFacility.ExpenseType.Random,
                    0.50,
                    FeeTypes.GetType("Meal")));
            RouteFacilities.AddFacility(
                new RouteFacility(
                    "104",
                    RouteFacility.FacilityType.Food,
                    "Basic Meal",
                    50,
                    RouteFacility.ExpenseType.Fixed,
                    0.50,
                    null));
            RouteFacilities.AddFacility(
                new RouteFacility(
                    "105",
                    RouteFacility.FacilityType.Food,
                    "Full Dinner",
                    100,
                    RouteFacility.ExpenseType.Fixed,
                    2,
                    null));
            RouteFacilities.AddFacility(
                new RouteFacility(
                    "106",
                    RouteFacility.FacilityType.Drinks,
                    "None",
                    -50,
                    RouteFacility.ExpenseType.Fixed,
                    0,
                    null));
            RouteFacilities.AddFacility(
                new RouteFacility(
                    "107",
                    RouteFacility.FacilityType.Drinks,
                    "Buyable",
                    25,
                    RouteFacility.ExpenseType.Random,
                    0.10,
                    FeeTypes.GetType("Drinks")));
            RouteFacilities.AddFacility(
                new RouteFacility(
                    "108",
                    RouteFacility.FacilityType.Drinks,
                    "Free",
                    80,
                    RouteFacility.ExpenseType.Fixed,
                    0.20,
                    null));
            RouteFacilities.AddFacility(
                new RouteFacility(
                    "109",
                    RouteFacility.FacilityType.AlcoholicDrinks,
                    "None",
                    0,
                    RouteFacility.ExpenseType.Fixed,
                    0,
                    null));
            RouteFacilities.AddFacility(
                new RouteFacility(
                    "110",
                    RouteFacility.FacilityType.AlcoholicDrinks,
                    "Buyable",
                    40,
                    RouteFacility.ExpenseType.Random,
                    0.05,
                    FeeTypes.GetType("Alcholic Drinks")));
            RouteFacilities.AddFacility(
                new RouteFacility(
                    "111",
                    RouteFacility.FacilityType.AlcoholicDrinks,
                    "Free",
                    100,
                    RouteFacility.ExpenseType.Fixed,
                    0.75,
                    null));
            RouteFacilities.AddFacility(
                new RouteFacility(
                    "112",
                    RouteFacility.FacilityType.WiFi,
                    "None",
                    0,
                    RouteFacility.ExpenseType.Fixed,
                    0,
                    null));
            RouteFacilities.AddFacility(
                new RouteFacility(
                    "113",
                    RouteFacility.FacilityType.WiFi,
                    "Buyable",
                    40,
                    RouteFacility.ExpenseType.Random,
                    0.5,
                    FeeTypes.GetType("WiFi"),
                    AirlineFacilities.GetFacility("107")));
            RouteFacilities.AddFacility(
                new RouteFacility(
                    "114",
                    RouteFacility.FacilityType.WiFi,
                    "Free",
                    100,
                    RouteFacility.ExpenseType.Fixed,
                    0.5,
                    null,
                    AirlineFacilities.GetFacility("107")));
            RouteFacilities.AddFacility(
                new RouteFacility(
                    "115",
                    RouteFacility.FacilityType.Magazines,
                    "None",
                    0,
                    RouteFacility.ExpenseType.Fixed,
                    0,
                    null));
            RouteFacilities.AddFacility(
                new RouteFacility(
                    "116",
                    RouteFacility.FacilityType.Magazines,
                    "Available",
                    40,
                    RouteFacility.ExpenseType.Fixed,
                    0,
                    null,
                    AirlineFacilities.GetFacility("101")));
            RouteFacilities.AddFacility(
                new RouteFacility(
                    "117",
                    RouteFacility.FacilityType.Newspapers,
                    "None",
                    0,
                    RouteFacility.ExpenseType.Fixed,
                    0,
                    null));
            RouteFacilities.AddFacility(
                new RouteFacility(
                    "118",
                    RouteFacility.FacilityType.Newspapers,
                    "Available",
                    35,
                    RouteFacility.ExpenseType.Fixed,
                    0,
                    null,
                    AirlineFacilities.GetFacility("102")));
        }

        private static void CreateHubTypes()
        {
            HubTypes.AddHubType(new HubType("Hub", 50000, HubType.TypeOfHub.Hub));
            HubTypes.AddHubType(new HubType("Regional hub", 40000, HubType.TypeOfHub.RegionalHub));
            HubTypes.AddHubType(new HubType("Focus city", 25000, HubType.TypeOfHub.FocusCity));
            HubTypes.AddHubType(new HubType("Fortress hub", 75000, HubType.TypeOfHub.FortressHub));
        }

        private static void CreatePilotRatings()
        {
            var ratingA = new PilotRating("A", 70, 3);
            ratingA.AddAircraft(TrainingAircraftTypes.GetAircraftType("Cessna 172"));
            ratingA.AddAircraft(TrainingAircraftTypes.GetAircraftType("Beechcraft King Air 350"));
            PilotRatings.AddRating(ratingA);

            var ratingB = new PilotRating("B", 85, 4);
            ratingB.AddAircraft(TrainingAircraftTypes.GetAircraftType("Cessna 172"));
            ratingB.AddAircraft(TrainingAircraftTypes.GetAircraftType("Beechcraft King Air 350"));
            PilotRatings.AddRating(ratingB);

            var ratingC = new PilotRating("C", 95, 5);
            ratingC.AddAircraft(TrainingAircraftTypes.GetAircraftType("Beechcraft King Air 350"));
            PilotRatings.AddRating(ratingC);

            var ratingD = new PilotRating("D", 120, 7);
            ratingD.AddAircraft(TrainingAircraftTypes.GetAircraftType("Beechcraft King Air 350"));
            PilotRatings.AddRating(ratingD);

            var ratingE = new PilotRating("E", 150, 10);
            ratingE.AddAircraft(TrainingAircraftTypes.GetAircraftType("Beechcraft King Air 350"));
            PilotRatings.AddRating(ratingE);
        }

        private static void CreatePilots()
        {
            int pilotsPool = 100*Airlines.GetAllAirlines().Count;

            GeneralHelpers.CreatePilots(pilotsPool);

            int instructorsPool = 75*Airlines.GetAllAirlines().Count;

            GeneralHelpers.CreateInstructors(instructorsPool);
        }

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

        private static void CreateTrainingAircraftTypes()
        {
            TrainingAircraftTypes.AddAircraftType(new TrainingAircraftType("Cessna 172", 26705, 2, 1));
            TrainingAircraftTypes.AddAircraftType(new TrainingAircraftType("Beechcraft King Air 350", 129520, 12, 2));
        }

        private static Airport FindComputerHomeBase(Airline airline)
        {
            Terminal.TerminalType terminaltype = airline.AirlineRouteFocus == Route.RouteType.Cargo ? Terminal.TerminalType.Cargo : Terminal.TerminalType.Passenger;
            if (airline.Profile.PreferedAirport != null
                && GeneralHelpers.IsAirportActive(airline.Profile.PreferedAirport)
                && airline.Profile.PreferedAirport.Terminals.GetFreeGates(terminaltype) > 1)
            {
                return airline.Profile.PreferedAirport;
            }
            List<Airport> airports =
                Airports.GetAirports(airline.Profile.Country).FindAll(a => a.Terminals.GetFreeGates(terminaltype) > 1);

            if (airports.Count < 4)
            {
                airports =
                    Airports.GetAirports(airline.Profile.Country.Region).FindAll(a => a.Terminals.GetFreeGates(terminaltype) > 1);
            }

            var list = new Dictionary<Airport, int>();
            airports.ForEach(
                a => list.Add(a, ((int) a.Profile.Size)*(AirportHelpers.GetAirportsNearAirport(a, 1000).Count) + 1));

            return AIHelpers.GetRandomItem(list);
        }

        private static Airport GetStartDataRoutesDestination(StartDataRoutes routes)
        {
            double maxRange =
                (AirlinerTypes.GetTypes(
                    t =>
                    t.Produced.From <= GameObject.GetInstance().GameTime
                    && t.Produced.To > GameObject.GetInstance().GameTime).Max(t => t.Range))*0.8;

            var airports = new List<Airport>();

            if (routes.RouteType == Route.RouteType.Cargo)
            {
                airports =
                    Airports.GetAirports(
                        a =>
                        routes.Countries.Contains(a.Profile.Country)
                        && MathHelpers.GetDistance(Airports.GetAirport(routes.Origin), a) < maxRange
                        && a != Airports.GetAirport(routes.Origin)
                        && ((int) a.Profile.Cargo) >= ((int) routes.MinimumSize) && a.Terminals.GetFreeGates(Terminal.TerminalType.Cargo) > 0);
            }

            if (routes.RouteType == Route.RouteType.Passenger || routes.RouteType == Route.RouteType.Mixed)
            {
                airports =
                    Airports.GetAirports(
                        a =>
                        routes.Countries.Contains(a.Profile.Country)
                        && MathHelpers.GetDistance(Airports.GetAirport(routes.Origin), a) < maxRange
                        && a != Airports.GetAirport(routes.Origin)
                        && ((int) a.Profile.Size) >= ((int) routes.MinimumSize) && a.Terminals.GetFreeGates(Terminal.TerminalType.Passenger) > 0);
            }

            return airports[Rnd.Next(airports.Count)];
        }

        /*!loads the different scenarios
         */

        /*! loads the airline facilities.
         */

        private static void LoadAirlineFacilities()
        {
            var doc = new XmlDocument();
            doc.Load(AppSettings.GetDataPath() + "\\airlinefacilities.xml");
            XmlElement root = doc.DocumentElement;

            XmlNodeList facilitiesList = root?.SelectNodes("//airlinefacility");

            if (facilitiesList != null)
                foreach (XmlElement element in facilitiesList)
                {
                    string section = root.Name;
                    string uid = element.Attributes["uid"].Value;
                    double price = XmlConvert.ToDouble(element.Attributes["price"].Value);
                    double monthlycost = XmlConvert.ToDouble(element.Attributes["monthlycost"].Value);
                    int fromyear = XmlConvert.ToInt16(element.Attributes["fromyear"].Value);

                    var levelElement = (XmlElement) element.SelectSingleNode("level");
                    if (levelElement != null)
                    {
                        int service = Convert.ToInt32(levelElement.Attributes["service"].Value);
                        int luxury = Convert.ToInt32(levelElement.Attributes["luxury"].Value);

                        AirlineFacilities.AddFacility(
                            new AirlineFacility(section, uid, price, monthlycost, fromyear, service, luxury));
                    }

                    if (element.SelectSingleNode("translations") != null)
                    {
                        Translator.GetInstance()
                            .AddTranslation(
                                root.Name,
                                element.Attributes["uid"].Value,
                                element.SelectSingleNode("translations"));
                    }
                }
        }

        private static void LoadAirlineMergers()
        {
            var doc = new XmlDocument();
            doc.Load(AppSettings.GetDataPath() + "\\addons\\airlines\\mergers\\mergers.xml");
            XmlElement root = doc.DocumentElement;

            XmlNodeList mergersList = root?.SelectNodes("//merger");

            if (mergersList != null)
                foreach (XmlElement element in mergersList)
                {
                    string mergerName = element.Attributes["name"].Value;
                    Airline airline1 = Airlines.GetAirline(element.Attributes["airline1"].Value);
                    Airline airline2 = Airlines.GetAirline(element.Attributes["airline2"].Value);
                    var mergerType =
                        (AirlineMerger.MergerType)
                            Enum.Parse(typeof (AirlineMerger.MergerType), element.Attributes["type"].Value);
                    DateTime mergerDate = DateTime.Parse(element.Attributes["date"].Value, new CultureInfo("en-US", false));

                    var merger = new AirlineMerger(mergerName, airline1, airline2, mergerDate, mergerType);

                    if (element.HasAttribute("newname"))
                    {
                        merger.NewName = element.Attributes["newname"].Value;
                    }

                    AirlineMergers.AddAirlineMerger(merger);
                }
        }

        private static AirlinerConfiguration LoadAirlinerConfiguration(XmlElement element)
        {
            string name = element.Attributes["name"].Value;
            string id = element.Attributes["id"].Value;
            int minimumSeats = Convert.ToInt16(element.Attributes["minimumseats"].Value);

            XmlNodeList classesList = element.SelectNodes("classes/class");

            var configuration = new AirlinerConfiguration(name, minimumSeats, true) {ID = id};

            if (classesList != null)
                foreach (XmlElement classElement in classesList)
                {
                    int seating = Convert.ToInt16(classElement.Attributes["seating"].Value);
                    int regularseating = Convert.ToInt16(classElement.Attributes["regularseating"].Value);
                    var classType =
                        (AirlinerClass.ClassType)
                        Enum.Parse(typeof (AirlinerClass.ClassType), classElement.Attributes["type"].Value);

                    var classConf = new AirlinerClassConfiguration(classType, seating, regularseating);
                    foreach (AirlinerFacility.FacilityType facType in Enum.GetValues(typeof (AirlinerFacility.FacilityType)))
                    {
                        string facUid = classElement.Attributes[facType.ToString()].Value;

                        classConf.AddFacility(AirlinerFacilities.GetFacility(facType, facUid));
                    }

                    configuration.AddClassConfiguration(classConf);
                }

            return configuration;
        }

        public static void LoadAirlinerFacilities()
        {
            var doc = new XmlDocument();
            doc.Load(AppSettings.GetDataPath() + "\\airlinerfacilities.xml");
            XmlElement root = doc.DocumentElement;

            XmlNodeList facilitiesList = root?.SelectNodes("//airlinerfacility");
            if (facilitiesList != null)
                foreach (XmlElement element in facilitiesList)
                {
                    string section = root.Name;
                    string uid = element.Attributes["uid"].Value;
                    var type =
                        (AirlinerFacility.FacilityType)
                            Enum.Parse(typeof (AirlinerFacility.FacilityType), element.Attributes["type"].Value);
                    int fromyear = Convert.ToInt16(element.Attributes["fromyear"].Value);

                    var levelElement = (XmlElement) element.SelectSingleNode("level");
                    if (levelElement != null)
                    {
                        int service = Convert.ToInt32(levelElement.Attributes["service"].Value);

                        var seatsElement = (XmlElement) element.SelectSingleNode("seats");
                        if (seatsElement != null)
                        {
                            double seatsPercent = XmlConvert.ToDouble(seatsElement.Attributes["percent"].Value);
                            double seatsPrice = XmlConvert.ToDouble(seatsElement.Attributes["price"].Value);
                            double seatuse = XmlConvert.ToDouble(seatsElement.Attributes["uses"].Value);

                            AirlinerFacilities.AddFacility(
                                new AirlinerFacility(section, uid, type, fromyear, service, seatsPercent, seatsPrice, seatuse));
                        }
                    }

                    if (element.SelectSingleNode("translations") != null)
                    {
                        Translator.GetInstance()
                            .AddTranslation(
                                root.Name,
                                element.Attributes["uid"].Value,
                                element.SelectSingleNode("translations"));
                    }
                }
        }

        private static void LoadAirlinerImages()
        {
            string file = AppSettings.GetDataPath() + "\\graphics\\airlinerimages\\images.xml";

            var doc = new XmlDocument();
            doc.Load(file);
            XmlElement root = doc.DocumentElement;

            XmlNodeList imagesList = root?.SelectNodes("//airlinerimage");

            if (imagesList != null)
                foreach (XmlElement image in imagesList)
                {
                    string imageFile =
                        $"{AppSettings.GetDataPath()}\\graphics\\airlinerimages\\{image.Attributes["image"].Value}.png";

                    string[] types = image.Attributes["types"].Value.Split(',');

                    foreach (string type in types)
                    {
                        AirlinerType airlinerType = AirlinerTypes.GetType(type);

                        if (airlinerType != null)
                        {
                            airlinerType.Image = imageFile;
                        }
                    }
                }
        }

        /*! loads the regions.
         */

        private static void LoadAirlinerTypeConfiguration(string file)
        {
            var doc = new XmlDocument();
            doc.Load(file);
            XmlElement root = doc.DocumentElement;

            XmlNodeList configurationsList = root?.SelectNodes("//configuration");

            if (configurationsList != null)
                foreach (XmlElement element in configurationsList)
                {
                    string name = element.Attributes["name"].Value;
                    string id = element.Attributes["id"].Value;
                    AirlinerType type = AirlinerTypes.GetType(element.Attributes["airliner"].Value);
                    int fromYear = Convert.ToInt16(element.Attributes["yearfrom"].Value);
                    int toYear = Convert.ToInt16(element.Attributes["yearto"].Value);
                    /*airliner="Boeing 747-400" name="Boeing 747-400 (1998)" yearfrom="1998" yearto="2199" id="301"*/

                    XmlNodeList classesList = element.SelectNodes("classes/class");

                    var configuration = new AirlinerTypeConfiguration(
                        name,
                        type,
                        new Period<DateTime>(new DateTime(fromYear, 1, 1), new DateTime(toYear, 12, 31)),
                        true) {ID = id};

                    if (classesList != null)
                        foreach (XmlElement classElement in classesList)
                        {
                            int seating = Convert.ToInt16(classElement.Attributes["seating"].Value);
                            var classType =
                                (AirlinerClass.ClassType)
                                    Enum.Parse(typeof (AirlinerClass.ClassType), classElement.Attributes["type"].Value);

                            var classConf = new AirlinerClassConfiguration(classType, seating, seating);
                            foreach (
                                AirlinerFacility.FacilityType facType in Enum.GetValues(typeof (AirlinerFacility.FacilityType)))
                            {
                                string facUid = classElement.Attributes[facType.ToString()].Value;

                                classConf.AddFacility(AirlinerFacilities.GetFacility(facType, facUid));
                            }

                            configuration.AddClassConfiguration(classConf);
                        }

                    Configurations.AddConfiguration(configuration);
                }
        }

        private static void LoadAirlinerTypeConfigurations()
        {
            try
            {
                var dir = new DirectoryInfo(AppSettings.GetDataPath() + "\\addons\\airliners\\configurations");

                foreach (FileInfo file in dir.GetFiles("*.xml"))
                {
                    LoadAirlinerTypeConfiguration(file.FullName);
                }
            }
            catch (Exception e)
            {
                Logger.Error("Error loading Airliner configurations", e);
            }
        }

        /*!loads the engine types
         */

        /*!loads the airliners.
         */

        private static void LoadAirliners()
        {
            try
            {
                var dir = new DirectoryInfo(AppSettings.GetDataPath() + "\\addons\\airliners");

                foreach (FileInfo file in dir.GetFiles("*.xml"))
                {
                    LoadAirliners(file.FullName);
                }
            }
            catch (Exception e)
            {
                Logger.Error("Error loading airliners", e);
            }
        }

        private static void LoadAirliners(string file)
        {
            string dir = AppSettings.GetDataPath() + "\\graphics\\airlinerimages\\";
            string id = "";
            try
            {
                var doc = new XmlDocument();
                doc.Load(file);
                XmlElement root = doc.DocumentElement;

                XmlNodeList airlinersList = root?.SelectNodes("//airliner");

                if (airlinersList != null)
                    foreach (XmlElement airliner in airlinersList)
                    {
                        AirlinerType.TypeOfAirliner airlinerType = airliner.HasAttribute("type")
                            ? (AirlinerType.TypeOfAirliner)
                                Enum.Parse(typeof (AirlinerType.TypeOfAirliner), airliner.Attributes["type"].Value)
                            : AirlinerType.TypeOfAirliner.Passenger;

                        string manufacturerName = airliner.Attributes["manufacturer"].Value;
                        Manufacturer manufacturer = Manufacturers.GetManufacturer(manufacturerName);

                        string name = airliner.Attributes["name"].Value;

                        string family;

                        if (airliner.HasAttribute("family"))
                        {
                            family = airliner.Attributes["family"].Value;
                        }
                        else
                        {
                            family = name.LastIndexOfAny(new[] {' ', '-'}) > 0 ? name.Substring(0, name.LastIndexOfAny(new[] {' ', '-'})) : name;
                        }

                        bool isConvertable;

                        if (airliner.HasAttribute("convertable"))
                        {
                            isConvertable = Convert.ToBoolean(airliner.Attributes["convertable"].Value);
                        }
                        else
                        {
                            if (airlinerType == AirlinerType.TypeOfAirliner.Cargo
                                || airlinerType == AirlinerType.TypeOfAirliner.Mixed
                                || airlinerType == AirlinerType.TypeOfAirliner.Helicopter)
                            {
                                isConvertable = false;
                            }
                            else
                            {
                                isConvertable = true;
                            }
                        }

                        long price = Convert.ToInt64(airliner.Attributes["price"].Value);

                        id = name;

                        var typeElement = (XmlElement) airliner.SelectSingleNode("type");
                        var body =
                            (AirlinerType.BodyType)
                                Enum.Parse(typeof (AirlinerType.BodyType), typeElement.Attributes["body"].Value);


                        var rangeType =
                            (AirlinerType.TypeRange)
                                Enum.Parse(typeof (AirlinerType.TypeRange), typeElement.Attributes["rangetype"].Value);
                        var engine =
                            (AirlinerType.TypeOfEngine)
                                Enum.Parse(typeof (AirlinerType.TypeOfEngine), typeElement.Attributes["engine"].Value);

                        var specsElement = (XmlElement) airliner.SelectSingleNode("specs");
                        double wingspan = XmlConvert.ToDouble(specsElement.Attributes["wingspan"].Value);
                        double length = XmlConvert.ToDouble(specsElement.Attributes["length"].Value);
                        long range = Convert.ToInt64(specsElement.Attributes["range"].Value);

                        double speed = XmlConvert.ToDouble(specsElement.Attributes["speed"].Value);
                        long fuelcapacity = XmlConvert.ToInt64(specsElement.Attributes["fuelcapacity"].Value);
                        double fuel = XmlConvert.ToDouble(specsElement.Attributes["consumption"].Value);
                        long runwaylenght = XmlConvert.ToInt64(specsElement.Attributes["runwaylengthrequired"].Value);

                        double weight = specsElement.HasAttribute("weight") ? Convert.ToDouble(specsElement.Attributes["weight"].Value) : AirlinerHelpers.GetCalculatedWeight(wingspan, length, fuelcapacity);

                        var capacityElement = (XmlElement) airliner.SelectSingleNode("capacity");

                        var producedElement = (XmlElement) airliner.SelectSingleNode("produced");
                        int fromYear = Convert.ToInt16(producedElement.Attributes["from"].Value);
                        int toYear = Convert.ToInt16(producedElement.Attributes["to"].Value);
                        int prodRate = producedElement.HasAttribute("rate")
                            ? Convert.ToInt16(producedElement.Attributes["rate"].Value)
                            : 10;

                        var from = new DateTime(fromYear, 1, 2);
                        var to = new DateTime(toYear, 12, 31);

                        AirlinerType type = null;

                        if (airlinerType == AirlinerType.TypeOfAirliner.Helicopter)
                        {
                            int passengers = Convert.ToInt16(capacityElement.Attributes["passengers"].Value);
                            int cockpitcrew = Convert.ToInt16(capacityElement.Attributes["cockpitcrew"].Value);
                            int cabincrew = Convert.ToInt16(capacityElement.Attributes["cabincrew"].Value);
                            int maxClasses = Convert.ToInt16(capacityElement.Attributes["maxclasses"].Value);
                            type = new AirlinerPassengerType(
                                manufacturer,
                                name,
                                family,
                                passengers,
                                cockpitcrew,
                                cabincrew,
                                speed,
                                range,
                                wingspan,
                                length,
                                weight,
                                fuel,
                                price,
                                maxClasses,
                                runwaylenght,
                                fuelcapacity,
                                body,
                                rangeType,
                                engine,
                                new Period<DateTime>(@from, to),
                                prodRate,
                                isConvertable) {TypeAirliner = AirlinerType.TypeOfAirliner.Helicopter};
                        }

                        if (airlinerType == AirlinerType.TypeOfAirliner.Passenger)
                        {
                            int passengers = Convert.ToInt16(capacityElement.Attributes["passengers"].Value);
                            int cockpitcrew = Convert.ToInt16(capacityElement.Attributes["cockpitcrew"].Value);
                            int cabincrew = Convert.ToInt16(capacityElement.Attributes["cabincrew"].Value);
                            int maxClasses = Convert.ToInt16(capacityElement.Attributes["maxclasses"].Value);
                            type = new AirlinerPassengerType(
                                manufacturer,
                                name,
                                family,
                                passengers,
                                cockpitcrew,
                                cabincrew,
                                speed,
                                range,
                                wingspan,
                                length,
                                weight,
                                fuel,
                                price,
                                maxClasses,
                                runwaylenght,
                                fuelcapacity,
                                body,
                                rangeType,
                                engine,
                                new Period<DateTime>(@from, to),
                                prodRate,
                                isConvertable);
                        }
                        if (airlinerType == AirlinerType.TypeOfAirliner.Cargo)
                        {
                            int cockpitcrew = Convert.ToInt16(capacityElement.Attributes["cockpitcrew"].Value);
                            double cargo = Convert.ToDouble(
                                capacityElement.Attributes["cargo"].Value,
                                CultureInfo.GetCultureInfo("en-US").NumberFormat);
                            type = new AirlinerCargoType(
                                manufacturer,
                                name,
                                family,
                                cockpitcrew,
                                cargo,
                                speed,
                                range,
                                wingspan,
                                length,
                                weight,
                                fuel,
                                price,
                                runwaylenght,
                                fuelcapacity,
                                body,
                                rangeType,
                                engine,
                                new Period<DateTime>(@from, to),
                                prodRate,
                                isConvertable);
                        }
                        if (airlinerType == AirlinerType.TypeOfAirliner.Mixed)
                        {
                            int passengers = Convert.ToInt16(capacityElement.Attributes["passengers"].Value);
                            int cockpitcrew = Convert.ToInt16(capacityElement.Attributes["cockpitcrew"].Value);
                            double cargo = Convert.ToDouble(
                                capacityElement.Attributes["cargo"].Value,
                                CultureInfo.GetCultureInfo("en-US").NumberFormat);
                            int cabincrew = Convert.ToInt16(capacityElement.Attributes["cabincrew"].Value);
                            int maxClasses = Convert.ToInt16(capacityElement.Attributes["maxclasses"].Value);

                            type = new AirlinerCombiType(
                                manufacturer,
                                name,
                                family,
                                passengers,
                                cockpitcrew,
                                cabincrew,
                                speed,
                                range,
                                wingspan,
                                length,
                                weight,
                                fuel,
                                price,
                                maxClasses,
                                runwaylenght,
                                fuelcapacity,
                                body,
                                rangeType,
                                engine,
                                new Period<DateTime>(@from, to),
                                prodRate,
                                cargo,
                                isConvertable);
                        }

                        //if (airliner.HasAttribute("image") && airliner.Attributes["image"].Value.Length > 1)
                        //type.Image = dir + airliner.Attributes["image"].Value + ".png";

                        if (type != null)
                        {
                            AirlinerTypes.AddType(type);
                        }
                    }
            }
            catch (Exception e)
            {
                Logger.Error("Error loading airliner {0}", id);
                Logger.Error(e);
            }
        }

        private static void LoadAirlines()
        {
            string f = "";
            try
            {
                var dir = new DirectoryInfo(AppSettings.GetDataPath() + "\\addons\\airlines");

                foreach (FileInfo file in dir.GetFiles("*.xml"))
                {
                    f = file.Name;
                    LoadAirline(file.FullName);
                }
                GameObject.GetInstance().SetHumanAirline(Airlines.GetAllAirlines()[0]);
                GameObject.GetInstance().MainAirline = GameObject.GetInstance().HumanAirline;

                CreateAirlineLogos();
            }
            catch (Exception e)
            {
                //var file = new StreamWriter(
                //    AppSettings.GetCommonApplicationDataPath() + "\\theairlinestartup.log",
                //    true);
                //file.WriteLine("Airlines failing: " + f);
                //file.WriteLine(e.ToString());
                //file.WriteLine(e.StackTrace);
                //file.Close();
                Logger.Error("Error loading airline: {0}", f);
                Logger.Error(e);
            }
        }

        private static void LoadAirportFacilities()
        {
            var doc = new XmlDocument();
            doc.Load(AppSettings.GetDataPath() + "\\airportfacilities.xml");
            XmlElement root = doc.DocumentElement;

            XmlNodeList facilitiesList = root?.SelectNodes("//airportfacility");

            if (facilitiesList != null)
                foreach (XmlElement element in facilitiesList)
                {
                    string section = root.Name;
                    string uid = element.Attributes["uid"].Value;
                    string shortname = element.Attributes["shortname"].Value;
                    var type =
                        (AirportFacility.FacilityType)
                            Enum.Parse(typeof (AirportFacility.FacilityType), element.Attributes["type"].Value);
                    int typeLevel = Convert.ToInt16(element.Attributes["typelevel"].Value);

                    double price = XmlConvert.ToDouble(element.Attributes["price"].Value);
                    int buildingDays = XmlConvert.ToInt32(element.Attributes["buildingdays"].Value);

                    var levelElement = (XmlElement) element.SelectSingleNode("level");
                    if (levelElement != null)
                    {
                        int service = Convert.ToInt32(levelElement.Attributes["service"].Value);
                        int luxury = Convert.ToInt32(levelElement.Attributes["luxury"].Value);

                        var facility = new AirportFacility(
                            section,
                            uid,
                            shortname,
                            type,
                            buildingDays,
                            typeLevel,
                            price,
                            service,
                            luxury);

                        AirportFacilities.AddFacility(facility);

                        var employeesElement = (XmlElement) element.SelectSingleNode("employees");

                        if (employeesElement != null)
                        {
                            var employeestype =
                                (AirportFacility.EmployeeTypes)
                                    Enum.Parse(typeof (AirportFacility.EmployeeTypes), employeesElement.Attributes["type"].Value);
                            int numberofemployees = Convert.ToInt16(employeesElement.Attributes["numberofemployees"].Value);

                            facility.EmployeeType = employeestype;
                            facility.NumberOfEmployees = numberofemployees;
                        }
                    }

                    if (element.SelectSingleNode("translations") != null)
                    {
                        Translator.GetInstance()
                            .AddTranslation(
                                root.Name,
                                element.Attributes["uid"].Value,
                                element.SelectSingleNode("translations"));
                    }
                }
        }

        private static void LoadAirportLogos()
        {
            var dir = new DirectoryInfo(AppSettings.GetDataPath() + "\\graphics\\airportlogos");

            foreach (FileInfo file in dir.GetFiles("*.png"))
            {
                string code = file.Name.Split('.')[0].ToUpper();
                Airport airport = Airports.GetAirport(code);

                if (airport != null)
                {
                    airport.Profile.Logo = file.FullName;
                }
                else
                {
                    Console.WriteLine(@"The logo {0} doesn't match any airports", code);
                }
            }
        }

        private static void LoadAirportMaps()
        {
            var dir = new DirectoryInfo(AppSettings.GetDataPath() + "\\graphics\\airportmaps");

            foreach (FileInfo file in dir.GetFiles("*.png"))
            {
                string code = file.Name.Split('.')[0].ToUpper();
                Airport airport = Airports.GetAirport(code);

                if (airport != null)
                {
                    airport.Profile.Map = file.FullName;
                }
                else
                {
                    code = "x";
                }
            }
        }

        /*!loads the airliner images
         */

        private static void LoadAirports(string filename)
        {
            string id = "";
            try
            {
                var doc = new XmlDocument();
                doc.Load(filename);
                XmlElement root = doc.DocumentElement;

                XmlNodeList airportsList = root?.SelectNodes("//airport");

                if (airportsList != null)
                    foreach (XmlElement airportElement in airportsList)
                    {
                        string name = airportElement.Attributes["name"].Value;
                        string icao = airportElement.Attributes["icao"].Value;
                        string iata = airportElement.Attributes["iata"].Value;

                        id = name + " iata: " + iata;

                        var type =
                            (AirportProfile.AirportType)
                                Enum.Parse(typeof (AirportProfile.AirportType), airportElement.Attributes["type"].Value);
                        var season =
                            (Weather.Season) Enum.Parse(typeof (Weather.Season), airportElement.Attributes["season"].Value);

                        var periodElement = (XmlElement) airportElement.SelectSingleNode("period");

                        Period<DateTime> airportPeriod;
                        if (periodElement != null)
                        {
                            DateTime airportFrom = Convert.ToDateTime(
                                periodElement.Attributes["from"].Value,
                                new CultureInfo("en-US", false));
                            DateTime airportTo = Convert.ToDateTime(
                                periodElement.Attributes["to"].Value,
                                new CultureInfo("en-US", false));

                            airportPeriod = new Period<DateTime>(airportFrom, airportTo);
                        }
                        else
                        {
                            airportPeriod = new Period<DateTime>(new DateTime(1959, 12, 31), new DateTime(2199, 12, 31));
                        }

                        var townElement = (XmlElement) airportElement.SelectSingleNode("town");
                        string town = townElement.Attributes["town"].Value;
                        string country = townElement.Attributes["country"].Value;
                        TimeSpan gmt = TimeSpan.Parse(townElement.Attributes["GMT"].Value);
                        TimeSpan dst = TimeSpan.Parse(townElement.Attributes["DST"].Value);

                        var latitudeElement = (XmlElement) airportElement.SelectSingleNode("coordinates/latitude");
                        var longitudeElement = (XmlElement) airportElement.SelectSingleNode("coordinates/longitude");
                        string[] latitude = latitudeElement.Attributes["value"].Value.Split(
                            new[] {'°', '\''},
                            StringSplitOptions.RemoveEmptyEntries);
                        string[] longitude =
                            longitudeElement.Attributes["value"].Value.Split(
                                new[] {'°', '\''},
                                StringSplitOptions.RemoveEmptyEntries);
                        var coords = new int[6];

                        //latitude
                        coords[0] = int.Parse(latitude[0]);
                        coords[1] = int.Parse(latitude[1]);
                        coords[2] = int.Parse(latitude[2]);

                        if (latitude[3] == "S")
                        {
                            coords[0] = -coords[0];
                        }

                        //longitude
                        coords[3] = int.Parse(longitude[0]);
                        coords[4] = int.Parse(longitude[1]);
                        coords[5] = int.Parse(longitude[2]);

                        if (longitude[3] == "W")
                        {
                            coords[3] = -coords[3];
                        }

                        /*
                    foreach(string l in latitude ) 
                    {
                        //int.TryParse(l, out coords[c]);
                        coords[c] = int.Parse(l);
                        c++;
                    }
                    c = 3;
					
                    foreach (string l in longitude)
                    {
                        //int.TryParse(l, out coords[c]);
                        coords[c] = int.Parse(l);
					
                        c++;
                    }*/

                        //cleaning up

                        //GeoCoordinate pos = new GeoCoordinate(MathHelpers.DMStoDeg(coords[0], coords[1], coords[2]),MathHelpers.DMStoDeg(coords[3],coords[4],coords[5]));
                        var pos = new Coordinates(
                            new Coordinate(coords[0], coords[1], coords[2]),
                            new Coordinate(coords[3], coords[4], coords[5]));

                        //double longitude = Coordinate.Parse(longitudeElement.Attributes["value"].Value);

                        var sizeElement = (XmlElement) airportElement.SelectSingleNode("size");

                        var paxValues = new List<PaxValue>();

                        if (!sizeElement.HasChildNodes)
                        {
                            var size =
                                (GeneralHelpers.Size)
                                    Enum.Parse(typeof (GeneralHelpers.Size), sizeElement.Attributes["value"].Value);
                            int pax = sizeElement.HasAttribute("pax")
                                ? Convert.ToInt32(sizeElement.Attributes["pax"].Value)
                                : 0;

                            paxValues.Add(new PaxValue(airportPeriod.From.Year, airportPeriod.To.Year, size, pax));
                        }
                        else
                        {
                            XmlNodeList yearsList = sizeElement.SelectNodes("yearvalues/yearvalue");

                            foreach (XmlElement yearElement in yearsList)
                            {
                                int fromYear = Convert.ToInt16(yearElement.Attributes["from"].Value);
                                int toYear = Convert.ToInt16(yearElement.Attributes["to"].Value);
                                var size =
                                    (GeneralHelpers.Size)
                                        Enum.Parse(typeof (GeneralHelpers.Size), yearElement.Attributes["value"].Value);
                                int pax = Convert.ToInt32(yearElement.Attributes["pax"].Value);

                                var paxValue = new PaxValue(fromYear, toYear, size, pax);

                                if (yearElement.HasAttribute("inflationafter"))
                                {
                                    paxValue.InflationAfterYear =
                                        Convert.ToDouble(
                                            yearElement.Attributes["inflationafter"].Value,
                                            CultureInfo.GetCultureInfo("en-US").NumberFormat);
                                }
                                if (yearElement.HasAttribute("inflationbefore"))
                                {
                                    paxValue.InflationBeforeYear =
                                        Convert.ToDouble(
                                            yearElement.Attributes["inflationbefore"].Value,
                                            CultureInfo.GetCultureInfo("en-US").NumberFormat);
                                }

                                paxValues.Add(paxValue);
                            }
                        }

                        GeneralHelpers.Size cargoSize;
                        double cargovolume = sizeElement.HasAttribute("cargovolume")
                            ? Convert.ToDouble(
                                sizeElement.Attributes["cargovolume"].Value,
                                CultureInfo.GetCultureInfo("en-US").NumberFormat)
                            : 0;

                        if (sizeElement.HasAttribute("cargo"))
                        {
                            cargoSize =
                                (GeneralHelpers.Size)
                                    Enum.Parse(typeof (GeneralHelpers.Size), sizeElement.Attributes["cargo"].Value);
                        }
                        else
                        {
                            //calculates the cargo size
                            var cargoSizes = (GeneralHelpers.Size[]) Enum.GetValues(typeof (GeneralHelpers.Size));

                            int i = 0;

                            var list = new Dictionary<GeneralHelpers.Size, int>();

                            while (i < cargoSizes.Length && cargoSizes[i] <= paxValues.First().Size)
                            {
                                list.Add(cargoSizes[i], 10 - i);
                                i++;
                            }

                            cargoSize = AIHelpers.GetRandomItem(list);
                        }

                        Town eTown;
                        if (town.Contains(","))
                        {
                            State state = States.GetState(Countries.GetCountry(country), town.Split(',')[1].Trim());

                            eTown = state == null ? new Town(town.Split(',')[0], Countries.GetCountry(country)) : new Town(town.Split(',')[0], Countries.GetCountry(country), state);
                        }
                        else
                        {
                            eTown = new Town(town, Countries.GetCountry(country));
                        }

                        var profile = new AirportProfile(
                            name,
                            iata,
                            icao,
                            type,
                            airportPeriod,
                            eTown,
                            gmt,
                            dst,
                            pos,
                            cargoSize,
                            cargovolume,
                            season) {PaxValues = paxValues};

                        var airport = new Airport(profile);

                        var destinationsElement = (XmlElement) airportElement.SelectSingleNode("destinations");

                        if (destinationsElement != null)
                        {
                            XmlNodeList majorDestinationsList = destinationsElement.SelectNodes("destination");

                            var majorDestinations = new Dictionary<string, int>();

                            foreach (XmlElement majorDestinationNode in majorDestinationsList)
                            {
                                string majorDestination = majorDestinationNode.Attributes["airport"].Value;
                                int majorDestinationPax = Convert.ToInt32(majorDestinationNode.Attributes["pax"].Value);

                                majorDestinations.Add(majorDestination, majorDestinationPax);
                            }

                            airport.Profile.MajorDestionations = majorDestinations;
                        }

                        XmlNodeList terminalList = airportElement.SelectNodes("terminals/terminal");

                        foreach (XmlElement terminalNode in terminalList)
                        {
                            string terminalName = terminalNode.Attributes["name"].Value;
                            int terminalGates = XmlConvert.ToInt32(terminalNode.Attributes["gates"].Value);

                            Terminal.TerminalType terminalType;

                            if (terminalNode.HasAttribute("type"))
                                terminalType =
                                    (Terminal.TerminalType)
                                        Enum.Parse(typeof (Terminal.TerminalType), terminalNode.Attributes["type"].Value);
                            else
                                terminalType = Terminal.TerminalType.Passenger;

                            airport.Terminals.AddTerminal(
                                new Terminal(airport, null, terminalName, terminalGates, new DateTime(1950, 1, 1), terminalType));
                        }

                        XmlNodeList runwaysList = airportElement.SelectNodes("runways/runway");

                        foreach (XmlElement runwayNode in runwaysList)
                        {
                            string runwayName = runwayNode.Attributes["name"].Value;
                            long runwayLength = XmlConvert.ToInt32(runwayNode.Attributes["length"].Value);
                            var surface =
                                (Runway.SurfaceType)
                                    Enum.Parse(typeof (Runway.SurfaceType), runwayNode.Attributes["surface"].Value);

                            var runwayType = Runway.RunwayType.Regular;

                            if (runwayNode.HasAttribute("type"))
                            {
                                runwayType =
                                    (Runway.RunwayType)
                                        Enum.Parse(typeof (Runway.RunwayType), runwayNode.Attributes["type"].Value);
                            }

                            airport.Runways.Add(
                                new Runway(runwayName, runwayLength, runwayType, surface, new DateTime(1900, 1, 1), true));
                        }

                        XmlNodeList expansionsList = airportElement.SelectNodes("expansions/expansion");

                        foreach (XmlElement expansionNode in expansionsList)
                        {
                            var expansionType =
                                (AirportExpansion.ExpansionType)
                                    Enum.Parse(typeof (AirportExpansion.ExpansionType), expansionNode.Attributes["type"].Value);

                            DateTime expansionDate = Convert.ToDateTime(
                                expansionNode.Attributes["date"].Value,
                                new CultureInfo("en-US", false));

                            bool expansionNotify = Convert.ToBoolean(expansionNode.Attributes["notify"].Value);

                            var expansion = new AirportExpansion(expansionType, expansionDate, expansionNotify);

                            if (expansionType == AirportExpansion.ExpansionType.Name)
                            {
                                string expansionName = expansionNode.Attributes["name"].Value;

                                expansion.Name = expansionName;
                            }
                            if (expansionType == AirportExpansion.ExpansionType.RunwayLength)
                            {
                                string expansionName = expansionNode.Attributes["name"].Value;
                                long length = Convert.ToInt64(expansionNode.Attributes["length"].Value);

                                expansion.Name = expansionName;
                                expansion.Length = length;
                            }
                            if (expansionType == AirportExpansion.ExpansionType.NewRunway)
                            {
                                string expansionName = expansionNode.Attributes["name"].Value;
                                long length = Convert.ToInt64(expansionNode.Attributes["length"].Value);

                                var surface =
                                    (Runway.SurfaceType)
                                        Enum.Parse(typeof (Runway.SurfaceType), expansionNode.Attributes["surface"].Value);

                                expansion.Name = expansionName;
                                expansion.Length = length;
                                expansion.Surface = surface;
                            }

                            if (expansionType == AirportExpansion.ExpansionType.NewTerminal)
                            {
                                string expansionName = expansionNode.Attributes["name"].Value;
                                int gates = Convert.ToInt16(expansionNode.Attributes["gates"].Value);

                                Terminal.TerminalType terminalType;

                                if (expansionNode.HasAttribute("terminaltype"))
                                    terminalType =
                                        (Terminal.TerminalType)
                                            Enum.Parse(typeof (Terminal.TerminalType), expansionNode.Attributes["terminaltype"].Value);
                                else
                                    terminalType = Terminal.TerminalType.Passenger;

                                expansion.Name = expansionName;
                                expansion.Gates = gates;
                                expansion.TerminalType = terminalType;
                            }
                            if (expansionType == AirportExpansion.ExpansionType.ExtraGates)
                            {
                                string expansionName = expansionNode.Attributes["name"].Value;
                                int gates = Convert.ToInt16(expansionNode.Attributes["gates"].Value);

                                expansion.Name = expansionName;
                                expansion.Gates = gates;
                            }
                            if (expansionType == AirportExpansion.ExpansionType.CloseTerminal)
                            {
                                string expansionName = expansionNode.Attributes["name"].Value;
                                int gates = Convert.ToInt16(expansionNode.Attributes["gates"].Value);

                                expansion.Name = expansionName;
                                expansion.Gates = gates;
                            }
                            airport.Profile.AddExpansion(expansion);
                        }

                        //30.06.14: Added for loading of landing fees

                        double landingFee = airportElement.HasAttribute("landingfee") ? Convert.ToDouble(airportElement.Attributes["landingfee"].Value, new CultureInfo("en-US", false)) : AirportHelpers.GetStandardLandingFee(airport);

                        airport.LandingFee = landingFee;

                        if (Airports.GetAirport(a => a.Profile.ID == airport.Profile.ID) == null)
                        {
                            Airports.AddAirport(airport);
                        }
                    }
            }
            catch (Exception e)
            {
                /*
                System.IO.StreamWriter file = new System.IO.StreamWriter(AppSettings.getCommonApplicationDataPath() + "\\theairlinestartup.log", true);
                file.WriteLine("Airport failing: " + id);
                file.WriteLine(e.ToString());
                file.WriteLine(e.StackTrace);
                file.Close();
                 * */

                Logger.Error("Airport {0} failing to load", id);
                Logger.Error(e);
            }
        }

        private static void LoadAlliance(string path)
        {
            var doc = new XmlDocument();
            doc.Load(path);
            XmlElement root = doc.DocumentElement;

            try
            {
                if (root != null)
                {
                    string allianceName = root.Attributes["name"].Value;
                    string logo = AppSettings.GetDataPath() + "\\graphics\\alliancelogos\\" + root.Attributes["logo"].Value
                                  + ".png";
                    DateTime formationDate = Convert.ToDateTime(
                        root.Attributes["formation"].Value,
                        new CultureInfo("en-US", false));

                    Airport headquarter = Airports.GetAirport(root.Attributes["headquarter"].Value);

                    var alliance = new Alliance(formationDate, allianceName, headquarter) {Logo = logo};

                    XmlNodeList membersList = root.SelectNodes("members/member");

                    if (membersList != null)
                        foreach (XmlElement memberNode in membersList)
                        {
                            Airline memberAirline = Airlines.GetAirline(memberNode.Attributes["airline"].Value);
                            DateTime joinedDate = Convert.ToDateTime(
                                memberNode.Attributes["joined"].Value,
                                new CultureInfo("en-US", false));

                            var member = new AllianceMember(memberAirline, joinedDate);
                            if (memberNode.HasAttribute("exited"))
                            {
                                member.ExitedDate = Convert.ToDateTime(
                                    memberNode.Attributes["exited"].Value,
                                    new CultureInfo("en-US", false));
                            }

                            alliance.AddMember(member);
                        }

                    Alliances.AddAlliance(alliance);
                }
            }
            catch (Exception e)
            {
                /*
                System.IO.StreamWriter file = new System.IO.StreamWriter(AppSettings.getCommonApplicationDataPath() + "\\theairlinestartup.log", true);
                file.WriteLine("Alliance failing: " + path);
                file.WriteLine(e.ToString());
                file.WriteLine(e.StackTrace);
			   
                file.Close();
                 * */

                Logger.Error("Alliance {0} failing to load", path);
                Logger.Error(e);
            }
        }

        private static void LoadAlliances()
        {
            var dir = new DirectoryInfo(AppSettings.GetDataPath() + "\\addons\\alliances");

            foreach (FileInfo file in dir.GetFiles("*.xml"))
            {
                LoadAlliance(file.FullName);
            }
        }

        private static void LoadCooperations()
        {
            var doc = new XmlDocument();
            doc.Load(AppSettings.GetDataPath() + "\\airlinecooperations.xml");
            XmlElement root = doc.DocumentElement;

            XmlNodeList cooperationsList = root?.SelectNodes("//cooperation");

            if (cooperationsList != null)
                foreach (XmlElement element in cooperationsList)
                {
                    string section = root.Name;
                    string uid = element.Attributes["uid"].Value;
                    double price = Convert.ToDouble(
                        element.Attributes["price"].Value,
                        CultureInfo.GetCultureInfo("en-US").NumberFormat);
                    int fromyear = Convert.ToInt16(element.Attributes["fromyear"].Value);
                    double monthlyprice = Convert.ToDouble(
                        element.Attributes["monthlyprice"].Value,
                        CultureInfo.GetCultureInfo("en-US").NumberFormat);
                    int servicelevel = Convert.ToInt16(element.Attributes["servicelevel"].Value);
                    double incomeperpax = Convert.ToDouble(
                        element.Attributes["incomeperpax"].Value,
                        CultureInfo.GetCultureInfo("en-US").NumberFormat);
                    var minsize =
                        (GeneralHelpers.Size) Enum.Parse(typeof (GeneralHelpers.Size), element.Attributes["minsize"].Value);

                    var type = new CooperationType(
                        section,
                        uid,
                        minsize,
                        fromyear,
                        price,
                        monthlyprice,
                        servicelevel,
                        incomeperpax);
                    CooperationTypes.AddCooperationType(type);
                    /*uid="101" price="250000" fromyear="1980" monthlyprice="10000" servicelevel="50" incomepercent="3"*/

                    if (element.SelectSingleNode("translations") != null)
                    {
                        Translator.GetInstance()
                            .AddTranslation(
                                root.Name,
                                element.Attributes["uid"].Value,
                                element.SelectSingleNode("translations"));
                    }
                }
        }

        private static void LoadCountries()
        {
            var territoryElements = new List<XmlElement>();

            var doc = new XmlDocument();
            doc.Load(AppSettings.GetDataPath() + "\\countries.xml");
            XmlElement root = doc.DocumentElement;

            XmlNodeList countriesList = root?.SelectNodes("//country");
            if (countriesList != null)
                foreach (XmlElement element in countriesList)
                {
                    var territoryElement = (XmlElement) element.SelectSingleNode("territoryof");

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

                        var country = new Country(section, uid, shortname, region, tailformat) {Flag = AppSettings.GetDataPath() + "\\graphics\\flags\\" + flag + ".png"};

                        Countries.AddCountry(country);

                        XmlNodeList currenciesList = element.SelectNodes("currency");

                        if (currenciesList != null)
                            foreach (XmlElement currencyElement in currenciesList)
                            {
                                string currencySymbol = currencyElement.Attributes["symbol"].Value;
                                double currencyRate = Convert.ToDouble(
                                    currencyElement.Attributes["rate"].Value,
                                    CultureInfo.GetCultureInfo("en-US").NumberFormat);
                                var currencyPosition =
                                    (CountryCurrency.CurrencyPosition)
                                        Enum.Parse(
                                            typeof (CountryCurrency.CurrencyPosition),
                                            currencyElement.Attributes["position"].Value);

                                var currencyFromDate = new DateTime(1900, 1, 1);
                                var currencyToDate = new DateTime(2199, 12, 31);

                                if (currencyElement.HasAttribute("from"))
                                {
                                    currencyFromDate = Convert.ToDateTime(currencyElement.Attributes["from"].Value);
                                }

                                if (currencyElement.HasAttribute("to"))
                                {
                                    currencyToDate = Convert.ToDateTime(currencyElement.Attributes["to"].Value);
                                }

                                country.AddCurrency(
                                    new CountryCurrency(
                                        currencyFromDate,
                                        currencyToDate,
                                        currencySymbol,
                                        currencyPosition,
                                        currencyRate));
                            }

                        if (element.SelectSingleNode("translations") != null)
                        {
                            Translator.GetInstance()
                                .AddTranslation(
                                    root.Name,
                                    element.Attributes["uid"].Value,
                                    element.SelectSingleNode("translations"));
                        }
                    }
                }
            //reads all countries which is a territory for another
            foreach (XmlElement element in territoryElements)
            {
                if (root != null)
                {
                    string section = root.Name;
                    string uid = element.Attributes["uid"].Value;
                    string shortname = element.Attributes["shortname"].Value;
                    string flag = element.Attributes["flag"].Value;
                    Region region = Regions.GetRegion(element.Attributes["region"].Value);
                    string tailformat = element.Attributes["tailformat"].Value;

                    var territoryElement = (XmlElement) element.SelectSingleNode("territoryof");

                    if (territoryElement != null)
                    {
                        Country territoryOf = Countries.GetCountry(territoryElement.Attributes["uid"].Value);

                        Country country = new TerritoryCountry(section, uid, shortname, region, tailformat, territoryOf);

                        country.Flag = AppSettings.GetDataPath() + "\\graphics\\flags\\" + flag + ".png";
                        Countries.AddCountry(country);
                    }
                }

                    if (element.SelectSingleNode("translations") != null)
                    {
                        if (root != null)
                            Translator.GetInstance()
                                      .AddTranslation(
                                          root.Name,
                                          element.Attributes["uid"].Value,
                                          element.SelectSingleNode("translations"));
                    }
            }
        }

        private static void LoadEngineTypes()
        {
            var dir = new DirectoryInfo(AppSettings.GetDataPath() + "\\addons\\engines");

            foreach (FileInfo file in dir.GetFiles("*.xml"))
            {
                LoadEngineTypes(file.FullName);
            }
        }

        private static void LoadEngineTypes(string file)
        {
            var doc = new XmlDocument();
            doc.Load(file);

            XmlElement root = doc.DocumentElement;

            XmlNodeList enginesList = root?.SelectNodes("//engine");

            if (enginesList != null)
                foreach (XmlElement engineElement in enginesList)
                {
                    string manufacturerName = engineElement.Attributes["manufacturer"].Value;
                    Manufacturer manufacturer = Manufacturers.GetManufacturer(manufacturerName);

                    string name = engineElement.Attributes["model"].Value;

                    var specsElement = (XmlElement) engineElement.SelectSingleNode("specs");
                    if (specsElement != null)
                    {
                        var engineType =
                            (EngineType.TypeOfEngine)
                                Enum.Parse(typeof (EngineType.TypeOfEngine), specsElement.Attributes["type"].Value);
                        var noiseLevel =
                            (EngineType.NoiseLevel)
                                Enum.Parse(typeof (EngineType.NoiseLevel), specsElement.Attributes["noise"].Value);
                        double consumption = Convert.ToDouble(
                            specsElement.Attributes["consumptionModifier"].Value,
                            CultureInfo.GetCultureInfo("en-US").NumberFormat);
                        long price = Convert.ToInt64(specsElement.Attributes["price"].Value);

                        var perfElement = (XmlElement) engineElement.SelectSingleNode("performance");
                        if (perfElement != null)
                        {
                            int speed = Convert.ToInt32(perfElement.Attributes["maxspeed"].Value);
                            int ceiling = Convert.ToInt32(perfElement.Attributes["ceiling"].Value);
                            double runway = Convert.ToDouble(
                                perfElement.Attributes["runwaylengthrequiredModifier"].Value,
                                CultureInfo.GetCultureInfo("en-US").NumberFormat);
                            double range = Convert.ToDouble(
                                perfElement.Attributes["rangeModifier"].Value,
                                CultureInfo.GetCultureInfo("en-US").NumberFormat);

                            var producedElement = (XmlElement) engineElement.SelectSingleNode("produced");
                            if (producedElement != null)
                            {
                                int from = Convert.ToInt16(producedElement.Attributes["from"].Value);
                                int to = Convert.ToInt16(producedElement.Attributes["to"].Value);

                                var aircraftElement = (XmlElement) engineElement.SelectSingleNode("aircraft");
                                if (aircraftElement != null)
                                {
                                    string modelsElement = aircraftElement.Attributes["models"].Value;

                                    var engine = new EngineType(
                                        name,
                                        manufacturer,
                                        engineType,
                                        noiseLevel,
                                        consumption,
                                        price,
                                        speed,
                                        ceiling,
                                        runway,
                                        range,
                                        new Period<int>(@from, to));

                                    string[] models = modelsElement.Split(',');

                                    foreach (string model in models)
                                    {
                                        AirlinerType airlinerType = AirlinerTypes.GetAllTypes().FirstOrDefault(a => a.Name == model.Trim());

                                        if (airlinerType != null)
                                        {
                                            engine.AddAirlinerType(airlinerType);
                                        }
                                        else
                                            Console.WriteLine(@"Missing airliner type for engine: " + model);
                                    }

                                    EngineTypes.AddEngineType(engine);
                                }
                            }
                        }
                    }
                }
        }

        private static void LoadFlightRestrictions()
        {
            var doc = new XmlDocument();
            doc.Load(AppSettings.GetDataPath() + "\\flightrestrictions.xml");
            XmlElement root = doc.DocumentElement;

            XmlNodeList restrictionsList = root?.SelectNodes("//restriction");
            if (restrictionsList != null)
                foreach (XmlElement element in restrictionsList)
                {
                    var type =
                        (FlightRestriction.RestrictionType)
                            Enum.Parse(typeof (FlightRestriction.RestrictionType), element.Attributes["type"].Value);

                    DateTime startDate = Convert.ToDateTime(
                        element.Attributes["start"].Value,
                        new CultureInfo("en-US", false));
                    DateTime endDate = Convert.ToDateTime(element.Attributes["end"].Value, new CultureInfo("en-US", false));

                    var countriesElement = (XmlElement) element.SelectSingleNode("countries");

                    BaseUnit from = null, to = null;

                    if (countriesElement != null && countriesElement.Attributes["fromtype"].Value == "C")
                    {
                        @from = Countries.GetCountry(countriesElement.Attributes["from"].Value);
                    }
                    else
                    {
                        if (countriesElement != null) @from = Unions.GetUnion(countriesElement.Attributes["from"].Value);
                    }

                    if (countriesElement != null && countriesElement.Attributes["totype"].Value == "C")
                    {
                        to = Countries.GetCountry(countriesElement.Attributes["to"].Value);
                    }
                    else
                    {
                        if (countriesElement != null) to = Unions.GetUnion(countriesElement.Attributes["to"].Value);
                    }

                    FlightRestrictions.AddRestriction(new FlightRestriction(type, startDate, endDate, @from, to));
                }
        }

        private static void LoadHistoricEvent(string filename)
        {
            var doc = new XmlDocument();
            doc.Load(filename);
            XmlElement root = doc.DocumentElement;

            if (root != null)
            {
                string name = root.Attributes["name"].Value;
                string text = root.Attributes["text"].Value;
                DateTime eventDate = Convert.ToDateTime(root.Attributes["date"].Value, new CultureInfo("en-US", false));

                var historicEvent = new HistoricEvent(name, text, eventDate);

                XmlNodeList influencesList = root.SelectNodes("influences/influence");

                if (influencesList != null)
                    foreach (XmlElement influenceElement in influencesList)
                    {
                        var type =
                            (HistoricEventInfluence.InfluenceType)
                            Enum.Parse(
                                typeof (HistoricEventInfluence.InfluenceType),
                                influenceElement.Attributes["type"].Value);
                        double value = Convert.ToDouble(
                            influenceElement.Attributes["value"].Value,
                            CultureInfo.GetCultureInfo("en-US").NumberFormat);
                        DateTime endDate = Convert.ToDateTime(
                            influenceElement.Attributes["enddate"].Value,
                            new CultureInfo("en-US", false));

                        historicEvent.AddInfluence(new HistoricEventInfluence(type, value, endDate));
                    }

                HistoricEvents.AddHistoricEvent(historicEvent);
            }
        }

        private static void LoadHistoricEvents()
        {
            var dir = new DirectoryInfo(AppSettings.GetDataPath() + "\\addons\\historicevents");

            foreach (FileInfo file in dir.GetFiles("*.xml"))
            {
                LoadHistoricEvent(file.FullName);
            }
        }

        private static void LoadHolidays()
        {
            string id = "";
            try
            {
                var doc = new XmlDocument();
                doc.Load(AppSettings.GetDataPath() + "\\holidays.xml");
                XmlElement root = doc.DocumentElement;

                XmlNodeList holidaysList = root?.SelectNodes("//holiday");

                if (holidaysList != null)
                    foreach (XmlElement element in holidaysList)
                    {
                        string uid = element.Attributes["uid"].Value;
                        string name = element.Attributes["name"].Value;

                        id = name;

                        var type =
                            (Holiday.HolidayType) Enum.Parse(typeof (Holiday.HolidayType), element.Attributes["type"].Value);
                        var traveltype =
                            (Holiday.TravelType)
                                Enum.Parse(typeof (Holiday.TravelType), element.Attributes["holidaytype"].Value);

                        var countries = new List<Country>();

                        XmlNodeList countriesList = element.SelectNodes("observers/observer");

                        if (countriesList != null)
                            countries.AddRange(from XmlElement eCountry in countriesList select Countries.GetCountry(eCountry.Attributes["country"].Value));

                        var dateElement = (XmlElement) element.SelectSingleNode("observationdate");

                        if (type == Holiday.HolidayType.FixedDate)
                        {
                            if (dateElement != null)
                            {
                                int month = Convert.ToInt16(dateElement.Attributes["month"].Value);
                                int day = Convert.ToInt16(dateElement.Attributes["day"].Value);

                                var date = new DateTime(1900, month, day);

                                foreach (Country country in countries)
                                {
                                    var holiday = new Holiday(root.Name, uid, type, name, traveltype, country) {Date = date};

                                    Holidays.AddHoliday(holiday);
                                }
                            }
                        }
                        if (type == Holiday.HolidayType.FixedMonth)
                        {
                            if (dateElement != null)
                            {
                                int month = Convert.ToInt16(dateElement.Attributes["month"].Value);

                                foreach (Country country in countries)
                                {
                                    var holiday = new Holiday(root.Name, uid, type, name, traveltype, country) {Month = month};

                                    Holidays.AddHoliday(holiday);
                                }
                            }
                        }
                        if (type == Holiday.HolidayType.FixedWeek)
                        {
                            if (dateElement != null)
                            {
                                int week = Convert.ToInt16(dateElement.Attributes["week"].Value);

                                foreach (Country country in countries)
                                {
                                    var holiday = new Holiday(root.Name, uid, type, name, traveltype, country) {Week = week};

                                    Holidays.AddHoliday(holiday);
                                }
                            }
                        }
                        if (type == Holiday.HolidayType.NonFixedDate)
                        {
                            if (dateElement != null)
                            {
                                int month = Convert.ToInt16(dateElement.Attributes["month"].Value);
                                int week = Convert.ToInt16(dateElement.Attributes["week"].Value);
                                var day = (DayOfWeek) Enum.Parse(typeof (DayOfWeek), dateElement.Attributes["dayofweek"].Value);

                                foreach (Country country in countries)
                                {
                                    var holiday = new Holiday(root.Name, uid, type, name, traveltype, country) {Month = month, Week = week, Day = day};

                                    Holidays.AddHoliday(holiday);
                                }
                            }
                        }

                        if (element.SelectSingleNode("translations") != null)
                        {
                            Translator.GetInstance()
                                .AddTranslation(
                                    root.Name,
                                    element.Attributes["uid"].Value,
                                    element.SelectSingleNode("translations"));
                        }
                    }
            }
            catch (Exception e)
            {
                Logger.Error("Error loading holiday {0}", id);
                Logger.Error(e);
            }
        }

        /*! loads the inflation years
         */

        public static void LoadInflationYears()
        {
            var doc = new XmlDocument();
            doc.Load(AppSettings.GetDataPath() + "\\inflations.xml");
            XmlElement root = doc.DocumentElement;

            XmlNodeList inflationsList = root?.SelectNodes("//inflation");

            if (inflationsList != null)
                foreach (XmlElement element in inflationsList)
                {
                    int year = Convert.ToInt16(element.Attributes["year"].Value);
                    double fuelprice = Convert.ToDouble(
                        element.Attributes["fuelprice"].Value,
                        CultureInfo.GetCultureInfo("en-US").NumberFormat);
                    double inflation = Convert.ToDouble(
                        element.Attributes["inflation"].Value,
                        CultureInfo.GetCultureInfo("en-US").NumberFormat);
                    double modifier = Convert.ToDouble(
                        element.Attributes["pricemodifier"].Value,
                        CultureInfo.GetCultureInfo("en-US").NumberFormat);

                    Inflations.AddInflationYear(new Inflation(year, fuelprice, inflation, modifier));
                }
        }

        /*!loads the major destinations
         */

        private static void LoadMajorDestinations()
        {
            try
            {
                var dir = new DirectoryInfo(AppSettings.GetDataPath() + "\\addons\\airports\\majordestinations");

                foreach (FileInfo file in dir.GetFiles("*.xml"))
                {
                    LoadMajorDestinations(file.FullName);
                }
            }
            catch (Exception e)
            {
                Logger.Error("Error loading destinations", e);
            }

            foreach (Airport airport in Airports.GetAllAirports())
            {
                int majorPax = airport.Profile.MajorDestionations.Sum(d => d.Value);

                if (majorPax > 0)
                {
                    airport.Profile.SetPaxValue(Math.Max(airport.Profile.Pax, majorPax));
                }
            }
        }

        private static void LoadMajorDestinations(string file)
        {
            var doc = new XmlDocument();
            doc.Load(file);
            XmlElement root = doc.DocumentElement;

            if (root != null)
            {
                XmlNodeList airportsList = root.SelectNodes("//majordestination");
                string id = "";
                try
                {
                    if (airportsList != null)
                        foreach (XmlElement airportElement in airportsList)
                        {
                            Airport airport = Airports.GetAirport(airportElement.Attributes["airport"].Value);

                            id = airport.Profile.IATACode;

                            XmlNodeList destinationsList = airportElement.SelectNodes("destinations/destination");

                            if (destinationsList != null)
                                foreach (XmlElement destinationElement in destinationsList)
                                {
                                    string destination = destinationElement.Attributes["airport"].Value;
                                    int pax = Convert.ToInt32(destinationElement.Attributes["pax"].Value);

                                    airport.AddMajorDestination(destination, pax);
                                }
                        }
                }
                catch (Exception e)
                {
                    Logger.Error("Error loading destination {0}", id);
                    Logger.Error(e);
                }
            }
        }

        private static void LoadManufacturerLogos()
        {
            var dir = new DirectoryInfo(AppSettings.GetDataPath() + "\\graphics\\manufacturerlogos");

            foreach (FileInfo file in dir.GetFiles("*.png"))
            {
                string name = file.Name.Split('.')[0];
                Manufacturer manufacturer = Manufacturers.GetManufacturer(name);

                if (manufacturer != null)
                {
                    manufacturer.Logo = file.FullName;
                }
                else
                {
                    name = "x";
                }
            }
        }

        private static void LoadManufacturers()
        {
            var doc = new XmlDocument();
            doc.Load(AppSettings.GetDataPath() + "\\manufacturers.xml");
            XmlElement root = doc.DocumentElement;

            XmlNodeList manufacturersList = root?.SelectNodes("//manufacturer");
            if (manufacturersList != null)
                foreach (XmlElement manufacturer in manufacturersList)
                {
                    string name = manufacturer.Attributes["name"].Value;
                    string shortname = manufacturer.Attributes["shortname"].Value;

                    Country country = Countries.GetCountry(manufacturer.Attributes["country"].Value);

                    bool isReal = !manufacturer.HasAttribute("isreal") || Convert.ToBoolean(manufacturer.Attributes["isreal"].Value);

                    Manufacturers.AddManufacturer(new Manufacturer(name, shortname, country, isReal));
                }
        }

        /*loads the special contracts*/

        public static void LoadSpecialContracts()
        {
            var dir = new DirectoryInfo(AppSettings.GetDataPath() + "\\addons\\contracts");

            foreach (FileInfo file in dir.GetFiles("*.xml"))
            {
                LoadSpecialContract(file.FullName);
            }
        }

        /*loads a special contract*/

        public static void LoadSpecialContract(string file)
        {
            var doc = new XmlDocument();
            doc.Load(file);
            XmlElement root = doc.DocumentElement;
            XmlNodeList contractsList = root?.SelectNodes("//contract");

            if (contractsList != null)
                foreach (XmlElement element in contractsList)
                {
                    string name = element.Attributes["name"].Value;
                    string text = element.Attributes["text"].Value;

                    var infoElement = (XmlElement) element.SelectSingleNode("information");

                    if (infoElement != null)
                    {
                        long payment = Convert.ToInt64(infoElement.Attributes["payment"].Value);
                        bool asbonus = Convert.ToBoolean(infoElement.Attributes["bonus"].Value);
                        long penalty = Convert.ToInt64(infoElement.Attributes["penalty"].Value);
                        bool isfixeddate = !infoElement.HasAttribute("frequency");

                        var scType = new SpecialContractType(name, text, payment, asbonus, penalty, isfixeddate);

                        if (isfixeddate)
                        {
                            DateTime fromdate = Convert.ToDateTime(
                                infoElement.Attributes["from"].Value,
                                new CultureInfo("en-US", false));

                            DateTime todate = Convert.ToDateTime(
                                infoElement.Attributes["to"].Value,
                                new CultureInfo("en-US", false));

                            scType.Period = new Period<DateTime>(fromdate, todate);
                        }
                        else
                        {
                            int frequency = Convert.ToInt32(infoElement.Attributes["frequency"].Value);

                            scType.Frequency = frequency;
                        }

                        XmlNodeList routesList = element.SelectNodes("routes/route");

                        if (routesList != null)
                            foreach (XmlElement routeElement in routesList)
                            {
                                // public SpecialContractRoute(Airport destination1, Airport destination2, long passengers,Boolean bothways)
                                Airport departure = Airports.GetAirport(routeElement.Attributes["departure"].Value);
                                Airport destination = Airports.GetAirport(routeElement.Attributes["destination"].Value);
                                bool bothways = Convert.ToBoolean(routeElement.Attributes["bothways"].Value);
                                long passengers = Convert.ToInt64(routeElement.Attributes["passengers"].Value);

                                var routetype = Route.RouteType.Passenger;

                                if (routeElement.HasAttribute("type"))
                                    routetype = (Route.RouteType)
                                        Enum.Parse(typeof (Route.RouteType), routeElement.Attributes["type"].Value);

                                var scRoute = new SpecialContractRoute(departure, destination, passengers, routetype, bothways);
                                scType.Routes.Add(scRoute);
                            }

                        XmlNodeList parametersList = element.SelectNodes("parameters/parameter");

                        if (parametersList != null)
                            foreach (XmlElement parameterElement in parametersList)
                            {
                                if (parameterElement.HasAttribute("departure"))
                                {
                                    var parameter = new ContractRequirement(ContractRequirement.RequirementType.Destination);

                                    Airport departure = Airports.GetAirport(parameterElement.Attributes["departure"].Value);

                                    parameter.Departure = departure;

                                    string destinationText = parameterElement.Attributes["destination"].Value;

                                    if (destinationText == "any")
                                    {
                                        foreach (SpecialContractRoute scroute in scType.Routes.Where(r => r.Departure == departure))
                                        {
                                            var tparameter = new ContractRequirement(ContractRequirement.RequirementType.Destination) {Departure = departure, Destination = scroute.Destination};

                                            scType.Requirements.Add(tparameter);
                                        }
                                    }
                                    else
                                    {
                                        parameter.Destination = Airports.GetAirport(destinationText);

                                        scType.Requirements.Add(parameter);
                                    }
                                }
                                else
                                {
                                    var parameter = new ContractRequirement(ContractRequirement.RequirementType.ClassType);

                                    var type =
                                        (AirlinerClass.ClassType)
                                            Enum.Parse(typeof (AirlinerClass.ClassType), parameterElement.Attributes["classtype"].Value);

                                    int seats = Convert.ToInt32(parameterElement.Attributes["minseats"].Value);

                                    parameter.ClassType = type;
                                    parameter.MinSeats = seats;

                                    scType.Requirements.Add(parameter);
                                }
                            }

                        SpecialContractTypes.AddType(scType);
                    }
                }
        }

        //loads the random events
        private static void LoadRandomEvents()
        {
            var doc = new XmlDocument();
            doc.Load(AppSettings.GetDataPath() + "\\addons\\events\\RandomEvents.xml");
            XmlElement root = doc.DocumentElement;
            XmlNodeList eventsList = root?.SelectNodes("//event");

            if (eventsList != null)
                foreach (XmlElement element in eventsList)
                {
                    string section = root.Name;
                    var effects = (XmlElement) element.SelectSingleNode("effects");
                    var demand = (XmlElement) element.SelectSingleNode("demand");
                    var valid = (XmlElement) element.SelectSingleNode("valid");
                    string uid = element.Attributes["uid"].Value;
                    var eventType = new RandomEvent.EventType();
                    string type = element.Attributes["type"].Value;
                    string focus = element.Attributes["type"].Value;
                    var eventFocus = new RandomEvent.Focus();
                    switch (type)
                    {
                        case "Safety":
                            eventType = RandomEvent.EventType.Safety;
                            break;
                        case "Security":
                            eventType = RandomEvent.EventType.Security;
                            break;
                        case "Customer":
                            eventType = RandomEvent.EventType.Customer;
                            break;
                        case "Employee":
                            eventType = RandomEvent.EventType.Employee;
                            break;
                        case "Maintenance":
                            eventType = RandomEvent.EventType.Maintenance;
                            break;
                        case "Political":
                            eventType = RandomEvent.EventType.Political;
                            break;
                    }

                    switch (focus)
                    {
                        case "Airline":
                            eventFocus = RandomEvent.Focus.Airline;
                            break;

                        case "Aircraft":
                            eventFocus = RandomEvent.Focus.Aircraft;
                            break;

                        case "Airport":
                            eventFocus = RandomEvent.Focus.Airport;
                            break;
                    }

                    string name = element.Attributes["name"].Value;
                    string message = element.Attributes["text"].Value;
                    int frequency = int.Parse(element.Attributes["frequency"].Value)/3;
                    DateTime start = valid != null && valid.HasAttribute("from")
                        ? DateTime.Parse(valid.Attributes["from"].Value)
                        : DateTime.Now.AddYears(100);
                    DateTime end = valid != null && valid.HasAttribute("to")
                        ? DateTime.Parse(valid.Attributes["to"].Value)
                        : DateTime.Now.AddYears(100);

                    bool critical = Convert.ToBoolean(element.Attributes["important"].Value);
                    // if (int.Parse(effects.Attributes["important"].Value) == 1) critical = true; else critical = false;<

                    if (effects != null)
                    {
                        int effectLength = int.Parse(effects.Attributes["duration"].Value);

                        int chEffect = effects.HasAttribute("customerHappiness")
                            ? int.Parse(effects.Attributes["customerHappiness"].Value)
                            : 0;
                        int ehEffect = effects.HasAttribute("employeeHappiness")
                            ? int.Parse(effects.Attributes["employeeHappiness"].Value)
                            : 0;
                        int aSecurityEffect = effects.HasAttribute("airlineSecurity")
                            ? int.Parse(effects.Attributes["airlineSecurity"].Value)
                            : 0;
                        int aSafetyEffect = effects.HasAttribute("airlineSafety")
                            ? int.Parse(effects.Attributes["airlineSafety"].Value)
                            : 0;
                        int damageEffect = effects.HasAttribute("aircraftDamage")
                            ? int.Parse(effects.Attributes["aircraftDamage"].Value)
                            : 0;
                        int financial = effects.HasAttribute("financial") ? int.Parse(effects.Attributes["financial"].Value) : 0;
                        double paxDemand = demand != null && demand.HasAttribute("passenger")
                            ? double.Parse(demand.Attributes["passenger"].Value)
                            : 0;
                        double cargoDemand = demand != null && demand.HasAttribute("cargo") ? double.Parse(demand.Attributes["cargo"].Value) : 0;

                        var rEvent = new RandomEvent(
                            eventType,
                            eventFocus,
                            aSafetyEffect,
                            financial,
                            uid,
                            frequency,
                            start,
                            end,
                            name,
                            message,
                            critical,
                            chEffect,
                            damageEffect,
                            aSecurityEffect,
                            ehEffect,
                            paxDemand,
                            cargoDemand,
                            effectLength);

                        RandomEvents.AddEvent(rEvent);
                    }
                }
        }

        private static void LoadRegions()
        {
            var doc = new XmlDocument();
            doc.Load(AppSettings.GetDataPath() + "\\regions.xml");
            XmlElement root = doc.DocumentElement;

            XmlNodeList regionsList = root?.SelectNodes("//region");
            if (regionsList != null)
                foreach (XmlElement element in regionsList)
                {
                    string section = root.Name;
                    string uid = element.Attributes["uid"].Value;
                    double fuelindex = Convert.ToDouble(
                        element.Attributes["fuel"].Value,
                        CultureInfo.GetCultureInfo("en-US").NumberFormat);

                    Regions.AddRegion(new Region(section, uid, fuelindex));

                    if (element.SelectSingleNode("translations") != null)
                    {
                        Translator.GetInstance()
                            .AddTranslation(
                                root.Name,
                                element.Attributes["uid"].Value,
                                element.SelectSingleNode("translations"));
                    }
                }
        }

        private static RouteClassesConfiguration LoadRouteClassesConfiguration(XmlElement element)
        {
            string name = element.Attributes["name"].Value;
            string id = element.Attributes["id"].Value;

            XmlNodeList classesList = element.SelectNodes("classes/class");

            var configuration = new RouteClassesConfiguration(name, true) {ID = id};

            if (classesList != null)
                foreach (XmlElement classElement in classesList)
                {
                    var type =
                        (AirlinerClass.ClassType)
                        Enum.Parse(typeof (AirlinerClass.ClassType), classElement.Attributes["type"].Value);

                    var classConfiguration = new RouteClassConfiguration(type);

                    foreach (RouteFacility.FacilityType facilityType in Enum.GetValues(typeof (RouteFacility.FacilityType)))
                    {
                        string value = classElement.Attributes[facilityType.ToString()].Value;
                        RouteFacility facility = RouteFacilities.GetFacility(value);

                        classConfiguration.AddFacility(facility);
                    }
                    configuration.AddClass(classConfiguration);
                }

            return configuration;
        }

        private static void LoadScenario(string filename)
        {
            var doc = new XmlDocument();
            doc.Load(filename);
            XmlElement element = doc.DocumentElement;

            try
            {
                if (element != null)
                {
                    string scenarioName = element.Attributes["name"].Value;
                    int startYear = Convert.ToInt32(element.Attributes["startYear"].Value);
                    long startCash = Convert.ToInt64(element.Attributes["startCash"].Value);
                    int endYear = Convert.ToInt32(element.Attributes["endYear"].Value);
                    DifficultyLevel difficulty = DifficultyLevels.GetDifficultyLevel(element.Attributes["difficulty"].Value);

                    var selectSingleNode = element.SelectSingleNode("intro");
                    if (selectSingleNode?.Attributes != null)
                    {
                        string description = selectSingleNode.Attributes["text"].Value;
                        var singleNode = element.SelectSingleNode("success");
                        if (singleNode?.Attributes != null)
                        {
                            string successText = singleNode.Attributes["text"].Value;
                        }

                        var startElement = (XmlElement) element.SelectSingleNode("start");

                        if (startElement != null)
                        {
                            Airline startAirline = Airlines.GetAirline(startElement.Attributes["airline"].Value);
                            Airport homebase = Airports.GetAirport(startElement.Attributes["homeBase"].Value);

                            if (startElement.HasAttribute("license"))
                            {
                                startAirline.License =
                                    (Airline.AirlineLicense)
                                        Enum.Parse(typeof (Airline.AirlineLicense), startElement.Attributes["license"].Value);
                            }

                            var scenario = new Scenario(
                                scenarioName,
                                description,
                                startAirline,
                                homebase,
                                startYear,
                                endYear,
                                startCash,
                                difficulty);
                            Scenarios.AddScenario(scenario);

                            XmlNodeList humanRoutesList = startElement.SelectNodes("routes/route");

                            if (humanRoutesList != null)
                                foreach (XmlElement humanRouteElement in humanRoutesList)
                                {
                                    Airport routeDestination1 = Airports.GetAirport(humanRouteElement.Attributes["departure"].Value);
                                    Airport routeDestination2 = Airports.GetAirport(humanRouteElement.Attributes["destination"].Value);
                                    AirlinerType routeAirlinerType =
                                        AirlinerTypes.GetType(humanRouteElement.Attributes["airliner"].Value);
                                    int routeQuantity = Convert.ToInt32(humanRouteElement.Attributes["quantity"].Value);

                                    scenario.AddRoute(
                                        new ScenarioAirlineRoute(routeDestination1, routeDestination2, routeAirlinerType, routeQuantity));
                                }

                            XmlNodeList destinationsList = startElement.SelectNodes("destinations/destination");

                            if (destinationsList != null)
                                foreach (XmlElement destinationElement in destinationsList)
                                {
                                    scenario.AddDestination(Airports.GetAirport(destinationElement.Attributes["airport"].Value));
                                }

                            XmlNodeList fleetList = startElement.SelectNodes("fleet/aircraft");

                            if (fleetList != null)
                                foreach (XmlElement fleetElement in fleetList)
                                {
                                    AirlinerType fleetAirlinerType = AirlinerTypes.GetType(fleetElement.Attributes["name"].Value);

                                    int fleetQuantity = Convert.ToInt32(fleetElement.Attributes["quantity"].Value);

                                    scenario.AddFleet(fleetAirlinerType, fleetQuantity);
                                }

                            XmlNodeList aiNodeList = startElement.SelectNodes("AI/airline");

                            if (aiNodeList != null)
                                foreach (XmlElement aiElement in aiNodeList)
                                {
                                    Airline aiAirline = Airlines.GetAirline(aiElement.Attributes["name"].Value, startYear);
                                    Airport aiHomebase = Airports.GetAirport(aiElement.Attributes["homeBase"].Value);

                                    var scenarioAirline = new ScenarioAirline(aiAirline, aiHomebase);

                                    XmlNodeList aiRoutesList = aiElement.SelectNodes("route");

                                    if (aiRoutesList != null)
                                        foreach (XmlElement aiRouteElement in aiRoutesList)
                                        {
                                            Airport aiRouteDestination1 = Airports.GetAirport(aiRouteElement.Attributes["departure"].Value);
                                            Airport aiRouteDestination2 = Airports.GetAirport(
                                                aiRouteElement.Attributes["destination"].Value);
                                            AirlinerType routeAirlinerType =
                                                AirlinerTypes.GetType(aiRouteElement.Attributes["airliner"].Value);
                                            int routeQuantity = Convert.ToInt32(aiRouteElement.Attributes["quantity"].Value);

                                            scenarioAirline.AddRoute(
                                                new ScenarioAirlineRoute(
                                                    aiRouteDestination1,
                                                    aiRouteDestination2,
                                                    routeAirlinerType,
                                                    routeQuantity));
                                        }

                                    scenario.AddOpponentAirline(scenarioAirline);
                                }

                            XmlNodeList modifiersList = element.SelectNodes("modifiers/paxDemand");

                            if (modifiersList != null)
                                foreach (XmlElement paxElement in modifiersList)
                                {
                                    Country country = null;
                                    Airport airport = null;

                                    if (paxElement.HasAttribute("country"))
                                    {
                                        country = Countries.GetCountry(paxElement.Attributes["country"].Value);
                                    }

                                    if (paxElement.HasAttribute("airport"))
                                    {
                                        airport = Airports.GetAirport(paxElement.Attributes["airport"].Value);
                                    }

                                    double factor = Convert.ToDouble(
                                        paxElement.Attributes["change"].Value,
                                        CultureInfo.GetCultureInfo("en-US").NumberFormat);

                                    var enddate =
                                        new DateTime(scenario.StartYear + Convert.ToInt32(paxElement.Attributes["length"].Value), 1, 1);

                                    scenario.AddPassengerDemand(new ScenarioPassengerDemand(factor, enddate, country, airport));
                                }

                            XmlNodeList parametersList = element.SelectNodes("parameters/failure");

                            foreach (XmlElement parameterElement in parametersList)
                            {
                                string id = parameterElement.Attributes["id"].Value;
                                var failureType =
                                    (ScenarioFailure.FailureType)
                                        Enum.Parse(typeof (ScenarioFailure.FailureType), parameterElement.Attributes["type"].Value);
                                object failureValue = parameterElement.Attributes["value"].Value;
                                double checkMonths = parameterElement.HasAttribute("at")
                                    ? 12
                                      *Convert.ToDouble(
                                          parameterElement.Attributes["at"].Value,
                                          CultureInfo.GetCultureInfo("en-US").NumberFormat)
                                    : 1;
                                string failureText = parameterElement.Attributes["text"].Value;
                                double monthsOfFailure = parameterElement.HasAttribute("for")
                                    ? 12
                                      *Convert.ToDouble(
                                          parameterElement.Attributes["for"].Value,
                                          CultureInfo.GetCultureInfo("en-US").NumberFormat)
                                    : 1;

                                var failure = new ScenarioFailure(
                                    id,
                                    failureType,
                                    (int) checkMonths,
                                    failureValue,
                                    failureText,
                                    (int) monthsOfFailure);

                                scenario.AddScenarioFailure(failure);
                            }
                        }
                    }
                }
            }
            catch (Exception e)
            {
                Logger.Error("Error loading scenario", e);
            }
        }

        private static void LoadScenarios()
        {
            var dir = new DirectoryInfo(AppSettings.GetDataPath() + "\\addons\\scenarios");

            foreach (FileInfo file in dir.GetFiles("*.xml"))
            {
                LoadScenario(file.FullName);
            }
        }

        private static void LoadStandardConfigurations()
        {
            var doc = new XmlDocument();
            doc.Load(AppSettings.GetDataPath() + "\\standardconfigurations.xml");
            XmlElement root = doc.DocumentElement;

            try
            {
                XmlNodeList configurationsList = root?.SelectNodes("//configuration");

                if (configurationsList != null)
                    foreach (XmlElement element in configurationsList)
                    {
                        string type = element.Attributes["type"].Value;

                        if (type == "Route")
                        {
                            Configurations.AddConfiguration(LoadRouteClassesConfiguration(element));
                        }
                        if (type == "Airliner")
                        {
                            Configurations.AddConfiguration(LoadAirlinerConfiguration(element));
                        }
                    }
            }
            catch (Exception e)
            {
                Logger.Error("Error loading configurations", e);
            }
        }

        /*!loads the states
         */

        private static void LoadStates()
        {
            var doc = new XmlDocument();
            doc.Load(AppSettings.GetDataPath() + "\\states.xml");
            XmlElement root = doc.DocumentElement;

            XmlNodeList statesList = root?.SelectNodes("//state");
            if (statesList != null)
                foreach (XmlElement element in statesList)
                {
                    Country country = Countries.GetCountry(element.Attributes["country"].Value);
                    string name = element.Attributes["name"].Value;
                    string shortname = element.Attributes["shortname"].Value;
                    bool overseas = false;

                    if (element.HasAttribute("overseas"))
                    {
                        overseas = Convert.ToBoolean(element.Attributes["overseas"].Value);
                    }

                    var state = new State(country, name, shortname, overseas)
                    {
                        Flag = AppSettings.GetDataPath() + "\\graphics\\flags\\states\\"
                               + element.Attributes["flag"].Value + ".png"
                    };

                    States.AddState(state);

                    if (!File.Exists(state.Flag))
                    {
                        name = "";
                    }
                }
        }

        /*!loads the countries.
         */

        /*! loads the temporary countries
         */

        private static void LoadTemporaryCountries()
        {
            var doc = new XmlDocument();
            doc.Load(AppSettings.GetDataPath() + "\\temporary countries.xml");
            XmlElement root = doc.DocumentElement;

            XmlNodeList countriesList = root?.SelectNodes("//country");
            if (countriesList != null)
                foreach (XmlElement element in countriesList)
                {
                    string section = root.Name;
                    string uid = element.Attributes["uid"].Value;
                    string shortname = element.Attributes["shortname"].Value;
                    string flag = element.Attributes["flag"].Value;
                    Region region = Regions.GetRegion(element.Attributes["region"].Value);
                    string tailformat = element.Attributes["tailformat"].Value;
                    var tempType =
                        (TemporaryCountry.TemporaryType)
                            Enum.Parse(typeof (TemporaryCountry.TemporaryType), element.Attributes["type"].Value);

                    var periodElement = (XmlElement) element.SelectSingleNode("period");
                    if (periodElement != null)
                    {
                        DateTime startDate = Convert.ToDateTime(
                            periodElement.Attributes["start"].Value,
                            new CultureInfo("en-US", false));
                        DateTime endDate = Convert.ToDateTime(
                            periodElement.Attributes["end"].Value,
                            new CultureInfo("en-US", false));

                        var country = new Country(section, uid, shortname, region, tailformat);

                        if (element.SelectSingleNode("translations") != null)
                        {
                            Translator.GetInstance()
                                .AddTranslation(
                                    root.Name,
                                    element.Attributes["uid"].Value,
                                    element.SelectSingleNode("translations"));
                        }

                        var historyElement = (XmlElement) element.SelectSingleNode("history");

                        var tCountry = new TemporaryCountry(tempType, country, startDate, endDate);

                        if (tempType == TemporaryCountry.TemporaryType.ManyToOne)
                        {
                            if (historyElement != null)
                            {
                                Country before = Countries.GetCountry(historyElement.Attributes["before"].Value);
                                Country after = Countries.GetCountry(historyElement.Attributes["after"].Value);

                                tCountry.CountryBefore = before;
                                tCountry.CountryAfter = after;
                            }
                        }
                        if (tempType == TemporaryCountry.TemporaryType.OneToMany)
                        {
                            XmlNodeList tempCountriesList = historyElement?.SelectNodes("tempcountries/tempcountry");

                            if (tempCountriesList != null)
                                foreach (XmlElement tempCountryElement in tempCountriesList)
                                {
                                    Country tempCountry = Countries.GetCountry(tempCountryElement.Attributes["id"].Value);
                                    DateTime cStartDate = Convert.ToDateTime(
                                        tempCountryElement.Attributes["start"].Value,
                                        new CultureInfo("en-US", false));
                                    DateTime cEndDate = Convert.ToDateTime(
                                        tempCountryElement.Attributes["end"].Value,
                                        new CultureInfo("en-US", false));

                                    tCountry.Countries.Add(new OneToManyCountry(tempCountry, cStartDate, cEndDate));
                                }
                        }

                        tCountry.Flag = AppSettings.GetDataPath() + "\\graphics\\flags\\" + flag + ".png";

                        TemporaryCountries.AddCountry(tCountry);
                    }
                }
        }

        /*! load the unions
         */

        private static void LoadUnions()
        {
            var doc = new XmlDocument();
            doc.Load(AppSettings.GetDataPath() + "\\unions.xml");
            XmlElement root = doc.DocumentElement;

            XmlNodeList unionsList = root?.SelectNodes("//union");
            if (unionsList != null)
                foreach (XmlElement element in unionsList)
                {
                    try
                    {
                        string section = root.Name;
                        string uid = element.Attributes["uid"].Value;
                        string shortname = element.Attributes["shortname"].Value;
                        string flag = element.Attributes["flag"].Value;

                        var periodElement = (XmlElement) element.SelectSingleNode("period");
                        if (periodElement != null)
                        {
                            DateTime creationDate = Convert.ToDateTime(
                                periodElement.Attributes["creation"].Value,
                                new CultureInfo("en-US", false));
                            DateTime obsoleteDate = Convert.ToDateTime(
                                periodElement.Attributes["obsolete"].Value,
                                new CultureInfo("en-US", false));

                            var union = new Union(section, uid, shortname, creationDate, obsoleteDate);
                            XmlNodeList membersList = element.SelectNodes("members/member");

                            if (membersList != null)
                                foreach (XmlElement memberNode in membersList)
                                {
                                    Country country = Countries.GetCountry(memberNode.Attributes["country"].Value);
                                    DateTime fromDate = Convert.ToDateTime(
                                        memberNode.Attributes["memberfrom"].Value,
                                        new CultureInfo("en-US", false));
                                    DateTime toDate = Convert.ToDateTime(
                                        memberNode.Attributes["memberto"].Value,
                                        new CultureInfo("en-US", false));

                                    if (country == null)
                                    {
                                        uid = "";
                                    }

                                    union.AddMember(new UnionMember(country, fromDate, toDate));
                                }

                            union.Flag = AppSettings.GetDataPath() + "\\graphics\\flags\\" + flag + ".png";
                            Unions.AddUnion(union);
                        }

                        if (element.SelectSingleNode("translations") != null)
                        {
                            Translator.GetInstance()
                                .AddTranslation(
                                    root.Name,
                                    element.Attributes["uid"].Value,
                                    element.SelectSingleNode("translations"));
                        }
                    }
                    catch (Exception e)
                    {
                        Logger.Error("Error loading unions", e);
                    }
                }
        }

        /*! load the airliner facilities.
         */

        /*! loads all weather averages
         */

        private static void LoadWeatherAverages()
        {
            var dir = new DirectoryInfo(AppSettings.GetDataPath() + "\\addons\\weather");

            foreach (FileInfo file in dir.GetFiles("*.xml"))
            {
                LoadWeatherAverages(file.FullName);
            }
        }

        /*! loads a weather averages
       */

        private static void LoadWeatherAverages(string path)
        {
            var doc = new XmlDocument();
            doc.Load(path);
            XmlElement root = doc.DocumentElement;

            Country country = null;
            Airport airport = null;
            Town town = null;

            if (root != null)
            {
                string type = root.Attributes["type"].Value;
                string value = root.Attributes["value"].Value;

                if (type == "country")
                {
                    country = Countries.GetCountry(value);
                }

                if (type == "town")
                {
                    town = Towns.GetTown(value);
                }

                if (type == "airport")
                {
                    airport = Airports.GetAirport(value);
                }
            }

            XmlNodeList monthsList = root?.SelectNodes("months/month");

            if (monthsList != null)
                foreach (XmlElement monthElement in monthsList)
                {
                    int month = Convert.ToInt16(monthElement.Attributes["month"].Value);
                    int precipitation = Convert.ToInt16(monthElement.Attributes["precipitation"].Value);

                    var tempElement = (XmlElement) monthElement.SelectSingleNode("temp");
                    if (tempElement != null)
                    {
                        double minTemp = Convert.ToDouble(
                            tempElement.Attributes["min"].Value,
                            CultureInfo.GetCultureInfo("en-US").NumberFormat);
                        double maxTemp = Convert.ToDouble(
                            tempElement.Attributes["max"].Value,
                            CultureInfo.GetCultureInfo("en-US").NumberFormat);

                        var windElement = (XmlElement) monthElement.SelectSingleNode("wind");
                        if (windElement != null)
                        {
                            var minWind =
                                (Weather.eWindSpeed) Enum.Parse(typeof (Weather.eWindSpeed), windElement.Attributes["min"].Value);
                            var maxWind =
                                (Weather.eWindSpeed) Enum.Parse(typeof (Weather.eWindSpeed), windElement.Attributes["max"].Value);

                            if (country != null)
                            {
                                WeatherAverages.AddWeatherAverage(
                                    new WeatherAverage(month, minTemp, maxTemp, precipitation, minWind, maxWind, country));
                            }

                            if (town != null)
                            {
                                WeatherAverages.AddWeatherAverage(
                                    new WeatherAverage(month, minTemp, maxTemp, precipitation, minWind, maxWind, town));
                            }

                            if (airport != null)
                            {
                                WeatherAverages.AddWeatherAverage(
                                    new WeatherAverage(month, minTemp, maxTemp, precipitation, minWind, maxWind, airport));
                            }
                        }
                    }
                }
        }

        private static void ReadSettingsFile()
        {
            if (File.Exists(AppSettings.GetCommonApplicationDataPath() + "\\game.settings"))
            {
                var file = new StreamReader(AppSettings.GetCommonApplicationDataPath() + "\\game.settings");

                string language = file.ReadLine();
                string screenMode = file.ReadLine();

                AppSettings.GetInstance().SetLanguage(Languages.GetLanguage(language));
                if (screenMode != null)
                    Settings.GetInstance().Mode =
                        (ScreenMode) Enum.Parse(typeof (ScreenMode), screenMode, true);
            }
        }

        private static void RemoveAirlines(int opponnents, bool sameRegion)
        {
            Airport humanAirport = GameObject.GetInstance().HumanAirline.Airports[0];
            int year = GameObject.GetInstance().GameTime.Year;

            List<Airline> notAvailableAirlines =
                Airlines.GetAirlines(a => !(a.Profile.Founded <= year && a.Profile.Folded > year));

            foreach (Airline airline in notAvailableAirlines)
            {
                Airlines.RemoveAirline(airline);
            }

            int count =
                Airlines.GetAirlines(a => !a.IsHuman && a.Profile.Founded <= year && a.Profile.Folded > year).Count;

            var airlines =
                new List<Airline>(
                    Airlines.GetAirlines(a => !a.IsHuman && a.Profile.Founded <= year && a.Profile.Folded > year));

            if (sameRegion)
            {
                airlines =
                    airlines.OrderByDescending(
                        a =>
                        a.Profile.PreferedAirport == null
                            ? double.MaxValue
                            : MathHelpers.GetDistance(a.Profile.PreferedAirport, humanAirport)).ToList();
            }
            else
            {
                airlines = MathHelpers.Shuffle(airlines);
            }

            for (int i = 0; i < count - opponnents; i++)
            {
                Airlines.RemoveAirline(airlines[i]);
            }
        }

        /*loads the alliances
         */

        /*! sets up the difficulty levels
         */

        private static void SetupDifficultyLevels()
        {
            DifficultyLevels.AddDifficultyLevel(new DifficultyLevel("Easy", 1.5, 0.75, 1.5, 1, 1.25, 5));
            DifficultyLevels.AddDifficultyLevel(new DifficultyLevel("Normal", 1, 1, 1.2, 1.1, 1, 2));
            DifficultyLevels.AddDifficultyLevel(new DifficultyLevel("Hard", 0.5, 1.25, 1, 1.2, 0.75, 1));
        }

        /*! sets up the statistics types.
         */

        private static void SetupMainGame()
        {
            CreatePilots();

            //creates the facilities for pilot training
            IEnumerable<string> aircraftFamilies =
                AirlinerTypes.GetTypes(
                    t =>
                    t.Produced.From.Year < GameObject.GetInstance().GameTime.Year
                    && t.Produced.To > GameObject.GetInstance().GameTime.AddYears(-30))
                             .Select(a => a.AirlinerFamily)
                             .Distinct();

            foreach (string family in aircraftFamilies)
            {
                AirlineFacilities.AddFacility(
                    new PilotTrainingFacility(
                        "airlinefacilities",
                        family,
                        9000,
                        1000,
                        GameObject.GetInstance().GameTime.Year,
                        0,
                        0,
                        family));
            }

            //sets all the facilities at an airport to none for all airlines
            Parallel.ForEach(
                Airports.GetAllAirports(),
                airport =>
                    {
                        foreach (Airline airline in Airlines.GetAllAirlines())
                        {
                            foreach (
                                AirportFacility.FacilityType type in Enum.GetValues(typeof (AirportFacility.FacilityType)))
                            {
                                AirportFacility noneFacility =
                                    AirportFacilities.GetFacilities(type)
                                                     .Find((facility => facility.TypeLevel == 0));

                                airport.AddAirportFacility(airline, noneFacility, GameObject.GetInstance().GameTime);
                            }
                        }
                        if (airport.Profile.Size == GeneralHelpers.Size.Largest
                            || airport.Profile.Size == GeneralHelpers.Size.VeryLarge
                            || airport.Profile.Size == GeneralHelpers.Size.Large)
                        {
                            if (!airport.Runways.Exists(r => r.Type == Runway.RunwayType.Helipad))
                            {
                                var helipad = new Runway("H1", Rnd.Next(17, 25), Runway.RunwayType.Helipad, Runway.SurfaceType.Concrete, GameObject.GetInstance().GameTime, true);

                                airport.Runways.Add(helipad);
                            }
                        }
                        if (airport.Profile.Cargo == GeneralHelpers.Size.VeryLarge
                            || airport.Profile.Cargo == GeneralHelpers.Size.Largest
                            || airport.Profile.Size == GeneralHelpers.Size.Largest
                            || airport.Profile.Size == GeneralHelpers.Size.VeryLarge)
                        {
                            AirportFacility cargoTerminal =
                                AirportFacilities.GetFacilities(AirportFacility.FacilityType.Cargo)
                                                 .Find(f => f.TypeLevel > 0);

                            airport.AddAirportFacility(null, cargoTerminal, GameObject.GetInstance().GameTime);

                            if (!airport.Terminals.AirportTerminals.Exists(t => t.Type == Terminal.TerminalType.Cargo))
                            {
                                airport.Terminals.AddTerminal(new Terminal(airport, "Cargo Terminal", ((int) airport.Profile.Cargo) + 5, GameObject.GetInstance().GameTime, Terminal.TerminalType.Cargo));
                            }
                        }

                        AirportHelpers.CreateAirportWeather(airport);

                        //creates the already existing expansions
                        IEnumerable<AirportExpansion> expansions = airport.Profile.Expansions.Where(e => GameObject.GetInstance().GameTime > e.Date);

                        foreach (AirportExpansion expansion in expansions)
                            AirportHelpers.SetAirportExpansion(airport, expansion, true);
                    });

            foreach (Airline airline in Airlines.GetAllAirlines())
            {
                airline.Money = GameObject.GetInstance().StartMoney;

                if (airline.IsHuman)
                {
                    GameObject.GetInstance().HumanMoney = airline.Money;
                }

                airline.StartMoney = airline.Money;
                airline.Fees = new AirlineFees();
                airline.AddAirlinePolicy(new AirlinePolicy("Cancellation Minutes", 150));

                AirlineHelpers.CreateStandardAirlineShares(airline);

                if (!airline.IsHuman)
                {
                    airline.Profile.Country = airline.Profile.Countries[Rnd.Next(airline.Profile.Countries.Count)];

                    CreateComputerRoutes(airline);
                }
            }

            /*
            Airports.GetAirport("BOS").Terminals.rentGate(GameObject.GetInstance().HumanAirline);
            Airports.GetAirport("AAR").Terminals.rentGate(GameObject.GetInstance().HumanAirline);
            Airports.GetAirport("CPH").Terminals.rentGate(GameObject.GetInstance().HumanAirline);
            Airports.GetAirport("CPH").Terminals.rentGate(GameObject.GetInstance().HumanAirline);
            Airports.GetAirport("SBY").Terminals.rentGate(GameObject.GetInstance().HumanAirline);

            Airliner airliner = Airliners.GetAirlinersForSale(a => a.Type.Name == "Boeing 737-900ER").First();
            AirlineHelpers.BuyAirliner(GameObject.GetInstance().HumanAirline, airliner, GameObject.GetInstance().HumanAirline.Airports[0]);
            */
            SetupAlliances();
            SetupMergers();
        }

        private static void SetupStatisticsTypes()
        {
            StatisticsTypes.AddStatisticsType(new StatisticsType("Arrivals", "Arrivals"));
            StatisticsTypes.AddStatisticsType(new StatisticsType("Departures", "Departures"));
            StatisticsTypes.AddStatisticsType(new StatisticsType("Cargo", "Cargo"));
            StatisticsTypes.AddStatisticsType(new StatisticsType("Cargo per flight", "Cargo%"));
            StatisticsTypes.AddStatisticsType(new StatisticsType("Passengers", "Passengers"));
            StatisticsTypes.AddStatisticsType(new StatisticsType("Passengers per flight", "Passengers%"));
            StatisticsTypes.AddStatisticsType(new StatisticsType("Passenger Capacity", "Capacity"));
            StatisticsTypes.AddStatisticsType(new StatisticsType("Airliner Income", "Airliner_Income"));
            StatisticsTypes.AddStatisticsType(new StatisticsType("On-Time flights", "On-Time"));
            StatisticsTypes.AddStatisticsType(new StatisticsType("Flights On-Time", "On-Time%"));
            StatisticsTypes.AddStatisticsType(new StatisticsType("Cancellations", "Cancellations"));
            StatisticsTypes.AddStatisticsType(new StatisticsType("Cancellation Percent", "Cancellation%"));
        }

        #endregion

        /*! private static variable rnd.
         * holds a random number.
         */

        /*! private static method ClearLists().
         * Resets game´s environment.
         */

        //sets up the airline mergers
    }
}