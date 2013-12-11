using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Device.Location;
using TheAirline.Model.AirlinerModel.RouteModel;
using TheAirline.Model.AirlinerModel;
using TheAirline.Model.AirlineModel;
using TheAirline.Model.AirportModel;
using TheAirline.Model.GeneralModel.StatisticsModel;
using TheAirline.Model.PassengerModel;
using TheAirline.GraphicsModel.UserControlModel.PopUpWindowsModel;
using TheAirline.GraphicsModel.UserControlModel.MessageBoxModel;
using TheAirline.Model.GeneralModel.HolidaysModel;
using TheAirline.Model.GeneralModel.HistoricEventModel;
using TheAirline.Model.GeneralModel.Helpers.WorkersModel;
using TheAirline.Model.GeneralModel.WeatherModel;
using System.Threading.Tasks;
using TheAirline.Model.PilotModel;
using TheAirline.Model.StatisticsModel;
using TheAirline.Model.AirlineModel.SubsidiaryModel;
using System.Diagnostics;
using TheAirline.GUIModel.ObjectsModel;
using TheAirline.Model.GeneralModel.CountryModel;
using TheAirline.GUIModel.PagesModel.GamePageModel;
using System.Globalization;
using TheAirline.GUIModel.HelpersModel;
using TheAirline.GUIModel.CustomControlsModel.PopUpWindowsModel;

namespace TheAirline.Model.GeneralModel.Helpers
{
    //the helper class for the game object
    public class GameObjectHelpers
    {
        private static Random rnd = new Random();
        private static DateTime LastTime;
        //simulates a "turn"
        public static void SimulateTurn()
        {
            if (GameObject.GetInstance().DayRoundEnabled)
            {
                Stopwatch sw = new Stopwatch();
                sw.Start();

                GameObject.GetInstance().GameTime = GameObject.GetInstance().GameTime.AddDays(1);

                DoDailyUpdate();

                if (MathHelpers.IsNewMonth(GameObject.GetInstance().GameTime)) DoMonthlyUpdate();

                if (MathHelpers.IsNewYear(GameObject.GetInstance().GameTime)) DoYearlyUpdate();


                Parallel.ForEach(Airlines.GetAllAirlines(), airline =>
                {
                    if (!airline.IsHuman)
                    {
                        AIHelpers.UpdateCPUAirline(airline);
                    }

                    DayTurnHelpers.SimulateAirlineFlights(airline);

                });

                // Console.WriteLine("{0} airlines: {1} airliners: {2} routes: {3} flights: {4} airports: {5} total time per round: {6} ms.", GameObject.GetInstance().GameTime.ToShortDateString(), Airlines.GetAllAirlines().Count, Airlines.GetAllAirlines().Sum(a => a.Fleet.Count), Airlines.GetAllAirlines().Sum(a => a.Routes.Count), Airlines.GetAllAirlines().Sum(a => a.Routes.Sum(r => r.TimeTable.Entries.Count)), Airports.GetAllAirports().Count, sw.ElapsedMilliseconds);
                sw.Stop();
            }
            else
            {
                Stopwatch sw = new Stopwatch();
                sw.Start();
                GameObject.GetInstance().GameTime = GameObject.GetInstance().GameTime.AddMinutes(Settings.GetInstance().MinutesPerTurn);

                CalibrateTime();

                if (MathHelpers.IsNewDay(GameObject.GetInstance().GameTime)) DoDailyUpdate();

                if (MathHelpers.IsNewYear(GameObject.GetInstance().GameTime)) DoYearlyUpdate();

                Parallel.ForEach(Airlines.GetAllAirlines(), airline =>
                {
                    if (GameObject.GetInstance().GameTime.Minute == 0 && GameObject.GetInstance().GameTime.Hour == airline.Airports.Count % 24)
                    {
                        if (!airline.IsHuman)
                        {
                            AIHelpers.UpdateCPUAirline(airline);
                        }
                    }

                    Parallel.ForEach(airline.Fleet, airliner =>
                        {
                            UpdateAirliner(airliner);
                        });

                });
                if (MathHelpers.IsNewMonth(GameObject.GetInstance().GameTime)) DoMonthlyUpdate();
                sw.Stop();
            }
        }
        //calibrates the time if needed
        private static void CalibrateTime()
        {
            if (Settings.GetInstance().MinutesPerTurn == 60 && !(GameObject.GetInstance().GameTime.Minute == 0))
                GameObject.GetInstance().GameTime = GameObject.GetInstance().GameTime.AddMinutes(-GameObject.GetInstance().GameTime.Minute);

            if (Settings.GetInstance().MinutesPerTurn == 30 && GameObject.GetInstance().GameTime.Minute == 15)
                GameObject.GetInstance().GameTime = GameObject.GetInstance().GameTime.AddMinutes(15);
        }

