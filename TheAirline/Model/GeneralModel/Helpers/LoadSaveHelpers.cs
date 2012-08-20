using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Text;
using System.Xml;
using TheAirline.Model.AirportModel;
using TheAirline.Model.AirlineModel;
using TheAirline.Model.AirlinerModel;
using TheAirline.Model.AirlinerModel.RouteModel;
using TheAirline.Model.GeneralModel.StatisticsModel;
using TheAirline.GraphicsModel.UserControlModel.MessageBoxModel;
using System.Globalization;
using TheAirline.GraphicsModel.SkinsModel;
using TheAirline.Model.PassengerModel;

namespace TheAirline.Model.GeneralModel.Helpers
{
    public class LoadSaveHelpers
    {
        //deletes a saved game
        public static void DeleteGame(string name)
        {
            RemoveSavedFile(name);

            File.Delete(AppSettings.getDataPath() + "\\saves\\" + name + ".xml");


        }
        //remove a file to the list of saved files
        public static void RemoveSavedFile(string name)
        {
            XmlDocument doc = new XmlDocument();
            doc.Load(AppSettings.getDataPath() + "\\saves\\saves.xml");

            XmlNode node = doc.SelectSingleNode("/saves/save[@file='" + name + "']");

            node.ParentNode.RemoveChild(node);

            doc.Save(AppSettings.getDataPath() + "\\saves\\saves.xml");

        }
        //loads a game
        public static void LoadGame(string name)
        {
            if (null == name || name == "")
            {
                WPFMessageBoxResult result = WPFMessageBox.Show(Translator.GetInstance().GetString("MessageBox", "101"), Translator.GetInstance().GetString("MessageBox", "101", "message"), WPFMessageBoxButtons.Ok);
                return;
            }

            XmlDocument doc = new XmlDocument();
            doc.Load(AppSettings.getDataPath() + "\\saves\\" + name + ".xml");
            XmlElement root = doc.DocumentElement;


            XmlNodeList tailnumbersList = root.SelectNodes("//tailnumbers/tailnumber");

            foreach (XmlElement tailnumberNode in tailnumbersList)
            {
                Country country = Countries.GetCountry(tailnumberNode.Attributes["country"].Value);
                country.TailNumbers.LastTailNumber = tailnumberNode.Attributes["value"].Value;

            }

            Airliners.Clear();

            XmlNodeList airlinersList = root.SelectNodes("//airliners/airliner");

            foreach (XmlElement airlinerNode in airlinersList)
            {
                AirlinerType type = AirlinerTypes.GetType(airlinerNode.Attributes["type"].Value);
                string tailnumber = airlinerNode.Attributes["tailnumber"].Value;
                string last_service = airlinerNode.Attributes["last_service"].Value;
                DateTime built = DateTime.Parse(airlinerNode.Attributes["built"].Value);
                double flown = Convert.ToDouble(airlinerNode.Attributes["flown"].Value);

                Airliner airliner = new Airliner(type, tailnumber, built);
                airliner.Flown = flown;
                airliner.clearAirlinerClasses();

                XmlNodeList airlinerClassList = airlinerNode.SelectNodes("classes/class");

                foreach (XmlElement airlinerClassNode in airlinerClassList)
                {
                    AirlinerClass.ClassType airlinerClassType = (AirlinerClass.ClassType)Enum.Parse(typeof(AirlinerClass.ClassType), airlinerClassNode.Attributes["type"].Value);
                    int airlinerClassSeating = Convert.ToInt16(airlinerClassNode.Attributes["seating"].Value);

                    AirlinerClass aClass = new AirlinerClass(airliner, airlinerClassType, airlinerClassSeating);
                    // chs, 2011-13-10 added for loading of airliner facilities
                    XmlNodeList airlinerClassFacilitiesList = airlinerClassNode.SelectNodes("facilities/facility");
                    foreach (XmlElement airlinerClassFacilityNode in airlinerClassFacilitiesList)
                    {
                        AirlinerFacility.FacilityType airlinerFacilityType = (AirlinerFacility.FacilityType)Enum.Parse(typeof(AirlinerFacility.FacilityType), airlinerClassFacilityNode.Attributes["type"].Value);

                        AirlinerFacility aFacility = AirlinerFacilities.GetFacility(airlinerFacilityType, airlinerClassFacilityNode.Attributes["uid"].Value);
                        aClass.forceSetFacility(aFacility);
                    }


                    airliner.addAirlinerClass(aClass);
                }


                Airliners.AddAirliner(airliner);

            }


            Airlines.Clear();

            XmlNodeList airlinesList = root.SelectNodes("//airlines/airline");

            foreach (XmlElement airlineNode in airlinesList)
            {
                // chs, 2011-21-10 changed for the possibility of creating a new airline
                string airlineName = airlineNode.Attributes["name"].Value;
                string airlineIATA = airlineNode.Attributes["code"].Value;
                Country airlineCountry = Countries.GetCountry(airlineNode.Attributes["country"].Value);
                string color = airlineNode.Attributes["color"].Value;
                string logo = AppSettings.getDataPath() + "\\graphics\\airlinelogos\\" + airlineNode.Attributes["logo"].Value;
                string airlineCEO = airlineNode.Attributes["CEO"].Value;
                double money = XmlConvert.ToDouble(airlineNode.Attributes["money"].Value);
                int reputation = Convert.ToInt16(airlineNode.Attributes["reputation"].Value);
                Airline.AirlineMentality mentality = (Airline.AirlineMentality)Enum.Parse(typeof(Airline.AirlineMentality), airlineNode.Attributes["mentality"].Value);
                Airline.AirlineMarket market = (Airline.AirlineMarket)Enum.Parse(typeof(Airline.AirlineMarket), airlineNode.Attributes["market"].Value);

                Airline airline = new Airline(new AirlineProfile(airlineName, airlineIATA, color, airlineCountry, airlineCEO), mentality, market);
                airline.Profile.Logo = logo;
                airline.Fleet.Clear();
                airline.Airports.Clear();
                airline.Routes.Clear();

                airline.Money = money;
                airline.Reputation = reputation;

                // chs, 2011-17-10 added for loading of passenger happiness
                XmlElement airlinePassengerNode = (XmlElement)airlineNode.SelectSingleNode("passengerhappiness");
                double passengerHappiness = XmlConvert.ToDouble(airlinePassengerNode.Attributes["value"].Value);
                PassengerHelpers.SetPassengerHappiness(airline, passengerHappiness);

                XmlNodeList airlineFacilitiesList = airlineNode.SelectNodes("facilities/facility");
                foreach (XmlElement airlineFacilityNode in airlineFacilitiesList)
                {
                    string airlineFacility = airlineFacilityNode.Attributes["uid"].Value;

                    airline.addFacility(AirlineFacilities.GetFacility(airlineFacility));
                }

                XmlNodeList airlineLoanList = airlineNode.SelectNodes("loans/loan");
                foreach (XmlElement airlineLoanNode in airlineLoanList)
                {
                    DateTime date = Convert.ToDateTime(airlineLoanNode.Attributes["date"].Value);
                    double rate = Convert.ToDouble(airlineLoanNode.Attributes["rate"].Value, new CultureInfo("de-DE", false));
                    double amount = XmlConvert.ToDouble(airlineLoanNode.Attributes["amount"].Value);
                    int length = Convert.ToInt16(airlineLoanNode.Attributes["length"].Value);
                    double payment = XmlConvert.ToDouble(airlineLoanNode.Attributes["payment"].Value);

                    Loan loan = new Loan(date, amount, length, rate);
                    loan.PaymentLeft = payment;

                    airline.addLoan(loan);
                }

                XmlNodeList airlineStatList = airlineNode.SelectNodes("stats/stat");
                foreach (XmlElement airlineStatNode in airlineStatList)
                {
                    int year = Convert.ToInt32(airlineStatNode.Attributes["year"].Value);
                    string airlineStatType = airlineStatNode.Attributes["type"].Value;
                    int value = Convert.ToInt32(airlineStatNode.Attributes["value"].Value);

                    airline.Statistics.setStatisticsValue(year, StatisticsTypes.GetStatisticsType(airlineStatType), value);
                }

                XmlNodeList airlineInvoiceList = airlineNode.SelectNodes("invoices/invoice");

                foreach (XmlElement airlineInvoiceNode in airlineInvoiceList)
                {
                    Invoice.InvoiceType type = (Invoice.InvoiceType)Enum.Parse(typeof(Invoice.InvoiceType), airlineInvoiceNode.Attributes["type"].Value);
                    DateTime invoiceDate = DateTime.Parse(airlineInvoiceNode.Attributes["date"].Value, new CultureInfo("de-DE", false));
                    double invoiceAmount = XmlConvert.ToDouble(airlineInvoiceNode.Attributes["amount"].Value);


                    airline.setInvoice(new Invoice(invoiceDate, type, invoiceAmount));
                }

                // chs, 2011-13-10 added for loading of airline advertisements
                XmlNodeList advertisementList = airlineNode.SelectNodes("advertisements/advertisement");

                foreach (XmlElement advertisementNode in advertisementList)
                {
                    AdvertisementType.AirlineAdvertisementType type = (AdvertisementType.AirlineAdvertisementType)Enum.Parse(typeof(AdvertisementType.AirlineAdvertisementType), advertisementNode.Attributes["type"].Value);
                    string advertisementName = advertisementNode.Attributes["name"].Value;

                    airline.setAirlineAdvertisement(AdvertisementTypes.GetType(type, advertisementName));
                }
                // chs, 2011-17-10 added for loading of fees
                XmlNodeList airlineFeeList = airlineNode.SelectNodes("fees/fee");
                foreach (XmlElement feeNode in airlineFeeList)
                {
                    string feeType = feeNode.Attributes["type"].Value;
                    double feeValue = Convert.ToDouble(feeNode.Attributes["value"].Value);

                    airline.Fees.setValue(FeeTypes.GetType(feeType), feeValue);
                }

                XmlNodeList airlineFleetList = airlineNode.SelectNodes("fleet/airliner");

                foreach (XmlElement airlineAirlinerNode in airlineFleetList)
                {
                    Airliner airliner = Airliners.GetAirliner(airlineAirlinerNode.Attributes["airliner"].Value);
                    string fAirlinerName = airlineAirlinerNode.Attributes["name"].Value;
                    Airport homebase = Airports.GetAirport(airlineAirlinerNode.Attributes["homebase"].Value);
                    FleetAirliner.PurchasedType purchasedtype = (FleetAirliner.PurchasedType)Enum.Parse(typeof(FleetAirliner.PurchasedType), airlineAirlinerNode.Attributes["purchased"].Value);
                    DateTime purchasedDate = DateTime.Parse(airlineAirlinerNode.Attributes["date"].Value, new CultureInfo("de-DE", false));
                    FleetAirliner.AirlinerStatus status = (FleetAirliner.AirlinerStatus)Enum.Parse(typeof(FleetAirliner.AirlinerStatus), airlineAirlinerNode.Attributes["status"].Value);

                    Coordinate latitude = Coordinate.Parse(airlineAirlinerNode.Attributes["latitude"].Value);
                    Coordinate longitude = Coordinate.Parse(airlineAirlinerNode.Attributes["longitude"].Value);

                    FleetAirliner fAirliner = new FleetAirliner(purchasedtype, purchasedDate, airline, airliner, fAirlinerName, homebase);
                    fAirliner.CurrentPosition = new Coordinates(latitude, longitude);
                    fAirliner.Status = status;


                    XmlNodeList airlinerStatList = airlineAirlinerNode.SelectNodes("stats/stat");

                    foreach (XmlElement airlinerStatNode in airlinerStatList)
                    {
                        int year = Convert.ToInt32(airlinerStatNode.Attributes["year"].Value);
                        string statType = airlinerStatNode.Attributes["type"].Value;
                        double statValue = Convert.ToDouble(airlinerStatNode.Attributes["value"].Value);
                        fAirliner.Statistics.setStatisticsValue(year, StatisticsTypes.GetStatisticsType(statType), statValue);
                    }




                    airline.addAirliner(fAirliner);

                }
                XmlNodeList routeList = airlineNode.SelectNodes("routes/route");

                foreach (XmlElement routeNode in routeList)
                {
                    string id = routeNode.Attributes["id"].Value;
                    Airport dest1 = Airports.GetAirport(routeNode.Attributes["destination1"].Value);
                    Airport dest2 = Airports.GetAirport(routeNode.Attributes["destination2"].Value);

                    Route route = new Route(id, dest1, dest2, 0);
                    route.Classes.Clear();


                    XmlNodeList routeClassList = routeNode.SelectNodes("routeclasses/routeclass");

                    foreach (XmlElement routeClassNode in routeClassList)
                    {
                        AirlinerClass.ClassType airlinerClassType = (AirlinerClass.ClassType)Enum.Parse(typeof(AirlinerClass.ClassType), routeClassNode.Attributes["type"].Value);
                        double fareprice = Convert.ToDouble(routeClassNode.Attributes["fareprice"].Value, new CultureInfo("de-DE", false));
                        int cabincrew = Convert.ToInt16(routeClassNode.Attributes["cabincrew"].Value);
                        RouteFacility drinks = RouteFacilities.GetFacilities(RouteFacility.FacilityType.Drinks).Find(delegate(RouteFacility facility) { return facility.Name == routeClassNode.Attributes["drinks"].Value; });
                        RouteFacility food = RouteFacilities.GetFacilities(RouteFacility.FacilityType.Food).Find(delegate(RouteFacility facility) { return facility.Name == routeClassNode.Attributes["food"].Value; });
                        // chs, 2011-18-10 added for loading of type of seating
                        RouteAirlinerClass.SeatingType seatingType = (RouteAirlinerClass.SeatingType)Enum.Parse(typeof(RouteAirlinerClass.SeatingType), routeClassNode.Attributes["seating"].Value);

                        RouteAirlinerClass rClass = new RouteAirlinerClass(airlinerClassType, RouteAirlinerClass.SeatingType.Reserved_Seating, fareprice);
                        rClass.CabinCrew = cabincrew;
                        rClass.DrinksFacility = drinks;
                        rClass.FoodFacility = food;
                        rClass.Seating = seatingType;

                        route.addRouteAirlinerClass(rClass);

                    }

                    RouteTimeTable timeTable = new RouteTimeTable(route);

                    XmlNodeList timetableList = routeNode.SelectNodes("timetable/timetableentry");

                    foreach (XmlElement entryNode in timetableList)
                    {
                        Airport entryDest = Airports.GetAirport(entryNode.Attributes["destination"].Value);
                        string flightCode = entryNode.Attributes["flightcode"].Value;
                        DayOfWeek day = (DayOfWeek)Enum.Parse(typeof(DayOfWeek), entryNode.Attributes["day"].Value);
                        TimeSpan time = TimeSpan.Parse(entryNode.Attributes["time"].Value);
                        FleetAirliner airliner = entryNode.Attributes["airliner"].Value == "-" ? null : airline.Fleet.Find(a => a.Name == entryNode.Attributes["airliner"].Value); ;

                        RouteTimeTableEntry entry = new RouteTimeTableEntry(timeTable, day, time, new RouteEntryDestination(entryDest, flightCode));
                        entry.Airliner = airliner;

                        if (airliner != null && !airliner.Routes.Contains(route))
                            airliner.Routes.Add(route);

                        timeTable.addEntry(entry);
                    }
                    route.TimeTable = timeTable;


                    airline.addRoute(route);
                }

                XmlNodeList flightNodes = airlineNode.SelectNodes("flights/flight");
                foreach (XmlElement flightNode in flightNodes)
                {

                    FleetAirliner airliner = airline.Fleet.Find(a => a.Name == flightNode.Attributes["airliner"].Value);
                    Route route = airline.Routes.Find(r => r.Id == flightNode.Attributes["route"].Value);
                    string destination = flightNode.Attributes["destination"].Value;
                    DayOfWeek day = (DayOfWeek)Enum.Parse(typeof(DayOfWeek), flightNode.Attributes["day"].Value);
                    TimeSpan time = TimeSpan.Parse(flightNode.Attributes["time"].Value);
                    DateTime.Parse(flightNode.Attributes["flighttime"].Value, new CultureInfo("de-DE", false));

                    RouteTimeTableEntry rtte = route.TimeTable.Entries.Find(delegate(RouteTimeTableEntry e) { return e.Destination.FlightCode == destination && e.Day == day && e.Time == time; });

                    Flight currentFlight = new Flight(rtte);
                    currentFlight.Classes.Clear();

                    XmlNodeList flightClassList = flightNode.SelectNodes("flightclasses/flightclass");

                    foreach (XmlElement flightClassNode in flightClassList)
                    {
                        AirlinerClass.ClassType airlinerClassType = (AirlinerClass.ClassType)Enum.Parse(typeof(AirlinerClass.ClassType), flightClassNode.Attributes["type"].Value);
                        int flightPassengers = 200;//Convert.ToInt16(flightClassNode.Attributes["passengers"].Value);

                        currentFlight.Classes.Add(new FlightAirlinerClass(route.getRouteAirlinerClass(airlinerClassType), flightPassengers));/*Rettes*/
                    }
                    airliner.CurrentFlight = currentFlight;

                }



                Airlines.AddAirline(airline);


            }
            XmlNodeList airportsList = root.SelectNodes("//airports/airport");


            foreach (XmlElement airportNode in airportsList)
            {
                Airport airport = Airports.GetAirport(airportNode.Attributes["iata"].Value);
                GeneralHelpers.Size airportSize = (GeneralHelpers.Size)Enum.Parse(typeof(GeneralHelpers.Size), airportNode.Attributes["size"].Value);
                airport.Profile.Size = airportSize;


                XmlNodeList airportHubsList = airportNode.SelectNodes("hubs/hub");
                foreach (XmlElement airportHubElement in airportHubsList)
                {
                    Airline airline = Airlines.GetAirline(airportHubElement.Attributes["airline"].Value);
                    airport.Hubs.Add(new Hub(airline));
                }

                XmlNodeList airportStatList = airportNode.SelectNodes("stats/stat");

                foreach (XmlElement airportStatNode in airportStatList)
                {
                    int year = Convert.ToInt32(airportStatNode.Attributes["year"].Value);
                    Airline airline = Airlines.GetAirline(airportStatNode.Attributes["airline"].Value);
                    string statType = airportStatNode.Attributes["type"].Value;
                    int statValue = Convert.ToInt32(airportStatNode.Attributes["value"].Value);
                    airport.Statistics.setStatisticsValue(year, airline, StatisticsTypes.GetStatisticsType(statType), statValue);
                }

                XmlNodeList airportFacilitiesList = airportNode.SelectNodes("facilities/facility");

                foreach (XmlElement airportFacilityNode in airportFacilitiesList)
                {
                    Airline airline = Airlines.GetAirline(airportFacilityNode.Attributes["airline"].Value);
                    AirportFacility airportFacility = AirportFacilities.GetFacility(airportFacilityNode.Attributes["name"].Value);
                    DateTime finishedDate = DateTime.Parse(airportFacilityNode.Attributes["finished"].Value, new CultureInfo("de-DE", false));


                    airport.setAirportFacility(airline, airportFacility, finishedDate);
                }
                airport.Terminals.clear();

                XmlNodeList terminalsList = airportNode.SelectNodes("terminals/terminal");

                foreach (XmlElement terminalNode in terminalsList)
                {
                    DateTime deliveryDate = DateTime.Parse(terminalNode.Attributes["delivery"].Value, new CultureInfo("de-DE", false));
                    Airline owner = Airlines.GetAirline(terminalNode.Attributes["owner"].Value);
                    string terminalName = terminalNode.Attributes["name"].Value;
                    int gates = Convert.ToInt32(terminalNode.Attributes["totalgates"].Value);

                    Terminal terminal = new Terminal(airport, owner, terminalName, gates, deliveryDate);
                    terminal.Gates.clear();

                    XmlNodeList airportGatesList = terminalNode.SelectNodes("gates/gate");


                    foreach (XmlElement airportGateNode in airportGatesList)
                    {
                        DateTime gateDeliveryDate = DateTime.Parse(airportGateNode.Attributes["delivery"].Value, new CultureInfo("de-DE", false));
                        Gate gate = new Gate(airport, gateDeliveryDate);
                        if (airportGateNode.Attributes["airline"].Value.Length > 0)
                        {
                            Airline airline = Airlines.GetAirline(airportGateNode.Attributes["airline"].Value);
                            gate.Airline = airline;

                            if (airportGateNode.Attributes["route"].Value.Length > 0)
                            {
                                string routeId = airportGateNode.Attributes["route"].Value;
                                airline.Routes.Find(delegate(Route r) { return r.Id == airportGateNode.Attributes["route"].Value; });
                            }
                        }

                        terminal.Gates.addGate(gate);
                    }

                    airport.addTerminal(terminal);




                }


            }

            XmlNodeList airportDestinationsList = root.SelectNodes("//airportdestinations/airportdestination");

            foreach (XmlElement airportDestinationElement in airportDestinationsList)
            {
                Airport targetAirport = Airports.GetAirport(airportDestinationElement.Attributes["id"].Value);

                XmlNodeList destinationsList = airportDestinationElement.SelectNodes("destinations/destination");
                foreach (XmlElement destinationElement in destinationsList)
                {
                    Airport destAirport = Airports.GetAirport(destinationElement.Attributes["id"].Value);
                    GeneralHelpers.Rate rate = (GeneralHelpers.Rate)Enum.Parse(typeof(GeneralHelpers.Rate), destinationElement.Attributes["rate"].Value);
                    long destPassengers = Convert.ToInt64(destinationElement.Attributes["passengers"].Value);

                    targetAirport.addDestinationStatistics(destAirport, destPassengers);
                    targetAirport.addDestinationPassengersRate(new DestinationPassengers(destAirport, rate));
                }
            }

            Alliances.Clear();

            XmlNodeList alliancesList = root.SelectNodes("//alliances/alliance");

            foreach (XmlElement allianceNode in alliancesList)
            {
                string allianceName = allianceNode.Attributes["name"].Value;
                DateTime formationDate = DateTime.Parse(allianceNode.Attributes["formation"].Value, new CultureInfo("de-DE"));
                Alliance.AllianceType allianceType = (Alliance.AllianceType)Enum.Parse(typeof(Alliance.AllianceType), allianceNode.Attributes["type"].Value);
                Airport allianceHeadquarter = Airports.GetAirport(allianceNode.Attributes["headquarter"].Value);

                Alliance alliance = new Alliance(formationDate, allianceType, allianceName, allianceHeadquarter);

                XmlNodeList membersList = allianceNode.SelectNodes("//members/member");

                foreach (XmlElement memberNode in membersList)
                {
                    alliance.addMember(Airlines.GetAirline(memberNode.Attributes["iata"].Value));
                }

                XmlNodeList pendingsList = allianceNode.SelectNodes("//pendings/pending");

                foreach (XmlElement pendingNode in pendingsList)
                {
                    Airline pendingAirline = Airlines.GetAirline(pendingNode.Attributes["airline"].Value);
                    DateTime pendingDate = DateTime.Parse(pendingNode.Attributes["date"].Value, new CultureInfo("de-DE"));
                    PendingAllianceMember.AcceptType pendingType = (PendingAllianceMember.AcceptType)Enum.Parse(typeof(PendingAllianceMember.AcceptType), pendingNode.Attributes["type"].Value);

                    alliance.addPendingMember(new PendingAllianceMember(pendingDate, alliance, pendingAirline, pendingType));
                }

                Alliances.AddAlliance(alliance);
            }

            XmlElement gameSettingsNode = (XmlElement)root.SelectSingleNode("//gamesettings");

            GameObject.GetInstance().Name = gameSettingsNode.Attributes["name"].Value;

            Airline humanAirline = Airlines.GetAirline(gameSettingsNode.Attributes["human"].Value);
            GameObject.GetInstance().HumanAirline = humanAirline;

            // chs, 2011-19-10 change to DateTime.Parse and read it using specific culture info
            string dateString = gameSettingsNode.Attributes["time"].Value;
            DateTime gameTime = DateTime.Parse(gameSettingsNode.Attributes["time"].Value, new CultureInfo("de-DE"));
            GameObject.GetInstance().GameTime = gameTime;

            double fuelPrice = Convert.ToDouble(gameSettingsNode.Attributes["fuelprice"].Value);
            GameObject.GetInstance().FuelPrice = fuelPrice;

            GameTimeZone timezone = TimeZones.GetTimeZones().Find(delegate(GameTimeZone gtz) { return gtz.UTCOffset == TimeSpan.Parse(gameSettingsNode.Attributes["timezone"].Value); });
            GameObject.GetInstance().TimeZone = timezone;

            Settings.GetInstance().MailsOnLandings = Convert.ToBoolean(gameSettingsNode.Attributes["mailonlandings"].Value);

            SkinObject.GetInstance().setCurrentSkin(Skins.GetSkin(gameSettingsNode.Attributes["skin"].Value));
            Settings.GetInstance().AirportCodeDisplay = (Settings.AirportCode)Enum.Parse(typeof(Settings.AirportCode), gameSettingsNode.Attributes["airportcode"].Value);
            GameTimer.GetInstance().setGameSpeed((GeneralHelpers.GameSpeedValue)Enum.Parse(typeof(GeneralHelpers.GameSpeedValue), gameSettingsNode.Attributes["gamespeed"].Value));
            AppSettings.GetInstance().setLanguage(Languages.GetLanguage(gameSettingsNode.Attributes["language"].Value));

            XmlNodeList newsList = gameSettingsNode.SelectNodes("news/new");
            GameObject.GetInstance().NewsBox.clear();

            foreach (XmlElement newsNode in newsList)
            {
                DateTime newsDate = DateTime.Parse(newsNode.Attributes["date"].Value, new CultureInfo("de-DE", false));
                News.NewsType newsType = (News.NewsType)Enum.Parse(typeof(News.NewsType), newsNode.Attributes["type"].Value);
                string newsSubject = newsNode.Attributes["subject"].Value;
                string newsBody = newsNode.Attributes["body"].Value;
                Boolean newsIsRead = Convert.ToBoolean(newsNode.Attributes["isread"].Value);

                News news = new News(newsType, newsDate, newsSubject, newsBody);
                news.IsRead = newsIsRead;


                GameObject.GetInstance().NewsBox.addNews(news);

            }
            foreach (Airline airline in Airlines.GetAllAirlines())
            {
                foreach (Route route in airline.Routes)
                {
                    route.Destination1.Terminals.getEmptyGate(airline).Route = route;
                    route.Destination2.Terminals.getEmptyGate(airline).Route = route;

                }
            }


        }
        //append a file to the list of saved files
        public static void AppendSavedFile(string name, string file)
        {
            XmlDocument doc = new XmlDocument();
            doc.Load(AppSettings.getDataPath() + "\\saves\\saves.xml");
            XmlElement root = doc.DocumentElement;

            XmlNode node = root.SelectSingleNode("//saves");

            XmlElement e = doc.CreateElement("save");
            e.SetAttribute("name", name);
            e.SetAttribute("file", file);

            node.AppendChild(e);

            doc.Save(AppSettings.getDataPath() + "\\saves\\saves.xml");

        }
        //returns the names of the saved games
        public static List<KeyValuePair<string, string>> GetSavedGames()
        {
            List<KeyValuePair<string, string>> saves = new List<KeyValuePair<string, string>>();

            XmlDocument doc = new XmlDocument();
            doc.Load(AppSettings.getDataPath() + "\\saves\\saves.xml");
            XmlElement root = doc.DocumentElement;

            XmlNodeList savesList = root.SelectNodes("//saves/save");

            foreach (XmlElement saveNode in savesList)
            {
                string name = saveNode.Attributes["name"].Value;
                string file = saveNode.Attributes["file"].Value;
                saves.Add(new KeyValuePair<string, string>(name, file));
            }
            return saves;


        }
        //saves the game
        public static void SaveGame(string file)
        {
            string path = AppSettings.getDataPath() + "\\saves\\" + file + ".xml";

            XmlDocument xmlDoc = new XmlDocument();

            XmlTextWriter xmlWriter = new XmlTextWriter(path, System.Text.Encoding.UTF8);
            xmlWriter.Formatting = Formatting.Indented;
            xmlWriter.WriteProcessingInstruction("xml", "version='1.0' encoding='UTF-8'");
            xmlWriter.WriteStartElement("game");
            xmlWriter.Close();
            xmlDoc.Load(path);

            XmlNode root = xmlDoc.DocumentElement;

            XmlElement tailnumbersNode = xmlDoc.CreateElement("tailnumbers");

            foreach (Country country in Countries.GetAllCountries())
            {
                XmlElement tailnumberNode = xmlDoc.CreateElement("tailnumber");
                tailnumberNode.SetAttribute("country", country.Uid);
                tailnumberNode.SetAttribute("value", country.TailNumbers.LastTailNumber);

                tailnumbersNode.AppendChild(tailnumberNode);
            }
            root.AppendChild(tailnumbersNode);

            XmlElement airlinersNode = xmlDoc.CreateElement("airliners");

            foreach (Airliner airliner in Airliners.GetAllAirliners())
            {
                XmlElement airlinerNode = xmlDoc.CreateElement("airliner");
                airlinerNode.SetAttribute("type", airliner.Type.Name);
                airlinerNode.SetAttribute("tailnumber", airliner.TailNumber);
                airlinerNode.SetAttribute("last_service", airliner.LastServiceCheck.ToString());
                airlinerNode.SetAttribute("built", airliner.BuiltDate.ToShortDateString());
                airlinerNode.SetAttribute("flown", string.Format("{0:0}", airliner.Flown));

                XmlElement airlinerClassesNode = xmlDoc.CreateElement("classes");
                foreach (AirlinerClass aClass in airliner.Classes)
                {
                    XmlElement airlinerClassNode = xmlDoc.CreateElement("class");
                    airlinerClassNode.SetAttribute("type", aClass.Type.ToString());
                    airlinerClassNode.SetAttribute("seating", aClass.SeatingCapacity.ToString());

                    XmlElement airlinerClassFacilitiesNode = xmlDoc.CreateElement("facilities");
                    foreach (AirlinerFacility.FacilityType facilityType in Enum.GetValues(typeof(AirlinerFacility.FacilityType)))
                    {
                        AirlinerFacility acFacility = aClass.getFacility(facilityType);

                        XmlElement airlinerClassFacilityNode = xmlDoc.CreateElement("facility");
                        airlinerClassFacilityNode.SetAttribute("type", acFacility.Type.ToString());
                        airlinerClassFacilityNode.SetAttribute("uid", acFacility.Uid);

                        airlinerClassFacilitiesNode.AppendChild(airlinerClassFacilityNode);
                    }

                    airlinerClassNode.AppendChild(airlinerClassFacilitiesNode);

                    airlinerClassesNode.AppendChild(airlinerClassNode);
                }
                airlinerNode.AppendChild(airlinerClassesNode);

                airlinersNode.AppendChild(airlinerNode);
            }

            root.AppendChild(airlinersNode);

            XmlElement passengersNode = xmlDoc.CreateElement("passengers");



            XmlElement airlinesNode = xmlDoc.CreateElement("airlines");
            foreach (Airline airline in Airlines.GetAllAirlines())
            {
                // chs, 2011-21-10 changed for the possibility of creating a new airline
                XmlElement airlineNode = xmlDoc.CreateElement("airline");
                airlineNode.SetAttribute("name", airline.Profile.Name);
                airlineNode.SetAttribute("code", airline.Profile.IATACode);
                airlineNode.SetAttribute("country", airline.Profile.Country.Uid);
                airlineNode.SetAttribute("color", airline.Profile.Color);
                airlineNode.SetAttribute("logo", airline.Profile.Logo.Substring(airline.Profile.Logo.LastIndexOf('\\') + 1));
                airlineNode.SetAttribute("CEO", airline.Profile.CEO);
                airlineNode.SetAttribute("money", string.Format("{0:0}", airline.Money));
                airlineNode.SetAttribute("reputation", airline.Reputation.ToString());
                airlineNode.SetAttribute("mentality", airline.Mentality.ToString());
                airlineNode.SetAttribute("market", airline.MarketFocus.ToString());

                // chs, 2011-13-10 added for saving of passenger happiness
                XmlElement airlineHappinessNode = xmlDoc.CreateElement("passengerhappiness");
                airlineHappinessNode.SetAttribute("value", string.Format("{0:0}", PassengerHelpers.GetHappinessValue(airline).ToString()));

                airlineNode.AppendChild(airlineHappinessNode);

                XmlElement airlineFacilitiesNode = xmlDoc.CreateElement("facilities");

                foreach (AirlineFacility facility in airline.Facilities)
                {
                    XmlElement airlineFacilityNode = xmlDoc.CreateElement("facility");
                    airlineFacilityNode.SetAttribute("uid", facility.Uid);

                    airlineFacilitiesNode.AppendChild(airlineFacilityNode);
                }
                airlineNode.AppendChild(airlineFacilitiesNode);


                XmlElement loansNode = xmlDoc.CreateElement("loans");
                foreach (Loan loan in airline.Loans)
                {
                    XmlElement loanNode = xmlDoc.CreateElement("loan");
                    loanNode.SetAttribute("date", loan.Date.ToShortDateString());
                    loanNode.SetAttribute("rate", loan.Rate.ToString());
                    loanNode.SetAttribute("amount", loan.Amount.ToString());
                    loanNode.SetAttribute("length", loan.Length.ToString());
                    loanNode.SetAttribute("payment", loan.PaymentLeft.ToString());

                    loansNode.AppendChild(loanNode);
                }
                airlineNode.AppendChild(loansNode);


                XmlElement airlineStatsNode = xmlDoc.CreateElement("stats");
                foreach (StatisticsType type in StatisticsTypes.GetStatisticsTypes())
                {
                    foreach (int year in airline.Statistics.getYears())
                    {
                        XmlElement airlineStatNode = xmlDoc.CreateElement("stat");

                        double value = airline.Statistics.getStatisticsValue(year, type);

                        airlineStatNode.SetAttribute("year", year.ToString());
                        airlineStatNode.SetAttribute("type", type.Shortname);
                        airlineStatNode.SetAttribute("value", value.ToString());

                        airlineStatsNode.AppendChild(airlineStatNode);
                    }
                }
                airlineNode.AppendChild(airlineStatsNode);

                XmlElement invoicesNode = xmlDoc.CreateElement("invoices");
                foreach (Invoice invoice in airline.getInvoices())
                {
                    XmlElement invoiceNode = xmlDoc.CreateElement("invoice");
                    invoiceNode.SetAttribute("type", invoice.Type.ToString());
                    invoiceNode.SetAttribute("date", invoice.Date.ToString(new CultureInfo("de-DE", false)));
                    invoiceNode.SetAttribute("amount", string.Format("{0:0}", invoice.Amount));

                    invoicesNode.AppendChild(invoiceNode);
                }
                airlineNode.AppendChild(invoicesNode);

                // chs, 2011-14-10 added for saving of airline advertisement
                XmlElement advertisementsNodes = xmlDoc.CreateElement("advertisements");
                foreach (AdvertisementType.AirlineAdvertisementType type in Enum.GetValues(typeof(AdvertisementType.AirlineAdvertisementType)))
                {
                    XmlElement advertisementNode = xmlDoc.CreateElement("advertisement");
                    advertisementNode.SetAttribute("type", type.ToString());
                    advertisementNode.SetAttribute("name", airline.getAirlineAdvertisement(type).Name);

                    advertisementsNodes.AppendChild(advertisementNode);
                }
                airlineNode.AppendChild(advertisementsNodes);

                // chs, 2011-17-10 added for saving of airline fees
                XmlElement feesNode = xmlDoc.CreateElement("fees");
                foreach (FeeType feetype in FeeTypes.GetTypes())
                {
                    XmlElement feeNode = xmlDoc.CreateElement("fee");
                    feeNode.SetAttribute("type", feetype.Name);
                    feeNode.SetAttribute("value", airline.Fees.getValue(feetype).ToString());

                    feesNode.AppendChild(feeNode);
                }



                airlineNode.AppendChild(feesNode);

                XmlElement fleetNode = xmlDoc.CreateElement("fleet");
                foreach (FleetAirliner airliner in airline.Fleet)
                {
                    XmlElement fleetAirlinerNode = xmlDoc.CreateElement("airliner");
                    fleetAirlinerNode.SetAttribute("airliner", airliner.Airliner.TailNumber);
                    fleetAirlinerNode.SetAttribute("name", airliner.Name);
                    fleetAirlinerNode.SetAttribute("homebase", airliner.Homebase.Profile.IATACode);
                    fleetAirlinerNode.SetAttribute("purchased", airliner.Purchased.ToString());
                    fleetAirlinerNode.SetAttribute("date", airliner.PurchasedDate.ToString(new CultureInfo("de-DE")));
                    fleetAirlinerNode.SetAttribute("status", airliner.Status.ToString());
                    fleetAirlinerNode.SetAttribute("latitude", airliner.CurrentPosition.Latitude.ToString());
                    fleetAirlinerNode.SetAttribute("longitude", airliner.CurrentPosition.Longitude.ToString());

                    XmlElement airlinerStatsNode = xmlDoc.CreateElement("stats");
                    foreach (StatisticsType type in StatisticsTypes.GetStatisticsTypes())
                    {
                        foreach (int year in airliner.Statistics.getYears())
                        {
                            XmlElement airlinerStatNode = xmlDoc.CreateElement("stat");

                            double value = airliner.Statistics.getStatisticsValue(year, type);
                            airlinerStatNode.SetAttribute("year", year.ToString());
                            airlinerStatNode.SetAttribute("type", type.Shortname);
                            airlinerStatNode.SetAttribute("value", value.ToString());

                            airlinerStatsNode.AppendChild(airlinerStatNode);
                        }
                    }






                    fleetAirlinerNode.AppendChild(airlinerStatsNode);

                    fleetNode.AppendChild(fleetAirlinerNode);
                }
                airlineNode.AppendChild(fleetNode);

                XmlElement routesNode = xmlDoc.CreateElement("routes");
                foreach (Route route in airline.Routes)
                {
                    XmlElement routeNode = xmlDoc.CreateElement("route");
                    routeNode.SetAttribute("id", route.Id);
                    routeNode.SetAttribute("destination1", route.Destination1.Profile.IATACode);
                    routeNode.SetAttribute("destination2", route.Destination2.Profile.IATACode);

                    XmlElement routeClassesNode = xmlDoc.CreateElement("routeclasses");

                    foreach (RouteAirlinerClass aClass in route.Classes)
                    {
                        XmlElement routeClassNode = xmlDoc.CreateElement("routeclass");
                        routeClassNode.SetAttribute("type", aClass.Type.ToString());
                        routeClassNode.SetAttribute("fareprice", string.Format("{0:0.##}", aClass.FarePrice));
                        routeClassNode.SetAttribute("cabincrew", aClass.CabinCrew.ToString());
                        routeClassNode.SetAttribute("drinks", aClass.DrinksFacility.Name);
                        routeClassNode.SetAttribute("food", aClass.FoodFacility.Name);
                        // chs, 2011-18-10 added for saving of type of seating
                        routeClassNode.SetAttribute("seating", aClass.Seating.ToString());

                        routeClassesNode.AppendChild(routeClassNode);
                    }
                    routeNode.AppendChild(routeClassesNode);

                    XmlElement timetableNode = xmlDoc.CreateElement("timetable");

                    foreach (RouteTimeTableEntry entry in route.TimeTable.Entries)
                    {
                        XmlElement ttEntryNode = xmlDoc.CreateElement("timetableentry");
                        ttEntryNode.SetAttribute("destination", entry.Destination.Airport.Profile.IATACode);
                        ttEntryNode.SetAttribute("flightcode", entry.Destination.FlightCode);
                        ttEntryNode.SetAttribute("day", entry.Day.ToString());
                        ttEntryNode.SetAttribute("time", entry.Time.ToString());
                        ttEntryNode.SetAttribute("airliner", entry.Airliner != null ? entry.Airliner.Name : "-");

                        timetableNode.AppendChild(ttEntryNode);
                    }

                    routeNode.AppendChild(timetableNode);



                    routesNode.AppendChild(routeNode);
                }
                airlineNode.AppendChild(routesNode);

                XmlElement flightsNode = xmlDoc.CreateElement("flights");
                foreach (FleetAirliner airliner in airline.Fleet)
                {
                    if (airliner.CurrentFlight != null)
                    {
                        XmlElement flightNode = xmlDoc.CreateElement("flight");

                        flightNode.SetAttribute("airliner", airliner.Name);
                        flightNode.SetAttribute("route", airliner.CurrentFlight.Entry.TimeTable.Route.Id);
                        flightNode.SetAttribute("destination", airliner.CurrentFlight.Entry.Destination.FlightCode);
                        flightNode.SetAttribute("day", airliner.CurrentFlight.Entry.Day.ToString());
                        flightNode.SetAttribute("time", airliner.CurrentFlight.Entry.Time.ToString());
                        flightNode.SetAttribute("flighttime", airliner.CurrentFlight.FlightTime.ToString(new CultureInfo("de-DE")));

                        XmlElement flightClassesNode = xmlDoc.CreateElement("flightclasses");
                        foreach (FlightAirlinerClass aClass in airliner.CurrentFlight.Classes)
                        {
                            XmlElement flightClassNode = xmlDoc.CreateElement("flightclass");
                            flightClassNode.SetAttribute("type", aClass.AirlinerClass.Type.ToString());
                            flightClassNode.SetAttribute("passengers", aClass.Passengers.ToString());

                            flightClassesNode.AppendChild(flightClassNode);
                        }
                        flightNode.AppendChild(flightClassesNode);

                        flightsNode.AppendChild(flightNode);

                    }
                }

                airlineNode.AppendChild(flightsNode);


                airlinesNode.AppendChild(airlineNode);
            }
            root.AppendChild(airlinesNode);


            XmlElement airportsNode = xmlDoc.CreateElement("airports");
            foreach (Airport airport in Airports.GetAllAirports())
            {
                XmlElement airportNode = xmlDoc.CreateElement("airport");
                airportNode.SetAttribute("iata", airport.Profile.IATACode);
                airportNode.SetAttribute("size", airport.Profile.Size.ToString());

                XmlElement airportHubsNode = xmlDoc.CreateElement("hubs");
                foreach (Hub hub in airport.Hubs)
                {
                    XmlElement airportHubNode = xmlDoc.CreateElement("hub");
                    airportHubNode.SetAttribute("airline", hub.Airline.Profile.IATACode);

                    airportHubsNode.AppendChild(airportHubNode);
                }

                airportNode.AppendChild(airportHubsNode);

                XmlElement airportStatsNode = xmlDoc.CreateElement("stats");
                foreach (Airline airline in Airlines.GetAllAirlines())
                {
                    foreach (StatisticsType type in StatisticsTypes.GetStatisticsTypes())
                    {
                        foreach (int year in airport.Statistics.getYears())
                        {
                            XmlElement airportStatNode = xmlDoc.CreateElement("stat");

                            double value = airport.Statistics.getStatisticsValue(year, airline, type);
                            airportStatNode.SetAttribute("year", year.ToString());
                            airportStatNode.SetAttribute("airline", airline.Profile.IATACode);
                            airportStatNode.SetAttribute("type", type.Shortname);
                            airportStatNode.SetAttribute("value", value.ToString());

                            airportStatsNode.AppendChild(airportStatNode);
                        }
                    }
                }

                airportNode.AppendChild(airportStatsNode);

                XmlElement airportFacilitiesNode = xmlDoc.CreateElement("facilities");
                foreach (Airline airline in Airlines.GetAllAirlines())
                {
                    foreach (AirlineAirportFacility facility in airport.getAirportFacilities(airline))
                    {
                        XmlElement airportFacilityNode = xmlDoc.CreateElement("facility");
                        airportFacilityNode.SetAttribute("airline", airline.Profile.IATACode);
                        airportFacilityNode.SetAttribute("name", facility.Facility.Shortname);
                        airportFacilityNode.SetAttribute("finished", facility.FinishedDate.ToString(new CultureInfo("de-DE")));

                        airportFacilitiesNode.AppendChild(airportFacilityNode);
                    }
                }
                airportNode.AppendChild(airportFacilitiesNode);


                // chs, 2011-02-11 added for saving the terminals
                XmlElement terminalsNode = xmlDoc.CreateElement("terminals");
                foreach (Terminal terminal in airport.Terminals.getTerminals())
                {
                    XmlElement terminalNode = xmlDoc.CreateElement("terminal");
                    terminalNode.SetAttribute("delivery", terminal.DeliveryDate.ToString(new CultureInfo("de-DE")));
                    terminalNode.SetAttribute("owner", terminal.Airline == null ? "airport" : terminal.Airline.Profile.IATACode);
                    terminalNode.SetAttribute("name", terminal.Name);
                    terminalNode.SetAttribute("totalgates", terminal.Gates.getGates().Count.ToString());
                    XmlElement gatesNode = xmlDoc.CreateElement("gates");
                    foreach (Gate gate in terminal.Gates.getGates())
                    {
                        XmlElement gateNode = xmlDoc.CreateElement("gate");
                        gateNode.SetAttribute("delivery", gate.DeliveryDate.ToString(new CultureInfo("de-DE")));
                        gateNode.SetAttribute("airline", gate.Airline == null ? "" : gate.Airline.Profile.IATACode);
                        gateNode.SetAttribute("route", gate.Route == null ? "" : gate.Route.Id);

                        gatesNode.AppendChild(gateNode);
                    }
                    terminalNode.AppendChild(gatesNode);

                    terminalsNode.AppendChild(terminalNode);
                }
                airportNode.AppendChild(terminalsNode);


                airportsNode.AppendChild(airportNode);



            }
            root.AppendChild(airportsNode);

            XmlElement airportDestinationsNode = xmlDoc.CreateElement("airportdestinations");
            foreach (Airport airport in Airports.GetAllAirports())
            {
                XmlElement airportDestinationNode = xmlDoc.CreateElement("airportdestination");
                airportDestinationNode.SetAttribute("id", airport.Profile.IATACode);

                XmlElement destinationsNode = xmlDoc.CreateElement("destinations");
                foreach (Airport dest in Airports.GetAirports(a => a != airport))
                {
                    XmlElement destinationNode = xmlDoc.CreateElement("destination");
                    destinationNode.SetAttribute("id", dest.Profile.IATACode);
                    destinationNode.SetAttribute("rate", airport.getDestinationPassengersRate(dest, AirlinerClass.ClassType.Economy_Class).ToString());
                    destinationNode.SetAttribute("passengers", airport.getDestinationStatistics(dest).ToString());

                    destinationsNode.AppendChild(destinationNode);
                }
                airportDestinationNode.AppendChild(destinationsNode);
                airportDestinationsNode.AppendChild(airportDestinationNode);
            }

            root.AppendChild(airportDestinationsNode);


            XmlElement alliancesNode = xmlDoc.CreateElement("alliances");
            foreach (Alliance alliance in Alliances.GetAlliances())
            {
                XmlElement allianceNode = xmlDoc.CreateElement("alliance");
                allianceNode.SetAttribute("name", alliance.Name);
                allianceNode.SetAttribute("formation", alliance.FormationDate.ToString(new CultureInfo("de-DE")));
                allianceNode.SetAttribute("type", alliance.Type.ToString());
                allianceNode.SetAttribute("headquarter", alliance.Headquarter.Profile.IATACode);

                XmlElement membersNode = xmlDoc.CreateElement("members");

                foreach (Airline airline in alliance.Members)
                {
                    XmlElement memberNode = xmlDoc.CreateElement("member");
                    memberNode.SetAttribute("iata", airline.Profile.IATACode);

                    membersNode.AppendChild(memberNode);
                }
                allianceNode.AppendChild(membersNode);

                XmlElement pendingsNode = xmlDoc.CreateElement("pendings");

                foreach (PendingAllianceMember pending in alliance.PendingMembers)
                {

                    XmlElement pendingNode = xmlDoc.CreateElement("pending");
                    pendingNode.SetAttribute("airline", pending.Airline.Profile.IATACode);
                    pendingNode.SetAttribute("date", pending.Date.ToString(new CultureInfo("de-DE")));
                    pendingNode.SetAttribute("type", pending.Type.ToString());

                    pendingsNode.AppendChild(pendingNode);
                }
                allianceNode.AppendChild(pendingsNode);

                alliancesNode.AppendChild(allianceNode);
            }
            root.AppendChild(alliancesNode);

            XmlElement gameSettingsNode = xmlDoc.CreateElement("gamesettings");
            gameSettingsNode.SetAttribute("name", GameObject.GetInstance().Name);
            gameSettingsNode.SetAttribute("human", GameObject.GetInstance().HumanAirline.Profile.IATACode);
            gameSettingsNode.SetAttribute("time", GameObject.GetInstance().GameTime.ToString(new CultureInfo("de-DE")));
            gameSettingsNode.SetAttribute("fuelprice", GameObject.GetInstance().FuelPrice.ToString());
            gameSettingsNode.SetAttribute("timezone", GameObject.GetInstance().TimeZone.UTCOffset.ToString());
            gameSettingsNode.SetAttribute("mailonlandings", Settings.GetInstance().MailsOnLandings.ToString());
            gameSettingsNode.SetAttribute("skin", SkinObject.GetInstance().CurrentSkin.Name);
            gameSettingsNode.SetAttribute("airportcode", Settings.GetInstance().AirportCodeDisplay.ToString());
            gameSettingsNode.SetAttribute("gamespeed", GameTimer.GetInstance().GameSpeed.ToString());
            gameSettingsNode.SetAttribute("language", AppSettings.GetInstance().getLanguage().Name);

            XmlElement newsNodes = xmlDoc.CreateElement("news");

            foreach (News news in GameObject.GetInstance().NewsBox.getNews())
            {
                XmlElement newsNode = xmlDoc.CreateElement("new");
                // chs, 2011-19-10 change to use a specific culture info for saving date format      
                newsNode.SetAttribute("date", news.Date.ToString(new CultureInfo("de-DE")));
                newsNode.SetAttribute("type", news.Type.ToString());
                newsNode.SetAttribute("subject", news.Subject);
                newsNode.SetAttribute("body", news.Body);
                newsNode.SetAttribute("isread", news.IsRead.ToString());

                newsNodes.AppendChild(newsNode);
            }
            gameSettingsNode.AppendChild(newsNodes);

            root.AppendChild(gameSettingsNode);


            xmlDoc.Save(path);


        }
    }
}
