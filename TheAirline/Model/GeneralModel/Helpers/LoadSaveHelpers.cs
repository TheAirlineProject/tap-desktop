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

namespace TheAirline.Model.GeneralModel.Helpers
{
    public class LoadSaveHelpers
    {
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
                double flown = Convert.ToDouble(airlinerNode.Attributes["flown"].Value);//Convert.ToDouble(airlinerNode.Attributes["flown"].Value, new CultureInfo("de-DE", false));//XmlConvert.ToDouble(airlinerNode.Attributes["flown"].Value);

                Airliner airliner = new Airliner(type, tailnumber, built);
                airliner.Flown = flown;
                airliner.clearAirlinerClasses();

                XmlNodeList airlinerClassList = airlinerNode.SelectNodes("classes/class");

                foreach (XmlElement airlinerClassNode in airlinerClassList)
                {
                    AirlinerClass.ClassType airlinerClassType =  (AirlinerClass.ClassType)Enum.Parse(typeof(AirlinerClass.ClassType), airlinerClassNode.Attributes["type"].Value);
                    int airlinerClassSeating = Convert.ToInt16(airlinerClassNode.Attributes["seating"].Value);

                    AirlinerClass aClass = new AirlinerClass(airliner,airlinerClassType,airlinerClassSeating);
                    // chs, 2011-13-10 added for loading of airliner facilities
                    XmlNodeList airlinerClassFacilitiesList = airlinerClassNode.SelectNodes("facilities/facility");
                    foreach (XmlElement airlinerClassFacilityNode in airlinerClassFacilitiesList)
                    {
                          AirlinerFacility.FacilityType airlinerFacilityType =  (AirlinerFacility.FacilityType)Enum.Parse(typeof(AirlinerFacility.FacilityType), airlinerClassFacilityNode.Attributes["type"].Value);
                 
                        AirlinerFacility aFacility = AirlinerFacilities.GetFacility(airlinerFacilityType,airlinerClassFacilityNode.Attributes["uid"].Value);
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
                string logo = airlineNode.Attributes["logo"].Value;
                string airlineCEO = airlineNode.Attributes["CEO"].Value;
                double money = XmlConvert.ToDouble(airlineNode.Attributes["money"].Value);
                int reputation = Convert.ToInt16(airlineNode.Attributes["reputation"].Value);

            

                Airline airline = new Airline(new AirlineProfile(airlineName,airlineIATA,color,airlineCountry,airlineCEO));
                airline.Profile.Logo = logo;
                airline.Fleet.Clear();
                airline.Airports.Clear();
                airline.Routes.Clear();
              
                airline.Money = money;
                airline.Reputation = reputation;
     
                // chs, 2011-17-10 added for loading of passenger happiness
                XmlElement airlinePassengerNode = (XmlElement)airlineNode.SelectSingleNode("passengerhappiness");
                double passengerHappiness = Convert.ToDouble(airlinePassengerNode.Attributes["value"].Value);
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
                    string airlineStatType = airlineStatNode.Attributes["type"].Value;
                    int value = Convert.ToInt32(airlineStatNode.Attributes["value"].Value);

                    airline.Statistics.setStatisticsValue(StatisticsTypes.GetStatisticsType(airlineStatType), value);
                }

                XmlNodeList airlineInvoiceList = airlineNode.SelectNodes("invoices/invoice");
    
                foreach (XmlElement airlineInvoiceNode in airlineInvoiceList)
                {
                    Invoice.InvoiceType type = (Invoice.InvoiceType)Enum.Parse(typeof(Invoice.InvoiceType), airlineInvoiceNode.Attributes["type"].Value);
                    DateTime invoiceDate = DateTime.Parse(airlineInvoiceNode.Attributes["date"].Value,new CultureInfo("de-DE", false));
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

                    airline.Fees.setValue(FeeTypes.GetType(feeType),feeValue);
                }

                XmlNodeList airlineFleetList = airlineNode.SelectNodes("fleet/airliner");

                foreach (XmlElement airlineAirlinerNode in airlineFleetList)
                {
                    Airliner airliner = Airliners.GetAirliner(airlineAirlinerNode.Attributes["airliner"].Value);
                    string fAirlinerName = airlineAirlinerNode.Attributes["name"].Value;
                    Airport homebase = Airports.GetAirport(airlineAirlinerNode.Attributes["homebase"].Value);
                    FleetAirliner.PurchasedType purchasedtype = (FleetAirliner.PurchasedType)Enum.Parse(typeof(FleetAirliner.PurchasedType), airlineAirlinerNode.Attributes["purchased"].Value);
                   
                    FleetAirliner fAirliner = new FleetAirliner(purchasedtype,airline,airliner,fAirlinerName,homebase);
                              
                    XmlNodeList airlinerStatList = airlineAirlinerNode.SelectNodes("stats/stat");

                    foreach (XmlElement airlinerStatNode in airlinerStatList)
                    {
                        string statType = airlinerStatNode.Attributes["type"].Value;
                        int statValue = Convert.ToInt32(airlinerStatNode.Attributes["value"].Value);
                        fAirliner.Statistics.setStatisticsValue(StatisticsTypes.GetStatisticsType(statType),statValue);
                    }
                              
                    airline.addAirliner(fAirliner);

                }
                XmlNodeList routeList = airlineNode.SelectNodes("routes/route");

                foreach (XmlElement routeNode in routeList)
                {
                    string id = routeNode.Attributes["id"].Value;
                    Airport dest1 = Airports.GetAirport(routeNode.Attributes["destination1"].Value);
                    Airport dest2 = Airports.GetAirport(routeNode.Attributes["destination2"].Value);
                    
                    Route route = new Route(id, dest1, dest2,0,"","");
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
  
                        RouteAirlinerClass rClass = new RouteAirlinerClass(airlinerClassType,RouteAirlinerClass.SeatingType.Reserved_Seating, fareprice);
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

                        timeTable.addEntry(new RouteTimeTableEntry(timeTable, day, time, new RouteEntryDestination(entryDest, flightCode)));
                    }
                    route.TimeTable = timeTable;

                    XmlElement routeAirlinerNode = (XmlElement)routeNode.SelectSingleNode("routeairliner");

                    if (routeAirlinerNode != null)
                    {
                        FleetAirliner fAirliner = airline.Fleet.Find(delegate(FleetAirliner fa) { return fa.Name == routeAirlinerNode.Attributes["airliner"].Value; });
                        
                        RouteAirliner.AirlinerStatus status = (RouteAirliner.AirlinerStatus)Enum.Parse(typeof(RouteAirliner.AirlinerStatus), routeAirlinerNode.Attributes["status"].Value);
                        Coordinate latitude = Coordinate.Parse(routeAirlinerNode.Attributes["latitude"].Value);
                        Coordinate longitude = Coordinate.Parse(routeAirlinerNode.Attributes["longitude"].Value);

                        RouteAirliner rAirliner = new RouteAirliner(fAirliner, route);
                        rAirliner.CurrentPosition = new Coordinates(latitude, longitude);
                        rAirliner.Status = status;
                        
                        XmlElement flightNode = (XmlElement)routeAirlinerNode.SelectSingleNode("flight");
                        if (flightNode != null)
                        {

                            string destination = flightNode.Attributes["destination"].Value;
                            DayOfWeek day = (DayOfWeek)Enum.Parse(typeof(DayOfWeek), flightNode.Attributes["day"].Value);
                            TimeSpan time = TimeSpan.Parse(flightNode.Attributes["time"].Value);

                            RouteTimeTableEntry rtte = timeTable.Entries.Find(delegate(RouteTimeTableEntry e) { return e.Destination.FlightCode == destination && e.Day == day && e.Time == time; }); 

                            Flight currentFlight = new Flight(rtte);
                            currentFlight.Classes.Clear();
                         
                            XmlNodeList flightClassList = flightNode.SelectNodes("flightclasses/flightclass");

                            foreach (XmlElement flightClassNode in flightClassList)
                            {
                                AirlinerClass.ClassType airlinerClassType = (AirlinerClass.ClassType)Enum.Parse(typeof(AirlinerClass.ClassType), flightClassNode.Attributes["type"].Value);
                                int passengers = Convert.ToInt16(flightClassNode.Attributes["passengers"].Value);

                                currentFlight.Classes.Add(new FlightAirlinerClass(route.getRouteAirlinerClass(airlinerClassType),passengers));
                            }
                            rAirliner.CurrentFlight = currentFlight;
                        }
                    }

                    airline.addRoute(route);
                }