        //do the daily update
        private static void DoDailyUpdate()
        { 
            //Clear stats when it on daily update
            if (Settings.GetInstance().ClearStats == Settings.Intervals.Daily)
                ClearAllUsedStats();

            //Auto save when it on daily
            if (Settings.GetInstance().AutoSave == Settings.Intervals.Daily)
                SerializedLoadSaveHelpers.SaveGame("autosave");
            
            //Clearing stats as an RAM work-a-round
            Airports.GetAllAirports().ForEach(a => a.clearDestinationPassengerStatistics());
            Airports.GetAllAirports().ForEach(a => a.clearDestinationCargoStatistics());

            var humanAirlines = Airlines.GetAirlines(a => a.IsHuman);

         
            //Console.WriteLine(GameObject.GetInstance().GameTime.ToShortDateString() + ": " + DateTime.Now.Subtract(LastTime).TotalMilliseconds + " ms." + " : routes: " + totalRoutes + " airliners on route: " + totalAirlinersOnRoute);

            LastTime = DateTime.Now;
            //changes the fuel prices 
            double fuelDiff = Inflations.GetInflation(GameObject.GetInstance().GameTime.Year + 1).FuelPrice - Inflations.GetInflation(GameObject.GetInstance().GameTime.Year).FuelPrice;
            double fuelPrice = (rnd.NextDouble() * (fuelDiff / 4));

            GameObject.GetInstance().FuelPrice = Inflations.GetInflation(GameObject.GetInstance().GameTime.Year).FuelPrice + fuelPrice;
            //checks for airports due to close in 14 days
            var closingAirports = Airports.GetAllAirports(a => a.Profile.Period.To.ToShortDateString() == GameObject.GetInstance().GameTime.AddDays(14).ToShortDateString());
            var openingAirports = Airports.GetAllAirports(a => a.Profile.Period.From.ToShortDateString() == GameObject.GetInstance().GameTime.AddDays(14).ToShortDateString());

            foreach (Airport airport in closingAirports)
            {
                Airport reallocatedAirport = openingAirports.Find(a => a.Profile.Town == airport.Profile.Town);

                if (reallocatedAirport == null)
                    GameObject.GetInstance().NewsBox.addNews(new News(News.NewsType.Airport_News, GameObject.GetInstance().GameTime, "Airport closing", string.Format("The airport [LI airport={0}]({1}) is closing in 14 days.\n\rPlease move all routes to another destination.", airport.Profile.IATACode, new AirportCodeConverter().Convert(airport).ToString())));
                else
                    GameObject.GetInstance().NewsBox.addNews(new News(News.NewsType.Airport_News, GameObject.GetInstance().GameTime, "Airport closing", string.Format("The airport [LI airport={0}]({1}) is closing in 14 days.\n\rThe airport will be replaced by {2}({3}) and all gates and routes from {0} will be reallocated to {2}.", airport.Profile.IATACode, new AirportCodeConverter().Convert(airport).ToString(), reallocatedAirport.Profile.Name, new AirportCodeConverter().Convert(reallocatedAirport).ToString())));

                CalendarItems.AddCalendarItem(new CalendarItem(CalendarItem.ItemType.Airport_Closing, airport.Profile.Period.To, "Airport closing", string.Format("{0}, {1}", airport.Profile.Name, ((Country)new CountryCurrentCountryConverter().Convert(airport.Profile.Country)).Name)));
            }

            foreach (Airport airport in openingAirports)
            {
                GameObject.GetInstance().NewsBox.addNews(new News(News.NewsType.Airport_News, GameObject.GetInstance().GameTime, "Airport opening", string.Format("A new airport {0}({1}) is opening in 14 days in {2}, {3}.", airport.Profile.Name, new AirportCodeConverter().Convert(airport).ToString(), airport.Profile.Town.Name, ((Country)new CountryCurrentCountryConverter().Convert(airport.Profile.Country)).Name)));
                CalendarItems.AddCalendarItem(new CalendarItem(CalendarItem.ItemType.Airport_Opening, airport.Profile.Period.From, "Airport opening", string.Format("{0}, {1}", airport.Profile.Name, ((Country)new CountryCurrentCountryConverter().Convert(airport.Profile.Country)).Name)));

            }
            //checks for new airports which are opening
            List<Airport> openedAirports = Airports.GetAllAirports(a => a.Profile.Period.From.ToShortDateString() == GameObject.GetInstance().GameTime.ToShortDateString());
            List<Airport> closedAirports = Airports.GetAllAirports(a => a.Profile.Period.To.ToShortDateString() == GameObject.GetInstance().GameTime.ToShortDateString());

            //checks for airports which are closing down
            foreach (Airport airport in closedAirports)
            {
                //check for airport which are reallocated 
                Airport reallocatedAirport = openedAirports.Find(a => a.Profile.Town == airport.Profile.Town);


                if (reallocatedAirport != null)
                {



                    var airlines = new List<Airline>(from c in airport.AirlineContracts select c.Airline).Distinct();
                    foreach (Airline airline in airlines)
                    {
                        AirlineHelpers.ReallocateAirport(airport, reallocatedAirport, airline);

                        if (airline.IsHuman)
                            GameObject.GetInstance().NewsBox.addNews(new News(News.NewsType.Airport_News, GameObject.GetInstance().GameTime, "Airport operations changed", string.Format("All your gates, routes and facilities has been moved from {0}({1}) to [LI airport={2}]({3})", airport.Profile.Name, new AirportCodeConverter().Convert(airport).ToString(), reallocatedAirport.Profile.IATACode, new AirportCodeConverter().Convert(reallocatedAirport).ToString())));
                    }
                }

                GameObject.GetInstance().NewsBox.addNews(new News(News.NewsType.Airport_News, GameObject.GetInstance().GameTime, "Airport closed", string.Format("The airport {0}({1}) has now been closed. \n\rAll routes to and from the airports has been cancelled.", airport.Profile.Name, new AirportCodeConverter().Convert(airport).ToString())));

                var obsoleteRoutes = (from r in Airlines.GetAllAirlines().SelectMany(a => a.Routes) where r.Destination1 == airport || r.Destination2 == airport select r);

                foreach (Route route in obsoleteRoutes)
                {
                    route.Banned = true;

                    foreach (FleetAirliner airliner in route.getAirliners())
                    {
                        if (airliner.Homebase == airport)
                        {
                            if (airliner.Airliner.Airline.IsHuman)
                            {

                                airliner.Homebase = (Airport)PopUpNewAirlinerHomebase.ShowPopUp(airliner);


                            }
                            else
                            {
                                AIHelpers.SetAirlinerHomebase(airliner);
                            }

                        }

                    }
                }
            }
            //checks for new airliner types for purchase
            foreach (AirlinerType aType in AirlinerTypes.GetTypes(a => a.Produced.From.ToShortDateString() == GameObject.GetInstance().GameTime.ToShortDateString()))
                GameObject.GetInstance().NewsBox.addNews(new News(News.NewsType.Airliner_News, GameObject.GetInstance().GameTime, "New airliner type available", string.Format("{0} has finished the design of {1} and it is now available for purchase", aType.Manufacturer.Name, aType.Name)));

            //checks for airliner types which are out of production
            foreach (AirlinerType aType in AirlinerTypes.GetTypes(a => a.Produced.To.ToShortDateString() == GameObject.GetInstance().GameTime.ToShortDateString()))
                GameObject.GetInstance().NewsBox.addNews(new News(News.NewsType.Airliner_News, GameObject.GetInstance().GameTime, "Airliner type out of production", string.Format("{0} has taken {1} out of production", aType.Manufacturer.Name, aType.Name)));

            //checks for airport facilities for the human airline
            var humanAirportFacilities = (from f in humanAirlines.SelectMany(ai => ai.Airports.SelectMany(a => a.getAirportFacilities(ai))) where f.FinishedDate.ToShortDateString() == GameObject.GetInstance().GameTime.ToShortDateString() select f);

            foreach (AirlineAirportFacility facility in humanAirportFacilities)
            {
                GameObject.GetInstance().NewsBox.addNews(new News(News.NewsType.Airport_News, GameObject.GetInstance().GameTime, "Airport facility", string.Format("Your airport facility {0} at [LI airport={1}] is now finished building", facility.Facility.Name, facility.Airport.Profile.IATACode)));
                facility.FinishedDate = GameObject.GetInstance().GameTime;
            }
            //checks for changed flight restrictions
            foreach (FlightRestriction restriction in FlightRestrictions.GetRestrictions().FindAll(r => r.StartDate.ToShortDateString() == GameObject.GetInstance().GameTime.ToShortDateString() || r.EndDate.ToShortDateString() == GameObject.GetInstance().GameTime.ToShortDateString()))
            {
                string restrictionNewsText = "";
                if (restriction.Type == FlightRestriction.RestrictionType.Flights)
                {
                    if (restriction.StartDate.ToShortDateString() == GameObject.GetInstance().GameTime.ToShortDateString())
                        restrictionNewsText = string.Format("All flights from {0} to {1} have been banned", restriction.From.Name, restriction.To.Name);
                    else
                        restrictionNewsText = string.Format("The ban for all flights from {0} to {1} have been lifted", restriction.From.Name, restriction.To.Name);
                }
                if (restriction.Type == FlightRestriction.RestrictionType.Airlines)
                {
                    if (restriction.StartDate.ToShortDateString() == GameObject.GetInstance().GameTime.ToShortDateString())
                        restrictionNewsText = string.Format("All airlines flying from {0} flying to {1} have been blacklisted", restriction.From.Name, restriction.To.Name);
                    else
                        restrictionNewsText = string.Format("The blacklist on all airlines from {0} flying to {1} have been lifted", restriction.From.Name, restriction.To.Name);

                }
                if (restriction.StartDate.ToShortDateString() == GameObject.GetInstance().GameTime.ToShortDateString())
                {
                    if (restriction.Type == FlightRestriction.RestrictionType.Flights)
                    {
                        var bannedRoutes = (from r in Airlines.GetAllAirlines().SelectMany(a => a.Routes) where FlightRestrictions.HasRestriction(r.Destination1.Profile.Country, r.Destination2.Profile.Country, GameObject.GetInstance().GameTime) select r);

                        foreach (Route route in bannedRoutes)
                        {
                            route.Banned = true;
                        }
                    }
                }
                GameObject.GetInstance().NewsBox.addNews(new News(News.NewsType.Standard_News, GameObject.GetInstance().GameTime, "Flight restriction", restrictionNewsText));

            }
            //checks for historic events
            foreach (HistoricEvent e in HistoricEvents.GetHistoricEvents(GameObject.GetInstance().GameTime))
            {
                GameObject.GetInstance().NewsBox.addNews(new News(News.NewsType.Standard_News, GameObject.GetInstance().GameTime, e.Name, e.Text));

                foreach (HistoricEventInfluence influence in e.Influences)
                {
                    SetHistoricEventInfluence(influence, false);
                }
            }
            //checks for historic events influences ending
            foreach (HistoricEventInfluence influence in HistoricEvents.GetHistoricEventInfluences(GameObject.GetInstance().GameTime))
            {
                SetHistoricEventInfluence(influence, true);
            }
   
            
            //updates airports
            Parallel.ForEach(Airports.GetAllActiveAirports(), airport =>
           {

               //AirportHelpers.CreateAirportWeather(airport);

               if (Settings.GetInstance().MailsOnBadWeather && humanAirlines.SelectMany(a => a.Airports.FindAll(aa => aa == airport)).Count() > 0 && (airport.Weather[airport.Weather.Length - 1].WindSpeed == Weather.eWindSpeed.Violent_Storm || airport.Weather[airport.Weather.Length - 1].WindSpeed == Weather.eWindSpeed.Hurricane))
               {
                   GameObject.GetInstance().NewsBox.addNews(new News(News.NewsType.Airport_News, GameObject.GetInstance().GameTime, Translator.GetInstance().GetString("News", "1002"), string.Format(Translator.GetInstance().GetString("News", "1002", "message"), airport.Profile.IATACode, GameObject.GetInstance().GameTime.AddDays(airport.Weather.Length - 1).DayOfWeek)));
               }
               // chs, 2011-01-11 changed for delivery of terminals
               foreach (Terminal terminal in airport.Terminals.getTerminals())
               {
                   if (terminal.DeliveryDate.Year == GameObject.GetInstance().GameTime.Year && terminal.DeliveryDate.Month == GameObject.GetInstance().GameTime.Month && terminal.DeliveryDate.Day == GameObject.GetInstance().GameTime.Day)
                   {
                       if (terminal.Airline == null)
                           GameObject.GetInstance().NewsBox.addNews(new News(News.NewsType.Airport_News, GameObject.GetInstance().GameTime, "Construction of terminal", string.Format("[LI airport={0}], {1} has build a new terminal with {2} gates", airport.Profile.IATACode, airport.Profile.Country.Name, terminal.Gates.NumberOfGates)));

                       if (terminal.Airline != null && terminal.Airline.IsHuman)
                           GameObject.GetInstance().NewsBox.addNews(new News(News.NewsType.Airport_News, GameObject.GetInstance().GameTime, "Construction of terminal", string.Format("Your terminal at [LI airport={0}], {1} is now finished and ready for use.", airport.Profile.IATACode, airport.Profile.Country.Name)));

                       if (terminal.Airline != null)
                       {
                           List<AirportContract> oldContracts = new List<AirportContract>(airport.getAirlineContracts(terminal.Airline));

                           if (oldContracts.Count > 0)
                           {
                               int totalGates = oldContracts.Sum(c => c.NumberOfGates);

                               int gatesDiff = totalGates - terminal.Gates.NumberOfGates;

                               if (gatesDiff > 0)
                               {
                                   int length = oldContracts.Max(c => c.Length);
                                   AirportContract newContract = new AirportContract(terminal.Airline, airport, GameObject.GetInstance().GameTime, gatesDiff, length, AirportHelpers.GetYearlyContractPayment(airport, gatesDiff, length) / 2);

                                   airport.addAirlineContract(newContract);

                               }

                               foreach (AirportContract oldContract in oldContracts)
                               {
                                   airport.removeAirlineContract(oldContract);

                                   for (int i=0;i<oldContract.NumberOfGates;i++)
                                   {
                                        Gate oldGate = airport.Terminals.getGates().Where(g => g.Airline == terminal.Airline).First();
                                        oldGate.Airline = null;
                                   }
                               }

                               

                           }
                           double yearlyPayment = AirportHelpers.GetYearlyContractPayment(airport, terminal.Gates.NumberOfGates, 20);

                           airport.addAirlineContract(new AirportContract(terminal.Airline, airport, GameObject.GetInstance().GameTime, terminal.Gates.NumberOfGates, 20, yearlyPayment * 0.75, false, false, terminal));

                           if (terminal.Airport.getAirportFacility(terminal.Airline, AirportFacility.FacilityType.CheckIn).TypeLevel == 0)
                           {


                               AirportFacility checkinFacility = AirportFacilities.GetFacilities(AirportFacility.FacilityType.CheckIn).Find(f => f.TypeLevel == 1);

                               terminal.Airport.addAirportFacility(terminal.Airline, checkinFacility, GameObject.GetInstance().GameTime);



                           }

                       }

                   }
                   //new gates in an existing terminal
                   else
                   {
                       int numberOfNewGates = terminal.Gates.getGates().Count(g => g.DeliveryDate.ToShortDateString() == GameObject.GetInstance().GameTime.ToShortDateString());

                       if (numberOfNewGates > 0)
                       {
                           GameObject.GetInstance().NewsBox.addNews(new News(News.NewsType.Airport_News, GameObject.GetInstance().GameTime, "Expansion of terminal", string.Format("[LI airport={0}], {1} has expanded {2} with {3} gates", airport.Profile.IATACode, airport.Profile.Country.Name, terminal.Name, numberOfNewGates)));

                           double yearlyPayment = AirportHelpers.GetYearlyContractPayment(airport, numberOfNewGates + terminal.Gates.NumberOfGates, 20);

                           AirportContract terminalContract = airport.AirlineContracts.Find(c => c.Terminal != null && c.Terminal == terminal);

                           if (terminalContract != null)
                           {
                               terminalContract.NumberOfGates += numberOfNewGates;
                               terminalContract.YearlyPayment = yearlyPayment;

                               for (int i=0;i<numberOfNewGates;i++)
                               {
                                        Gate newGate = airport.Terminals.getGates().Where(g => g.Airline == null).First();
                                        newGate.Airline = terminalContract.Airline;
                               }
                           }
                       }


                   }
                   //expired contracts
                   var airlineContracts = new List<AirportContract>(airport.AirlineContracts.FindAll(c => c.ExpireDate.ToShortDateString() == GameObject.GetInstance().GameTime.ToShortDateString()));

                   foreach (AirportContract contract in airlineContracts)
                   {
                        for (int i = 0; i < contract.NumberOfGates; i++)
                        {
                           Gate gate = airport.Terminals.getGates().Where(g => g.Airline == contract.Airline).First();
                           gate.Airline = null;
                        
                        }

                       if (contract.Airline.IsHuman)
                       {
                           int totalContractGates = airport.AirlineContracts.Where(c => c.Airline.IsHuman).Sum(c => c.NumberOfGates);

                           var airlineRoutes = new List<Route>(AirportHelpers.GetAirportRoutes(airport, contract.Airline));

                           var remainingContracts = new List<AirportContract>(airport.AirlineContracts.FindAll(c => c.Airline == contract.Airline && c != contract));

                           Boolean canFillRoutes = AirportHelpers.CanFillRoutesEntries(airport, contract.Airline, remainingContracts);

                           if (!canFillRoutes)
                           {
                               GameObject.GetInstance().NewsBox.addNews(new News(News.NewsType.Airport_News, GameObject.GetInstance().GameTime, "Airport contract expired", string.Format("Your contract for {0} gates at [LI airport={1}], {2} is now expired, and a number of routes has been cancelled", contract.NumberOfGates, contract.Airport.Profile.IATACode, contract.Airport.Profile.Country.Name)));

                               int currentRoute = 0;
                               while (!canFillRoutes)
                               {
                                   Route routeToDelete = airlineRoutes[currentRoute];

                                   foreach (FleetAirliner fAirliner in routeToDelete.getAirliners())
                                   {
                                       fAirliner.Status = FleetAirliner.AirlinerStatus.Stopped;
                                       fAirliner.removeRoute(routeToDelete);
                                   }

                                   contract.Airline.removeRoute(routeToDelete);

                                   currentRoute++;

                                   canFillRoutes = AirportHelpers.CanFillRoutesEntries(airport, contract.Airline, remainingContracts);
                               }

                           }
                           else
                           {
                               GameObject.GetInstance().NewsBox.addNews(new News(News.NewsType.Airport_News, GameObject.GetInstance().GameTime, "Airport contract expired", string.Format("Your contract for {0} gates at [LI airport={1}], {2} is now expired", contract.NumberOfGates, contract.Airport.Profile.IATACode, contract.Airport.Profile.Country.Name)));

                           }

                           airport.removeAirlineContract(contract);
                       }
                       else
                       {
                           int numberOfRoutes = AirportHelpers.GetAirportRoutes(airport, contract.Airline).Count;

                           if (numberOfRoutes > 0)
                           {
                               contract.ContractDate = GameObject.GetInstance().GameTime;
                               contract.ExpireDate = GameObject.GetInstance().GameTime.AddYears(contract.Length);
                           }
                           else
                               airport.removeAirlineContract(contract);
                       }
                   }

               }

           }
           );
            //checks for airliners for the human airline
            foreach (FleetAirliner airliner in humanAirlines.SelectMany(a => a.Fleet.FindAll(f => f.Airliner.BuiltDate == GameObject.GetInstance().GameTime && f.Purchased != FleetAirliner.PurchasedType.BoughtDownPayment)))
                GameObject.GetInstance().NewsBox.addNews(new News(News.NewsType.Fleet_News, GameObject.GetInstance().GameTime, "Delivery of airliner", string.Format("Your new airliner [LI airliner={0}] as been delivered to your fleet.\nThe airliner is currently at [LI airport={1}], {2}.", airliner.Airliner.TailNumber, airliner.Homebase.Profile.IATACode, airliner.Homebase.Profile.Country.Name)));


            Parallel.ForEach(Airlines.GetAllAirlines(), airline =>
            {
                lock (airline.Fleet)
                {
                    var fleet = new List<FleetAirliner>(airline.Fleet);
                    foreach (FleetAirliner airliner in fleet.FindAll(a => a!=null && a.Airliner.BuiltDate == GameObject.GetInstance().GameTime && a.Purchased == FleetAirliner.PurchasedType.BoughtDownPayment))
                    {
                        if (airline.Money >= airliner.Airliner.Type.Price)
                        {
                            AirlineHelpers.AddAirlineInvoice(airline, GameObject.GetInstance().GameTime, Invoice.InvoiceType.Purchases, -airliner.Airliner.Type.Price);
                            airliner.Purchased = FleetAirliner.PurchasedType.Bought;

                            if (airline.IsHuman)
                                GameObject.GetInstance().NewsBox.addNews(new News(News.NewsType.Fleet_News, GameObject.GetInstance().GameTime, "Delivery of airliner", string.Format("Your new airliner [LI airliner={0}] as been delivered to your fleet.\nThe airliner is currently at [LI airport={1}], {2}", airliner.Airliner.TailNumber, airliner.Homebase.Profile.IATACode, airliner.Homebase.Profile.Country.Name)));

                        }
                        else
                        {
                            airline.removeAirliner(airliner);

                            if (airline.IsHuman)
                                GameObject.GetInstance().NewsBox.addNews(new News(News.NewsType.Fleet_News, GameObject.GetInstance().GameTime, "Delivery of airliner", string.Format("Your new airliner {0} can't be delivered to your fleet.\nYou don't have enough money to purchase it.", airliner.Name)));

                        }
                    }
                }


                if (airline.Contract != null && airline.Contract.ExpireDate.ToShortDateString() == GameObject.GetInstance().GameTime.ToShortDateString())
                {
                    int missingAirliners = airline.Contract.Airliners - airline.Contract.PurchasedAirliners;

                    if (missingAirliners > 0)
                    {
                        double missingFee = (airline.Contract.getTerminationFee() / (airline.Contract.Length * 2)) * missingAirliners;
                        AirlineHelpers.AddAirlineInvoice(airline, GameObject.GetInstance().GameTime, Invoice.InvoiceType.Purchases, -missingFee);

                        if (airline.IsHuman)
                            GameObject.GetInstance().NewsBox.addNews(new News(News.NewsType.Fleet_News, GameObject.GetInstance().GameTime, "Contract expired", string.Format("Your contract with {0} has now expired.\nYou didn't purchased enough airliners with costs a fee of {1:C} for missing {2} airliners", airline.Contract.Manufacturer.Name, missingFee, missingAirliners)));
                    }
                    else
                        if (airline.IsHuman)
                            GameObject.GetInstance().NewsBox.addNews(new News(News.NewsType.Fleet_News, GameObject.GetInstance().GameTime, "Contract expired", string.Format("Your contract with {0} has now expired.", airline.Contract.Manufacturer.Name)));

                    airline.Contract = null;

                }
                //checks for students educated
                var educatedStudents = airline.FlightSchools.SelectMany(f => f.Students.FindAll(s => s.EndDate.ToShortDateString() == GameObject.GetInstance().GameTime.ToShortDateString()));

                foreach (PilotStudent student in educatedStudents)
                {
                    Pilot pilot = new Pilot(student.Profile, GameObject.GetInstance().GameTime, GeneralHelpers.GetPilotStudentRanking(student));
                    student.Instructor.removeStudent(student);
                    student.Instructor.FlightSchool.removeStudent(student);
                    student.Instructor = null;

                    airline.addPilot(pilot);

                    if (airline.IsHuman)
                        GameObject.GetInstance().NewsBox.addNews(new News(News.NewsType.Flight_News, GameObject.GetInstance().GameTime, Translator.GetInstance().GetString("News", "1006"), string.Format(Translator.GetInstance().GetString("News", "1006", "message"), pilot.Profile.Name)));



                }

            }

            );
            //checks for mergers
            foreach (AirlineMerger merger in AirlineMergers.GetAirlineMergers(GameObject.GetInstance().GameTime))
            {
                if (merger.Type == AirlineMerger.MergerType.Merger)
                {
                    AirlineHelpers.SwitchAirline(merger.Airline2, merger.Airline1);

                    Airlines.RemoveAirline(merger.Airline2);

                    if (merger.NewName != null && merger.NewName.Length > 1)
                        merger.Airline1.Profile.Name = merger.NewName;
                }
                if (merger.Type == AirlineMerger.MergerType.Subsidiary)
                {
                    string oldLogo = merger.Airline2.Profile.Logo;

                    SubsidiaryAirline sAirline = new SubsidiaryAirline(merger.Airline1, merger.Airline2.Profile, merger.Airline2.Mentality, merger.Airline2.MarketFocus, merger.Airline2.License, merger.Airline2.AirlineRouteFocus);

                    AirlineHelpers.SwitchAirline(merger.Airline2, merger.Airline1);

                    merger.Airline1.addSubsidiaryAirline(sAirline);

                    Airlines.RemoveAirline(merger.Airline2);

                    sAirline.Profile.Logos = merger.Airline2.Profile.Logos;
                    sAirline.Profile.Color = merger.Airline2.Profile.Color;

                }

                GameObject.GetInstance().NewsBox.addNews(new News(News.NewsType.Airline_News, GameObject.GetInstance().GameTime, "Airline merger", merger.Name));

            }
            /*
            //does monthly budget work
           DateTime budgetExpires = GameObject.GetInstance().HumanAirline.Budget.BudgetExpires;
            if (budgetExpires <= GameObject.GetInstance().GameTime.AddDays(30))
            {
                GameObject.GetInstance().NewsBox.addNews(new News(News.NewsType.Airline_News, GameObject.GetInstance().GameTime, "Budget Expires Soon", "Your budget will expire within the next 30 days. Please go to the budget screen and adjust it as needed and click 'Apply'."));
            }
            else if (budgetExpires <= GameObject.GetInstance().GameTime)
            {
                //GraphicsModel.PageModel.PageFinancesModel.PageFinances.ResetValues();
            }

            if (GameObject.GetInstance().HumanAirline.Money < GameObject.GetInstance().HumanAirline.Budget.TotalBudget / 12)
            {
                WPFMessageBox.Show("Low Cash!", "Your current cash is less than your budget deduction for the month! Decrease your budget immediately or you will be negative!", WPFMessageBoxButtons.Ok);
            }
            else { GameObject.GetInstance().HumanAirline.Money -= GameObject.GetInstance().HumanAirline.Budget.TotalBudget / 12; }
             * */

            //check for insurance settlements and do maintenance
            foreach (Airline a in Airlines.GetAllAirlines())
            {
                AirlineHelpers.CheckInsuranceSettlements(a);
                foreach (FleetAirliner airliner in a.Fleet)
                {
                    if (airliner != null)
                    {
                        FleetAirlinerHelpers.DoMaintenance(airliner);
                        FleetAirlinerHelpers.RestoreMaintRoutes(airliner);
                    }
                }
            }

            if (GameObject.GetInstance().GameTime.Day % 7 == 0)
            {
                GameObject.GetInstance().HumanAirline.OverallScore += StatisticsHelpers.GetWeeklyScore(GameObject.GetInstance().HumanAirline);
            }

        }

