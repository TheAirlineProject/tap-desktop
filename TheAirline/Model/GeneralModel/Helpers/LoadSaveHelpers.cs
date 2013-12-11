using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Text;
using System.Xml;
using System.Device.Location;
using TheAirline.Model.AirportModel;
using TheAirline.Model.AirlineModel;
using TheAirline.Model.AirlinerModel;
using TheAirline.Model.AirlinerModel.RouteModel;
using TheAirline.Model.GeneralModel.StatisticsModel;
using TheAirline.GraphicsModel.UserControlModel.MessageBoxModel;
using System.Globalization;
using TheAirline.Model.PassengerModel;
using TheAirline.Model.GeneralModel.InvoicesModel;
using TheAirline.Model.AirlineModel.SubsidiaryModel;
using TheAirline.Model.GeneralModel.WeatherModel;
using System.Threading.Tasks;
using TheAirline.Model.PilotModel;
using TheAirline.Model.GeneralModel.CountryModel.TownModel;
using System.Diagnostics;
using TheAirline.Model.GeneralModel.ScenarioModel;

namespace TheAirline.Model.GeneralModel.Helpers
{
    public class LoadSaveHelpers
    {
        /*
        //creates the saves.xml
        public static void CreateBaseXml(string path)
        {
            XmlDocument doc = new XmlDocument();// Create the XML Declaration, and append it to XML document
            XmlDeclaration dec = doc.CreateXmlDeclaration("1.0", null, null);
            doc.AppendChild(dec);// Create the root element

            XmlElement root = doc.CreateElement("saves");
            doc.AppendChild(root);


            doc.Save(path + "\\saves.xml");
        }*/
        //deletes a saved game
        public static void DeleteGame(string name)
        {
           
            File.Delete(AppSettings.getCommonApplicationDataPath() + "\\saves\\" + name + ".sav");
        }
       
        //loads a game
        public static void LoadGame(string name)
        {
            if (null == name || name == "")
            {
                WPFMessageBoxResult result = WPFMessageBox.Show(Translator.GetInstance().GetString("MessageBox", "101"), Translator.GetInstance().GetString("MessageBox", "101", "message"), WPFMessageBoxButtons.Ok);
                return;
            }
            string fileName = AppSettings.getCommonApplicationDataPath() + "\\saves\\" + name + ".sav";

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

                if (country != null)
                    country.TailNumbers.LastTailNumber = tailnumberNode.Attributes["value"].Value;

            }

            XmlNodeList airlinerTypesList = root.SelectNodes("//airlinertypes/airlinertype");

            foreach (XmlElement airlinerTypeNode in airlinerTypesList)
            {
                AirlinerType.TypeOfAirliner airlinerType = (AirlinerType.TypeOfAirliner)Enum.Parse(typeof(AirlinerType.TypeOfAirliner), airlinerTypeNode.Attributes["type"].Value);
                AirlinerType baseType = AirlinerTypes.GetType(airlinerTypeNode.Attributes["basetype"].Value);
                string airlinerTypeName = airlinerTypeNode.Attributes["name"].Value;

                AirlinerType type = null;

                if (airlinerType == AirlinerType.TypeOfAirliner.Passenger)
                {
                    int cabincrew = Convert.ToInt16(airlinerTypeNode.Attributes["cabincrew"].Value);
                    int passengers = Convert.ToInt16(airlinerTypeNode.Attributes["passengers"].Value);
                    int maxclasses = Convert.ToInt16(airlinerTypeNode.Attributes["maxclasses"].Value);
                 
                    type = new AirlinerPassengerType(baseType.Manufacturer,airlinerTypeName,"",passengers,baseType.CockpitCrew,cabincrew,baseType.CruisingSpeed,baseType.Range,baseType.Wingspan,baseType.Length,baseType.FuelConsumption,baseType.Price,maxclasses,baseType.MinRunwaylength,baseType.FuelCapacity,baseType.Body,baseType.RangeType,baseType.Engine,baseType.Produced,baseType.ProductionRate,false);
                }
                if (airlinerType == AirlinerType.TypeOfAirliner.Cargo)
                {
                    double cargo = Convert.ToDouble(airlinerTypeNode.Attributes["cargo"].Value,new CultureInfo("de-DE", false));
                    type = new AirlinerCargoType(baseType.Manufacturer,airlinerTypeName,"",baseType.CockpitCrew,cargo,baseType.CruisingSpeed,baseType.Range,baseType.Wingspan,baseType.Length,baseType.FuelConsumption,baseType.Price,baseType.MinRunwaylength,baseType.FuelCapacity,baseType.Body,baseType.RangeType,baseType.Engine,baseType.Produced,baseType.ProductionRate, false);
                }
                type.BaseType = baseType;

                AirlinerTypes.AddType(type);

            }

             Airliners.Clear();

             XmlNodeList airlinersList = root.SelectNodes("//airliners/airliner");

