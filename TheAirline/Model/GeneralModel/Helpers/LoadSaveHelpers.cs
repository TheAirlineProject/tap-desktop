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
using TheAirline.Model.GeneralModel.InvoicesModel;
using TheAirline.Model.AirlineModel.SubsidiaryModel;
using TheAirline.Model.GeneralModel.WeatherModel;

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
            string fileName = AppSettings.getDataPath() + "\\saves\\" + name + ".sav";

            XmlDocument doc = new XmlDocument();

            using (FileStream fs = new FileStream(fileName, FileMode.Open))
            {
                Stream s;

                s = new GZipStream(fs, CompressionMode.Decompress);
                doc.Load(s);
                s.Close();



            }
            //doc.Load(AppSettings.getDataPath() + "\\saves\\" + name + ".xml");
            XmlElement root = doc.DocumentElement;


            DateTime gameTime = DateTime.Parse(root.Attributes["time"].Value, new CultureInfo("de-DE"));
            GameObject.GetInstance().GameTime = gameTime;


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
                double damaged = Convert.ToDouble(airlinerNode.Attributes["damaged"].Value);

                Airliner airliner = new Airliner(type, tailnumber, built);
                airliner.Damaged = damaged;
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

            XmlNodeList airlinesList = root.SelectNodes("//airlines/airline[@subsidiary='False']");

            foreach (XmlElement airlineNode in airlinesList)
                LoadAirline(airlineNode);

            XmlNodeList subsidiaryList = root.SelectNodes("//airlines/airline[@subsidiary='True']");

            foreach (XmlElement airlineNode in subsidiaryList)
                LoadAirline(airlineNode);
         
            
            XmlNodeList airportsList = root.SelectNodes("//airports/airport");


            foreach (XmlElement airportNode in airportsList)
            {
                Airport airport = Airports.GetAirportFromID(airportNode.Attributes["id"].Value);
                
                GeneralHelpers.Size airportSize = (GeneralHelpers.Size)Enum.Parse(typeof(GeneralHelpers.Size), airportNode.Attributes["size"].Value);
                airport.Profile.Size = airportSize;
                airport.Income = Convert.ToInt64(airportNode.Attributes["income"].Value);

                XmlNodeList airportHubsList = airportNode.SelectNodes("hubs/hub");
                airport.Hubs.Clear();

                foreach (XmlElement airportHubElement in airportHubsList)
                {
                    Airline airline = Airlines.GetAirline(airportHubElement.Attributes["airline"].Value);
                    airport.Hubs.Add(new Hub(airline));
                }

                XmlNodeList airportWeatherList = airportNode.SelectNodes("weathers/weather");

                for (int i=0;i<airportWeatherList.Count;i++)
                {
                    XmlElement airportWeatherElement = airportWeatherList[i] as XmlElement;

                    DateTime weatherDate = DateTime.Parse(airportWeatherElement.Attributes["date"].Value, new CultureInfo("de-DE", false));
                    Weather.WindDirection windDirection = (Weather.WindDirection)Enum.Parse(typeof(Weather.WindDirection), airportWeatherElement.Attributes["direction"].Value);
                    Weather.eWindSpeed windSpeed = (Weather.eWindSpeed)Enum.Parse(typeof(Weather.eWindSpeed), airportWeatherElement.Attributes["windspeed"].Value);
                    Weather.CloudCover cover = airportWeatherElement.HasAttribute("cover")? (Weather.CloudCover)Enum.Parse(typeof(Weather.CloudCover),airportWeatherElement.Attributes["cover"].Value) : Weather.CloudCover.Clear;
                    Weather.Precipitation precip = airportWeatherElement.HasAttribute("precip") ? (Weather.Precipitation)Enum.Parse(typeof(Weather.Precipitation), airportWeatherElement.Attributes["precip"].Value) : Weather.Precipitation.None;
                    double temperatureLow =  airportWeatherElement.HasAttribute("temperatureLow") ? Convert.ToDouble(airportWeatherElement.Attributes["temperaturelow"].Value) : 0;
                    double temperatureHigh = airportWeatherElement.HasAttribute("temperatureHigh") ? Convert.ToDouble(airportWeatherElement.Attributes["temperaturehigh"].Value) : 20;

                    XmlNodeList airportTemperatureList = airportNode.SelectNodes("temperatures/temperature");
                    HourlyWeather[] temperatures = new HourlyWeather[airportTemperatureList.Count];
              
                    int t=0;
                    foreach (XmlElement airportTemperatureNode in airportTemperatureList)
                    {
                        double hourlyTemperature = Convert.ToDouble(airportTemperatureNode.Attributes["temp"].Value);
                        Weather.CloudCover hourlyCover = (Weather.CloudCover)Enum.Parse(typeof(Weather.CloudCover), airportTemperatureNode.Attributes["cover"].Value);
                        Weather.Precipitation hourlyPrecip = (Weather.Precipitation)Enum.Parse(typeof(Weather.Precipitation), airportTemperatureNode.Attributes["precip"].Value);
                        Weather.eWindSpeed hourlyWindspeed = (Weather.eWindSpeed)Enum.Parse(typeof(Weather.eWindSpeed),airportTemperatureNode.Attributes["windspeed"].Value);
                        Weather.WindDirection hourlyDirection = (Weather.WindDirection)Enum.Parse(typeof(Weather.WindDirection),airportTemperatureNode.Attributes["direction"].Value);

                        temperatures[t] = new HourlyWeather(hourlyTemperature, hourlyCover, hourlyPrecip,hourlyWindspeed,hourlyDirection);
                        t++;
                    }
                   
                 
                    airport.Weather[i] = new Weather(weatherDate,windSpeed,windDirection,cover,precip,temperatures,temperatureLow,temperatureHigh);
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
                airport.clearFacilities();

                foreach (XmlElement airportFacilityNode in airportFacilitiesList)
                {
                    Airline airline = Airlines.GetAirline(airportFacilityNode.Attributes["airline"].Value);
                    AirportFacility airportFacility = AirportFacilities.GetFacility(airportFacilityNode.Attributes["name"].Value);
                    DateTime finishedDate = DateTime.Parse(airportFacilityNode.Attributes["finished"].Value, new CultureInfo("de-DE", false));

                    airport.addAirportFacility(airline, airportFacility, finishedDate);
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

                            gate.HasRoute = Convert.ToBoolean(airportGateNode.Attributes["route"].Value);

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

                if (targetAirport != null)
                {
                    targetAirport.clearDestinationPassengers();

                    XmlNodeList destinationsList = airportDestinationElement.SelectNodes("destinations/destination");
                    foreach (XmlElement destinationElement in destinationsList)
                    {
                        Airport destAirport = Airports.GetAirport(destinationElement.Attributes["id"].Value);

                        if (destAirport != null)
                        {
                            ushort rate = ushort.Parse(destinationElement.Attributes["rate"].Value);
                            AirlinerClass.ClassType classtype = (AirlinerClass.ClassType)Enum.Parse(typeof(AirlinerClass.ClassType), destinationElement.Attributes["classtype"].Value);
                            long destPassengers = Convert.ToInt64(destinationElement.Attributes["passengers"].Value);

                            targetAirport.addDestinationStatistics(destAirport, destPassengers);
                            targetAirport.addDestinationPassengersRate(new DestinationPassengers(classtype, destAirport, rate));
                        }
                    }
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
            Configurations.Clear();

            XmlNodeList configurationsList = root.SelectNodes("//configurations/configuration");

            foreach (XmlElement confElement in configurationsList)
            {
                string confName = confElement.Attributes["name"].Value;
                string confid = confElement.Attributes["id"].Value;
                Boolean standard = Convert.ToBoolean(confElement.Attributes["standard"].Value);

                int minimumSeats = Convert.ToInt16(confElement.Attributes["minimumseats"].Value);

                AirlinerConfiguration configuration = new AirlinerConfiguration(confName, minimumSeats, standard);
                configuration.ID = confid;

                XmlNodeList classesList = confElement.SelectNodes("classes/class");

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
                Configurations.AddConfiguration(configuration);
            }

            XmlNodeList routeConfigurationsList = root.SelectNodes("//routeclassesconfigurations/routeclassesconfiguration");

            foreach (XmlElement confElement in routeConfigurationsList)
            {
                string routeConfName = confElement.Attributes["name"].Value;
                string confid = confElement.Attributes["id"].Value;
                Boolean standard = Convert.ToBoolean(confElement.Attributes["standard"].Value);

                XmlNodeList classesList = confElement.SelectNodes("classes/class");

                RouteClassesConfiguration classesConfiguration = new RouteClassesConfiguration(routeConfName, standard);
                classesConfiguration.ID = confid;

                foreach (XmlElement classElement in classesList)
                {
                    AirlinerClass.ClassType classType = (AirlinerClass.ClassType)Enum.Parse(typeof(AirlinerClass.ClassType), classElement.Attributes["type"].Value);

                    RouteClassConfiguration classConf = new RouteClassConfiguration(classType);
                    foreach (RouteFacility.FacilityType facType in Enum.GetValues(typeof(RouteFacility.FacilityType)))
                    {
                        if (classElement.HasAttribute(facType.ToString()))
                        {
                            string facilityName = classElement.Attributes[facType.ToString()].Value;

                            classConf.addFacility(RouteFacilities.GetFacilities(facType).Find(f => f.Name == facilityName));
                        }
                    }

                    classesConfiguration.addClass(classConf);
                }

                Configurations.AddConfiguration(classesConfiguration);
            }



            XmlElement gameSettingsNode = (XmlElement)root.SelectSingleNode("//gamesettings");

            GameObject.GetInstance().PassengerDemandFactor = Convert.ToDouble(gameSettingsNode.Attributes["passengerdemand"].Value);

            GameObject.GetInstance().Name = gameSettingsNode.Attributes["name"].Value;

            Airline humanAirline = Airlines.GetAirline(gameSettingsNode.Attributes["human"].Value);
            GameObject.GetInstance().HumanAirline = humanAirline;

            Airline mainAirline = Airlines.GetAirline(gameSettingsNode.Attributes["mainairline"].Value);
            GameObject.GetInstance().MainAirline = mainAirline;

            double fuelPrice = Convert.ToDouble(gameSettingsNode.Attributes["fuelprice"].Value);
            GameObject.GetInstance().FuelPrice = fuelPrice;

            GameTimeZone timezone = TimeZones.GetTimeZones().Find(delegate(GameTimeZone gtz) { return gtz.UTCOffset == TimeSpan.Parse(gameSettingsNode.Attributes["timezone"].Value); });
            GameObject.GetInstance().TimeZone = timezone;

            Settings.GetInstance().MailsOnLandings = Convert.ToBoolean(gameSettingsNode.Attributes["mailonlandings"].Value);
            Settings.GetInstance().MailsOnBadWeather = Convert.ToBoolean(gameSettingsNode.Attributes["mailonbadweather"].Value);

            SkinObject.GetInstance().setCurrentSkin(Skins.GetSkin(gameSettingsNode.Attributes["skin"].Value));
            Settings.GetInstance().AirportCodeDisplay = (Settings.AirportCode)Enum.Parse(typeof(Settings.AirportCode), gameSettingsNode.Attributes["airportcode"].Value);
            GameTimer.GetInstance().setGameSpeed((GeneralHelpers.GameSpeedValue)Enum.Parse(typeof(GeneralHelpers.GameSpeedValue), gameSettingsNode.Attributes["gamespeed"].Value));
            if (gameSettingsNode.HasAttribute("minutesperturn")) Settings.GetInstance().MinutesPerTurn = Convert.ToInt16(gameSettingsNode.Attributes["minutesperturn"].Value);
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
            /*
              foreach (Airline airline in Airlines.GetAllAirlines())
              {
                  foreach (Route route in airline.Routes)
                  {
                      Gate gate1 = route.Destination1.Terminals.getEmptyGate(airline);
                      Gate gate2 = route.Destination2.Terminals.getEmptyGate(airline);

                      if (gate1!=null) gate1.Route = route;
                      if (gate2!=null) gate2.Route = route;

                  }
              }
            
              */

        }
        //loads an airline from the saved file
        private static void LoadAirline(XmlElement airlineNode)
        {
            // chs, 2011-21-10 changed for the possibility of creating a new airline
            string airlineName = airlineNode.Attributes["name"].Value;
            string airlineIATA = airlineNode.Attributes["code"].Value;
            Boolean airlineIsSubsidiary = Convert.ToBoolean(airlineNode.Attributes["subsidiary"].Value);
            Country airlineCountry = Countries.GetCountry(airlineNode.Attributes["country"].Value);
            string color = airlineNode.Attributes["color"].Value;
            string logo = AppSettings.getDataPath() + "\\graphics\\airlinelogos\\" + airlineNode.Attributes["logo"].Value;
            string airlineCEO = airlineNode.Attributes["CEO"].Value;
            double money = XmlConvert.ToDouble(airlineNode.Attributes["money"].Value);
            int reputation = Convert.ToInt16(airlineNode.Attributes["reputation"].Value);
            Airline.AirlineMentality mentality = (Airline.AirlineMentality)Enum.Parse(typeof(Airline.AirlineMentality), airlineNode.Attributes["mentality"].Value);
            Airline.AirlineFocus market = (Airline.AirlineFocus)Enum.Parse(typeof(Airline.AirlineFocus), airlineNode.Attributes["market"].Value);

            Boolean isReal = airlineNode.HasAttribute("real") ? Convert.ToBoolean(airlineNode.Attributes["real"].Value) : true;
            int founded =airlineNode.HasAttribute("founded") ? Convert.ToInt16(airlineNode.Attributes["founded"].Value) : 1950;
            int folded = airlineNode.HasAttribute("folded") ? Convert.ToInt16(airlineNode.Attributes["folded"].Value) : 2199;
      
            Airline airline;
            if (airlineIsSubsidiary)
            {
                Airline parent = Airlines.GetAirline(airlineNode.Attributes["parentairline"].Value);
                airline = new SubsidiaryAirline(parent, new AirlineProfile(airlineName, airlineIATA, color, airlineCountry, airlineCEO,isReal,founded,folded), mentality, market);
                parent.addSubsidiaryAirline((SubsidiaryAirline)airline);
            }
            else
                airline = new Airline(new AirlineProfile(airlineName, airlineIATA, color, airlineCountry, airlineCEO,isReal,founded,folded), mentality, market);

            airline.Profile.Logo = logo;
            airline.Fleet.Clear();
            airline.Airports.Clear();
            airline.Routes.Clear();

            airline.Money = money;
            airline.Reputation = reputation;

            XmlElement airlineContractNode = (XmlElement)airlineNode.SelectSingleNode("contract");
            if (airlineContractNode != null)
            {
                Manufacturer contractManufacturer = Manufacturers.GetManufacturer(airlineContractNode.Attributes["manufacturer"].Value);
                DateTime contractSigningDate = Convert.ToDateTime(airlineContractNode.Attributes["signingdate"].Value);
                int contractLength = Convert.ToInt16(airlineContractNode.Attributes["length"].Value);
                double contractDiscount = Convert.ToDouble(airlineContractNode.Attributes["discount"].Value);
                int contractAirliners = Convert.ToInt16(airlineContractNode.Attributes["airliners"].Value);

                ManufacturerContract contract = new ManufacturerContract(contractManufacturer, contractSigningDate, contractLength, contractDiscount);
                contract.PurchasedAirliners = contractAirliners;

                airline.Contract = contract;

            }

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
                DateTime date = DateTime.Parse(airlineLoanNode.Attributes["date"].Value, new CultureInfo("de-DE", false));
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
                int invoiceYear = Convert.ToInt16(airlineInvoiceNode.Attributes["year"].Value);
                int invoiceMonth = Convert.ToInt16(airlineInvoiceNode.Attributes["month"].Value);
                double invoiceAmount = XmlConvert.ToDouble(airlineInvoiceNode.Attributes["amount"].Value);

                airline.setInvoice(type, invoiceYear, invoiceMonth, invoiceAmount);
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
            AirlineFees fees = new AirlineFees();

            XmlNodeList airlineFeeList = airlineNode.SelectNodes("fees/fee");
            foreach (XmlElement feeNode in airlineFeeList)
            {
                string feeType = feeNode.Attributes["type"].Value;
                double feeValue = Convert.ToDouble(feeNode.Attributes["value"].Value);

                fees.setValue(FeeTypes.GetType(feeType), feeValue);
            }

            airline.Fees = fees;


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
                Boolean isBanned = Convert.ToBoolean(routeNode.Attributes["isbanned"].Value);

                Route route = new Route(id, dest1, dest2, 0);
                route.Banned = isBanned;
                route.Classes.Clear();


                XmlNodeList routeClassList = routeNode.SelectNodes("routeclasses/routeclass");

                foreach (XmlElement routeClassNode in routeClassList)
                {
                    AirlinerClass.ClassType airlinerClassType = (AirlinerClass.ClassType)Enum.Parse(typeof(AirlinerClass.ClassType), routeClassNode.Attributes["type"].Value);
                    double fareprice = Convert.ToDouble(routeClassNode.Attributes["fareprice"].Value);
                    //RouteFacility drinks = RouteFacilities.GetFacilities(RouteFacility.FacilityType.Drinks).Find(delegate(RouteFacility facility) { return facility.Name == routeClassNode.Attributes["drinks"].Value; });
                    //RouteFacility food = RouteFacilities.GetFacilities(RouteFacility.FacilityType.Food).Find(delegate(RouteFacility facility) { return facility.Name == routeClassNode.Attributes["food"].Value; });
                    // chs, 2011-18-10 added for loading of type of seating
                    RouteAirlinerClass.SeatingType seatingType = (RouteAirlinerClass.SeatingType)Enum.Parse(typeof(RouteAirlinerClass.SeatingType), routeClassNode.Attributes["seating"].Value);

                    RouteAirlinerClass rClass = new RouteAirlinerClass(airlinerClassType, RouteAirlinerClass.SeatingType.Reserved_Seating, fareprice);
                    rClass.Seating = seatingType;

                    foreach (RouteFacility.FacilityType ftype in Enum.GetValues(typeof(RouteFacility.FacilityType)))
                    {
                        if (routeClassNode.HasAttribute(ftype.ToString()))
                        {
                            RouteFacility facility = RouteFacilities.GetFacility(routeClassNode.Attributes[ftype.ToString()].Value);
                            rClass.addFacility(facility);
                        }
                    }

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

                XmlNodeList routeInvoiceList = routeNode.SelectNodes("invoices/invoice");

                foreach (XmlElement routeInvoiceNode in routeInvoiceList)
                {
                    Invoice.InvoiceType type = (Invoice.InvoiceType)Enum.Parse(typeof(Invoice.InvoiceType), routeInvoiceNode.Attributes["type"].Value);
                    int invoiceYear = Convert.ToInt16(routeInvoiceNode.Attributes["year"].Value);
                    int invoiceMonth = Convert.ToInt16(routeInvoiceNode.Attributes["month"].Value);
                    double invoiceAmount = XmlConvert.ToDouble(routeInvoiceNode.Attributes["amount"].Value);

                    route.setRouteInvoice(type, invoiceYear, invoiceMonth, invoiceAmount);
                }

                airline.addRoute(route);
            }

            XmlNodeList flightNodes = airlineNode.SelectNodes("flights/flight");
            foreach (XmlElement flightNode in flightNodes)
            {

                FleetAirliner airliner = airline.Fleet.Find(a => a.Name == flightNode.Attributes["airliner"].Value);
                Route route = airline.Routes.Find(r => r.Id == flightNode.Attributes["route"].Value);

                if (route != null)
                {
                    string destination = flightNode.Attributes["destination"].Value;

                    DayOfWeek day = (DayOfWeek)Enum.Parse(typeof(DayOfWeek), flightNode.Attributes["day"].Value);
                    TimeSpan time = TimeSpan.Parse(flightNode.Attributes["time"].Value);
                    DateTime flightTime = DateTime.Parse(flightNode.Attributes["flighttime"].Value, new CultureInfo("de-DE", false));



                    if (destination != "Service")
                    {
                        RouteTimeTableEntry rtte = route.TimeTable.Entries.Find(delegate(RouteTimeTableEntry e) { return e.Destination.FlightCode == destination && e.Day == day && e.Time == time; });

                        Flight currentFlight = new Flight(rtte);
                        currentFlight.FlightTime = flightTime;
                        currentFlight.Classes.Clear();

                        XmlNodeList flightClassList = flightNode.SelectNodes("flightclasses/flightclass");

                        foreach (XmlElement flightClassNode in flightClassList)
                        {
                            AirlinerClass.ClassType airlinerClassType = (AirlinerClass.ClassType)Enum.Parse(typeof(AirlinerClass.ClassType), flightClassNode.Attributes["type"].Value);
                            int flightPassengers = Convert.ToInt16(flightClassNode.Attributes["passengers"].Value);

                            currentFlight.Classes.Add(new FlightAirlinerClass(route.getRouteAirlinerClass(airlinerClassType), flightPassengers));/*Rettes*/
                        }
                        airliner.CurrentFlight = currentFlight;
                    }
                    else
                    {
                        
                        airliner.CurrentFlight = new Flight(new RouteTimeTableEntry(route.TimeTable, GameObject.GetInstance().GameTime.DayOfWeek, GameObject.GetInstance().GameTime.TimeOfDay, new RouteEntryDestination(airliner.Homebase, "Service")));

                        airliner.Status = FleetAirliner.AirlinerStatus.On_service;
                    }



                }
                else
                    airliner.Status = FleetAirliner.AirlinerStatus.Stopped;
            }


            Airlines.AddAirline(airline);
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
            string path = AppSettings.getDataPath() + "\\saves\\" + file + ".sav";

            XmlDocument xmlDoc = new XmlDocument();

            XmlDeclaration xmlDeclaration = xmlDoc.CreateXmlDeclaration("1.0","UTF-8", null);
            xmlDoc.AppendChild(xmlDeclaration);

            XmlElement root = xmlDoc.CreateElement("game");
            xmlDoc.AppendChild(root);
            /*
            XmlTextWriter xmlWriter = new XmlTextWriter(path, System.Text.Encoding.UTF8);
            xmlWriter.Formatting = Formatting.Indented;
            xmlWriter.WriteProcessingInstruction("xml", "version='1.0' encoding='UTF-8'");
            xmlWriter.WriteStartElement("game");
            xmlWriter.Close();
            xmlDoc.Load(path);
            */

            root.SetAttribute("time", GameObject.GetInstance().GameTime.ToString(new CultureInfo("de-DE")));


            XmlElement tailnumbersNode = xmlDoc.CreateElement("tailnumbers");

            foreach (Country country in Countries.GetAllCountries())
            {
                XmlElement tailnumberNode = xmlDoc.CreateElement("tailnumber");
                tailnumberNode.SetAttribute("country", country.Uid);
                tailnumberNode.SetAttribute("value", country.TailNumbers.LastTailNumber == null ? country.TailNumbers.getNextTailNumber() : country.TailNumbers.LastTailNumber);


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
                airlinerNode.SetAttribute("damaged", airliner.Damaged.ToString());

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
                airlineNode.SetAttribute("subsidiary", airline.IsSubsidiary.ToString());
                if (airline.IsSubsidiary)
                    airlineNode.SetAttribute("parentairline", ((SubsidiaryAirline)airline).Airline.Profile.IATACode);

                airlineNode.SetAttribute("country", airline.Profile.Country.Uid);
                airlineNode.SetAttribute("color", airline.Profile.Color);
                airlineNode.SetAttribute("logo", airline.Profile.Logo.Substring(airline.Profile.Logo.LastIndexOf('\\') + 1));
                airlineNode.SetAttribute("CEO", airline.Profile.CEO);
                airlineNode.SetAttribute("money", string.Format("{0:0}", airline.Money));
                airlineNode.SetAttribute("reputation", airline.Reputation.ToString());
                airlineNode.SetAttribute("mentality", airline.Mentality.ToString());
                airlineNode.SetAttribute("market", airline.MarketFocus.ToString());
                airlineNode.SetAttribute("isreal", airline.Profile.IsReal.ToString());
                airlineNode.SetAttribute("founded", airline.Profile.Founded.ToString());
                airlineNode.SetAttribute("folded", airline.Profile.Folded.ToString());
              
                if (airline.Contract != null)
                {
                    XmlElement airlineContractNode = xmlDoc.CreateElement("contract");
                    airlineContractNode.SetAttribute("manufacturer", airline.Contract.Manufacturer.ShortName);
                    airlineContractNode.SetAttribute("signingdate", airline.Contract.SigningDate.ToShortDateString());
                    airlineContractNode.SetAttribute("length", airline.Contract.Length.ToString());
                    airlineContractNode.SetAttribute("discount", airline.Contract.Discount.ToString());
                    airlineContractNode.SetAttribute("airliners", airline.Contract.PurchasedAirliners.ToString());

                    airlineNode.AppendChild(airlineContractNode);
                }

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
                foreach (MonthlyInvoice invoice in airline.getInvoices().MonthlyInvoices)
                {
                    XmlElement invoiceNode = xmlDoc.CreateElement("invoice");
                    invoiceNode.SetAttribute("type", invoice.Type.ToString());
                    invoiceNode.SetAttribute("year", invoice.Year.ToString());
                    invoiceNode.SetAttribute("month", invoice.Month.ToString());
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
                    routeNode.SetAttribute("isbanned", route.Banned.ToString());

                    XmlElement routeClassesNode = xmlDoc.CreateElement("routeclasses");

                    foreach (RouteAirlinerClass aClass in route.Classes)
                    {
                        XmlElement routeClassNode = xmlDoc.CreateElement("routeclass");
                        routeClassNode.SetAttribute("type", aClass.Type.ToString());
                        routeClassNode.SetAttribute("fareprice", string.Format("{0:0.##}", aClass.FarePrice));

                        foreach (RouteFacility facility in aClass.getFacilities())
                            routeClassNode.SetAttribute(facility.Type.ToString(), facility.Uid);
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

                    XmlElement routeInvoicesNode = xmlDoc.CreateElement("invoices");
                    foreach (MonthlyInvoice invoice in route.getInvoices().MonthlyInvoices)
                    {
                        XmlElement routeInvoiceNode = xmlDoc.CreateElement("invoice");
                        routeInvoiceNode.SetAttribute("type", invoice.Type.ToString());
                        routeInvoiceNode.SetAttribute("year", invoice.Year.ToString());
                        routeInvoiceNode.SetAttribute("month", invoice.Month.ToString());
                        routeInvoiceNode.SetAttribute("amount", string.Format("{0:0}", invoice.Amount));

                        routeInvoicesNode.AppendChild(routeInvoiceNode);
                    }
                    routesNode.AppendChild(routeInvoicesNode);

                    routesNode.AppendChild(routeNode);
                }
                airlineNode.AppendChild(routesNode);

                XmlElement flightsNode = xmlDoc.CreateElement("flights");
                foreach (FleetAirliner airliner in airline.Fleet)
                {
                    if (airliner.CurrentFlight != null && airliner.CurrentFlight.Entry != null)
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
                airportNode.SetAttribute("id", airport.Profile.ID);
                airportNode.SetAttribute("size", airport.Profile.Size.ToString());
                airportNode.SetAttribute("income", airport.Income.ToString());

                XmlElement airportHubsNode = xmlDoc.CreateElement("hubs");
                foreach (Hub hub in airport.Hubs)
                {
                    XmlElement airportHubNode = xmlDoc.CreateElement("hub");
                    airportHubNode.SetAttribute("airline", hub.Airline.Profile.IATACode);

                    airportHubsNode.AppendChild(airportHubNode);
                }

                airportNode.AppendChild(airportHubsNode);

                XmlElement airportWeathersNode = xmlDoc.CreateElement("weathers");
                foreach (Weather weather in airport.Weather)
                {
                    XmlElement airportWeatherNode = xmlDoc.CreateElement("weather");
                    airportWeatherNode.SetAttribute("date", weather.Date.ToString(new CultureInfo("de-DE")));
                    airportWeatherNode.SetAttribute("direction", weather.Direction.ToString());
                    airportWeatherNode.SetAttribute("windspeed", weather.WindSpeed.ToString());
                    airportWeatherNode.SetAttribute("cover", weather.Cover.ToString());
                    airportWeatherNode.SetAttribute("precip", weather.Precip.ToString());
                    airportWeatherNode.SetAttribute("temperaturelow", weather.TemperatureLow.ToString());
                    airportWeatherNode.SetAttribute("temperaturehigh", weather.TemperatureHigh.ToString());

                    XmlElement temperaturesNode = xmlDoc.CreateElement("temperatures");
                    for (int i = 0; i < weather.Temperatures.Length; i++)
                    {
                        XmlElement temperatureNode = xmlDoc.CreateElement("temperature");
                        temperatureNode.SetAttribute("temp", weather.Temperatures[i].ToString());
                        temperatureNode.SetAttribute("cover", weather.Temperatures[i].Cover.ToString());
                        temperatureNode.SetAttribute("precip", weather.Temperatures[i].Precip.ToString());
                        temperatureNode.SetAttribute("windspeed", weather.Temperatures[i].WindSpeed.ToString());
                        temperatureNode.SetAttribute("direction", weather.Temperatures[i].Direction.ToString());

                        temperaturesNode.AppendChild(temperatureNode);
                
                    }
                    airportWeatherNode.AppendChild(temperaturesNode);

                    airportWeathersNode.AppendChild(airportWeatherNode);
                }
                airportNode.AppendChild(airportWeathersNode);

                XmlElement airportStatsNode = xmlDoc.CreateElement("stats");
                foreach (Airline airline in Airlines.GetAllAirlines())
                {
                    foreach (StatisticsType type in StatisticsTypes.GetStatisticsTypes())
                    {
                        foreach (int year in airport.Statistics.getYears())
                        {
                            double value = airport.Statistics.getStatisticsValue(year, airline, type);
                            
                            if (value > 0)
                            {
                                XmlElement airportStatNode = xmlDoc.CreateElement("stat");

                                airportStatNode.SetAttribute("year", year.ToString());
                                airportStatNode.SetAttribute("airline", airline.Profile.IATACode);
                                airportStatNode.SetAttribute("type", type.Shortname);
                                airportStatNode.SetAttribute("value", value.ToString());

                                airportStatsNode.AppendChild(airportStatNode);
                            }
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
                        gateNode.SetAttribute("route", gate.HasRoute.ToString());

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
                foreach (Airport dest in Airports.GetAirports(a => a != airport && (airport.hasDestinationPassengersRate(a) || airport.hasDestinationStatistics(a))))
                {
                    foreach (AirlinerClass.ClassType classType in Enum.GetValues(typeof(AirlinerClass.ClassType)))
                    {
                        XmlElement destinationNode = xmlDoc.CreateElement("destination");
                        destinationNode.SetAttribute("id", dest.Profile.IATACode);
                        destinationNode.SetAttribute("classtype", classType.ToString());
                        destinationNode.SetAttribute("rate", airport.getDestinationPassengersRate(dest, classType).ToString());
                        destinationNode.SetAttribute("passengers", airport.getDestinationStatistics(dest).ToString());

                        destinationsNode.AppendChild(destinationNode);
                    }
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

            XmlElement configurationsNode = xmlDoc.CreateElement("configurations");

            foreach (AirlinerConfiguration conf in Configurations.GetConfigurations(Configuration.ConfigurationType.Airliner))
            {
                XmlElement configurationNode = xmlDoc.CreateElement("configuration");
                configurationNode.SetAttribute("name", conf.Name);
                configurationNode.SetAttribute("id", conf.ID);
                configurationNode.SetAttribute("standard", conf.Standard.ToString());

                configurationNode.SetAttribute("minimumseats", conf.MinimumSeats.ToString());

                XmlElement classesNode = xmlDoc.CreateElement("classes");

                foreach (AirlinerClassConfiguration aClass in conf.Classes)
                {
                    XmlElement classNode = xmlDoc.CreateElement("class");
                    classNode.SetAttribute("seating", aClass.SeatingCapacity.ToString());
                    classNode.SetAttribute("regularseating", aClass.RegularSeatingCapacity.ToString());
                    classNode.SetAttribute("type", aClass.Type.ToString());

                    foreach (AirlinerFacility aFac in aClass.Facilities)
                    {
                        classNode.SetAttribute(aFac.Type.ToString(), aFac.Uid);
                    }

                    classesNode.AppendChild(classNode);


                }
                configurationNode.AppendChild(classesNode);

                configurationsNode.AppendChild(configurationNode);

            }

            root.AppendChild(configurationsNode);

            XmlElement routeConfigurationsNode = xmlDoc.CreateElement("routeclassesconfigurations");

            foreach (RouteClassesConfiguration configuration in Configurations.GetConfigurations(Configuration.ConfigurationType.Routeclasses))
            {
                XmlElement routeConfigurationNode = xmlDoc.CreateElement("routeclassesconfiguration");
                routeConfigurationNode.SetAttribute("name", configuration.Name);
                routeConfigurationNode.SetAttribute("id", configuration.ID);
                routeConfigurationNode.SetAttribute("standard", configuration.Standard.ToString());

                XmlElement classesNode = xmlDoc.CreateElement("classes");

                foreach (RouteClassConfiguration classConfiguration in configuration.getClasses())
                {
                    XmlElement classNode = xmlDoc.CreateElement("class");
                    classNode.SetAttribute("type", classConfiguration.Type.ToString());

                    foreach (RouteFacility aFac in classConfiguration.getFacilities())
                    {
                        classNode.SetAttribute(aFac.Type.ToString(), aFac.Name);
                    }

                    classesNode.AppendChild(classNode);
                }

                routeConfigurationNode.AppendChild(classesNode);
                routeConfigurationsNode.AppendChild(routeConfigurationNode);

            }

            root.AppendChild(routeConfigurationsNode);

            XmlElement gameSettingsNode = xmlDoc.CreateElement("gamesettings");
            gameSettingsNode.SetAttribute("passengerdemand", GameObject.GetInstance().PassengerDemandFactor.ToString());
            gameSettingsNode.SetAttribute("name", GameObject.GetInstance().Name);
            gameSettingsNode.SetAttribute("human", GameObject.GetInstance().HumanAirline.Profile.IATACode);
            gameSettingsNode.SetAttribute("mainairline", GameObject.GetInstance().MainAirline.Profile.IATACode);
            gameSettingsNode.SetAttribute("fuelprice", GameObject.GetInstance().FuelPrice.ToString());
            gameSettingsNode.SetAttribute("timezone", GameObject.GetInstance().TimeZone.UTCOffset.ToString());
            gameSettingsNode.SetAttribute("mailonlandings", Settings.GetInstance().MailsOnLandings.ToString());
            gameSettingsNode.SetAttribute("mailonbadweather", Settings.GetInstance().MailsOnBadWeather.ToString());
            gameSettingsNode.SetAttribute("skin", SkinObject.GetInstance().CurrentSkin.Name);
            gameSettingsNode.SetAttribute("airportcode", Settings.GetInstance().AirportCodeDisplay.ToString());
            gameSettingsNode.SetAttribute("gamespeed", GameTimer.GetInstance().GameSpeed.ToString());
            gameSettingsNode.SetAttribute("minutesperturn", Settings.GetInstance().MinutesPerTurn.ToString());
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



            using (FileStream fs = new FileStream(path, FileMode.Create))
            {
                Stream s;

                s = new GZipStream(fs, CompressionMode.Compress);

                xmlDoc.Save(s);
                s.Close();




            }

            // xmlDoc.Save(path);


        }
    }
}