        //do the yearly update
        private static void DoYearlyUpdate()
        {
            //Clear stats when it on yearly
            if (Settings.GetInstance().ClearStats == Settings.Intervals.Yearly)
                ClearAllUsedStats();

            //Auto save when it on yearly
            if (Settings.GetInstance().AutoSave == Settings.Intervals.Yearly)
                SerializedLoadSaveHelpers.SaveGame("autosave");

            AirlineHelpers.ClearRoutesStatistics();
            //updates holidays 
            GeneralHelpers.CreateHolidays(GameObject.GetInstance().GameTime.Year);

            double yearlyRaise = 1.03; //3 % 

            foreach (Airline airline in Airlines.GetAllAirlines())
            {
                foreach (FleetAirliner airliner in airline.Fleet)
                    AirlineHelpers.AddAirlineInvoice(airline, GameObject.GetInstance().GameTime, Invoice.InvoiceType.Maintenances, -airliner.Airliner.Type.getMaintenance());

                foreach (FeeType feeType in FeeTypes.GetTypes())
                    airline.Fees.setValue(feeType, airline.Fees.getValue(feeType) * yearlyRaise);

            }


            //increases the passenger demand between airports with up to 5%
            Parallel.ForEach(Airports.GetAllActiveAirports(), airport =>
                {
                    foreach (DestinationDemand destPax in airport.getDestinationsPassengers())
                        destPax.Rate = (ushort)(destPax.Rate * MathHelpers.GetRandomDoubleNumber(0.97, 1.05));
                });

            //removes the oldest pilots/instructors and creates some new ones
            var oldPilots = Pilots.GetUnassignedPilots().OrderByDescending(p => p.Profile.Age).ToList();
            var oldInstructors = Instructors.GetUnassignedInstructors().OrderByDescending(i => i.Profile.Age).ToList();

            for (int i = 0; i < Math.Min(15, oldPilots.Count); i++)
                Pilots.RemovePilot(oldPilots[i]);

            for (int i = 0; i < Math.Min(10, oldInstructors.Count); i++)
                Instructors.RemoveInstructor(oldInstructors[i]);

            GeneralHelpers.CreatePilots(15);
            GeneralHelpers.CreateInstructors(10);

            foreach (Airline airline in Airlines.GetAllAirlines())
            {
                EventsHelpers.GenerateEvents(airline);
            }


        }
        //do the monthly update
        private static void DoMonthlyUpdate()
        {
            //Clear stats when it on monthly
            if (Settings.GetInstance().ClearStats == Settings.Intervals.Monthly)
                ClearAllUsedStats();

            //Auto save when it on monthly
            if (Settings.GetInstance().AutoSave == Settings.Intervals.Monthly)
                SerializedLoadSaveHelpers.SaveGame("autosave");

            //deletes all used airliners older than 1 years
            List<Airliner> oldAirliners = new List<Airliner>(Airliners.GetAirlinersForSale(a => a.BuiltDate.Year <= GameObject.GetInstance().GameTime.Year - 2));

            //creates some new used airliners
            int gametime = GameObject.GetInstance().GameTime.Year - GameObject.GetInstance().StartDate.Year;

            //Set the amount if planes that should be made its decreased alot over time
            int upper = (Airlines.GetAllAirlines().Count - (gametime * 5)) / 2;
            int lower = (Airlines.GetAllAirlines().Count - (gametime * 5)) / 4;
            if (upper <= 0) { upper = 3; }
            if (lower <= 0) { lower = 1; }
            int airliners = rnd.Next(lower, upper);

            for (int i = 0; i < airliners; i++)
            {
                Airliners.AddAirliner(AirlinerHelpers.CreateAirlinerFromYear(GameObject.GetInstance().GameTime.Year - rnd.Next(1, 10)));
            }

            foreach (Airliner airliner in oldAirliners)
                Airliners.RemoveAirliner(airliner);

            //checks for new airports which are opening
            List<Airport> openedAirports = Airports.GetAllAirports(a => a.Profile.Period.From.ToShortDateString() == GameObject.GetInstance().GameTime.ToShortDateString());
            List<Airport> closedAirports = Airports.GetAllAirports(a => a.Profile.Period.To.ToShortDateString() == GameObject.GetInstance().GameTime.ToShortDateString());


            foreach (Airport airport in openedAirports)
            {
                if (closedAirports.Find(a => a.Profile.Town == airport.Profile.Town) != null)
                    AirportHelpers.ReallocateAirport(closedAirports.Find(a => a.Profile.Town == airport.Profile.Town), airport);
                else
                    PassengerHelpers.CreateDestinationPassengers(airport);

                foreach (Airport dAirport in Airports.GetAirports(a => a != airport && a.Profile.Town != airport.Profile.Town && MathHelpers.GetDistance(a.Profile.Coordinates, airport.Profile.Coordinates) > 25))
                    PassengerHelpers.CreateDestinationPassengers(dAirport, airport);

                int count = Airports.GetAirports(a => a.Profile.Town == airport.Profile.Town && airport != a && a.Terminals.getNumberOfGates(GameObject.GetInstance().MainAirline) > 0).Count;

                if (count == 1)
                {
                    Airport allocateFromAirport = Airports.GetAirports(a => a.Profile.Town == airport.Profile.Town && airport != a && a.Terminals.getNumberOfGates(GameObject.GetInstance().HumanAirline) > 0).First();
                    GameObject.GetInstance().NewsBox.addNews(new News(News.NewsType.Airport_News, GameObject.GetInstance().GameTime, "New airport opened", string.Format("A new airport [LI airport={0}]({1}) is opened in {2}, {3}.\n\rYou can reallocate all your operations from {4}({5}) for free within the next 30 days", airport.Profile.IATACode, new AirportCodeConverter().Convert(airport).ToString(), airport.Profile.Town.Name, ((Country)new CountryCurrentCountryConverter().Convert(airport.Profile.Country)).Name, allocateFromAirport.Profile.Name, new AirportCodeConverter().Convert(allocateFromAirport).ToString())));
                }
                else
                    GameObject.GetInstance().NewsBox.addNews(new News(News.NewsType.Airport_News, GameObject.GetInstance().GameTime, "New airport opened", string.Format("A new airport [LI airport={0}]({1}) is opened in {2}, {3}", airport.Profile.IATACode, new AirportCodeConverter().Convert(airport).ToString(), airport.Profile.Town.Name, ((Country)new CountryCurrentCountryConverter().Convert(airport.Profile.Country)).Name)));



            }

            //check if pilots are retireing
            int retirementAge = Pilot.RetirementAge;

            Parallel.ForEach(Airlines.GetAllAirlines(), airline =>
            {

                var pilotsToRetire = airline.Pilots.FindAll(p => p.Profile.Birthdate.AddYears(retirementAge).AddMonths(-1) < GameObject.GetInstance().GameTime);
                var pilotsToRetirement = new List<Pilot>(airline.Pilots.FindAll(p => p.Profile.Birthdate.AddYears(retirementAge) < GameObject.GetInstance().GameTime));

                var instructorsToRetire = airline.FlightSchools.SelectMany(f => f.Instructors).Where(i => i.Profile.Birthdate.AddYears(retirementAge).AddMonths(-1) < GameObject.GetInstance().GameTime);
                var instructorsToRetirement = new List<Instructor>(airline.FlightSchools.SelectMany(f => f.Instructors).Where(i => i.Profile.Birthdate.AddYears(retirementAge) < GameObject.GetInstance().GameTime));

                foreach (Pilot pilot in pilotsToRetire)
                    if (airline.IsHuman)
                    {
                        GameObject.GetInstance().NewsBox.addNews(new News(News.NewsType.Flight_News, GameObject.GetInstance().GameTime, Translator.GetInstance().GetString("News", "1007"), string.Format(Translator.GetInstance().GetString("News", "1007", "message"), pilot.Profile.Name, pilot.Profile.Age + 1)));

                    }
                    else
                    {
                        if (pilot.Airliner != null)
                        {
                            if (Pilots.GetNumberOfUnassignedPilots() == 0)
                                GeneralHelpers.CreatePilots(10);

                            Pilot newPilot = Pilots.GetUnassignedPilots().Find(p => p.Rating == pilot.Rating);

                            if (newPilot == null)
                                newPilot = Pilots.GetUnassignedPilots()[0];

                            airline.addPilot(newPilot);

                            newPilot.Airliner = pilot.Airliner;
                            newPilot.Airliner.addPilot(newPilot);

                            pilot.Airliner.removePilot(pilot);
                            pilot.Airliner = null;
                        }
                    }

                foreach (Instructor instructor in instructorsToRetire)
                    if (airline.IsHuman)
                        GameObject.GetInstance().NewsBox.addNews(new News(News.NewsType.Flight_News, GameObject.GetInstance().GameTime, Translator.GetInstance().GetString("News", "1008"), string.Format(Translator.GetInstance().GetString("News", "1008", "message"), instructor.Profile.Name, instructor.Profile.Age + 1)));

                foreach (Pilot pilot in pilotsToRetirement)
                {
                    if (pilot.Airliner != null)
                    {
                        //pilot.Airliner.Status = FleetAirliner.AirlinerStatus.Stopped;

                        if (airline.IsHuman)
                        {
                            GameObject.GetInstance().NewsBox.addNews(new News(News.NewsType.Flight_News, GameObject.GetInstance().GameTime, Translator.GetInstance().GetString("News", "1010"), string.Format(Translator.GetInstance().GetString("News", "1010", "message"), pilot.Profile.Name, pilot.Airliner.Name)));
                            if (pilot.Airliner != null)
                            {
                                if (Pilots.GetNumberOfUnassignedPilots() == 0)
                                    GeneralHelpers.CreatePilots(10);

                                if (GameObject.GetInstance().HumanAirline.Pilots.Exists(p => p.Airliner == null))
                                {
                                    Pilot newPilot = GameObject.GetInstance().HumanAirline.Pilots.Find(p => p.Airliner == null);

                                    newPilot.Airliner = pilot.Airliner;
                                    newPilot.Airliner.addPilot(newPilot);
                                }
                                else
                                {

                                    var pilots = Pilots.GetUnassignedPilots(p => p.Profile.Town.Country == pilot.Airliner.Airliner.Airline.Profile.Country);

                                    if (pilots.Count == 0)
                                        pilots = Pilots.GetUnassignedPilots(p => p.Profile.Town.Country.Region == pilot.Airliner.Airliner.Airline.Profile.Country.Region);

                                    if (pilots.Count == 0)
                                        pilots = Pilots.GetUnassignedPilots();

                                    Pilot newPilot = pilots.First();

                                    pilot.Airliner.Airliner.Airline.addPilot(newPilot);
                                    newPilot.Airliner = pilot.Airliner;
                                    newPilot.Airliner.addPilot(newPilot);
                                }

                            }
                        }
                        pilot.Airliner.removePilot(pilot);

                    }
                    else
                    {
                        if (airline.IsHuman)
                            GameObject.GetInstance().NewsBox.addNews(new News(News.NewsType.Flight_News, GameObject.GetInstance().GameTime, Translator.GetInstance().GetString("News", "1009"), string.Format(Translator.GetInstance().GetString("News", "1009", "message"), pilot.Profile.Name)));

                    }
                    airline.removePilot(pilot);
                    pilot.Airline = null;
                    Pilots.RemovePilot(pilot);
                }

                foreach (Instructor instructor in instructorsToRetirement)
                {

                    int studentsCapacity = (instructor.FlightSchool.Instructors.Count - 1) * FlightSchool.MaxNumberOfStudentsPerInstructor;

                    if (instructor.Students.Count > 0)
                    {
                        if (instructor.FlightSchool.NumberOfStudents > studentsCapacity)
                        {
                            Instructor newInstructor;

                            if (airline.IsHuman)
                                newInstructor = Instructors.GetUnassignedInstructors().OrderBy(i => i.Rating).ToList()[0];
                            else
                                newInstructor = Instructors.GetUnassignedInstructors()[rnd.Next(Instructors.GetUnassignedInstructors().Count)];

                            foreach (PilotStudent student in instructor.Students)
                                student.Instructor = newInstructor;

                            newInstructor.FlightSchool = instructor.FlightSchool;
                            newInstructor.FlightSchool.addInstructor(newInstructor);
                            instructor.Students.Clear();

                            if (airline.IsHuman)
                                GameObject.GetInstance().NewsBox.addNews(new News(News.NewsType.Flight_News, GameObject.GetInstance().GameTime, Translator.GetInstance().GetString("News", "1012"), string.Format(Translator.GetInstance().GetString("News", "1012", "message"), instructor.Profile.Name, newInstructor.Profile.Name)));

                        }
                        else
                        {
                            while (instructor.Students.Count > 0)
                            {
                                PilotStudent student = instructor.Students[0];

                                Instructor newInstructor = instructor.FlightSchool.Instructors.Find(i => i.Students.Count < FlightSchool.MaxNumberOfStudentsPerInstructor && i != instructor);
                                newInstructor.addStudent(student);
                                student.Instructor = newInstructor;

                                instructor.removeStudent(student);
                            }

                            if (airline.IsHuman)
                                GameObject.GetInstance().NewsBox.addNews(new News(News.NewsType.Flight_News, GameObject.GetInstance().GameTime, Translator.GetInstance().GetString("News", "1011"), string.Format(Translator.GetInstance().GetString("News", "1011", "message"), instructor.Profile.Name)));

                        }


                    }
                    else
                    {
                        if (airline.IsHuman)
                            GameObject.GetInstance().NewsBox.addNews(new News(News.NewsType.Flight_News, GameObject.GetInstance().GameTime, Translator.GetInstance().GetString("News", "1011"), string.Format(Translator.GetInstance().GetString("News", "1011", "message"), instructor.Profile.Name)));

                    }

                    instructor.FlightSchool.removeInstructor(instructor);
                    instructor.FlightSchool = null;

                    Instructors.RemoveInstructor(instructor);

                }

                //wages
                foreach (Pilot pilot in airline.Pilots)
                {

                    double salary = ((int)pilot.Rating) * airline.Fees.getValue(FeeTypes.GetType("Pilot Base Salary"));
                    AirlineHelpers.AddAirlineInvoice(airline, GameObject.GetInstance().GameTime, Invoice.InvoiceType.Wages, -salary);
                }

                foreach (Instructor instructor in airline.FlightSchools.SelectMany(f => f.Instructors))
                {
                    double salary = ((int)instructor.Rating) * airline.Fees.getValue(FeeTypes.GetType("Instructor Base Salary"));
                    AirlineHelpers.AddAirlineInvoice(airline, GameObject.GetInstance().GameTime, Invoice.InvoiceType.Wages, -salary);

                }
                //check for employee happiness and wages
                //Console.WriteLine("Airline: {0} Avg. Wages: {1} Happiness: {2}", airline.Profile.Name, StatisticsHelpers.GetEmployeeWages()[airline], Ratings.GetEmployeeHappiness(airline));

                /*
                double employeeHapiness = Ratings.GetEmployeeHappiness(airline);
                double avgCompetitorsWages = StatisticsHelpers.GetAverageEmployeeWages(airline);
                double airlineWage = StatisticsHelpers.GetEmployeeWages()[airline];

                double wagesDiff = (airlineWage / avgCompetitorsWages) * 100;

                //striking if below 80% of the average
                if (wagesDiff < 80 && employeeHapiness < 80)
                {
                   // if (airline.IsHuman)
                    //    GameObject.GetInstance().NewsBox.addNews(new News(News.NewsType.Flight_News, GameObject.GetInstance().GameTime, Translator.GetInstance().GetString("News", "1012"), string.Format(Translator.GetInstance().GetString("News", "1012", "message"), instructor.Profile.Name, newInstructor.Profile.Name)));

                  

                    //tjek for union ved skift af løn - forhandling på års niveau

                }*/

                foreach (AirlineFacility facility in airline.Facilities)
                    AirlineHelpers.AddAirlineInvoice(airline, GameObject.GetInstance().GameTime, Invoice.InvoiceType.Airline_Expenses, -facility.MonthlyCost);

                foreach (FleetAirliner airliner in airline.Fleet.FindAll((delegate(FleetAirliner a) { return a.Purchased == FleetAirliner.PurchasedType.Leased; })))
                    AirlineHelpers.AddAirlineInvoice(airline, GameObject.GetInstance().GameTime, Invoice.InvoiceType.Rents, -airliner.Airliner.LeasingPrice);

                foreach (Airport airport in airline.Airports)
                {
                    List<AirportContract> contracts = new List<AirportContract>(airport.getAirlineContracts(airline));

                    double contractPrice = contracts.Sum(c => c.YearlyPayment) / 12;

                    AirlineHelpers.AddAirlineInvoice(airline, GameObject.GetInstance().GameTime, Invoice.InvoiceType.Rents, -contractPrice);

                    long airportIncome = Convert.ToInt64(contractPrice);
                    airport.Income += airportIncome;

                    //expired contracts
                    var airlineContracts = new List<AirportContract>(airport.AirlineContracts.FindAll(c => c.ExpireDate < GameObject.GetInstance().GameTime.AddMonths(2)));

                    foreach (AirportContract contract in airlineContracts)
                    {
                        if (contract.Airline.IsHuman)
                            GameObject.GetInstance().NewsBox.addNews(new News(News.NewsType.Airport_News, GameObject.GetInstance().GameTime, "Airport contract expires", string.Format("Your contract for {0} gates at [LI airport={1}], {2} expires soon. Please extend the contracts.", contract.NumberOfGates, contract.Airport.Profile.IATACode, contract.Airport.Profile.Country.Name)));
                        else
                        {
                            int numberOfRoutes = AirportHelpers.GetAirportRoutes(airport, contract.Airline).Count;

                            if (numberOfRoutes > 0)
                            {
                                contract.ContractDate = GameObject.GetInstance().GameTime;
                                contract.ExpireDate = GameObject.GetInstance().GameTime.AddYears(contract.Length);
                            }
                            else
                                airport.removeAirlineContract(contract);
                        }
                    }

                    //wages
                    foreach (AirportFacility facility in airport.getCurrentAirportFacilities(airline))
                    {
                        double wage = 0;

                        if (facility.EmployeeType == AirportFacility.EmployeeTypes.Maintenance)
                            wage = airline.Fees.getValue(FeeTypes.GetType("Maintenance Wage"));
                        if (facility.EmployeeType == AirportFacility.EmployeeTypes.Support)
                            wage = airline.Fees.getValue(FeeTypes.GetType("Support Wage"));

                        double facilityWage = facility.NumberOfEmployees * wage * (40 * 4.33); //40 hours per week and 4.33 weeks per month

                        AirlineHelpers.AddAirlineInvoice(airline, GameObject.GetInstance().GameTime, Invoice.InvoiceType.Wages, -facilityWage);
                    }
                    //passenger growth if ticket office
                    //checking if someone in the alliance has a ticket office for the route (if in an alliance).
                    if (airline.Alliances.Count > 0)
                    {
                        int highest = airline.Alliances.SelectMany(a => a.Members).Select(m => m.Airline).Max(m => airport.getAirlineAirportFacility(m, AirportFacility.FacilityType.TicketOffice).Facility.ServiceLevel);
                        Boolean hasTicketOffice = airline.Alliances.Where(a => a.Type == Alliance.AllianceType.Full).SelectMany(a => a.Members).Select(m => m.Airline).Where(m => airport.getAirlineAirportFacility(m, AirportFacility.FacilityType.TicketOffice).Facility.TypeLevel > 0) != null;

                        //If there is an service level update the routes
                        if (hasTicketOffice)
                        {
                            foreach (Route route in airline.Routes.Where(r => r.Destination1 == airport || r.Destination2 == airport))
                            {
                                Airport destination = airport == route.Destination1 ? route.Destination2 : route.Destination1;
                                airport.addDestinationPassengersRate(destination, (ushort)highest);
                            }
                        }
                    }


                    //not in an alliance so we will look for the airline only
                    else
                    {
                        if (airport.getAirlineAirportFacility(airline, AirportFacility.FacilityType.TicketOffice).Facility.TypeLevel > 0)
                        {
                            foreach (Route route in airline.Routes.Where(r => r.Destination1 == airport || r.Destination2 == airport))
                            {
                                Airport destination = airport == route.Destination1 ? route.Destination2 : route.Destination1;

                                airport.addDestinationPassengersRate(destination, (ushort)airport.getAirlineAirportFacility(airline, AirportFacility.FacilityType.TicketOffice).Facility.ServiceLevel);

                            }
                        }

                    }
                }
                //passenger demand
                int advertisementFactor = airline.getAirlineAdvertisements().Sum(a => a.ReputationLevel);

                foreach (Route route in airline.Routes)
                {
                    route.Destination1.addDestinationPassengersRate(route.Destination2, (ushort)(5 * advertisementFactor));
                    route.Destination2.addDestinationPassengersRate(route.Destination1, (ushort)(5 * advertisementFactor));

                }
                foreach (Loan loan in airline.Loans)
                {
                    if (loan.IsActive)
                    {

                        double amount = Math.Min(loan.getMonthlyPayment(), loan.PaymentLeft);

                        AirlineHelpers.AddAirlineInvoice(airline, GameObject.GetInstance().GameTime, Invoice.InvoiceType.Loans, -amount);


                        loan.PaymentLeft -= amount;

                    }

                }
                // chs, 2011-17-10 change so it only looks at the advertisement types which are invented at the time
                foreach (AdvertisementType.AirlineAdvertisementType type in Enum.GetValues(typeof(AdvertisementType.AirlineAdvertisementType)))
                {
                    if (GameObject.GetInstance().GameTime.Year >= (int)type)
                    {

                        AirlineHelpers.AddAirlineInvoice(airline, GameObject.GetInstance().GameTime, Invoice.InvoiceType.Airline_Expenses, -airline.getAirlineAdvertisement(type).Price);

                        if (airline.Reputation < 100)
                        {
                            airline.Reputation += airline.getAirlineAdvertisement(type).ReputationLevel;
                        }

                    }
                }
                //maintenance, insurance, and ratings

                AirlineInsuranceHelpers.CheckExpiredInsurance(airline);
                if (airline.InsurancePolicies != null)
                {
                    AirlineInsuranceHelpers.MakeInsurancePayment(airline);
                }

                RandomEvent.CheckExpired(GameObject.GetInstance().GameTime);

            });


            if (Pilots.GetNumberOfUnassignedPilots() < 25)
                GeneralHelpers.CreatePilots(100);

            if (Instructors.GetNumberOfUnassignedInstructors() < 20)
                GeneralHelpers.CreateInstructors(75);

            //checks if the airport will increase the number of gates either by new terminal or by extending existing
            Parallel.ForEach(Airports.GetAllAirports(a => a.Terminals.getInusePercent() > 90), airport =>
            {
                AirportHelpers.CheckForExtendGates(airport);
            });

            long longestRequiredRunwayLenght = AirlinerTypes.GetTypes(a => a.Produced.From <= GameObject.GetInstance().GameTime && a.Produced.To >= GameObject.GetInstance().GameTime).Max(a => a.MinRunwaylength);

            Parallel.ForEach(Airports.GetAllAirports(a => a.Runways.Count > 0 && a.Runways.Select(r => r.Length).Max() < longestRequiredRunwayLenght / 2), airport =>
                {
                    AirportHelpers.CheckForExtendRunway(airport);
                });

            foreach (Airport airport in Airports.GetAllAirports(a => AirportHelpers.GetAirportRoutes(a).Count > 0))
            {
                double airportRoutes = AirportHelpers.GetAirportRoutes(airport).Count;
                double growth = Math.Min(0.5 * airportRoutes, 2);

                PassengerHelpers.ChangePaxDemand(airport, growth);
            }

            if (GameObject.GetInstance().Scenario != null)
                ScenarioHelpers.UpdateScenario(GameObject.GetInstance().Scenario);

            CreateMontlySummary();
        }
        //creates the monthly summary report for the human airline
        private static void CreateMontlySummary()
        {
            Airline airline = GameObject.GetInstance().HumanAirline;

            string monthName = GameObject.GetInstance().GameTime.AddMonths(-1).ToString("MMMM", CultureInfo.InvariantCulture);

            string summary = "[HEAD=Routes Summary]\n";

            var routes = airline.Routes.OrderByDescending(r=>r.getBalance(GameObject.GetInstance().GameTime.AddMonths(-1), GameObject.GetInstance().GameTime));
            var homeAirport = airline.Airports[0];

            foreach (Route route in routes)
            {
                var monthBalance = route.getBalance(GameObject.GetInstance().GameTime.AddMonths(-1), GameObject.GetInstance().GameTime);
                summary += string.Format("[WIDTH=300 {0}-{1}]Balance in month: {2}\n", route.Destination1.Profile.Name, route.Destination2.Profile.Name, new ValueCurrencyConverter().Convert(monthBalance));
                
            }

            summary += "\n\n";

            summary += "[HEAD=Destinations Advice]\n";

            Airport largestDestination;

            if (airline.AirlineRouteFocus == Route.RouteType.Cargo)
                largestDestination = homeAirport.getDestinationDemands().Where(a => a!=null && GeneralHelpers.IsAirportActive(a) && !airline.Routes.Exists(r=>(r.Destination1 == homeAirport && r.Destination2==a) || (r.Destination2 == homeAirport && r.Destination1 == a))).OrderByDescending(a => homeAirport.getDestinationCargoRate(a)).FirstOrDefault();
            else
                largestDestination = homeAirport.getDestinationDemands().Where(a => a!=null && GeneralHelpers.IsAirportActive(a) && !airline.Routes.Exists(r=>(r.Destination1 == homeAirport && r.Destination2==a) || (r.Destination2 == homeAirport && r.Destination1 == a))).OrderByDescending(a => homeAirport.getDestinationPassengersRate(a, AirlinerClass.ClassType.Economy_Class)).FirstOrDefault();
            
            if (largestDestination != null)
                summary += string.Format("The largest destination in terms of demand from [LI airport={0}] where you don't have a route, is [LI airport={1}]", homeAirport.Profile.IATACode, largestDestination.Profile.IATACode);

            summary += "\n[HEAD=Fleet Summary]\n";

            int fleetSize = GameObject.GetInstance().HumanAirline.DeliveredFleet.Count;
            int inorderFleetSize = GameObject.GetInstance().HumanAirline.Fleet.Count - fleetSize;

            int airlinersWithoutRoute = GameObject.GetInstance().HumanAirline.DeliveredFleet.Count(f => !f.HasRoute);

            summary += string.Format("[WIDTH=300 Fleet Size:]{0}\n", fleetSize);
            summary += string.Format("[WIDTH=300 Airliners in Order:]{0}\n", inorderFleetSize);
            summary += string.Format("[WIDTH=300 Airliners Without Routes:]{0}\n", airlinersWithoutRoute);
            
            GameObject.GetInstance().NewsBox.addNews(new News(News.NewsType.Airline_News, GameObject.GetInstance().GameTime,string.Format("{0} {1} Summary",monthName,GameObject.GetInstance().GameTime.AddMonths(-1).Year),summary));// Translator.GetInstance().GetString("News", "1003"), string.Format(Translator.GetInstance().GetString("News", "1003", "message"), airliner.Airliner.TailNumber, airport.Profile.IATACode)));
                        
        }
        //updates an airliner
        private static void UpdateAirliner(FleetAirliner airliner)
        {
            if (airliner.HasRoute)
            {
                switch (airliner.Status)
                {
                    case FleetAirliner.AirlinerStatus.On_route:
                        UpdateOnRouteAirliner(airliner);
                        break;
                    case FleetAirliner.AirlinerStatus.On_service:
                        UpdateOnRouteAirliner(airliner);
                        break;
                    case FleetAirliner.AirlinerStatus.To_homebase:
                        UpdateOnRouteAirliner(airliner);
                        break;
                    case FleetAirliner.AirlinerStatus.To_route_start:
                        UpdateToRouteStartAirliner(airliner);
                        break;

                    case FleetAirliner.AirlinerStatus.Resting:
                        DateTime nextFlightTime = GetNextFlightTime(airliner);
                        if (nextFlightTime <= GameObject.GetInstance().GameTime)
                            SimulateTakeOff(airliner);
                        break;
                }
            }


        }