             Parallel.For(0, airlinersList.Count, i =>
              //foreach (XmlElement airlinerNode in airlinersList)
              {
                  XmlElement airlinerNode = (XmlElement)airlinersList[i];
                  AirlinerType type = AirlinerTypes.GetType(airlinerNode.Attributes["type"].Value);

                  if (type != null)
                  {
                      string tailnumber = airlinerNode.Attributes["tailnumber"].Value;
                      string id = airlinerNode.HasAttribute("id") ? airlinerNode.Attributes["id"].Value : tailnumber;

                      string last_service = airlinerNode.Attributes["last_service"].Value;
                      DateTime built = DateTime.Parse(airlinerNode.Attributes["built"].Value, new CultureInfo("de-DE", false));
                      double flown = Convert.ToDouble(airlinerNode.Attributes["flown"].Value, new CultureInfo("de-DE", false));
                      double damaged = Convert.ToDouble(airlinerNode.Attributes["damaged"].Value, new CultureInfo("de-DE", false));

                      Airliner airliner = new Airliner(id, type, tailnumber, built);
                      airliner.Condition = damaged;
                      airliner.Flown = flown;
                      airliner.clearAirlinerClasses();

                      XmlNodeList airlinerClassList = airlinerNode.SelectNodes("classes/class");

                      foreach (XmlElement airlinerClassNode in airlinerClassList)
                      {
                          AirlinerClass.ClassType airlinerClassType = (AirlinerClass.ClassType)Enum.Parse(typeof(AirlinerClass.ClassType), airlinerClassNode.Attributes["type"].Value);
                          int airlinerClassSeating = Convert.ToInt16(airlinerClassNode.Attributes["seating"].Value);

                          AirlinerClass aClass = new AirlinerClass(airlinerClassType, airlinerClassSeating);
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
              });


             Airlines.Clear();

             XmlNodeList airlinesList = root.SelectNodes("//airlines/airline[@subsidiary='False']");

             foreach (XmlElement airlineNode in airlinesList)
                 LoadAirline(airlineNode);

             XmlNodeList subsidiaryList = root.SelectNodes("//airlines/airline[@subsidiary='True']");

             foreach (XmlElement airlineNode in subsidiaryList)
                 LoadAirline(airlineNode);


             XmlNodeList airportsList = root.SelectNodes("//airports/airport");

             List<Airport> airportsToKeep = new List<Airport>();

             foreach (XmlElement airportNode in airportsList)
             {
                 Airport airport = Airports.GetAirportFromID(airportNode.Attributes["id"].Value);
                 airportsToKeep.Add(airport);

                 /*
                  *   XmlElement airportPaxNode = xmlDoc.CreateElement("paxvalue");
                    airportPaxNode.SetAttribute("from", paxValue.FromYear.ToString());
                    airportPaxNode.SetAttribute("to", paxValue.ToYear.ToString());
                    airportPaxNode.SetAttribute("size", paxValue.Size.ToString());
                    airportPaxNode.SetAttribute("pax", paxValue.Pax.ToString());
                    airportPaxNode.SetAttribute("inflationbefore", paxValue.InflationBeforeYear.ToString());
                    airportPaxNode.SetAttribute("inflationafter", paxValue.InflationAfterYear.ToString());
*/

                 airport.Income = Convert.ToInt64(airportNode.Attributes["income"].Value);

                 airport.Profile.PaxValues.Clear();

                 XmlNodeList paxvaluesList = airportNode.SelectNodes("paxvalues/paxvalue");

                 foreach (XmlElement paxElement in paxvaluesList)
                 {
                     int fromYear = Convert.ToInt16(paxElement.Attributes["from"].Value);
                     int toYear = Convert.ToInt16(paxElement.Attributes["to"].Value);
                     GeneralHelpers.Size airportSize = (GeneralHelpers.Size)Enum.Parse(typeof(GeneralHelpers.Size), paxElement.Attributes["size"].Value);
                     double pax = Convert.ToDouble(paxElement.Attributes["pax"].Value);
                     double inflationBefore = Convert.ToDouble(paxElement.Attributes["inflationbefore"].Value);
                     double inflationAfter = Convert.ToDouble(paxElement.Attributes["inflationafter"].Value);

                     PaxValue paxValue = new PaxValue(fromYear, toYear, airportSize, pax);
                     paxValue.InflationAfterYear = inflationAfter;
                     paxValue.InflationBeforeYear = inflationBefore;

                     airport.Profile.PaxValues.Add(paxValue);
                 }

                  XmlNodeList runwaysList = airportNode.SelectNodes("runways/runway");

                 foreach (XmlElement runwayElement in runwaysList)
                 {
                     string runwayName = runwayElement.Attributes["name"].Value;
                     long runwayLenght = Convert.ToInt64(runwayElement.Attributes["lenght"].Value);
                     Runway.SurfaceType runwaySurface = (Runway.SurfaceType)Enum.Parse(typeof(Runway.SurfaceType), runwayElement.Attributes["surface"].Value);
                     DateTime runwayDate = DateTime.Parse(runwayElement.Attributes["date"].Value, new CultureInfo("de-DE", false));

                     airport.Runways.Add(new Runway(runwayName, runwayLenght, runwaySurface, runwayDate, false));
                 }

                 XmlNodeList airportHubsList = airportNode.SelectNodes("hubs/hub");
                // airport.Hubs.Clear();

                 foreach (XmlElement airportHubElement in airportHubsList)
                 {
                     Airline airline = Airlines.GetAirline(airportHubElement.Attributes["airline"].Value);
                    // airport.Hubs.Add(new Hub(airline,null));
                 }

               
                 XmlNodeList airportWeatherList = airportNode.SelectNodes("weathers/weather");

                 for (int i = 0; i < airportWeatherList.Count; i++)
                 {
                     XmlElement airportWeatherElement = airportWeatherList[i] as XmlElement;

                     DateTime weatherDate = DateTime.Parse(airportWeatherElement.Attributes["date"].Value, new CultureInfo("de-DE", false));
                     Weather.WindDirection windDirection = (Weather.WindDirection)Enum.Parse(typeof(Weather.WindDirection), airportWeatherElement.Attributes["direction"].Value);
                     Weather.eWindSpeed windSpeed = (Weather.eWindSpeed)Enum.Parse(typeof(Weather.eWindSpeed), airportWeatherElement.Attributes["windspeed"].Value);
                     Weather.CloudCover cover = airportWeatherElement.HasAttribute("cover") ? (Weather.CloudCover)Enum.Parse(typeof(Weather.CloudCover), airportWeatherElement.Attributes["cover"].Value) : Weather.CloudCover.Clear;
                     Weather.Precipitation precip = airportWeatherElement.HasAttribute("precip") ? (Weather.Precipitation)Enum.Parse(typeof(Weather.Precipitation), airportWeatherElement.Attributes["precip"].Value) : Weather.Precipitation.None;
                     double temperatureLow = airportWeatherElement.HasAttribute("temperatureLow") ? Convert.ToDouble(airportWeatherElement.Attributes["temperaturelow"].Value, new CultureInfo("de-DE", false)) : 0;
                     double temperatureHigh = airportWeatherElement.HasAttribute("temperatureHigh") ? Convert.ToDouble(airportWeatherElement.Attributes["temperaturehigh"].Value, new CultureInfo("de-DE", false)) : 20;

                     XmlNodeList airportTemperatureList = airportWeatherElement.SelectNodes("temperatures/temperature");
                     HourlyWeather[] temperatures = new HourlyWeather[airportTemperatureList.Count];

                     int t = 0;
                     foreach (XmlElement airportTemperatureNode in airportTemperatureList)
                     {
                         double hourlyTemperature = Convert.ToDouble(airportTemperatureNode.Attributes["temp"].Value, new CultureInfo("de-DE", false));
                         Weather.CloudCover hourlyCover = (Weather.CloudCover)Enum.Parse(typeof(Weather.CloudCover), airportTemperatureNode.Attributes["cover"].Value);
                         Weather.Precipitation hourlyPrecip = (Weather.Precipitation)Enum.Parse(typeof(Weather.Precipitation), airportTemperatureNode.Attributes["precip"].Value);
                         Weather.eWindSpeed hourlyWindspeed = (Weather.eWindSpeed)Enum.Parse(typeof(Weather.eWindSpeed), airportTemperatureNode.Attributes["windspeed"].Value);
                         Weather.WindDirection hourlyDirection = (Weather.WindDirection)Enum.Parse(typeof(Weather.WindDirection), airportTemperatureNode.Attributes["direction"].Value);

                         temperatures[t] = new HourlyWeather(hourlyTemperature, hourlyCover, hourlyPrecip, hourlyWindspeed, hourlyDirection);
                         t++;
                     }


                     airport.Weather[i] = new Weather(weatherDate, windSpeed, windDirection, cover, precip, temperatures, temperatureLow, temperatureHigh);
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
                         Gate gate = new Gate(gateDeliveryDate);

                         terminal.Gates.addGate(gate);
                     }


                     airport.addTerminal(terminal);




                 }
                 airport.clearAirlineContracts();

                 XmlNodeList contractsList = airportNode.SelectNodes("contracts/contract");

                 foreach (XmlElement contractNode in contractsList)
                 {
                     Airline contractAirline = Airlines.GetAirline(contractNode.Attributes["airline"].Value);
                     int contractLength = Convert.ToInt16(contractNode.Attributes["length"].Value);
                     DateTime contractDate = DateTime.Parse(contractNode.Attributes["date"].Value, new CultureInfo("de-DE", false));
                     int contractGates = Convert.ToInt16(contractNode.Attributes["gates"].Value);
                     double contractPayment = Convert.ToDouble(contractNode.Attributes["payment"].Value, new CultureInfo("de-DE", false));
                     Boolean contractExclusive = Convert.ToBoolean(contractNode.Attributes["exclusive"].Value);
                     Terminal contractTerminal = contractNode.HasAttribute("terminal") ? airport.Terminals.AirportTerminals.Find(t => t.Name == contractNode.Attributes["terminal"].Value) : null;

                     AirportContract contract = new AirportContract(contractAirline, airport, contractDate, contractGates, contractLength, contractPayment,false, contractExclusive, contractTerminal);
                     airport.addAirlineContract(contract);
                 }


             }

             Airports.RemoveAirports(a => !airportsToKeep.Contains(a));

             XmlNodeList airportDestinationsList = root.SelectNodes("//airportdestinations/airportdestination");

             foreach (XmlElement airportDestinationElement in airportDestinationsList)
             {
                 Airport targetAirport = Airports.GetAirport(airportDestinationElement.Attributes["id"].Value);

                 if (targetAirport != null)
                 {
                     targetAirport.clearDestinationPassengers();

                     XmlNodeList destinationsList = airportDestinationElement.SelectNodes("destinations/destination");

                     Parallel.For(0, destinationsList.Count, i =>
                     //foreach (XmlElement destinationElement in destinationsList)
                     {
                         XmlElement destinationElement = (XmlElement)destinationsList[i];
                         Airport destAirport = Airports.GetAirport(destinationElement.Attributes["id"].Value);

                         if (destAirport != null)
                         {
                             ushort rate = ushort.Parse(destinationElement.Attributes["rate"].Value);
                             long destPassengers = Convert.ToInt64(destinationElement.Attributes["passengers"].Value);

                             targetAirport.addPassengerDestinationStatistics(destAirport, destPassengers);
                             targetAirport.addDestinationPassengersRate(new DestinationDemand(destAirport.Profile.IATACode, rate));

                             if (destinationElement.HasAttribute("cargo"))
                             {
                                 targetAirport.addDestinationCargoRate(new DestinationDemand(destAirport.Profile.IATACode, ushort.Parse(destinationElement.Attributes["cargo"].Value)));
                                 targetAirport.addCargoDestinationStatistics(destAirport, Convert.ToDouble(destinationElement.Attributes["cargostats"].Value, new CultureInfo("de-DE", false)));
                             }
                         }
                     });
                 }
             }
             Instructors.Clear();

             XmlNodeList instructorsList = root.SelectNodes("//instructors/instructor");

             foreach (XmlElement instructorNode in instructorsList)
             {
                 string firstname = instructorNode.Attributes["firstname"].Value;
                 string lastname = instructorNode.Attributes["lastname"].Value;
                 DateTime birthdate = DateTime.Parse(instructorNode.Attributes["birthdate"].Value, new CultureInfo("de-DE", false));
                 Town town = Towns.GetTown(instructorNode.Attributes["town"].Value);
                 Pilot.PilotRating rating = (Pilot.PilotRating)Enum.Parse(typeof(Pilot.PilotRating), instructorNode.Attributes["rating"].Value);
                 string id = instructorNode.Attributes["id"].Value;

                 Instructor instructor = new Instructor(new PilotProfile(firstname, lastname, birthdate, town), rating);

                 if (id != "-")
                 {
                     FlightSchool fs = Airlines.GetAllAirlines().SelectMany(a => a.FlightSchools).Where(f => f.ID == id).FirstOrDefault();
                     instructor.FlightSchool = fs;
                     fs.addInstructor(instructor);
                 }

                 XmlNodeList studentsList = instructorNode.SelectNodes("students/student");

                 foreach (XmlElement studentNode in studentsList)
                 {
                     PilotStudent student = instructor.FlightSchool.Students.Find(s => s.Profile.Name == studentNode.Attributes["name"].Value);
                     student.Instructor = instructor;
                     instructor.addStudent(student);
                 }

                 Instructors.AddInstructor(instructor);
             }

             if (Instructors.GetInstructors().Count == 0)
                 GeneralHelpers.CreateInstructors(75 * Airlines.GetAllAirlines().Count);

             Pilots.Clear();

             XmlNodeList pilotsList = root.SelectNodes("//pilots/pilot");

             foreach (XmlElement pilotNode in pilotsList)
             {
                 string firstname = pilotNode.Attributes["firstname"].Value;
                 string lastname = pilotNode.Attributes["lastname"].Value;
                 DateTime birthdate = DateTime.Parse(pilotNode.Attributes["birthdate"].Value, new CultureInfo("de-DE", false));
                 Town town = Towns.GetTown(pilotNode.Attributes["town"].Value);
                 DateTime educationdate = DateTime.Parse(pilotNode.Attributes["education"].Value, new CultureInfo("de-DE", false));

                 Pilot.PilotRating rating = (Pilot.PilotRating)Enum.Parse(typeof(Pilot.PilotRating), pilotNode.Attributes["rating"].Value);

                 Pilot pilot = new Pilot(new PilotProfile(firstname, lastname, birthdate, town), educationdate, rating);

                 if (pilotNode.Attributes["airline"].Value != "-")
                 {
                     Airline pilotAirline = Airlines.GetAirline(pilotNode.Attributes["airline"].Value);
                     DateTime airlinesigneddate = DateTime.Parse(pilotNode.Attributes["airlinesigned"].Value, new CultureInfo("de-DE", false));

                     pilotAirline.addPilot(pilot);
                     pilot.AirlineSignedDate = airlinesigneddate;

                     if (pilotNode.Attributes["airliner"].Value != "-")
                     {
                         FleetAirliner airliner = pilotAirline.Fleet.Find(f => f.Airliner.ID == pilotNode.Attributes["airliner"].Value);

                         if (airliner != null)
                         {
                             pilot.Airliner = airliner;
                             airliner.addPilot(pilot);
                         }
                     }

                   }

                 Pilots.AddPilot(pilot);
             }

             if (Pilots.GetNumberOfPilots() == 0)
             {
                 Random rnd = new Random();

                 GeneralHelpers.CreatePilots(100 * Airlines.GetAllAirlines().Count);

                 foreach (FleetAirliner airliner in Airlines.GetAllAirlines().SelectMany(a => a.Fleet))
                 {
                     Pilot pilot = Pilots.GetPilots()[rnd.Next(Pilots.GetNumberOfPilots())];
                     airliner.Airliner.Airline.addPilot(pilot);
                     pilot.Airliner = airliner;
                     airliner.addPilot(pilot);
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

                 XmlNodeList membersList = allianceNode.SelectNodes("members/member");

                 foreach (XmlElement memberNode in membersList)
                 {
                     Airline allianceMember = Airlines.GetAirline(memberNode.Attributes["airline"].Value);
                     DateTime joinedDate = DateTime.Parse(memberNode.Attributes["joined"].Value, new CultureInfo("de-DE"));

                     if (allianceMember != null)
                         alliance.addMember(new AllianceMember(allianceMember, joinedDate));
                 }

                 XmlNodeList pendingsList = allianceNode.SelectNodes("pendings/pending");

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

             XmlElement difficultyNode = (XmlElement)root.SelectSingleNode("//difficulty");
             string difficultyName = difficultyNode.Attributes["name"].Value;
             double moneyLevel = Convert.ToDouble(difficultyNode.Attributes["money"].Value,new CultureInfo("de-DE", false));
             double priceLevel = Convert.ToDouble(difficultyNode.Attributes["price"].Value, new CultureInfo("de-DE", false));
             double loanLevel = Convert.ToDouble(difficultyNode.Attributes["loan"].Value, new CultureInfo("de-DE", false));
             double passengersLevel = Convert.ToDouble(difficultyNode.Attributes["passengers"].Value, new CultureInfo("de-DE", false));
             double aiLevel = Convert.ToDouble(difficultyNode.Attributes["ai"].Value, new CultureInfo("de-DE", false));

             GameObject.GetInstance().Difficulty = new DifficultyLevel(difficultyName, moneyLevel, loanLevel, passengersLevel, priceLevel, aiLevel,1);

             XmlElement scenarioNode = (XmlElement)root.SelectSingleNode("//scenario");

             if (scenarioNode != null)
             {
                 Scenario scenario = Scenarios.GetScenario(scenarioNode.Attributes["name"].Value);

                 ScenarioObject so = new ScenarioObject(scenario);
                 so.IsSuccess = Convert.ToBoolean(scenarioNode.Attributes["success"].Value);

                 if (scenarioNode.HasAttribute("failed"))
                     so.ScenarioFailed = scenario.Failures.Find(f => f.ID == scenarioNode.Attributes["failed"].Value);

                 XmlNodeList failuresList = scenarioNode.SelectNodes("failures/failure");

                 foreach (XmlElement failureNode in failuresList)
                 {
                     ScenarioFailure failure = scenario.Failures.Find(f => f.ID == failureNode.Attributes["id"].Value);
                     int failureCount = Convert.ToInt16(failureNode.Attributes["count"].Value);
                     DateTime lastFailureTime = DateTime.Parse(failureNode.Attributes["lastfailuretime"].Value, new CultureInfo("de-DE", false));

                     so.getScenarioFailure(failure).LastFailureTime = lastFailureTime;
                     so.getScenarioFailure(failure).Failures = failureCount;
                 }

                 GameObject.GetInstance().Scenario = so;

             }

             XmlElement gameSettingsNode = (XmlElement)root.SelectSingleNode("//gamesettings");

             GameObject.GetInstance().Name = gameSettingsNode.Attributes["name"].Value;

             Airline humanAirline = Airlines.GetAirline(gameSettingsNode.Attributes["human"].Value);
             GameObject.GetInstance().setHumanAirline(humanAirline);
       
             Airline mainAirline = Airlines.GetAirline(gameSettingsNode.Attributes["mainairline"].Value);
             GameObject.GetInstance().MainAirline = mainAirline;

             double fuelPrice = Convert.ToDouble(gameSettingsNode.Attributes["fuelprice"].Value, new CultureInfo("de-DE", false));
             GameObject.GetInstance().FuelPrice = fuelPrice;

             GameTimeZone timezone = TimeZones.GetTimeZones().Find(delegate(GameTimeZone gtz) { return gtz.UTCOffset == TimeSpan.Parse(gameSettingsNode.Attributes["timezone"].Value); });
             GameObject.GetInstance().TimeZone = timezone;

             Settings.GetInstance().MailsOnLandings = Convert.ToBoolean(gameSettingsNode.Attributes["mailonlandings"].Value);
             Settings.GetInstance().MailsOnBadWeather = Convert.ToBoolean(gameSettingsNode.Attributes["mailonbadweather"].Value);

             Settings.GetInstance().AirportCodeDisplay = (Settings.AirportCode)Enum.Parse(typeof(Settings.AirportCode), gameSettingsNode.Attributes["airportcode"].Value);
             if (gameSettingsNode.HasAttribute("minutesperturn")) Settings.GetInstance().MinutesPerTurn = Convert.ToInt16(gameSettingsNode.Attributes["minutesperturn"].Value);
             AppSettings.GetInstance().setLanguage(Languages.GetLanguage(gameSettingsNode.Attributes["language"].Value));
             GameObject.GetInstance().DayRoundEnabled = Convert.ToBoolean(gameSettingsNode.Attributes["dayround"].Value);

             XmlNodeList itemsList = gameSettingsNode.SelectNodes("calendaritems/calendaritem");

             CalendarItems.Clear();

             foreach (XmlElement itemNode in itemsList)
             {
                 CalendarItem.ItemType itemType = (CalendarItem.ItemType)Enum.Parse(typeof(CalendarItem.ItemType), itemNode.Attributes["type"].Value);
                 DateTime itemDate = DateTime.Parse(itemNode.Attributes["date"].Value, new CultureInfo("de-DE", false));
                 string itemHeader = itemNode.Attributes["header"].Value;
                 string itemSubject = itemNode.Attributes["subject"].Value;

                 CalendarItems.AddCalendarItem(new CalendarItem(itemType, itemDate, itemHeader, itemSubject));
             }

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
            string airlineCEO = airlineNode.Attributes["CEO"].Value;
            double money = Convert.ToDouble(airlineNode.Attributes["money"].Value, new CultureInfo("de-DE", false));
            int reputation = Convert.ToInt16(airlineNode.Attributes["reputation"].Value);
            Airline.AirlineMentality mentality = (Airline.AirlineMentality)Enum.Parse(typeof(Airline.AirlineMentality), airlineNode.Attributes["mentality"].Value);
            Airline.AirlineFocus market = (Airline.AirlineFocus)Enum.Parse(typeof(Airline.AirlineFocus), airlineNode.Attributes["market"].Value);
            Airline.AirlineLicense license = (Airline.AirlineLicense)Enum.Parse(typeof(Airline.AirlineLicense), airlineNode.Attributes["license"].Value);
            Route.RouteType routefocus = airlineNode.HasAttribute("routefocus") ? (Route.RouteType)Enum.Parse(typeof(Route.RouteType), airlineNode.Attributes["routefocus"].Value) : Route.RouteType.Passenger;

            Boolean isReal = airlineNode.HasAttribute("real") ? Convert.ToBoolean(airlineNode.Attributes["real"].Value) : true;
            int founded = airlineNode.HasAttribute("founded") ? Convert.ToInt16(airlineNode.Attributes["founded"].Value) : 1950;
            int folded = airlineNode.HasAttribute("folded") ? Convert.ToInt16(airlineNode.Attributes["folded"].Value) : 2199;


            Airline airline;
            if (airlineIsSubsidiary)
            {
                Airline parent = Airlines.GetAirline(airlineNode.Attributes["parentairline"].Value);
                airline = new SubsidiaryAirline(parent, new AirlineProfile(airlineName, airlineIATA, color, airlineCEO, isReal, founded, folded), mentality, market, license, routefocus);
                airline.Profile.Country = airlineCountry;
                parent.addSubsidiaryAirline((SubsidiaryAirline)airline);
            }
            else
            {
                airline = new Airline(new AirlineProfile(airlineName, airlineIATA, color, airlineCEO, isReal, founded, folded), mentality, market, license, routefocus);
                airline.Profile.Country = airlineCountry;
            }
            if (airlineNode.HasAttribute("logo"))
            {
                string logo = AppSettings.getDataPath() + "\\graphics\\airlinelogos\\" + airlineNode.Attributes["logo"].Value;

                airline.Profile.addLogo(new AirlineLogo(logo));
            }
            airline.Fleet.Clear();
            airline.Airports.Clear();
            airline.Routes.Clear();
            airline.Alliances.Clear();

            airline.Money = money;
            airline.Reputation = reputation;

            XmlNodeList logosList = airlineNode.SelectNodes("logos/logo");

            foreach (XmlElement logoElement in logosList)
            {
                int logoFromYear = Convert.ToInt16(logoElement.Attributes["from"].Value);
                int logoToYear = Convert.ToInt16(logoElement.Attributes["to"].Value);
                string logoPath = logoElement.Attributes["path"].Value;

                if (!File.Exists(logoPath))
                    logoPath = AppSettings.getDataPath() + "\\graphics\\airlinelogos\\" + logoPath.Substring(logoPath.LastIndexOf('\\') + 1);

                airline.Profile.addLogo(new AirlineLogo(logoFromYear, logoToYear, logoPath));
            }

            XmlElement airlineContractNode = (XmlElement)airlineNode.SelectSingleNode("contract");
            if (airlineContractNode != null)
            {
                Manufacturer contractManufacturer = Manufacturers.GetManufacturer(airlineContractNode.Attributes["manufacturer"].Value);
                DateTime contractSigningDate = DateTime.Parse(airlineContractNode.Attributes["signingdate"].Value, new CultureInfo("de-DE", false));
                int contractLength = Convert.ToInt16(airlineContractNode.Attributes["length"].Value);
                double contractDiscount = Convert.ToDouble(airlineContractNode.Attributes["discount"].Value, new CultureInfo("de-DE", false));
                int contractAirliners = Convert.ToInt16(airlineContractNode.Attributes["airliners"].Value);

                ManufacturerContract contract = new ManufacturerContract(contractManufacturer, contractSigningDate, contractLength, contractDiscount);
                contract.PurchasedAirliners = contractAirliners;

                airline.Contract = contract;

            }

            // chs, 2011-17-10 added for loading of passenger happiness
            XmlElement airlinePassengerNode = (XmlElement)airlineNode.SelectSingleNode("passengerhappiness");
            double passengerHappiness = Convert.ToDouble(airlinePassengerNode.Attributes["value"].Value, new CultureInfo("de-DE", false));
            PassengerHelpers.SetPassengerHappiness(airline, passengerHappiness);

            XmlNodeList airlineFacilitiesList = airlineNode.SelectNodes("facilities/facility");
            foreach (XmlElement airlineFacilityNode in airlineFacilitiesList)
            {
                string airlineFacility = airlineFacilityNode.Attributes["uid"].Value;

                airline.addFacility(AirlineFacilities.GetFacility(airlineFacility));
            }

            XmlNodeList airlinePoliciesList = airlineNode.SelectNodes("policies/policy");

            foreach (XmlElement airlinePolicyNode in airlinePoliciesList)
            {
                string policyName = airlinePolicyNode.Attributes["name"].Value;
                object policyValue = (object)airlinePolicyNode.Attributes["value"].Value;

                int number;
                if (int.TryParse(policyValue.ToString(), out number))
                    airline.addAirlinePolicy(new AirlinePolicy(policyName, number));
                else
                    airline.addAirlinePolicy(new AirlinePolicy(policyName, policyValue));
            }

            XmlNodeList airlineLoanList = airlineNode.SelectNodes("loans/loan");
            foreach (XmlElement airlineLoanNode in airlineLoanList)
            {
                DateTime date = DateTime.Parse(airlineLoanNode.Attributes["date"].Value, new CultureInfo("de-DE", false));
                double rate = Convert.ToDouble(airlineLoanNode.Attributes["rate"].Value, new CultureInfo("de-DE", false));
                double amount = Convert.ToDouble(airlineLoanNode.Attributes["amount"].Value, new CultureInfo("de-DE", false));
                int length = Convert.ToInt16(airlineLoanNode.Attributes["length"].Value);
                double payment = Convert.ToDouble(airlineLoanNode.Attributes["payment"].Value, new CultureInfo("de-DE", false));

                Loan loan = new Loan(date, amount, length, rate);
                loan.PaymentLeft = payment;

                airline.addLoan(loan);
            }

            XmlNodeList airlineStatList = airlineNode.SelectNodes("stats/stat");
            foreach (XmlElement airlineStatNode in airlineStatList)
            {
                int year = Convert.ToInt32(airlineStatNode.Attributes["year"].Value);
                string airlineStatType = airlineStatNode.Attributes["type"].Value;
                double value = Convert.ToDouble(airlineStatNode.Attributes["value"].Value, new CultureInfo("de-DE", false));

                airline.Statistics.setStatisticsValue(year, StatisticsTypes.GetStatisticsType(airlineStatType), value);
            }

            XmlNodeList airlineInvoiceList = airlineNode.SelectNodes("invoices/invoice");

            foreach (XmlElement airlineInvoiceNode in airlineInvoiceList)
            {
                Invoice.InvoiceType type = (Invoice.InvoiceType)Enum.Parse(typeof(Invoice.InvoiceType), airlineInvoiceNode.Attributes["type"].Value);
                int invoiceYear = Convert.ToInt16(airlineInvoiceNode.Attributes["year"].Value);
                int invoiceMonth = Convert.ToInt16(airlineInvoiceNode.Attributes["month"].Value);
                double invoiceAmount = Convert.ToDouble(airlineInvoiceNode.Attributes["amount"].Value, new CultureInfo("de-DE", false));

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
                double feeValue = Convert.ToDouble(feeNode.Attributes["value"].Value, new CultureInfo("de-DE", false));

                fees.setValue(FeeTypes.GetType(feeType), feeValue);
            }

            airline.Fees = fees;

            XmlNodeList flightschoolsList = airlineNode.SelectNodes("flightschools/flightschool");

            foreach (XmlElement flightschoolNode in flightschoolsList)
            {
                string fsID = flightschoolNode.Attributes["id"].Value;
                Airport fsAirport = Airports.GetAirportFromID(flightschoolNode.Attributes["airport"].Value);

                FlightSchool fs = new FlightSchool(fsAirport);
                fs.ID = fsID;

                XmlNodeList aircraftsList = flightschoolNode.SelectNodes("trainingaircrafts/trainingaircraft");

                foreach (XmlElement aircraftNode in aircraftsList)
                {
                    TrainingAircraftType aircraftType = TrainingAircraftTypes.GetAircraftType(aircraftNode.Attributes["type"].Value);
                    DateTime aircraftDate = DateTime.Parse(aircraftNode.Attributes["date"].Value, new CultureInfo("de-DE", false));

                    fs.addTrainingAircraft(new TrainingAircraft(aircraftType, aircraftDate, fs));
                }

                XmlNodeList studentsList = flightschoolNode.SelectNodes("students/student");

                foreach (XmlElement studentNode in studentsList)
                {
                    string firstname = studentNode.Attributes["firstname"].Value;
                    string lastname = studentNode.Attributes["lastname"].Value;
                    DateTime birthdate = DateTime.Parse(studentNode.Attributes["birthdate"].Value, new CultureInfo("de-DE", false));
                    Town town = Towns.GetTown(studentNode.Attributes["town"].Value);
                    DateTime startdate = DateTime.Parse(studentNode.Attributes["startdate"].Value, new CultureInfo("de-DE", false));

                    fs.addStudent(new PilotStudent(new PilotProfile(firstname, lastname, birthdate, town), startdate, null));
                }

                airline.addFlightSchool(fs);
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
                DateTime groundedDate = DateTime.Parse(airlineAirlinerNode.Attributes["groundeddate"].Value, new CultureInfo("de-DE", false));

               // Coordinate latitude = Coordinate.Parse(airlineAirlinerNode.Attributes["latitude"].Value);
                //Coordinate longitude = Coordinate.Parse(airlineAirlinerNode.Attributes["longitude"].Value);

                string[] latitude = airlineAirlinerNode.Attributes["value"].Value.Split(new Char[] { '°', '\'', '\'' });
                string[] longitude = airlineAirlinerNode.Attributes["value"].Value.Split(new Char[] { '°', '\'', '\'' });
                int[] coords = new int[6];
                foreach (string l in latitude)
                {
                    int c = 0;
                    int.TryParse(l, out coords[c]);
                    c++;
                }

                foreach (string l in longitude)
                {
                    int c = 3;
                    int.TryParse(l, out coords[c]);
                    c++;
                }

                //cleaning up
                latitude = null;
                longitude = null;

                GeoCoordinate pos = new GeoCoordinate(MathHelpers.DMStoDeg(coords[0], coords[1], coords[2]), MathHelpers.DMStoDeg(coords[3], coords[4], coords[5]));

                FleetAirliner fAirliner = new FleetAirliner(purchasedtype, purchasedDate, airline, airliner, homebase);
                fAirliner.CurrentPosition = null;
                fAirliner.Status = status;
                fAirliner.GroundedToDate = groundedDate;

                XmlNodeList airlinerStatList = airlineAirlinerNode.SelectNodes("stats/stat");

                foreach (XmlElement airlinerStatNode in airlinerStatList)
                {
                    int year = Convert.ToInt32(airlinerStatNode.Attributes["year"].Value);
                    string statType = airlinerStatNode.Attributes["type"].Value;
                    double statValue = Convert.ToDouble(airlinerStatNode.Attributes["value"].Value, new CultureInfo("de-DE", false));
                    fAirliner.Statistics.setStatisticsValue(year, StatisticsTypes.GetStatisticsType(statType), statValue);
                }




                airline.addAirliner(fAirliner);

            }
            XmlNodeList routeList = airlineNode.SelectNodes("routes/route");

            foreach (XmlElement routeNode in routeList)
            {
                airline.addRoute(LoadRoute(routeNode, airline));
            }

            XmlNodeList flightNodes = airlineNode.SelectNodes("flights/flight");
            foreach (XmlElement flightNode in flightNodes)
            {

                FleetAirliner airliner = airline.Fleet.Find(a => a.Airliner.ID == flightNode.Attributes["airliner"].Value);

                if (airliner != null)
                {
                    Route route = airline.Routes.Find(r => r.Id == flightNode.Attributes["route"].Value);

                    if (route != null)
                    {
                        string destination = flightNode.Attributes["destination"].Value;

                        DayOfWeek day = (DayOfWeek)Enum.Parse(typeof(DayOfWeek), flightNode.Attributes["day"].Value);
                        TimeSpan time = TimeSpan.Parse(flightNode.Attributes["time"].Value);
                        DateTime flightTime = DateTime.Parse(flightNode.Attributes["flighttime"].Value, new CultureInfo("de-DE", false));

                        XmlElement stopoverNode = (XmlElement)flightNode.SelectSingleNode("stopover");

                        Boolean isStopoverFlight = stopoverNode != null;


                        if (destination != "Service")
                        {
                            RouteTimeTableEntry rtte = route.TimeTable.Entries.Find(delegate(RouteTimeTableEntry e) { return e.Destination.FlightCode == destination && e.Day == day && e.Time == time; });

                            Flight currentFlight = new Flight(rtte);
                            currentFlight.FlightTime = flightTime;
                            currentFlight.Classes.Clear();

                            if (currentFlight.isPassengerFlight())
                            {
                                XmlNodeList flightClassList = flightNode.SelectNodes("flightclasses/flightclass");

                                foreach (XmlElement flightClassNode in flightClassList)
                                {
                                    AirlinerClass.ClassType airlinerClassType = (AirlinerClass.ClassType)Enum.Parse(typeof(AirlinerClass.ClassType), flightClassNode.Attributes["type"].Value);
                                    int flightPassengers = Convert.ToInt16(flightClassNode.Attributes["passengers"].Value);

                                    currentFlight.Classes.Add(new FlightAirlinerClass(((PassengerRoute)route).getRouteAirlinerClass(airlinerClassType), flightPassengers));
                                }


                            }
                            if (currentFlight.isCargoFlight())
                            {
                                XmlElement flightCargoNode = (XmlElement)flightNode.SelectSingleNode("flightcargo");
                                double flightCargo = Convert.ToDouble(flightCargoNode.Attributes["cargo"].Value, new CultureInfo("de-DE", false));

                                currentFlight.Cargo = flightCargo;

                            }
                            if (isStopoverFlight)
                            {
                                int currentStopoverFlight = Convert.ToInt32(stopoverNode.Attributes["currentflight"].Value);


                                XmlNodeList stopoverClassesList = stopoverNode.SelectNodes("classes/class");

                                Dictionary<RouteTimeTableEntry, List<FlightAirlinerClass>> stopEntryAllClasses = new Dictionary<RouteTimeTableEntry, List<FlightAirlinerClass>>();


                                foreach (XmlElement stopoverClassNode in stopoverClassesList)
                                {

                                    RouteTimeTableEntry stopoverEntry = airline.Routes.SelectMany(r => r.TimeTable.Entries).ToList().Find(e => e.ID == stopoverClassNode.Attributes["id"].Value);

                                    List<FlightAirlinerClass> stopoverFlightClasses = new List<FlightAirlinerClass>();

                                    XmlNodeList stopoverFlightClassList = stopoverClassNode.SelectNodes("flightclasses/flightclass");

                                    foreach (XmlElement stopoverFlightClassNode in stopoverFlightClassList)
                                    {
                                        AirlinerClass.ClassType stopoverAirlinerClassType = (AirlinerClass.ClassType)Enum.Parse(typeof(AirlinerClass.ClassType), stopoverFlightClassNode.Attributes["type"].Value);
                                        int stopoverFlightPassengers = Convert.ToInt16(stopoverFlightClassNode.Attributes["passengers"].Value);

                                        stopoverFlightClasses.Add(new FlightAirlinerClass(((PassengerRoute)route).getRouteAirlinerClass(stopoverAirlinerClassType), stopoverFlightPassengers));

                                    }

                                    stopEntryAllClasses.Add(stopoverEntry, stopoverFlightClasses);

                                }

                                XmlNodeList stopoverCargosList = stopoverNode.SelectNodes("cargos/cargo");

                                Dictionary<RouteTimeTableEntry, double> stopEntryAllCargo = new Dictionary<RouteTimeTableEntry, double>();

                                foreach (XmlElement stopoverCargoNode in stopoverCargosList)
                                {

                                    RouteTimeTableEntry stopoverEntry = airline.Routes.SelectMany(r => r.TimeTable.Entries).ToList().Find(e => e.ID == stopoverCargoNode.Attributes["id"].Value);
                                    double cargoValue = Convert.ToDouble(stopoverCargoNode.Attributes["value"].Value, new CultureInfo("de-DE", false));

                                    stopEntryAllCargo.Add(stopoverEntry, cargoValue);

                                }

                                StopoverFlight stopoverFlight = new StopoverFlight(rtte);
                                stopoverFlight.AllClasses = stopEntryAllClasses;
                                stopoverFlight.AllCargo = stopEntryAllCargo;
                                stopoverFlight.CurrentFlight = currentStopoverFlight;
                                stopoverFlight.FlightTime = currentFlight.FlightTime;
                                stopoverFlight.Classes = currentFlight.Classes;

                                airliner.CurrentFlight = stopoverFlight;

                            }
                            else
                            {

                                airliner.CurrentFlight = currentFlight;
                            }
                        }
                        else
                        {

                            airliner.CurrentFlight = new Flight(new RouteTimeTableEntry(route.TimeTable, GameObject.GetInstance().GameTime.DayOfWeek, GameObject.GetInstance().GameTime.TimeOfDay, new RouteEntryDestination(airliner.Homebase, "Service",null),null));

                            airliner.Status = FleetAirliner.AirlinerStatus.On_service;
                        }



                    }
                    else
                        airliner.Status = FleetAirliner.AirlinerStatus.Stopped;
                }

            }
            Airlines.AddAirline(airline);
        }
        /*
        //append a file to the list of saved files
        public static void AppendSavedFile(string name, string file)
        {
            XmlDocument doc = new XmlDocument();
            doc.Load(AppSettings.getCommonApplicationDataPath() + "\\saves\\saves.xml");
            XmlElement root = doc.DocumentElement;

            XmlNode node = root.SelectSingleNode("//saves");

            XmlElement e = doc.CreateElement("save");
            e.SetAttribute("name", name);
            e.SetAttribute("file", file);

            node.AppendChild(e);

            doc.Save(AppSettings.getCommonApplicationDataPath() + "\\saves\\saves.xml");

        }*/
        //returns the names of the saved games
        public static List<string> GetSavedGames()
        {
            List<string> saves = new List<string>();

            DirectoryInfo dir = new DirectoryInfo(AppSettings.getCommonApplicationDataPath() + "\\saves\\");

            foreach (FileInfo file in dir.GetFiles("*.sav"))
            {
                string name = file.Name.Split('.')[0];
                saves.Add(name);
            }

            return saves;


        }
        //saves the game
        public static void SaveGame(string file)
        {
            Stopwatch sw = new Stopwatch();
            sw.Start();

            string path = AppSettings.getCommonApplicationDataPath() + "\\saves\\" + file + ".sav";

            XmlDocument xmlDoc = new XmlDocument();

            XmlDeclaration xmlDeclaration = xmlDoc.CreateXmlDeclaration("1.0", "UTF-8", null);
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

            XmlElement airlinerTypesNode = xmlDoc.CreateElement("airlinertypes");

            foreach (AirlinerType airlinerType in AirlinerTypes.GetNonStandardTypes())
            {
                XmlElement airlinerTypeNode = xmlDoc.CreateElement("airlinertype");
                airlinerTypeNode.SetAttribute("type", airlinerType.TypeAirliner.ToString());
                airlinerTypeNode.SetAttribute("basetype", airlinerType.BaseType.Name);
                airlinerTypeNode.SetAttribute("name",airlinerType.Name);

                if (airlinerType.TypeAirliner == AirlinerType.TypeOfAirliner.Passenger)
                {
                    airlinerTypeNode.SetAttribute("cabincrew", ((AirlinerPassengerType)airlinerType).CabinCrew.ToString());
                    airlinerTypeNode.SetAttribute("passengers", ((AirlinerPassengerType)airlinerType).MaxSeatingCapacity.ToString());
                    airlinerTypeNode.SetAttribute("maxclasses",((AirlinerPassengerType)airlinerType).MaxAirlinerClasses.ToString());
                }
                if (airlinerType.TypeAirliner == AirlinerType.TypeOfAirliner.Cargo)
                    airlinerTypeNode.SetAttribute("cargo", ((AirlinerCargoType)airlinerType).CargoSize.ToString(new CultureInfo("de-DE", false)));

                airlinerTypesNode.AppendChild(airlinerTypeNode);
             }

            root.AppendChild(airlinerTypesNode);

            XmlElement airlinersNode = xmlDoc.CreateElement("airliners");

            foreach (Airliner airliner in Airliners.GetAllAirliners())
            //Parallel.ForEach(Airliners.GetAllAirliners(),airliner=>
            {
                XmlElement airlinerNode = xmlDoc.CreateElement("airliner");
                airlinerNode.SetAttribute("type", airliner.Type.Name);
                airlinerNode.SetAttribute("tailnumber", airliner.TailNumber);
                airlinerNode.SetAttribute("id", airliner.ID);

                airlinerNode.SetAttribute("last_service", airliner.LastServiceCheck.ToString());
                airlinerNode.SetAttribute("built", airliner.BuiltDate.ToString(new CultureInfo("de-DE")));
                airlinerNode.SetAttribute("flown", airliner.Flown.ToString(new CultureInfo("de-DE", false)));
                airlinerNode.SetAttribute("damaged", airliner.Condition.ToString());

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
            };

            root.AppendChild(airlinersNode);

           // XmlElement passengersNode = xmlDoc.CreateElement("passengers");



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
                //airlineNode.SetAttribute("logo", airline.Profile.Logo.Substring(airline.Profile.Logo.LastIndexOf('\\') + 1));
                airlineNode.SetAttribute("CEO", airline.Profile.CEO);
                airlineNode.SetAttribute("money", string.Format("{0:0}", airline.Money));
                airlineNode.SetAttribute("reputation", airline.Reputation.ToString());
                airlineNode.SetAttribute("mentality", airline.Mentality.ToString());
                airlineNode.SetAttribute("market", airline.MarketFocus.ToString());
                airlineNode.SetAttribute("license", airline.License.ToString());
                airlineNode.SetAttribute("routefocus", airline.AirlineRouteFocus.ToString());
                airlineNode.SetAttribute("isreal", airline.Profile.IsReal.ToString());
                airlineNode.SetAttribute("founded", airline.Profile.Founded.ToString());
                airlineNode.SetAttribute("folded", airline.Profile.Folded.ToString());

                XmlElement airlineLogosList = xmlDoc.CreateElement("logos");

                foreach (AirlineLogo logo in airline.Profile.Logos)
                {
                    XmlElement logoElement = xmlDoc.CreateElement("logo");

                    logoElement.SetAttribute("from", logo.FromYear.ToString());
                    logoElement.SetAttribute("to", logo.ToYear.ToString());
                    logoElement.SetAttribute("path", logo.Path);

                    airlineLogosList.AppendChild(logoElement);
                }
                airlineNode.AppendChild(airlineLogosList);


                if (airline.Contract != null)
                {
                    XmlElement airlineContractNode = xmlDoc.CreateElement("contract");
                    airlineContractNode.SetAttribute("manufacturer", airline.Contract.Manufacturer.ShortName);
                    airlineContractNode.SetAttribute("signingdate", airline.Contract.SigningDate.ToString(new CultureInfo("de-DE")));
                    airlineContractNode.SetAttribute("length", airline.Contract.Length.ToString());
                    airlineContractNode.SetAttribute("discount", airline.Contract.Discount.ToString(new CultureInfo("de-DE", false)));
                    airlineContractNode.SetAttribute("airliners", airline.Contract.PurchasedAirliners.ToString());

                    airlineNode.AppendChild(airlineContractNode);
                }

                // chs, 2011-13-10 added for saving of passenger happiness
                XmlElement airlineHappinessNode = xmlDoc.CreateElement("passengerhappiness");
                airlineHappinessNode.SetAttribute("value", PassengerHelpers.GetHappinessValue(airline).ToString(new CultureInfo("de-DE", false)));

                airlineNode.AppendChild(airlineHappinessNode);

                XmlElement airlineFacilitiesNode = xmlDoc.CreateElement("facilities");

                foreach (AirlineFacility facility in airline.Facilities)
                {
                    XmlElement airlineFacilityNode = xmlDoc.CreateElement("facility");
                    airlineFacilityNode.SetAttribute("uid", facility.Uid);

                    airlineFacilitiesNode.AppendChild(airlineFacilityNode);
                }
                airlineNode.AppendChild(airlineFacilitiesNode);

                XmlElement airlinePoliciesNode = xmlDoc.CreateElement("policies");

                foreach (AirlinePolicy policy in airline.Policies)
                {
                    XmlElement airlinePolicyNode = xmlDoc.CreateElement("policy");

                    airlinePolicyNode.SetAttribute("name", policy.Name);
                    airlinePolicyNode.SetAttribute("value", policy.PolicyValue.ToString());

                    airlinePoliciesNode.AppendChild(airlinePolicyNode);
                }

                airlineNode.AppendChild(airlinePoliciesNode);

                XmlElement loansNode = xmlDoc.CreateElement("loans");
                foreach (Loan loan in airline.Loans)
                {
                    XmlElement loanNode = xmlDoc.CreateElement("loan");
                    loanNode.SetAttribute("date", loan.Date.ToString(new CultureInfo("de-DE")));
                    loanNode.SetAttribute("rate", loan.Rate.ToString(new CultureInfo("de-DE", false)));
                    loanNode.SetAttribute("amount", loan.Amount.ToString(new CultureInfo("de-DE", false)));
                    loanNode.SetAttribute("length", loan.Length.ToString());
                    loanNode.SetAttribute("payment", loan.PaymentLeft.ToString(new CultureInfo("de-DE", false)));

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
                        airlineStatNode.SetAttribute("value", value.ToString(new CultureInfo("de-DE", false)));

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
                    invoiceNode.SetAttribute("amount", invoice.Amount.ToString(new CultureInfo("de-DE", false)));

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
                    feeNode.SetAttribute("value", airline.Fees.getValue(feetype).ToString(new CultureInfo("de-DE", false)));

                    feesNode.AppendChild(feeNode);
                }

                airlineNode.AppendChild(feesNode);

                XmlElement flightSchoolsNode = xmlDoc.CreateElement("flightschools");
                foreach (FlightSchool fs in airline.FlightSchools)
                {
                    XmlElement flightSchoolNode = xmlDoc.CreateElement("flightschool");
                    flightSchoolNode.SetAttribute("id", fs.ID);
                    flightSchoolNode.SetAttribute("airport", fs.Airport.Profile.ID);

                    XmlElement trainingAircraftsNode = xmlDoc.CreateElement("trainingaircrafts");
                    foreach (TrainingAircraft aircraft in fs.TrainingAircrafts)
                    {
                        XmlElement trainingAircraftNode = xmlDoc.CreateElement("trainingaircraft");
                        trainingAircraftNode.SetAttribute("type", aircraft.Type.Name);
                        trainingAircraftNode.SetAttribute("date", aircraft.BoughtDate.ToString(new CultureInfo("de-DE")));

                        trainingAircraftsNode.AppendChild(trainingAircraftNode);
                    }

                    flightSchoolNode.AppendChild(trainingAircraftsNode);

                    XmlElement studentsNode = xmlDoc.CreateElement("students");
                    foreach (PilotStudent student in fs.Students)
                    {
                        XmlElement studentNode = xmlDoc.CreateElement("student");

                        studentNode.SetAttribute("firstname", student.Profile.Firstname);
                        studentNode.SetAttribute("lastname", student.Profile.Lastname);
                        studentNode.SetAttribute("birthdate", student.Profile.Birthdate.ToString(new CultureInfo("de-DE")));
                        studentNode.SetAttribute("town", student.Profile.Town.Name);
                        studentNode.SetAttribute("startdate", student.StartDate.ToString(new CultureInfo("de-DE")));

                        studentsNode.AppendChild(studentNode);
                    }

                    flightSchoolNode.AppendChild(studentsNode);
                    flightSchoolsNode.AppendChild(flightSchoolNode);
                }

                airlineNode.AppendChild(flightSchoolsNode);

                XmlElement fleetNode = xmlDoc.CreateElement("fleet");
                foreach (FleetAirliner airliner in airline.Fleet)
                {
                    XmlElement fleetAirlinerNode = xmlDoc.CreateElement("airliner");
                    fleetAirlinerNode.SetAttribute("airliner", airliner.Airliner.TailNumber);
                    fleetAirlinerNode.SetAttribute("name", airliner.Airliner.ID);
                    fleetAirlinerNode.SetAttribute("homebase", airliner.Homebase.Profile.IATACode);
                    fleetAirlinerNode.SetAttribute("purchased", airliner.Purchased.ToString());
                    fleetAirlinerNode.SetAttribute("date", airliner.PurchasedDate.ToString(new CultureInfo("de-DE")));
                    fleetAirlinerNode.SetAttribute("groundeddate", airliner.GroundedToDate.ToString(new CultureInfo("de-DE")));

                    fleetAirlinerNode.SetAttribute("status", airliner.Status.ToString());
                   // fleetAirlinerNode.SetAttribute("latitude", airliner.CurrentPosition.Latitude.ToString());
                   // fleetAirlinerNode.SetAttribute("longitude", airliner.CurrentPosition.Longitude.ToString());

                    XmlElement airlinerStatsNode = xmlDoc.CreateElement("stats");
                    foreach (StatisticsType type in StatisticsTypes.GetStatisticsTypes())
                    {
                        foreach (int year in airliner.Statistics.getYears())
                        {
                            XmlElement airlinerStatNode = xmlDoc.CreateElement("stat");

                            double value = airliner.Statistics.getStatisticsValue(year, type);
                            airlinerStatNode.SetAttribute("year", year.ToString());
                            airlinerStatNode.SetAttribute("type", type.Shortname);
                            airlinerStatNode.SetAttribute("value", value.ToString(new CultureInfo("de-DE", false)));

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
                    routesNode.AppendChild(SaveRoute(xmlDoc, route));
                }
                airlineNode.AppendChild(routesNode);

                XmlElement flightsNode = xmlDoc.CreateElement("flights");
                foreach (FleetAirliner airliner in airline.Fleet)
                {
                    if (airliner.CurrentFlight != null && airliner.CurrentFlight.Entry != null)
                    {
                        XmlElement flightNode = xmlDoc.CreateElement("flight");

                        flightNode.SetAttribute("airliner", airliner.Airliner.ID);
                        flightNode.SetAttribute("route", airliner.CurrentFlight.Entry.TimeTable.Route.Id);
                        flightNode.SetAttribute("destination", airliner.CurrentFlight.Entry.Destination.FlightCode);
                        flightNode.SetAttribute("day", airliner.CurrentFlight.Entry.Day.ToString());
                        flightNode.SetAttribute("time", airliner.CurrentFlight.Entry.Time.ToString());
                        flightNode.SetAttribute("flighttime", airliner.CurrentFlight.FlightTime.ToString(new CultureInfo("de-DE")));

                        if (airliner.CurrentFlight is StopoverFlight)
                        {
                            XmlElement stopoverNode = xmlDoc.CreateElement("stopover");
                            stopoverNode.SetAttribute("currentflight", ((StopoverFlight)airliner.CurrentFlight).CurrentFlight.ToString());

                            if (airliner.CurrentFlight.isPassengerFlight())
                            {
                                XmlElement stopoverClassesNode = xmlDoc.CreateElement("classes");

                                foreach (RouteTimeTableEntry stopEntry in ((StopoverFlight)airliner.CurrentFlight).AllClasses.Keys)
                                {
                                    XmlElement stopoverClassNode = xmlDoc.CreateElement("class");
                                    stopoverClassNode.SetAttribute("id", stopEntry.ID);

                                    XmlElement stopoverFlightClassesNode = xmlDoc.CreateElement("flightclasses");
                                    foreach (FlightAirlinerClass faClass in ((StopoverFlight)airliner.CurrentFlight).AllClasses[stopEntry])
                                    {
                                        XmlElement stopoverFlightClassNode = xmlDoc.CreateElement("flightclass");
                                        stopoverFlightClassNode.SetAttribute("type", faClass.AirlinerClass.Type.ToString());
                                        stopoverFlightClassNode.SetAttribute("passengers", faClass.Passengers.ToString());

                                        stopoverFlightClassesNode.AppendChild(stopoverFlightClassNode);
                                    }
                                    stopoverClassNode.AppendChild(stopoverFlightClassesNode);
                                    stopoverClassesNode.AppendChild(stopoverClassNode);
                                }

                                stopoverNode.AppendChild(stopoverClassesNode);
                            }
                            if (airliner.CurrentFlight.isCargoFlight())
                            {

                                XmlElement stopoverCargosNode = xmlDoc.CreateElement("cargos");

                                foreach (RouteTimeTableEntry stopEntry in ((StopoverFlight)airliner.CurrentFlight).AllCargo.Keys)
                                {
                                    XmlElement stopoverCargoNode = xmlDoc.CreateElement("cargo");
                                    stopoverCargoNode.SetAttribute("id", stopEntry.ID);

                                    stopoverCargoNode.SetAttribute("value", ((StopoverFlight)airliner.CurrentFlight).AllCargo[stopEntry].ToString(new CultureInfo("de-DE", false)));

                                    stopoverCargosNode.AppendChild(stopoverCargoNode);
                                }

                                stopoverNode.AppendChild(stopoverCargosNode);
                            }
                            flightNode.AppendChild(stopoverNode);
                        }

                        if (airliner.CurrentFlight.isPassengerFlight())
                        {
                            XmlElement flightClassesNode = xmlDoc.CreateElement("flightclasses");
                            foreach (FlightAirlinerClass aClass in airliner.CurrentFlight.Classes)
                            {
                                XmlElement flightClassNode = xmlDoc.CreateElement("flightclass");
                                flightClassNode.SetAttribute("type", aClass.AirlinerClass.Type.ToString());
                                flightClassNode.SetAttribute("passengers", aClass.Passengers.ToString());

                                flightClassesNode.AppendChild(flightClassNode);
                            }
                            flightNode.AppendChild(flightClassesNode);
                        }

                        if (airliner.CurrentFlight.isCargoFlight())
                        {
                            XmlElement flightCargoNode = xmlDoc.CreateElement("flightcargo");
                            flightCargoNode.SetAttribute("cargo", airliner.CurrentFlight.Cargo.ToString(new CultureInfo("de-DE", false)));

                            flightNode.AppendChild(flightCargoNode);
                        }

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
                airportNode.SetAttribute("income", airport.Income.ToString());

                XmlElement airportPaxsNode = xmlDoc.CreateElement("paxvalues");
                foreach (PaxValue paxValue in airport.Profile.PaxValues)
                {
                    XmlElement airportPaxNode = xmlDoc.CreateElement("paxvalue");
                    airportPaxNode.SetAttribute("from", paxValue.FromYear.ToString());
                    airportPaxNode.SetAttribute("to", paxValue.ToYear.ToString());
                    airportPaxNode.SetAttribute("size", paxValue.Size.ToString());
                    airportPaxNode.SetAttribute("pax", paxValue.Pax.ToString());
                    airportPaxNode.SetAttribute("inflationbefore", paxValue.InflationBeforeYear.ToString());
                    airportPaxNode.SetAttribute("inflationafter", paxValue.InflationAfterYear.ToString());

                    airportPaxsNode.AppendChild(airportPaxNode);
                }
                airportNode.AppendChild(airportPaxsNode);

                XmlElement airportRunwaysNode = xmlDoc.CreateElement("runways");

                foreach (Runway runway in airport.Runways.FindAll(r => !r.Standard))
                {
                    XmlElement airportRunwayNode = xmlDoc.CreateElement("runway");
                    airportRunwayNode.SetAttribute("name", runway.Name);
                    airportRunwayNode.SetAttribute("lenght", runway.Length.ToString());
                    airportRunwayNode.SetAttribute("surface", runway.Surface.ToString());
                    airportRunwayNode.SetAttribute("date", runway.BuiltDate.ToString(new CultureInfo("de-DE")));

                    airportRunwaysNode.AppendChild(airportRunwayNode);
                }

                airportNode.AppendChild(airportRunwaysNode);

                XmlElement airportHubsNode = xmlDoc.CreateElement("hubs");
                foreach (Hub hub in airport.getHubs(HubType.TypeOfHub.Hub))
                {
                    XmlElement airportHubNode = xmlDoc.CreateElement("hub");
                    airportHubNode.SetAttribute("airline", hub.Airline.Profile.IATACode);

                    airportHubsNode.AppendChild(airportHubNode);
                }

                airportNode.AppendChild(airportHubsNode);

             
                XmlElement airportWeathersNode = xmlDoc.CreateElement("weathers");
                foreach (Weather weather in airport.Weather)
                {
                    if (weather != null)
                    {
                        XmlElement airportWeatherNode = xmlDoc.CreateElement("weather");
                        airportWeatherNode.SetAttribute("date", weather.Date.ToString(new CultureInfo("de-DE")));
                        airportWeatherNode.SetAttribute("direction", weather.Direction.ToString());
                        airportWeatherNode.SetAttribute("windspeed", weather.WindSpeed.ToString());
                        airportWeatherNode.SetAttribute("cover", weather.Cover.ToString());
                        airportWeatherNode.SetAttribute("precip", weather.Precip.ToString());
                        airportWeatherNode.SetAttribute("temperaturelow", weather.TemperatureLow.ToString(new CultureInfo("de-DE", false)));
                        airportWeatherNode.SetAttribute("temperaturehigh", weather.TemperatureHigh.ToString(new CultureInfo("de-DE", false)));

                        XmlElement temperaturesNode = xmlDoc.CreateElement("temperatures");
                        for (int i = 0; i < weather.Temperatures.Length; i++)
                        {
                            XmlElement temperatureNode = xmlDoc.CreateElement("temperature");
                            temperatureNode.SetAttribute("temp", weather.Temperatures[i].Temperature.ToString(new CultureInfo("de-DE", false)));
                            temperatureNode.SetAttribute("cover", weather.Temperatures[i].Cover.ToString());
                            temperatureNode.SetAttribute("precip", weather.Temperatures[i].Precip.ToString());
                            temperatureNode.SetAttribute("windspeed", weather.Temperatures[i].WindSpeed.ToString());
                            temperatureNode.SetAttribute("direction", weather.Temperatures[i].Direction.ToString());

                            temperaturesNode.AppendChild(temperatureNode);

                        }
                        airportWeatherNode.AppendChild(temperaturesNode);

                        airportWeathersNode.AppendChild(airportWeatherNode);
                    }
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
                                airportStatNode.SetAttribute("value", value.ToString(new CultureInfo("de-DE", false)));

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

                foreach (AirlineAirportFacility facility in airport.getAirportFacilities().Where(f=>f.Airline == null))
                {
                    XmlElement airportFacilityNode = xmlDoc.CreateElement("facility");
                    airportFacilityNode.SetAttribute("airline","-");
                    airportFacilityNode.SetAttribute("name", facility.Facility.Shortname);
                    airportFacilityNode.SetAttribute("finished", facility.FinishedDate.ToString(new CultureInfo("de-DE")));

                    airportFacilitiesNode.AppendChild(airportFacilityNode);
           
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
                  
                        gatesNode.AppendChild(gateNode);
                    }
                    terminalNode.AppendChild(gatesNode);

                    terminalsNode.AppendChild(terminalNode);
                }
                airportNode.AppendChild(terminalsNode);

                XmlElement contractsNode = xmlDoc.CreateElement("contracts");

                foreach (AirportContract contract in airport.AirlineContracts)
                {
                    XmlElement contractNode = xmlDoc.CreateElement("contract");

                    contractNode.SetAttribute("airline", contract.Airline.Profile.IATACode);
                    contractNode.SetAttribute("length", contract.Length.ToString());
                    contractNode.SetAttribute("date", contract.ContractDate.ToString(new CultureInfo("de-DE")));
                    contractNode.SetAttribute("gates", contract.NumberOfGates.ToString());
                    contractNode.SetAttribute("payment", contract.YearlyPayment.ToString(new CultureInfo("de-DE", false)));
                    contractNode.SetAttribute("exclusive", contract.IsExclusiveDeal.ToString());

                    if (contract.Terminal != null)
                        contractNode.SetAttribute("terminal", contract.Terminal.Name);

           
                    contractsNode.AppendChild(contractNode);
                }
                airportNode.AppendChild(contractsNode);

                airportsNode.AppendChild(airportNode);



            }
            root.AppendChild(airportsNode);

            sw.Restart();

            XmlElement airportDestinationsNode = xmlDoc.CreateElement("airportdestinations");

            var destAirports = Airports.GetAllAirports();

            foreach (Airport airport in Airports.GetAllAirports())
            {
                XmlElement airportDestinationNode = xmlDoc.CreateElement("airportdestination");
                airportDestinationNode.SetAttribute("id", airport.Profile.IATACode);

                XmlElement destinationsNode = xmlDoc.CreateElement("destinations");
                //var dests = Airports.GetAirports(a => a != airport && (airport.hasDestinationPassengersRate(a) || airport.hasDestinationStatistics(a)));
             //foreach (Airport dest in Airports.GetAirports(a => a != airport && (airport.hasDestinationPassengersRate(a) || airport.hasDestinationPassengerStatistics(a) || airport.hasDestinationCargStatistics(a))))
                //Parallel.ForEach(dests, dest =>
               foreach (Airport dest in destAirports)
                {

                    if (dest != airport && (airport.hasDestinationPassengersRate(dest) || airport.hasDestinationPassengerStatistics(dest) || airport.hasDestinationCargStatistics(dest)))
                    {
                        XmlElement destinationNode = xmlDoc.CreateElement("destination");
                        destinationNode.SetAttribute("id", dest.Profile.IATACode);
                        destinationNode.SetAttribute("rate", airport.getDestinationPassengersRate(dest, AirlinerClass.ClassType.Economy_Class).ToString());
                        destinationNode.SetAttribute("passengers", airport.getDestinationPassengerStatistics(dest).ToString());

                        destinationNode.SetAttribute("cargo", airport.getDestinationCargoRate(dest).ToString());
                        destinationNode.SetAttribute("cargostats", airport.getDestinationCargoStatistics(dest).ToString());

                        destinationsNode.AppendChild(destinationNode);
                    }
                }//);
                airportDestinationNode.AppendChild(destinationsNode);
                airportDestinationsNode.AppendChild(airportDestinationNode);
            }

            root.AppendChild(airportDestinationsNode);

            Console.WriteLine("Saving of destinations: {0} ms.", sw.ElapsedMilliseconds);

            XmlElement instructorsNode = xmlDoc.CreateElement("instructors");

            foreach (Instructor instructor in Instructors.GetInstructors())
            {
                XmlElement instructorNode = xmlDoc.CreateElement("instructor");
                instructorNode.SetAttribute("firstname", instructor.Profile.Firstname);
                instructorNode.SetAttribute("lastname", instructor.Profile.Lastname);
                instructorNode.SetAttribute("birthdate", instructor.Profile.Birthdate.ToString(new CultureInfo("de-DE")));
                instructorNode.SetAttribute("town", instructor.Profile.Town.Name);
                instructorNode.SetAttribute("rating", instructor.Rating.ToString());
                instructorNode.SetAttribute("id", instructor.FlightSchool == null ? "-" : instructor.FlightSchool.ID);

                XmlElement studentsNode = xmlDoc.CreateElement("students");

                foreach (PilotStudent student in instructor.Students)
                {
                    XmlElement studentNode = xmlDoc.CreateElement("student");
                    studentNode.SetAttribute("name", student.Profile.Name);

                    studentsNode.AppendChild(studentNode);
                }

                instructorNode.AppendChild(studentsNode);

                instructorsNode.AppendChild(instructorNode);
            }

            root.AppendChild(instructorsNode);

            XmlElement pilotsNode = xmlDoc.CreateElement("pilots");

            foreach (Pilot pilot in Pilots.GetPilots())
            {
                XmlElement pilotNode = xmlDoc.CreateElement("pilot");
                pilotNode.SetAttribute("firstname", pilot.Profile.Firstname);
                pilotNode.SetAttribute("lastname", pilot.Profile.Lastname);
                pilotNode.SetAttribute("birthdate", pilot.Profile.Birthdate.ToString(new CultureInfo("de-DE")));
                pilotNode.SetAttribute("town", pilot.Profile.Town.Name);
                pilotNode.SetAttribute("education", pilot.EducationTime.ToString(new CultureInfo("de-DE")));
                pilotNode.SetAttribute("rating", pilot.Rating.ToString());
                pilotNode.SetAttribute("airline", pilot.Airline == null ? "-" : pilot.Airline.Profile.IATACode);
                pilotNode.SetAttribute("airlinesigned", pilot.AirlineSignedDate.ToString(new CultureInfo("de-DE")));
                pilotNode.SetAttribute("airliner", pilot.Airliner == null ? "-" : pilot.Airliner.Airliner.ID);

                pilotsNode.AppendChild(pilotNode);
            }

            root.AppendChild(pilotsNode);

            XmlElement alliancesNode = xmlDoc.CreateElement("alliances");
            foreach (Alliance alliance in Alliances.GetAlliances())
            {
                XmlElement allianceNode = xmlDoc.CreateElement("alliance");
                allianceNode.SetAttribute("name", alliance.Name);
                allianceNode.SetAttribute("formation", alliance.FormationDate.ToString(new CultureInfo("de-DE")));
                allianceNode.SetAttribute("type", alliance.Type.ToString());
                allianceNode.SetAttribute("headquarter", alliance.Headquarter.Profile.IATACode);

                XmlElement membersNode = xmlDoc.CreateElement("members");

                foreach (AllianceMember airline in alliance.Members)
                {
                    XmlElement memberNode = xmlDoc.CreateElement("member");
                    memberNode.SetAttribute("airline", airline.Airline.Profile.IATACode);
                    memberNode.SetAttribute("joined", airline.JoinedDate.ToString(new CultureInfo("de-DE")));

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

            XmlElement difficultyNode = xmlDoc.CreateElement("difficulty");
            difficultyNode.SetAttribute("name", GameObject.GetInstance().Difficulty.Name);
            difficultyNode.SetAttribute("money", GameObject.GetInstance().Difficulty.MoneyLevel.ToString(new CultureInfo("de-DE", false)));
            difficultyNode.SetAttribute("price", GameObject.GetInstance().Difficulty.PriceLevel.ToString(new CultureInfo("de-DE", false)));
            difficultyNode.SetAttribute("loan", GameObject.GetInstance().Difficulty.LoanLevel.ToString(new CultureInfo("de-DE", false)));
            difficultyNode.SetAttribute("passengers", GameObject.GetInstance().Difficulty.PassengersLevel.ToString(new CultureInfo("de-DE", false)));
            difficultyNode.SetAttribute("ai", GameObject.GetInstance().Difficulty.AILevel.ToString(new CultureInfo("de-DE", false)));

            root.AppendChild(difficultyNode);

            if (GameObject.GetInstance().Scenario != null)
            {
                XmlElement scenarioNode = xmlDoc.CreateElement("scenario");
                scenarioNode.SetAttribute("name", GameObject.GetInstance().Scenario.Scenario.Name);
                scenarioNode.SetAttribute("success", GameObject.GetInstance().Scenario.IsSuccess.ToString());

                if (GameObject.GetInstance().Scenario.ScenarioFailed != null)
                    scenarioNode.SetAttribute("failed", GameObject.GetInstance().Scenario.ScenarioFailed.ID);

                XmlElement failuresNode = xmlDoc.CreateElement("failures");

                foreach (ScenarioFailureObject sfo in GameObject.GetInstance().Scenario.getScenarioFailures())
                {
                    XmlElement failureNode = xmlDoc.CreateElement("failure");

                    failureNode.SetAttribute("id", sfo.Failure.ID);
                    failureNode.SetAttribute("count", sfo.Failures.ToString());
                    failureNode.SetAttribute("lastfailuretime", sfo.LastFailureTime.ToString(new CultureInfo("de-DE")));

                    failuresNode.AppendChild(failureNode);
                }

                scenarioNode.AppendChild(failuresNode);

                root.AppendChild(scenarioNode);
            }

            XmlElement gameSettingsNode = xmlDoc.CreateElement("gamesettings");
            gameSettingsNode.SetAttribute("name", GameObject.GetInstance().Name);
            gameSettingsNode.SetAttribute("human", GameObject.GetInstance().HumanAirline.Profile.IATACode);
            gameSettingsNode.SetAttribute("mainairline", GameObject.GetInstance().MainAirline.Profile.IATACode);
            gameSettingsNode.SetAttribute("fuelprice", GameObject.GetInstance().FuelPrice.ToString(new CultureInfo("de-DE", false)));
            gameSettingsNode.SetAttribute("timezone", GameObject.GetInstance().TimeZone.UTCOffset.ToString());
            gameSettingsNode.SetAttribute("mailonlandings", Settings.GetInstance().MailsOnLandings.ToString());
            gameSettingsNode.SetAttribute("mailonbadweather", Settings.GetInstance().MailsOnBadWeather.ToString());
            gameSettingsNode.SetAttribute("airportcode", Settings.GetInstance().AirportCodeDisplay.ToString());
            gameSettingsNode.SetAttribute("minutesperturn", Settings.GetInstance().MinutesPerTurn.ToString());
            gameSettingsNode.SetAttribute("language", AppSettings.GetInstance().getLanguage().Name);
            gameSettingsNode.SetAttribute("dayround", GameObject.GetInstance().DayRoundEnabled.ToString());

            XmlElement itemsNodes = xmlDoc.CreateElement("calendaritems");

            foreach (CalendarItem item in CalendarItems.GetCalendarItems())
            {
                XmlElement itemNode = xmlDoc.CreateElement("calendaritem");

                itemNode.SetAttribute("type", item.Type.ToString());
                itemNode.SetAttribute("date", item.Date.ToString(new CultureInfo("de-DE")));
                itemNode.SetAttribute("header", item.Header);
                itemNode.SetAttribute("subject", item.Subject);

                itemsNodes.AppendChild(itemNode);
            }

            gameSettingsNode.AppendChild(itemsNodes);


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

            sw.Stop();

            //Console.WriteLine("Time elapsed: {0} ms.", sw.ElapsedMilliseconds);

            //WaitFor.tasks
            using (FileStream fs = new FileStream(path, FileMode.Create))
            {
                Stream s;

                s = new GZipStream(fs, CompressionMode.Compress);

                xmlDoc.Save(s);
                s.Close();




            }

            // xmlDoc.Save(path);


        }
        //loads a route
        private static Route LoadRoute(XmlElement routeNode, Airline airline, Route.RouteType routetype = Route.RouteType.Passenger)
        {
            string id = routeNode.Attributes["id"].Value;
            Airport dest1 = Airports.GetAirport(routeNode.Attributes["destination1"].Value);
            Airport dest2 = Airports.GetAirport(routeNode.Attributes["destination2"].Value);
            Boolean isBanned = Convert.ToBoolean(routeNode.Attributes["isbanned"].Value);

            if (routeNode.HasAttribute("type"))
                routetype = (Route.RouteType)Enum.Parse(typeof(Route.RouteType), routeNode.Attributes["type"].Value);

            Route route;

            if (routetype == Route.RouteType.Passenger || routetype == Route.RouteType.Mixed)
                route = new PassengerRoute(id, dest1, dest2, 0);
            else
                route = new CargoRoute(id, dest1, dest2, 0);

            route.Banned = isBanned;

            /* foreach (StopoverRoute stopover in route.Stopovers)
            {
                XmlElement routeStopoverNode = xmlDoc.CreateElement("stopover");
                routeStopoverNode.SetAttribute("airport", stopover.Stopover.Profile.ID);

                XmlElement stopoverLegsNode = xmlDoc.CreateElement("legs");
                foreach (Route leg in stopover.Legs)
                {
                    XmlElement stopoverLegNode = xmlDoc.CreateElement("leg");

                    stopoverLegNode.AppendChild(SaveRoute(xmlDoc, leg));

                    stopoverLegsNode.AppendChild(stopoverLegNode);
                }
                routeStopoverNode.AppendChild(stopoverLegsNode);
                routeStopoversNode.AppendChild(routeStopoverNode);*/

            XmlNodeList routeStopoverList = routeNode.SelectNodes("stopovers/stopover");

            foreach (XmlElement routeStopoverNode in routeStopoverList)
            {
                Airport stopoverAirport = Airports.GetAirportFromID(routeStopoverNode.Attributes["airport"].Value);

                StopoverRoute stopoverRoute = new StopoverRoute(stopoverAirport);

                XmlNodeList legsList = routeStopoverNode.SelectNodes("legs/leg");

                foreach (XmlElement legNode in legsList)
                {

                    stopoverRoute.addLeg(LoadRoute((XmlElement)legNode.SelectSingleNode("route"), airline, routetype));
                }

                route.addStopover(stopoverRoute);

            }

            if (routetype == Route.RouteType.Passenger || routetype == Route.RouteType.Mixed)
            {
                ((PassengerRoute)route).Classes.Clear();

                XmlNodeList routeClassList = routeNode.SelectNodes("routeclasses/routeclass");

                foreach (XmlElement routeClassNode in routeClassList)
                {
                    AirlinerClass.ClassType airlinerClassType = (AirlinerClass.ClassType)Enum.Parse(typeof(AirlinerClass.ClassType), routeClassNode.Attributes["type"].Value);
                    double fareprice = Convert.ToDouble(routeClassNode.Attributes["fareprice"].Value,new CultureInfo("de-DE", false));
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

                    ((PassengerRoute)route).addRouteAirlinerClass(rClass);

                }
            }
            if (routetype == Route.RouteType.Mixed || routetype == Route.RouteType.Cargo)
            {
                XmlElement routeCargoNode = (XmlElement)routeNode.SelectSingleNode("cargo");
                double unitPrice = Convert.ToDouble(routeCargoNode.Attributes["priceperunit"].Value,new CultureInfo("de-DE", false));

                ((CargoRoute)route).PricePerUnit = unitPrice;

            }

            RouteTimeTable timeTable = new RouteTimeTable(route);

            XmlNodeList timetableList = routeNode.SelectNodes("timetable/timetableentry");

            foreach (XmlElement entryNode in timetableList)
            {
                Airport entryDest = Airports.GetAirport(entryNode.Attributes["destination"].Value);
                string flightCode = entryNode.Attributes["flightcode"].Value;
                DayOfWeek day = (DayOfWeek)Enum.Parse(typeof(DayOfWeek), entryNode.Attributes["day"].Value);
                TimeSpan time = TimeSpan.Parse(entryNode.Attributes["time"].Value);
                FleetAirliner airliner = entryNode.Attributes["airliner"].Value == "-" ? null : airline.Fleet.Find(a => a.Airliner.ID == entryNode.Attributes["airliner"].Value); ;

                RouteTimeTableEntry entry = new RouteTimeTableEntry(timeTable, day, time, new RouteEntryDestination(entryDest, flightCode,null),null);

                if (entryNode.HasAttribute("id"))
                    entry.ID = entryNode.Attributes["id"].Value;

                if (entryNode.HasAttribute("mainentry")) entry.MainEntry = airline.Routes.SelectMany(r => r.TimeTable.Entries).ToList().Find(e => e.ID == entryNode.Attributes["mainentry"].Value);

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
                double invoiceAmount = Convert.ToDouble(routeInvoiceNode.Attributes["amount"].Value, new CultureInfo("de-DE", false));

                route.setRouteInvoice(type, invoiceYear, invoiceMonth, invoiceAmount);
            }

            return route;
        }
        //saves a route
        private static XmlElement SaveRoute(XmlDocument xmlDoc, Route route)
        {
            XmlElement routeNode = xmlDoc.CreateElement("route");
            routeNode.SetAttribute("id", route.Id);
            routeNode.SetAttribute("destination1", route.Destination1.Profile.IATACode);
            routeNode.SetAttribute("destination2", route.Destination2.Profile.IATACode);
            routeNode.SetAttribute("isbanned", route.Banned.ToString());
            routeNode.SetAttribute("type", route.Type.ToString());

            XmlElement routeStopoversNode = xmlDoc.CreateElement("stopovers");

            foreach (StopoverRoute stopover in route.Stopovers)
            {
                XmlElement routeStopoverNode = xmlDoc.CreateElement("stopover");
                routeStopoverNode.SetAttribute("airport", stopover.Stopover.Profile.ID);

                XmlElement stopoverLegsNode = xmlDoc.CreateElement("legs");
                foreach (Route leg in stopover.Legs) 
                {
                    XmlElement stopoverLegNode = xmlDoc.CreateElement("leg");

                    stopoverLegNode.AppendChild(SaveRoute(xmlDoc, leg));

                    stopoverLegsNode.AppendChild(stopoverLegNode);
                }
                routeStopoverNode.AppendChild(stopoverLegsNode);
                routeStopoversNode.AppendChild(routeStopoverNode);
            }

            routeNode.AppendChild(routeStopoversNode);

            if (route.Type == Route.RouteType.Passenger || route.Type == Route.RouteType.Mixed)
            {

                XmlElement routeClassesNode = xmlDoc.CreateElement("routeclasses");

                foreach (RouteAirlinerClass aClass in ((PassengerRoute)route).Classes)
                {
                    XmlElement routeClassNode = xmlDoc.CreateElement("routeclass");
                    routeClassNode.SetAttribute("type", aClass.Type.ToString());
                    routeClassNode.SetAttribute("fareprice", aClass.FarePrice.ToString(new CultureInfo("de-DE", false)));

                    foreach (RouteFacility facility in aClass.getFacilities())
                        routeClassNode.SetAttribute(facility.Type.ToString(), facility.Uid);
                    // chs, 2011-18-10 added for saving of type of seating
                    routeClassNode.SetAttribute("seating", aClass.Seating.ToString());

                    routeClassesNode.AppendChild(routeClassNode);
                }
                routeNode.AppendChild(routeClassesNode);

            }

            if (route.Type == Route.RouteType.Mixed || route.Type == Route.RouteType.Cargo)
            {
                XmlElement routeCargoNode = xmlDoc.CreateElement("cargo");
                routeCargoNode.SetAttribute("priceperunit", ((CargoRoute)route).PricePerUnit.ToString(new CultureInfo("de-DE", false)));

                routeNode.AppendChild(routeCargoNode);
            }

            XmlElement timetableNode = xmlDoc.CreateElement("timetable");

            foreach (RouteTimeTableEntry entry in route.TimeTable.Entries)
            {
                XmlElement ttEntryNode = xmlDoc.CreateElement("timetableentry");
                ttEntryNode.SetAttribute("destination", entry.Destination.Airport.Profile.IATACode);
                ttEntryNode.SetAttribute("flightcode", entry.Destination.FlightCode);
                ttEntryNode.SetAttribute("day", entry.Day.ToString());
                ttEntryNode.SetAttribute("time", entry.Time.ToString());
                ttEntryNode.SetAttribute("airliner", entry.Airliner != null ? entry.Airliner.Airliner.ID : "-");
                ttEntryNode.SetAttribute("id", entry.ID);
                if (entry.MainEntry != null) ttEntryNode.SetAttribute("mainentry", entry.MainEntry.ID);


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
                routeInvoiceNode.SetAttribute("amount",invoice.Amount.ToString(new CultureInfo("de-DE", false)));

                routeInvoicesNode.AppendChild(routeInvoiceNode);
            }
            routeNode.AppendChild(routeInvoicesNode);

            return routeNode;
        }

    }
}