                Airlines.AddAirline(airline);

    
            }
            XmlNodeList airportsList = root.SelectNodes("//airports/airport");


            foreach (XmlElement airportNode in airportsList)
            {
                Airport airport = Airports.GetAirport(airportNode.Attributes["iata"].Value);
                AirportProfile.AirportSize airportSize = (AirportProfile.AirportSize)Enum.Parse(typeof(AirportProfile.AirportSize), airportNode.Attributes["size"].Value);
                airport.Profile.Size = airportSize;


                XmlNodeList airportStatList = airportNode.SelectNodes("stats/stat");

                foreach (XmlElement airportStatNode in airportStatList)
                {
                    Airline airline = Airlines.GetAirline(airportStatNode.Attributes["airline"].Value);
                    string statType = airportStatNode.Attributes["type"].Value; 
                    int statValue = Convert.ToInt32(airportStatNode.Attributes["value"].Value);
                    airport.Statistics.setStatisticsValue(airline,StatisticsTypes.GetStatisticsType(statType), statValue);
                }

                XmlNodeList airportFacilitiesList = airportNode.SelectNodes("facilities/facility");

                foreach (XmlElement airportFacilityNode in airportFacilitiesList)
                {
                    Airline airline = Airlines.GetAirline(airportFacilityNode.Attributes["airline"].Value);
                    AirportFacility airportFacility = AirportFacilities.GetFacility(airportFacilityNode.Attributes["name"].Value);

                    airport.setAirportFacility(airline, airportFacility);
                }
                airport.Terminals.clear();

                XmlNodeList terminalsList = airportNode.SelectNodes("terminals/terminal");
        
                foreach (XmlElement terminalNode in terminalsList)
                {
                    DateTime deliveryDate = DateTime.Parse(terminalNode.Attributes["delivery"].Value, new CultureInfo("de-DE", false));
                    Airline owner = Airlines.GetAirline(terminalNode.Attributes["owner"].Value);
                    string terminalName = terminalNode.Attributes["name"].Value;
                    int gates = Convert.ToInt32(terminalNode.Attributes["totalgates"].Value);

                    Terminal terminal = new Terminal(airport, owner,terminalName, gates, deliveryDate);
                    terminal.Gates.clear();

                    XmlNodeList airportGatesList = terminalNode.SelectNodes("gates/gate");

               
                    foreach (XmlElement airportGateNode in airportGatesList)
                    {
                        DateTime gateDeliveryDate = DateTime.Parse(airportGateNode.Attributes["delivery"].Value, new CultureInfo("de-DE", false));
                        Gate gate = new Gate(airport,gateDeliveryDate);
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

            GameTimeZone timezone = TimeZones.GetTimeZones().Find(delegate(GameTimeZone gtz) { return gtz.UTCOffset == TimeSpan.Parse(gameSettingsNode.Attributes["timezone"].Value);  });
            GameObject.GetInstance().TimeZone = timezone;

            Settings.GetInstance().MailsOnLandings = Convert.ToBoolean(gameSettingsNode.Attributes["mailonlandings"].Value);

            SkinObject.GetInstance().setCurrentSkin(Skins.GetSkin(gameSettingsNode.Attributes["skin"].Value));
            Settings.GetInstance().AirportCodeDisplay = (Settings.AirportCode)Enum.Parse(typeof(Settings.AirportCode), gameSettingsNode.Attributes["airportcode"].Value);
            GameTimer.GetInstance().setGameSpeed((GeneralHelpers.GameSpeedValue)Enum.Parse(typeof(GeneralHelpers.GameSpeedValue),gameSettingsNode.Attributes["gamespeed"].Value));
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
            foreach (Airline airline in Airlines.GetAirlines())
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

            foreach (Airliner airliner in Airliners.GetAirliners())
            {
                XmlElement airlinerNode = xmlDoc.CreateElement("airliner");
                airlinerNode.SetAttribute("type", airliner.Type.Name);
                airlinerNode.SetAttribute("tailnumber", airliner.TailNumber);
                airlinerNode.SetAttribute("last_service", airliner.LastServiceCheck.ToString());
                airlinerNode.SetAttribute("built", airliner.BuiltDate.ToShortDateString());
                airlinerNode.SetAttribute("flown", string.Format("{0:0}",airliner.Flown));

                XmlElement airlinerClassesNode = xmlDoc.CreateElement("classes");
                foreach (AirlinerClass aClass in airliner.Classes)
                {
                    XmlElement airlinerClassNode = xmlDoc.CreateElement("class");
                    airlinerClassNode.SetAttribute("type", aClass.Type.ToString());
                    airlinerClassNode.SetAttribute("seating", aClass.SeatingCapacity.ToString());
              
                    XmlElement airlinerClassFacilitiesNode = xmlDoc.CreateElement("facilities");
                    foreach (AirlinerFacility.FacilityType facilityType in Enum.GetValues(typeof(AirlinerFacility.FacilityType)))
                    {
                        AirlinerFacility acFacility= aClass.getFacility(facilityType);

                        XmlElement airlinerClassFacilityNode = xmlDoc.CreateElement("facility");
                        airlinerClassFacilityNode.SetAttribute("type", acFacility.Type.ToString());
                        airlinerClassFacilityNode.SetAttribute("uid",acFacility.Uid);
                  
                        airlinerClassFacilitiesNode.AppendChild(airlinerClassFacilityNode);
                    }

                    airlinerClassNode.AppendChild(airlinerClassFacilitiesNode);

                    airlinerClassesNode.AppendChild(airlinerClassNode);
                }
                airlinerNode.AppendChild(airlinerClassesNode);

                airlinersNode.AppendChild(airlinerNode);
            }

            root.AppendChild(airlinersNode);

      

            XmlElement airlinesNode = xmlDoc.CreateElement("airlines");
            foreach (Airline airline in Airlines.GetAirlines())
            {
                // chs, 2011-21-10 changed for the possibility of creating a new airline
                XmlElement airlineNode = xmlDoc.CreateElement("airline");
                airlineNode.SetAttribute("name", airline.Profile.Name);
                airlineNode.SetAttribute("code", airline.Profile.IATACode);
                airlineNode.SetAttribute("country", airline.Profile.Country.Uid);
                airlineNode.SetAttribute("color", airline.Profile.Color);
                airlineNode.SetAttribute("logo", airline.Profile.Logo);
                airlineNode.SetAttribute("CEO", airline.Profile.CEO);
                airlineNode.SetAttribute("money",string.Format("{0:0}",airline.Money));
                airlineNode.SetAttribute("reputation", airline.Reputation.ToString());

                // chs, 2011-13-10 added for saving of passenger happiness
                XmlElement airlineHappinessNode = xmlDoc.CreateElement("passengerhappiness");
                airlineHappinessNode.SetAttribute("value", PassengerHelpers.GetHappinessValue(airline).ToString());

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
                    XmlElement airlineStatNode = xmlDoc.CreateElement("stat");

                    int value = airline.Statistics.getStatisticsValue(type);
                    airlineStatNode.SetAttribute("type", type.Shortname);
                    airlineStatNode.SetAttribute("value", value.ToString());

                    airlineStatsNode.AppendChild(airlineStatNode);
                }
                airlineNode.AppendChild(airlineStatsNode);

                XmlElement invoicesNode = xmlDoc.CreateElement("invoices");
                foreach (Invoice invoice in airline.getInvoices())
                {
                    XmlElement invoiceNode = xmlDoc.CreateElement("invoice");
                    invoiceNode.SetAttribute("type", invoice.Type.ToString());
                    invoiceNode.SetAttribute("date", invoice.Date.ToString(new CultureInfo("de-DE", false)));
                    invoiceNode.SetAttribute("amount", string.Format("{0:0}",invoice.Amount));

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

                    XmlElement airlinerStatsNode = xmlDoc.CreateElement("stats");
                    foreach (StatisticsType type in StatisticsTypes.GetStatisticsTypes())
                    {
                        XmlElement airlinerStatNode = xmlDoc.CreateElement("stat");

                        int value = airliner.Statistics.getStatisticsValue(type);
                        airlinerStatNode.SetAttribute("type", type.Shortname);
                        airlinerStatNode.SetAttribute("value", value.ToString());

                        airlinerStatsNode.AppendChild(airlinerStatNode);
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
                        routeClassNode.SetAttribute("fareprice", string.Format("{0:0.##}",aClass.FarePrice));
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

                        timetableNode.AppendChild(ttEntryNode);
                    }

                    routeNode.AppendChild(timetableNode);

                    if (route.Airliner != null)
                    {
                        XmlElement routeAirlinerNode = xmlDoc.CreateElement("routeairliner");

                        routeAirlinerNode.SetAttribute("airliner", route.Airliner.Airliner.Name);
                        routeAirlinerNode.SetAttribute("status", route.Airliner.Status.ToString());
                        routeAirlinerNode.SetAttribute("latitude", route.Airliner.CurrentPosition.Latitude.ToString());
                        routeAirlinerNode.SetAttribute("longitude", route.Airliner.CurrentPosition.Longitude.ToString());

                        if (route.Airliner.CurrentFlight != null)
                        {
                            XmlElement flightNode = xmlDoc.CreateElement("flight");


                            flightNode.SetAttribute("destination", route.Airliner.CurrentFlight.Entry.Destination.FlightCode);
                            flightNode.SetAttribute("day", route.Airliner.CurrentFlight.Entry.Day.ToString());
                            flightNode.SetAttribute("time", route.Airliner.CurrentFlight.Entry.Time.ToString());


                            XmlElement flightClassesNode = xmlDoc.CreateElement("flightclasses");
                            foreach (FlightAirlinerClass aClass in route.Airliner.CurrentFlight.Classes)
                            {
                                XmlElement flightClassNode = xmlDoc.CreateElement("flightclass");
                                flightClassNode.SetAttribute("type", aClass.AirlinerClass.Type.ToString());
                                flightClassNode.SetAttribute("passengers", aClass.Passengers.ToString());

                                flightClassesNode.AppendChild(flightClassNode);
                            }
                            flightNode.AppendChild(flightClassesNode);

                            routeAirlinerNode.AppendChild(flightNode);

                        }


                        routeNode.AppendChild(routeAirlinerNode);

                    }

                    routesNode.AppendChild(routeNode);
                }
                airlineNode.AppendChild(routesNode);



                airlinesNode.AppendChild(airlineNode);
            }
            root.AppendChild(airlinesNode);


            XmlElement airportsNode = xmlDoc.CreateElement("airports");
            foreach (Airport airport in Airports.GetAirports())
            {
                XmlElement airportNode = xmlDoc.CreateElement("airport");
                airportNode.SetAttribute("iata", airport.Profile.IATACode);
                airportNode.SetAttribute("size", airport.Profile.Size.ToString());

                XmlElement airportStatsNode = xmlDoc.CreateElement("stats");
                foreach (Airline airline in Airlines.GetAirlines())
                {
                    foreach (StatisticsType type in StatisticsTypes.GetStatisticsTypes())
                    {
                        XmlElement airportStatNode = xmlDoc.CreateElement("stat");

                        int value = airport.Statistics.getStatisticsValue(airline, type);
                        airportStatNode.SetAttribute("airline", airline.Profile.IATACode);
                        airportStatNode.SetAttribute("type", type.Shortname);
                        airportStatNode.SetAttribute("value", value.ToString());

                        airportStatsNode.AppendChild(airportStatNode);
                    }
                }

                airportNode.AppendChild(airportStatsNode);

                XmlElement airportFacilitiesNode = xmlDoc.CreateElement("facilities");
                foreach (Airline airline in Airlines.GetAirlines())
                {
                    foreach (AirportFacility facility in airport.getAirportFacilities(airline))
                    {
                        XmlElement airportFacilityNode = xmlDoc.CreateElement("facility");
                        airportFacilityNode.SetAttribute("airline", airline.Profile.IATACode);
                        airportFacilityNode.SetAttribute("name", facility.Shortname);

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