        //the method for updating a route airliner
        private static void UpdateOnRouteAirliner(FleetAirliner airliner)
        {

            if (airliner.CurrentFlight == null)
            {
                Route route = GetNextRoute(airliner);

                if (route == null || route.TimeTable.getNextEntry(GameObject.GetInstance().GameTime, airliner.CurrentPosition) == null)
                    airliner.CurrentFlight = null;
                else
                {
                    airliner.CurrentFlight = new Flight(route.TimeTable.getNextEntry(GameObject.GetInstance().GameTime, airliner.CurrentPosition));

                }
            }
            if (airliner.CurrentFlight != null)
            {
                double adistance = airliner.CurrentFlight.DistanceToDestination;//airliner.CurrentPosition.GetDistanceTo(airliner.CurrentFlight.Entry.Destination.Airport.Profile.Coordinates);

                double speed = airliner.Airliner.Type.CruisingSpeed;

                /* Leaving it out until we get a position implemented agian
                Weather currentWeather = GetAirlinerWeather(airliner);
                int wind = GetWindInfluence(airliner) * (int)currentWeather.WindSpeed;
                speed = airliner.Airliner.Type.CruisingSpeed  + wind;
                */

                if (adistance > 4)
                    MathHelpers.MoveObject(airliner, speed);

                double distance = MathHelpers.GetDistance(airliner.CurrentPosition.Profile.Coordinates, airliner.CurrentFlight.Entry.Destination.Airport.Profile.Coordinates);

                if (airliner.CurrentFlight.DistanceToDestination < 5)
                {
                    if (airliner.Status == FleetAirliner.AirlinerStatus.On_route)
                        SimulateLanding(airliner);
                    else if (airliner.Status == FleetAirliner.AirlinerStatus.On_service)
                        SimulateService(airliner);
                    else if (airliner.Status == FleetAirliner.AirlinerStatus.To_homebase)
                        SimulateToHomebase(airliner);
                    //else if (airliner.Status == FleetAirliner.AirlinerStatus.To_route_start)
                    //  SimulateRouteStart(airliner);
                }
            }
            else
                airliner.Status = FleetAirliner.AirlinerStatus.To_route_start;

        }

        //the method for updating a route airliner with status toroutestart
        private static void UpdateToRouteStartAirliner(FleetAirliner airliner)
        {
            if (airliner.CurrentFlight == null)
            {
                Route route = GetNextRoute(airliner);

                if (route == null)
                {
                    airliner.Status = FleetAirliner.AirlinerStatus.To_homebase;
                }
                else
                {

                    airliner.CurrentFlight = new Flight(route.TimeTable.getNextEntry(GameObject.GetInstance().GameTime, airliner));
                }
            }
            if (airliner.CurrentFlight != null)
            {

                double adistance = airliner.CurrentFlight.DistanceToDestination;

                double speed = airliner.Airliner.Type.CruisingSpeed;

                /* leaving it out until we get position working agian
                if (airliner.CurrentFlight != null)
                {
                    Weather currentWeather = GetAirlinerWeather(airliner);

                    int wind = GetWindInfluence(airliner) * (int)currentWeather.WindSpeed;
                    speed = airliner.Airliner.Type.CruisingSpeed  + wind;

                } */

                if (adistance > 4)
                    MathHelpers.MoveObject(airliner, speed);

                if (airliner.CurrentFlight.DistanceToDestination < 5)
                {
                    airliner.Status = FleetAirliner.AirlinerStatus.Resting;
                    airliner.CurrentPosition = airliner.CurrentFlight.Entry.DepartureAirport;// new GeoCoordinate(destination.Latitude, destination.Longitude);

                }

            }
            else
            {
                airliner.Status = FleetAirliner.AirlinerStatus.To_homebase;
            }


        }
        //simulates a route airliner going to homebase
        private static void SimulateToHomebase(FleetAirliner airliner)
        {
            airliner.CurrentPosition = airliner.CurrentFlight.Entry.Destination.Airport; //new GeoCoordinate(airliner.CurrentFlight.Entry.Destination.Airport.Profile.Coordinates.Latitude, airliner.CurrentFlight.Entry.Destination.Airport.Profile.Coordinates.Longitude);
            Airport airport = airliner.CurrentPosition;
            airliner.Status = FleetAirliner.AirlinerStatus.To_homebase;

            if (!airliner.CurrentFlight.Entry.Destination.Airport.Profile.Coordinates.Equals(airliner.Homebase.Profile.Coordinates))
                airliner.Status = FleetAirliner.AirlinerStatus.Stopped;
            else
                airliner.CurrentFlight = new Flight(new RouteTimeTableEntry(airliner.CurrentFlight.Entry.TimeTable, GameObject.GetInstance().GameTime.DayOfWeek, GameObject.GetInstance().GameTime.TimeOfDay, new RouteEntryDestination(airliner.Homebase, "Service",null)));

        }

        //simulates a route airliner taking off
        private static void SimulateTakeOff(FleetAirliner airliner)
        {

            KeyValuePair<FleetAirlinerHelpers.DelayType, int> delayedMinutes = FleetAirlinerHelpers.GetDelayedMinutes(airliner);

            //cancelled/delay
            if (delayedMinutes.Value >= Convert.ToInt16(airliner.Airliner.Airline.getAirlinePolicy("Cancellation Minutes").PolicyValue))
            {
                if (airliner.Airliner.Airline.IsHuman)
                {
                    Flight flight = airliner.CurrentFlight;

                    switch (delayedMinutes.Key)
                    {
                        case FleetAirlinerHelpers.DelayType.Airliner_problems:
                            GameObject.GetInstance().NewsBox.addNews(new News(News.NewsType.Flight_News, GameObject.GetInstance().GameTime, Translator.GetInstance().GetString("News", "1004"), string.Format(Translator.GetInstance().GetString("News", "1004", "message"), flight.Entry.Destination.FlightCode, flight.Entry.DepartureAirport.Profile.IATACode, flight.Entry.Destination.Airport.Profile.IATACode)));
                            break;
                        case FleetAirlinerHelpers.DelayType.Bad_weather:
                            GameObject.GetInstance().NewsBox.addNews(new News(News.NewsType.Flight_News, GameObject.GetInstance().GameTime, Translator.GetInstance().GetString("News", "1005"), string.Format(Translator.GetInstance().GetString("News", "1005", "message"), flight.Entry.Destination.FlightCode, flight.Entry.DepartureAirport.Profile.IATACode, flight.Entry.Destination.Airport.Profile.IATACode)));
                            break;
                    }
                }
                airliner.Airliner.Airline.Statistics.addStatisticsValue(GameObject.GetInstance().GameTime.Year, StatisticsTypes.GetStatisticsType("Cancellations"), 1);

                double cancellationPercent = airliner.Airliner.Airline.Statistics.getStatisticsValue(GameObject.GetInstance().GameTime.Year, StatisticsTypes.GetStatisticsType("Cancellations")) / (airliner.Airliner.Airline.Statistics.getStatisticsValue(GameObject.GetInstance().GameTime.Year, StatisticsTypes.GetStatisticsType("Arrivals")) + airliner.Airliner.Airline.Statistics.getStatisticsValue(GameObject.GetInstance().GameTime.Year, StatisticsTypes.GetStatisticsType("Cancellations")));
                airliner.Airliner.Airline.Statistics.setStatisticsValue(GameObject.GetInstance().GameTime.Year, StatisticsTypes.GetStatisticsType("Cancellation%"), cancellationPercent * 100);

                SetNextFlight(airliner);

            }
            else
            {
                airliner.CurrentFlight.addDelayMinutes(delayedMinutes.Value);

                if (delayedMinutes.Value > 0)
                    airliner.CurrentFlight.IsOnTime = false;

            }

            if (airliner.CurrentFlight.FlightTime <= GameObject.GetInstance().GameTime)
            {

                if (AirportHelpers.HasBadWeather(airliner.CurrentFlight.Entry.DepartureAirport) || AirportHelpers.HasBadWeather(airliner.CurrentFlight.Entry.Destination.Airport))
                {
                    if (airliner.Airliner.Airline.IsHuman)
                    {
                        Airport airport = AirportHelpers.HasBadWeather(airliner.CurrentFlight.Entry.Destination.Airport) ? airliner.CurrentFlight.Entry.Destination.Airport : airliner.CurrentFlight.Entry.DepartureAirport;
                        GameObject.GetInstance().NewsBox.addNews(new News(News.NewsType.Flight_News, GameObject.GetInstance().GameTime, Translator.GetInstance().GetString("News", "1003"), string.Format(Translator.GetInstance().GetString("News", "1003", "message"), airliner.Airliner.TailNumber, airport.Profile.IATACode)));

                    }
                    SetNextFlight(airliner);
                }
                else
                {
                    airliner.Status = FleetAirliner.AirlinerStatus.On_route;

                    if (airliner.CurrentFlight.Entry.MainEntry == null)
                    {
                        if (airliner.CurrentFlight.isPassengerFlight())
                        {
                            var classes = new List<AirlinerClass>(airliner.Airliner.Classes);

                            foreach (AirlinerClass aClass in classes)
                            {
                                airliner.CurrentFlight.Classes.Add(new FlightAirlinerClass(((PassengerRoute)airliner.CurrentFlight.Entry.TimeTable.Route).getRouteAirlinerClass(aClass.Type), GetPassengers(airliner, aClass.Type)));
                            }
                        }
                        if (airliner.CurrentFlight.isCargoFlight())
                            airliner.CurrentFlight.Cargo = PassengerHelpers.GetFlightCargo(airliner);
                    }
                    else
                        airliner.Status = FleetAirliner.AirlinerStatus.On_route;

                }
            }
        }
        //simulates a route airliner landing
        public static void SimulateLanding(FleetAirliner airliner)
        {
            DateTime landingTime = airliner.CurrentFlight.FlightTime.Add(MathHelpers.GetFlightTime(airliner.CurrentFlight.Entry.DepartureAirport.Profile.Coordinates, airliner.CurrentFlight.Entry.Destination.Airport.Profile.Coordinates, airliner.Airliner.Type));
            double fdistance = MathHelpers.GetDistance(airliner.CurrentFlight.getDepartureAirport(), airliner.CurrentFlight.Entry.Destination.Airport);

            TimeSpan flighttime = landingTime.Subtract(airliner.CurrentFlight.FlightTime);
            double groundTaxPerPassenger = 5;

            double tax = 0;

            if (airliner.CurrentFlight.Entry.Destination.Airport.Profile.Country.Name != airliner.CurrentFlight.getDepartureAirport().Profile.Country.Name)
                tax = 2 * tax;
            
            double ticketsIncome = 0;
            double feesIncome = 0;
            double mealExpenses = 0;
            double fuelExpenses = 0;

            if (airliner.CurrentFlight.isPassengerFlight())
            {
                tax = groundTaxPerPassenger * airliner.CurrentFlight.getTotalPassengers();
                fuelExpenses = GameObject.GetInstance().FuelPrice * fdistance * airliner.CurrentFlight.getTotalPassengers() * airliner.Airliner.Type.FuelConsumption;

                foreach (FlightAirlinerClass fac in airliner.CurrentFlight.Classes)
                {

                    ticketsIncome += fac.Passengers * ((PassengerRoute)airliner.CurrentFlight.Entry.TimeTable.Route).getRouteAirlinerClass(fac.AirlinerClass.Type).FarePrice;
                }

                FeeType employeeDiscountType = FeeTypes.GetType("Employee Discount");
                double employeesDiscount = airliner.Airliner.Airline.Fees.getValue(employeeDiscountType);

                double totalDiscount = ticketsIncome * (employeeDiscountType.Percentage / 100.0) * (employeesDiscount / 100.0);
                ticketsIncome = ticketsIncome - totalDiscount;

                foreach (FeeType feeType in FeeTypes.GetTypes(FeeType.eFeeType.Fee))
                {
                    if (GameObject.GetInstance().GameTime.Year >= feeType.FromYear)
                    {
                        foreach (FlightAirlinerClass fac in airliner.CurrentFlight.Classes)
                        {
                            double percent = 0.10;
                            double maxValue = Convert.ToDouble(feeType.Percentage) * (1 + percent);
                            double minValue = Convert.ToDouble(feeType.Percentage) * (1 - percent);

                            double value = Convert.ToDouble(rnd.Next((int)minValue, (int)maxValue)) / 100;

                            feesIncome += fac.Passengers * value * airliner.Airliner.Airline.Fees.getValue(feeType);
                        }
                    }
                }

                foreach (FlightAirlinerClass fac in airliner.CurrentFlight.Classes)
                {
                    foreach (RouteFacility facility in ((PassengerRoute)airliner.CurrentFlight.Entry.TimeTable.Route).getRouteAirlinerClass(fac.AirlinerClass.Type).getFacilities())
                    {
                        if (facility.EType == RouteFacility.ExpenseType.Fixed)
                            mealExpenses += fac.Passengers * facility.ExpensePerPassenger;
                        else
                        {
                            FeeType feeType = facility.FeeType;
                            double percent = 0.10;
                            double maxValue = Convert.ToDouble(feeType.Percentage) * (1 + percent);
                            double minValue = Convert.ToDouble(feeType.Percentage) * (1 - percent);

                            double value = Convert.ToDouble(rnd.Next((int)minValue, (int)maxValue)) / 100;

                            mealExpenses -= fac.Passengers * value * airliner.Airliner.Airline.Fees.getValue(feeType);

                        }
                    }
                }

            }
            if (airliner.CurrentFlight.isCargoFlight())
            {
                tax = groundTaxPerPassenger * airliner.CurrentFlight.Cargo;
                fuelExpenses = GameObject.GetInstance().FuelPrice * fdistance * airliner.CurrentFlight.Cargo * airliner.Airliner.Type.FuelConsumption;

                ticketsIncome = airliner.CurrentFlight.Cargo * airliner.CurrentFlight.getCargoPrice();
            }


            Airport dest = airliner.CurrentFlight.Entry.Destination.Airport;
            Airport dept = airliner.CurrentFlight.Entry.DepartureAirport;

            double dist = MathHelpers.GetDistance(dest.Profile.Coordinates, dept.Profile.Coordinates);

            double expenses = fuelExpenses + dest.getLandingFee() + tax;
            if (double.IsNaN(expenses))
                expenses = 0;

            if (double.IsNaN(ticketsIncome) || ticketsIncome < 0)
                ticketsIncome = 0;

            FleetAirlinerHelpers.SetFlightStats(airliner);

            long airportIncome = Convert.ToInt64(dest.getLandingFee());
            dest.Income += airportIncome;

            Airline airline = airliner.Airliner.Airline;

            AirlineHelpers.AddAirlineInvoice(airline, GameObject.GetInstance().GameTime, Invoice.InvoiceType.Flight_Expenses, -expenses);
            AirlineHelpers.AddAirlineInvoice(airline, GameObject.GetInstance().GameTime, Invoice.InvoiceType.Tickets, ticketsIncome);
            AirlineHelpers.AddAirlineInvoice(airline, GameObject.GetInstance().GameTime, Invoice.InvoiceType.OnFlight_Income, -mealExpenses);
            AirlineHelpers.AddAirlineInvoice(airline, GameObject.GetInstance().GameTime, Invoice.InvoiceType.Fees, feesIncome);

            airliner.CurrentFlight.Entry.TimeTable.Route.addRouteInvoice(new Invoice(GameObject.GetInstance().GameTime, Invoice.InvoiceType.Flight_Expenses, -expenses));
            airliner.CurrentFlight.Entry.TimeTable.Route.addRouteInvoice(new Invoice(GameObject.GetInstance().GameTime, Invoice.InvoiceType.Tickets, ticketsIncome));
            airliner.CurrentFlight.Entry.TimeTable.Route.addRouteInvoice(new Invoice(GameObject.GetInstance().GameTime, Invoice.InvoiceType.OnFlight_Income, -mealExpenses));
            airliner.CurrentFlight.Entry.TimeTable.Route.addRouteInvoice(new Invoice(GameObject.GetInstance().GameTime, Invoice.InvoiceType.Fees, feesIncome));

            double wages = 0;

            if (airliner.CurrentFlight.isPassengerFlight())
            {
                int cabinCrew = ((AirlinerPassengerType)airliner.Airliner.Type).CabinCrew;

                wages = cabinCrew * flighttime.TotalHours * airliner.Airliner.Airline.Fees.getValue(FeeTypes.GetType("Cabin Wage"));// +(airliner.CurrentFlight.Entry.TimeTable.Route.getTotalCabinCrew() * airliner.Airliner.Airline.Fees.getValue(FeeTypes.GetType("Cabin kilometer rate")) * fdistance) + (airliner.Airliner.Type.CockpitCrew * airliner.Airliner.Airline.Fees.getValue(FeeTypes.GetType("Cockpit kilometer rate")) * fdistance);
                //wages
                AirlineHelpers.AddAirlineInvoice(airline, GameObject.GetInstance().GameTime, Invoice.InvoiceType.Wages, -wages);

                HolidayYearEvent holiday = HolidayYear.GetHoliday(airline.Profile.Country, GameObject.GetInstance().GameTime);

                if (holiday != null && (holiday.Holiday.Travel == Holiday.TravelType.Both || holiday.Holiday.Travel == Holiday.TravelType.Normal))
                    wages = wages * 1.50;

                airliner.CurrentFlight.Entry.TimeTable.Route.addRouteInvoice(new Invoice(GameObject.GetInstance().GameTime, Invoice.InvoiceType.Wages, -wages));

                CreatePassengersHappiness(airliner);


            }


            airliner.Statistics.addStatisticsValue(GameObject.GetInstance().GameTime.Year, StatisticsTypes.GetStatisticsType("Airliner_Income"), ticketsIncome - expenses - mealExpenses + feesIncome - wages);

            airliner.Airliner.Flown += fdistance;

            if (airliner.Airliner.Airline.IsHuman && Settings.GetInstance().MailsOnLandings)
                GameObject.GetInstance().NewsBox.addNews(new News(News.NewsType.Flight_News, GameObject.GetInstance().GameTime, string.Format("{0} landed", airliner.Name), string.Format("Your airliner [LI airliner={0}] has landed in [LI airport={1}], {2} with {3} passengers.\nThe airliner flow from [LI airport={4}], {5}", new object[] { airliner.Airliner.TailNumber, dest.Profile.IATACode, dest.Profile.Country.Name, airliner.CurrentFlight.getTotalPassengers(), dept.Profile.IATACode, dept.Profile.Country.Name })));

            SetNextFlight(airliner);

            CheckForService(airliner);



        }
        //creates the happiness for a landed route airliner
        private static void CreatePassengersHappiness(FleetAirliner airliner)
        {
            int serviceLevel = 0;//airliner.Route.DrinksFacility.ServiceLevel + airliner.Route.FoodFacility.ServiceLevel + airliner.Airliner.Airliner.getFacility(AirlinerFacility.FacilityType.Audio).ServiceLevel + airliner.Airliner.Airliner.getFacility(AirlinerFacility.FacilityType.Seat).ServiceLevel + airliner.Airliner.Airliner.getFacility(AirlinerFacility.FacilityType.Video).ServiceLevel;
            int happyValue = airliner.CurrentFlight.IsOnTime ? 10 : 20;
            happyValue -= (serviceLevel / 25);
            for (int i = 0; i < airliner.CurrentFlight.getTotalPassengers(); i++)
            {
                Boolean isHappy = rnd.Next(100) > happyValue;


                if (isHappy) PassengerHelpers.AddPassengerHappiness(airliner.Airliner.Airline);
            }
        }
        //simualtes an airliner for service
        private static void SimulateService(FleetAirliner airliner)
        {
            double servicePrice = 10000;

            airliner.CurrentPosition = airliner.CurrentFlight.Entry.Destination.Airport;// new GeoCoordinate(airliner.CurrentFlight.Entry.Destination.Airport.Profile.Coordinates.Latitude, airliner.CurrentFlight.Entry.Destination.Airport.Profile.Coordinates.Longitude);
            airliner.Status = FleetAirliner.AirlinerStatus.To_route_start;

            double fdistance = MathHelpers.GetDistance(airliner.CurrentFlight.Entry.Destination.Airport, airliner.CurrentPosition);
            double expenses = GameObject.GetInstance().FuelPrice * fdistance * airliner.CurrentFlight.getTotalPassengers() * airliner.Airliner.Type.FuelConsumption + airliner.CurrentPosition.getLandingFee();

            servicePrice += expenses;

            airliner.Airliner.Flown += fdistance;

            airliner.Airliner.LastServiceCheck = airliner.Airliner.Flown;

            AirlineHelpers.AddAirlineInvoice(airliner.Airliner.Airline, GameObject.GetInstance().GameTime, Invoice.InvoiceType.Maintenances, -servicePrice);

            airliner.CurrentFlight.Entry.TimeTable.Route.addRouteInvoice(new Invoice(GameObject.GetInstance().GameTime, Invoice.InvoiceType.Maintenances, -servicePrice));
            airliner.Statistics.addStatisticsValue(GameObject.GetInstance().GameTime.Year, StatisticsTypes.GetStatisticsType("Airliner_Income"), -servicePrice);

            SetNextFlight(airliner);

        }
        //checks for an airliner should go to service
        private static void CheckForService(FleetAirliner airliner)
        {
            double serviceCheck = 500000000;
            double sinceLastService = airliner.Airliner.Flown - airliner.Airliner.LastServiceCheck;


            if (sinceLastService > serviceCheck)
            {
                airliner.Status = FleetAirliner.AirlinerStatus.On_service;
                airliner.CurrentFlight.Entry.Destination = new RouteEntryDestination(airliner.Homebase, "Service",null);

            }


        }
        //gets the weather for an airliner
        private static Weather GetAirlinerWeather(FleetAirliner airliner)
        {
            double distance = MathHelpers.GetDistance(airliner.CurrentPosition, airliner.CurrentFlight.Entry.Destination.Airport);
            Airport dest = airliner.CurrentFlight.Entry.Destination.Airport;
            Airport dept = airliner.CurrentFlight.getDepartureAirport();

            double totalDistance = MathHelpers.GetDistance(dept.Profile.Coordinates, dest.Profile.Coordinates);

            return distance > totalDistance / 2 ? dept.Weather[0] : dest.Weather[0];
        }
        //sets the next flight for a route airliner
        private static void SetNextFlight(FleetAirliner airliner)
        {
            Route route = GetNextRoute(airliner);

            if ((airliner.CurrentFlight == null && route != null && route.HasStopovers) || (airliner.CurrentFlight != null && airliner.CurrentFlight is StopoverFlight && ((StopoverFlight)airliner.CurrentFlight).IsLastTrip) || (airliner.CurrentFlight != null && airliner.CurrentFlight.Entry.MainEntry == null && route != null && route.HasStopovers))
            {
                if (airliner.GroundedToDate > GameObject.GetInstance().GameTime)
                    airliner.CurrentFlight = new StopoverFlight(route.TimeTable.getNextEntry(airliner.GroundedToDate, airliner));
                else
                    airliner.CurrentFlight = new StopoverFlight(route.TimeTable.getNextEntry(GameObject.GetInstance().GameTime, airliner));

            }

            airliner.Status = FleetAirliner.AirlinerStatus.To_route_start;

            if (airliner.CurrentFlight is StopoverFlight && !((StopoverFlight)airliner.CurrentFlight).IsLastTrip)
            {
                ((StopoverFlight)airliner.CurrentFlight).setNextEntry();
                //airliner.CurrentFlight.FlightTime = new DateTime(Math.Max(GameObject.GetInstance().GameTime.Add(RouteTimeTable.MinTimeBetweenFlights).Ticks, airliner.CurrentFlight.FlightTime.Ticks));
            }
            else
            {
                if (route != null)
                    if (airliner.GroundedToDate > GameObject.GetInstance().GameTime)
                        airliner.CurrentFlight = new Flight(route.TimeTable.getNextEntry(airliner.GroundedToDate, airliner));
                    else
                        airliner.CurrentFlight = new Flight(route.TimeTable.getNextEntry(GameObject.GetInstance().GameTime, airliner));
                else
                    airliner.Status = FleetAirliner.AirlinerStatus.To_homebase;
            }

        }
        //returns if the wind is tail (1), head (-1), or from side (0)
        private static int GetWindInfluence(FleetAirliner airliner)
        {
            double direction = MathHelpers.GetDirection(airliner.CurrentPosition.Profile.Coordinates, airliner.CurrentFlight.getNextDestination().Profile.Coordinates);

            Weather.WindDirection windDirection = MathHelpers.GetWindDirectionFromDirection(direction);

            Weather currentWeather = GetAirlinerWeather(airliner);
            //W+E = 0+4= 5, N+S=2+6 - = Abs(Count/2) -> Head, Abs(0) -> Tail -> if ends/starts with same => tail, indexof +-1 -> tail, (4+(indexof))+-1 -> head 

            int windDirectionLenght = Enum.GetValues(typeof(Weather.WindDirection)).Length;
            int indexCurrentPosition = Array.IndexOf(Enum.GetValues(typeof(Weather.WindDirection)), windDirection);
            //int indexWeather = Array.IndexOf(Enum.GetValues(typeof(Weather.WindDirection)),currentWeather.WindSpeed);

            //check for tail wind
            Weather.WindDirection windTailLeft = indexCurrentPosition > 0 ? (Weather.WindDirection)indexCurrentPosition - 1 : (Weather.WindDirection)windDirectionLenght - 1;
            Weather.WindDirection windTailRight = indexCurrentPosition < windDirectionLenght - 1 ? (Weather.WindDirection)indexCurrentPosition + 1 : (Weather.WindDirection)0;

            if (windTailLeft == currentWeather.Direction || windTailRight == currentWeather.Direction || windDirection == currentWeather.Direction)
                return 1;

            Weather.WindDirection windOpposite = indexCurrentPosition - (windDirectionLenght / 2) > 0 ? (Weather.WindDirection)indexCurrentPosition - (windDirectionLenght / 2) : (Weather.WindDirection)windDirectionLenght - 1 - indexCurrentPosition - (windDirectionLenght / 2);
            int indexOpposite = Array.IndexOf(Enum.GetValues(typeof(Weather.WindDirection)), windOpposite);

            Weather.WindDirection windHeadLeft = indexOpposite > 0 ? (Weather.WindDirection)indexOpposite - 1 : (Weather.WindDirection)windDirectionLenght - 1;
            Weather.WindDirection windHeadRight = indexOpposite < windDirectionLenght - 1 ? (Weather.WindDirection)indexOpposite + 1 : (Weather.WindDirection)0;

            if (windHeadLeft == currentWeather.Direction || windHeadRight == currentWeather.Direction || windOpposite == currentWeather.Direction)
                return -1;

            return 0;
        }
        //finds the next flight time for an airliner - checks also for delay
        private static DateTime GetNextFlightTime(FleetAirliner airliner)
        {

            if (airliner.CurrentFlight == null)
            {
                SetNextFlight(airliner);
                return airliner.CurrentFlight.FlightTime;
            }
            else
                if (airliner.CurrentFlight.Entry.TimeTable.Route.Banned)
                {
                    SetNextFlight(airliner);
                    if (airliner.CurrentFlight.Entry.TimeTable.Route.Banned)
                    {
                        airliner.Status = FleetAirliner.AirlinerStatus.Stopped;
                        return new DateTime(2500, 1, 1);

                    }
                    else
                        return airliner.CurrentFlight.FlightTime;
                }
                else
                    return airliner.CurrentFlight.FlightTime;

        }
        //returns the passengers for an airliner
        public static int GetPassengers(FleetAirliner airliner, AirlinerClass.ClassType type)
        {
            return PassengerHelpers.GetFlightPassengers(airliner, type);
        }
        //returns the next route for an airliner 
        private static Route GetNextRoute(FleetAirliner airliner)
        {

            var entries = from e in airliner.Routes.Select(r => r.TimeTable.getNextEntry(GameObject.GetInstance().GameTime, airliner)) where e != null orderby MathHelpers.ConvertEntryToDate(e) select e;

            if (entries.Count() > 0)
                return entries.First().TimeTable.Route;
            else
                return null;

        }
        //handles an influence for a historic event
        public static void SetHistoricEventInfluence(HistoricEventInfluence e, Boolean onEndDate)
        {
            double value = onEndDate ? -e.Value : e.Value;

            switch (e.Type)
            {
                case HistoricEventInfluence.InfluenceType.PassengerDemand:
                    PassengerHelpers.ChangePaxDemand(value);
                    break;
                case HistoricEventInfluence.InfluenceType.FuelPrices:
                    double percent = (100 - value) / 100;
                    GameObject.GetInstance().FuelPrice = GameObject.GetInstance().FuelPrice * percent;
                    break;
                case HistoricEventInfluence.InfluenceType.Stocks:
                    break;
            }
        }
        //creates a new game
        public static void CreateGame(StartDataObject startData)
        {
            if (startData.RealData)
            {
                var notRealAirlines = Airlines.GetAirlines(a=>!a.Profile.IsReal && a != startData.Airline);
                var notRealManufacturers = Manufacturers.GetManufacturers(m=>!m.IsReal);
                var notRealAirliners = AirlinerTypes.GetTypes(a => notRealManufacturers.Contains(a.Manufacturer));

                foreach (Airline notRealAirliner in notRealAirlines)
                    Airlines.RemoveAirline(notRealAirliner);

                foreach (AirlinerType airliner in notRealAirliners)
                    AirlinerTypes.RemoveType(airliner);
                
            }

            int startYear = startData.Year;
            int opponents = startData.NumberOfOpponents;
            Airline airline = startData.Airline;
            Continent continent = startData.Continent;
            Region region = startData.Region;

            GameTimeZone gtz = startData.TimeZone;
            GameObject.GetInstance().DayRoundEnabled = startData.UseDayTurns;
            GameObject.GetInstance().TimeZone = gtz;
            GameObject.GetInstance().Difficulty = startData.Difficulty;
            GameObject.GetInstance().GameTime = new DateTime(startYear, 1, 1);
            GameObject.GetInstance().StartDate = GameObject.GetInstance().GameTime;
            //sets the fuel price
            GameObject.GetInstance().FuelPrice = Inflations.GetInflation(GameObject.GetInstance().GameTime.Year).FuelPrice;

            airline.Profile.Country = startData.HomeCountry;
            airline.Profile.CEO = startData.CEO;

            GameObject.GetInstance().setHumanAirline(airline);
            GameObject.GetInstance().MainAirline = GameObject.GetInstance().HumanAirline;

            if (startData.LocalCurrency)
                GameObject.GetInstance().CurrencyCountry = airline.Profile.Country;
            // AppSettings.GetInstance().resetCurrencyFormat();

            Airport airport = startData.Airport;

            AirportHelpers.RentGates(airport, airline, 2);

            AirportFacility checkinFacility = AirportFacilities.GetFacilities(AirportFacility.FacilityType.CheckIn).Find(f => f.TypeLevel == 1);
            AirportFacility facility = AirportFacilities.GetFacilities(AirportFacility.FacilityType.Service).Find((delegate(AirportFacility f) { return f.TypeLevel == 1; }));

            airport.addAirportFacility(GameObject.GetInstance().HumanAirline, facility, GameObject.GetInstance().GameTime);
            airport.addAirportFacility(GameObject.GetInstance().HumanAirline, checkinFacility, GameObject.GetInstance().GameTime);

            if (continent.Uid != "100" || region.Uid != "100")
            {
                var airlines = Airlines.GetAirlines(a => a.Profile.Country.Region == region || (region.Uid == "100" && continent.hasRegion(a.Profile.Country.Region)) && a.Profile.Founded <= startYear && a.Profile.Folded > startYear);
                var airports = Airports.GetAirports(a => a.Profile.Country.Region == region || (region.Uid == "100" && continent.hasRegion(a.Profile.Country.Region)) && a.Profile.Period.From.Year <= startYear && a.Profile.Period.To.Year > startYear);

                Airports.Clear();
                foreach (Airport a in airports)
                    Airports.AddAirport(a);

                Airlines.Clear();
                foreach (Airline a in airlines)
                    Airlines.AddAirline(a);
            }

            PassengerHelpers.CreateAirlineDestinationDemand();

            AirlinerHelpers.CreateStartUpAirliners();

            if (startData.RandomOpponents || startData.Opponents == null)
                Setup.SetupMainGame(opponents, startData.SameRegion);
            else
                Setup.SetupMainGame(startData.Opponents,startData.NumberOfOpponents);


            airline.MarketFocus = startData.Focus;

            GeneralHelpers.CreateHolidays(GameObject.GetInstance().GameTime.Year);

            if (startData.IsPaused)
                GameObjectWorker.GetInstance().startPaused();
            else
                GameObjectWorker.GetInstance().start();

            GameObject.GetInstance().NewsBox.addNews(new News(News.NewsType.Standard_News, GameObject.GetInstance().GameTime, Translator.GetInstance().GetString("News", "1001"), string.Format(Translator.GetInstance().GetString("News", "1001", "message"), GameObject.GetInstance().HumanAirline.Profile.CEO, GameObject.GetInstance().HumanAirline.Profile.IATACode)));

            if (startData.MajorAirports)
            {
                var majorAirports = Airports.GetAllAirports(a => a.Profile.Size == GeneralHelpers.Size.Largest || a.Profile.Size == GeneralHelpers.Size.Large || a.Profile.Size == GeneralHelpers.Size.Very_large || a.Profile.Size == GeneralHelpers.Size.Medium);
                var usedAirports = Airlines.GetAllAirlines().SelectMany(a => a.Airports);

                majorAirports.AddRange(usedAirports);

                Airports.Clear();

                foreach (Airport majorAirport in majorAirports.Distinct())
                    Airports.AddAirport(majorAirport);

            }

            Action action = () =>
            {
                Stopwatch swPax = new Stopwatch();
                swPax.Start();

                PassengerHelpers.CreateDestinationDemand();

                Console.WriteLine("Demand have been created in {0} ms.", swPax.ElapsedMilliseconds);
                swPax.Stop();
            };

            Task.Factory.StartNew(action);
            //Task.Run(action);
            //Task t2 = Task.Factory.StartNew(action, "passengers");


        }

        private static void ClearAllUsedStats()
        {
            Airports.GetAllAirports().ForEach(a => a.clearDestinationPassengerStatistics());
            Airports.GetAllAirports().ForEach(a => a.clearDestinationCargoStatistics());
            AirlineHelpers.ClearRoutesStatistics();
            AirlineHelpers.ClearAirlinesStatistics();
            AirportHelpers.ClearAirportStatistics();
        }

    }
}
