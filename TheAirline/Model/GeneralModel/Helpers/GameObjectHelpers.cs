using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using TheAirline.Model.AirlinerModel.RouteModel;
using TheAirline.Model.AirlinerModel;
using TheAirline.Model.AirlineModel;
using TheAirline.Model.AirportModel;
using TheAirline.Model.GeneralModel.StatisticsModel;
using TheAirline.Model.PassengerModel;
using TheAirline.GraphicsModel.Converters;
using TheAirline.GraphicsModel.UserControlModel.PopUpWindowsModel;
using TheAirline.GraphicsModel.UserControlModel.MessageBoxModel;
using TheAirline.Model.GeneralModel.HolidaysModel;

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
            GameObject.GetInstance().GameTime = GameObject.GetInstance().GameTime.AddMinutes(Settings.GetInstance().MinutesPerTurn);

            CalibrateTime();

            if (MathHelpers.IsNewDay(GameObject.GetInstance().GameTime)) DoDailyUpdate();

            if (MathHelpers.IsNewMonth(GameObject.GetInstance().GameTime)) DoMonthlyUpdate();

            if (MathHelpers.IsNewYear(GameObject.GetInstance().GameTime)) DoYearlyUpdate();

            foreach (Airline airline in Airlines.GetAllAirlines())
            {
                if (GameObject.GetInstance().GameTime.Hour % 3 == 0 && GameObject.GetInstance().GameTime.Minute == 0)
                {
                    if (airline != GameObject.GetInstance().HumanAirline)
                        AIHelpers.UpdateCPUAirline(airline);
                }


                foreach (FleetAirliner airliner in airline.Fleet)
                {
                    UpdateAirliner(airliner);
                }

            }

            GC.Collect();
            GC.WaitForPendingFinalizers();
            GC.Collect();

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
            int totalRoutes = (from r in Airlines.GetAllAirlines().SelectMany(a => a.Routes) select r).Count();
            int totalAirlinersOnRoute = (from a in Airlines.GetAllAirlines().SelectMany(t => t.Fleet) where a.HasRoute select a).Count();

            Console.WriteLine(GameObject.GetInstance().GameTime.ToShortDateString() + ": " + DateTime.Now.Subtract(LastTime).TotalMilliseconds + " ms." + " : routes: " + totalRoutes + " airliners on route: " + totalAirlinersOnRoute);

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
                    GameObject.GetInstance().NewsBox.addNews(new News(News.NewsType.Airport_News, GameObject.GetInstance().GameTime, "Airport closing", string.Format("The airport {0}({1}) is closing in 14 days.\n\rPlease move all routes to another destination.", airport.Profile.Name, new AirportCodeConverter().Convert(airport).ToString())));
                else
                    GameObject.GetInstance().NewsBox.addNews(new News(News.NewsType.Airport_News, GameObject.GetInstance().GameTime, "Airport closing", string.Format("The airport {0}({1}) is closing in 14 days.\n\rThe airport will be replaced by {2}({3}) and all gates and routes from {0} will be reallocated to {2}.", airport.Profile.Name, new AirportCodeConverter().Convert(airport).ToString(), reallocatedAirport.Profile.Name, new AirportCodeConverter().Convert(reallocatedAirport).ToString())));


            }
            //checks for new airports which are opening
            List<Airport> openedAirports = Airports.GetAllAirports(a => a.Profile.Period.From.ToShortDateString() == GameObject.GetInstance().GameTime.ToShortDateString());

            foreach (Airport airport in openedAirports)
            {

                PassengerHelpers.CreateDestinationPassengers(airport);

                foreach (Airport dAirport in Airports.GetAirports(a => a != airport && a.Profile.Town != airport.Profile.Town && MathHelpers.GetDistance(a.Profile.Coordinates, airport.Profile.Coordinates) > 25))
                    PassengerHelpers.CreateDestinationPassengers(dAirport, airport);

                int count = Airports.GetAirports(a => a.Profile.Town == airport.Profile.Town && airport != a && a.Terminals.getNumberOfGates(GameObject.GetInstance().HumanAirline) > 0).Count;

                if (count == 1)
                {
                    Airport allocateFromAirport = Airports.GetAirports(a => a.Profile.Town == airport.Profile.Town && airport != a && a.Terminals.getNumberOfGates(GameObject.GetInstance().HumanAirline) > 0).First();
                    GameObject.GetInstance().NewsBox.addNews(new News(News.NewsType.Airport_News, GameObject.GetInstance().GameTime, "New airport opened", string.Format("A new airport {0}({1}) is opened in {2}, {3}.\n\rYou can reallocate all your operations from {4}({5}) for free within the next 30 days", airport.Profile.Name, new AirportCodeConverter().Convert(airport).ToString(), airport.Profile.Town, ((Country)new CountryCurrentCountryConverter().Convert(airport.Profile.Country)).Name, allocateFromAirport.Profile.Name, new AirportCodeConverter().Convert(allocateFromAirport).ToString())));
                }
                else
                    GameObject.GetInstance().NewsBox.addNews(new News(News.NewsType.Airport_News, GameObject.GetInstance().GameTime, "New airport opened", string.Format("A new airport {0}({1}) is opened in {2}, {3}", airport.Profile.Name, new AirportCodeConverter().Convert(airport).ToString(), airport.Profile.Town, ((Country)new CountryCurrentCountryConverter().Convert(airport.Profile.Country)).Name)));



            }
            //checks for airports which are closing down
            List<Airport> closedAirports = Airports.GetAllAirports(a => a.Profile.Period.To.ToShortDateString() == GameObject.GetInstance().GameTime.ToShortDateString());
            foreach (Airport airport in closedAirports)
            {
                //check for airport which are reallocated 
                Airport reallocatedAirport = openedAirports.Find(a => a.Profile.Town == airport.Profile.Town);

                if (reallocatedAirport != null)
                {
                    var airlines = new List<Airline>(from g in airport.Terminals.getUsedGates() select g.Airline).Distinct();
                    foreach (Airline airline in airlines)
                    {
                        AirlineHelpers.ReallocateAirport(airport, reallocatedAirport, airline);

                        if (airline.IsHuman)
                            GameObject.GetInstance().NewsBox.addNews(new News(News.NewsType.Airport_News, GameObject.GetInstance().GameTime, "Airport operations changed", string.Format("All your gates, routes and facilities has been moved from {0}({1}) to {2}({3})", airport.Profile.Name, new AirportCodeConverter().Convert(airport).ToString(), reallocatedAirport.Profile.Name, new AirportCodeConverter().Convert(reallocatedAirport).ToString())));
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
                                GameTimer.GetInstance().pause();

                                airliner.Homebase = (Airport)PopUpNewAirlinerHomeBase.ShowPopUp(airliner);

                                GameTimer.GetInstance().start();

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
            var humanAirportFacilities = (from f in GameObject.GetInstance().HumanAirline.Airports.SelectMany(a => a.getAirportFacilities(GameObject.GetInstance().HumanAirline)) where f.FinishedDate.ToShortDateString() == GameObject.GetInstance().GameTime.ToShortDateString() select f);

            foreach (AirlineAirportFacility facility in humanAirportFacilities)
            {
                GameObject.GetInstance().NewsBox.addNews(new News(News.NewsType.Airport_News, GameObject.GetInstance().GameTime, "Airport facility", string.Format("Your airport facility {0} at {1} is now finished building", facility.Facility.Name, facility.Airport.Profile.Name)));

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
            //
            //updates airports
            foreach (Airport airport in Airports.GetAllActiveAirports())
            {
                Weather.eWindSpeed[] windSpeedValues = (Weather.eWindSpeed[])Enum.GetValues(typeof(Weather.eWindSpeed));
                Weather.eWindSpeed windSpeed = windSpeedValues[rnd.Next(windSpeedValues.Length)];

                Weather.WindDirection[] windDirectionValues = (Weather.WindDirection[])Enum.GetValues(typeof(Weather.WindDirection));
                Weather.WindDirection windDirection = windDirectionValues[rnd.Next(windDirectionValues.Length)];

                airport.Weather.WindSpeed = windSpeed;
                airport.Weather.Direction = windDirection;

                // chs, 2011-01-11 changed for delivery of terminals
                foreach (Terminal terminal in airport.Terminals.getTerminals())
                {
                    if (terminal.DeliveryDate.Year == GameObject.GetInstance().GameTime.Year && terminal.DeliveryDate.Month == GameObject.GetInstance().GameTime.Month && terminal.DeliveryDate.Day == GameObject.GetInstance().GameTime.Day)
                    {
                        if (terminal.Airline.IsHuman)
                            GameObject.GetInstance().NewsBox.addNews(new News(News.NewsType.Airport_News, GameObject.GetInstance().GameTime, "Construction of terminal", string.Format("Your terminal at {0}, {1} is now finished and ready for use.", airport.Profile.Name, airport.Profile.Country.Name)));

                        //moves the "old" rented gates into the new terminal
                        foreach (Terminal tTerminal in airport.Terminals.getTerminals().FindAll((delegate(Terminal t) { return t.Airline == null; })))
                        {
                            foreach (Gate gate in tTerminal.Gates.getGates(terminal.Airline))
                            {
                                Gate nGate = terminal.Gates.getEmptyGate(terminal.Airline);
                                if (nGate != null)
                                {
                                    nGate.HasRoute = gate.HasRoute;

                                    gate.Airline = null;
                                    gate.HasRoute = false;
                                }


                            }

                        }



                    }

                }

            }

            //checks for airliners for the human airline
            foreach (FleetAirliner airliner in GameObject.GetInstance().HumanAirline.Fleet.FindAll((delegate(FleetAirliner a) { return a.Airliner.BuiltDate == GameObject.GetInstance().GameTime && a.Purchased != FleetAirliner.PurchasedType.BoughtDownPayment; })))
                GameObject.GetInstance().NewsBox.addNews(new News(News.NewsType.Fleet_News, GameObject.GetInstance().GameTime, "Delivery of airliner", string.Format("Your new airliner {0} as been delivered to your fleet.\nThe airliner is currently at {1}, {2}.", airliner.Name, airliner.Homebase.Profile.Name, airliner.Homebase.Profile.Country.Name)));


            foreach (Airline airline in Airlines.GetAllAirlines())
                foreach (FleetAirliner airliner in airline.Fleet.FindAll(a => a.Airliner.BuiltDate == GameObject.GetInstance().GameTime && a.Purchased == FleetAirliner.PurchasedType.BoughtDownPayment))
                {
                    airliner.Purchased = FleetAirliner.PurchasedType.Bought;
                    if (airline.Money >= airliner.Airliner.Type.Price)
                    {
                        AirlineHelpers.AddAirlineInvoice(airline, GameObject.GetInstance().GameTime, Invoice.InvoiceType.Purchases, -airliner.Airliner.Type.Price);

                        if (airline.IsHuman)
                            GameObject.GetInstance().NewsBox.addNews(new News(News.NewsType.Fleet_News, GameObject.GetInstance().GameTime, "Delivery of airliner", string.Format("Your new airliner {0} as been delivered to your fleet.\nThe airliner is currently at {1}, {2}", airliner.Name, airliner.Homebase.Profile.Name, airliner.Homebase.Profile.Country.Name)));

                    }
                    else
                    {
                        if (airline.IsHuman)
                            GameObject.GetInstance().NewsBox.addNews(new News(News.NewsType.Fleet_News, GameObject.GetInstance().GameTime, "Delivery of airliner", string.Format("Your new airliner {0} can't be delivered to your fleet.\nYou don't have enough money to purchase it.", airliner.Name)));

                    }
                }



        }
        //do the yearly update
        private static void DoYearlyUpdate()
        {
            //updates holidays 
            GeneralHelpers.CreateHolidays(GameObject.GetInstance().GameTime.Year);

            foreach (Airline airline in Airlines.GetAllAirlines())
            {
                foreach (FleetAirliner airliner in airline.Fleet)
                    AirlineHelpers.AddAirlineInvoice(airline, GameObject.GetInstance().GameTime, Invoice.InvoiceType.Maintenances, -airliner.Airliner.Type.getMaintenance());
            }
        }
        //do the monthly update
        private static void DoMonthlyUpdate()
        {
            foreach (Airline airline in Airlines.GetAllAirlines())
            {
                //AirlineHelpers.MergeInvoicesMonthly(airline);
                foreach (AirlineFacility facility in airline.Facilities)
                    AirlineHelpers.AddAirlineInvoice(airline, GameObject.GetInstance().GameTime, Invoice.InvoiceType.Airline_Expenses, -facility.MonthlyCost);

                foreach (FleetAirliner airliner in airline.Fleet.FindAll((delegate(FleetAirliner a) { return a.Purchased == FleetAirliner.PurchasedType.Leased; })))
                    AirlineHelpers.AddAirlineInvoice(airline, GameObject.GetInstance().GameTime, Invoice.InvoiceType.Rents, -airliner.Airliner.LeasingPrice);

                // chs, 2011-28-10 changed so a terminal only costs 75% of gate price
                foreach (Airport airport in airline.Airports)
                {
                    foreach (Terminal terminal in airport.Terminals.getDeliveredTerminals())
                    {
                        double gates = Convert.ToDouble(terminal.Gates.getNumberOfGates(airline));
                        double gatePrice = airline == terminal.Airline ? airport.getGatePrice() * 0.75 : airport.getGatePrice();

                        AirlineHelpers.AddAirlineInvoice(airline, GameObject.GetInstance().GameTime, Invoice.InvoiceType.Rents, -gatePrice * gates);

                        long airportIncome = Convert.ToInt64(gatePrice * gates);
                        airport.Income += airportIncome;


                    }
                    //wages
                    foreach (AirportFacility facility in airport.getCurrentAirportFacilities(airline))
                    {
                        double wage = 0;

                        if (facility.EmployeeType == AirportFacility.EmployeeTypes.Maintenance)
                            wage = airline.Fees.getValue(FeeTypes.GetType("Maintenance wage"));
                        if (facility.EmployeeType == AirportFacility.EmployeeTypes.Support)
                            wage = airline.Fees.getValue(FeeTypes.GetType("Support wage"));

                        double facilityWage = facility.NumberOfEmployees * wage * (40 * 4.33); //40 hours per week and 4.33 weeks per month

                        AirlineHelpers.AddAirlineInvoice(airline, GameObject.GetInstance().GameTime, Invoice.InvoiceType.Wages, -facilityWage);
                    }
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
                            airline.Reputation += airline.getAirlineAdvertisement(type).ReputationLevel;

                    }
                }


            }
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
                airliner.CurrentFlight = new Flight(route.TimeTable.getNextEntry(GameObject.GetInstance().GameTime, airliner.CurrentPosition));
            }
            double adistance = MathHelpers.GetDistance(airliner.CurrentPosition, airliner.CurrentFlight.Entry.Destination.Airport.Profile.Coordinates);

            double speed = airliner.Airliner.Type.CruisingSpeed / (60 / Settings.GetInstance().MinutesPerTurn);
            if (airliner.CurrentFlight != null)
            {
                Weather currentWeather = GetAirlinerWeather(airliner);
                int wind = currentWeather.Direction == Weather.WindDirection.Tail ? (int)currentWeather.WindSpeed / (60 / Settings.GetInstance().MinutesPerTurn) : -(int)currentWeather.WindSpeed / (60 / Settings.GetInstance().MinutesPerTurn);
                speed = airliner.Airliner.Type.CruisingSpeed / (60 / Settings.GetInstance().MinutesPerTurn) + wind;

            }
            if (adistance > 4)
                MathHelpers.MoveObject(airliner.CurrentPosition, airliner.CurrentFlight.Entry.Destination.Airport.Profile.Coordinates, Math.Min(speed, MathHelpers.GetDistance(airliner.CurrentPosition, airliner.CurrentFlight.Entry.Destination.Airport.Profile.Coordinates)));

            double distance = MathHelpers.GetDistance(airliner.CurrentPosition, airliner.CurrentFlight.Entry.Destination.Airport.Profile.Coordinates);

            if (MathHelpers.GetDistance(airliner.CurrentPosition, airliner.CurrentFlight.Entry.Destination.Airport.Profile.Coordinates) < 5)
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
        //the method for updating a route airliner with status toroutestart
        private static void UpdateToRouteStartAirliner(FleetAirliner airliner)
        {
            if (airliner.CurrentFlight == null)
            {
                Route route = GetNextRoute(airliner);
                airliner.CurrentFlight = new Flight(route.TimeTable.getNextEntry(GameObject.GetInstance().GameTime, airliner));
            }
            Coordinates destination = airliner.CurrentFlight.Entry.DepartureAirport.Profile.Coordinates;

            double adistance = MathHelpers.GetDistance(airliner.CurrentPosition, destination);

            double speed = airliner.Airliner.Type.CruisingSpeed / (60 / Settings.GetInstance().MinutesPerTurn);
            if (airliner.CurrentFlight != null)
            {
                Weather currentWeather = GetAirlinerWeather(airliner);
                int wind = currentWeather.Direction == Weather.WindDirection.Tail ? (int)currentWeather.WindSpeed / (60 / Settings.GetInstance().MinutesPerTurn) : -(int)currentWeather.WindSpeed / (60 / Settings.GetInstance().MinutesPerTurn);
                speed = airliner.Airliner.Type.CruisingSpeed / (60 / Settings.GetInstance().MinutesPerTurn) + wind;

            }
            if (adistance > 4)
                MathHelpers.MoveObject(airliner.CurrentPosition, destination, Math.Min(speed, MathHelpers.GetDistance(airliner.CurrentPosition, destination)));

            double distance = MathHelpers.GetDistance(airliner.CurrentPosition, destination);

            if (MathHelpers.GetDistance(airliner.CurrentPosition, destination) < 5)
            {
                airliner.Status = FleetAirliner.AirlinerStatus.Resting;
                airliner.CurrentPosition = new Coordinates(destination.Latitude, destination.Longitude);

            }



        }
        //simulates a route airliner going to homebase
        private static void SimulateToHomebase(FleetAirliner airliner)
        {
            airliner.CurrentPosition = new Coordinates(airliner.CurrentFlight.Entry.Destination.Airport.Profile.Coordinates.Latitude, airliner.CurrentFlight.Entry.Destination.Airport.Profile.Coordinates.Longitude);
            Airport airport = Airports.GetAirport(airliner.CurrentPosition);
            airliner.Status = FleetAirliner.AirlinerStatus.To_homebase;

            if (airliner.CurrentFlight.Entry.Destination.Airport.Profile.Coordinates.CompareTo(airliner.Homebase.Profile.Coordinates) == 0)
                airliner.Status = FleetAirliner.AirlinerStatus.Stopped;
            else
                airliner.CurrentFlight = new Flight(new RouteTimeTableEntry(airliner.CurrentFlight.Entry.TimeTable, GameObject.GetInstance().GameTime.DayOfWeek, GameObject.GetInstance().GameTime.TimeOfDay, new RouteEntryDestination(airliner.Homebase, "Service")));

        }

        //simulates a route airliner taking off
        private static void SimulateTakeOff(FleetAirliner airliner)
        {


            airliner.Status = FleetAirliner.AirlinerStatus.On_route;

            foreach (AirlinerClass aClass in airliner.Airliner.Classes)
            {
                airliner.CurrentFlight.Classes.Add(new FlightAirlinerClass(airliner.CurrentFlight.Entry.TimeTable.Route.getRouteAirlinerClass(aClass.Type), GetPassengers(airliner, aClass.Type)));
            }

            Airport airport = Airports.GetAirport(airliner.CurrentPosition);

            if (airport != null)
            {
                airport.Statistics.addStatisticsValue(GameObject.GetInstance().GameTime.Year, airliner.Airliner.Airline, StatisticsTypes.GetStatisticsType("Departures"), 1);

                double destPassengers = airport.Statistics.getStatisticsValue(GameObject.GetInstance().GameTime.Year, airliner.Airliner.Airline, StatisticsTypes.GetStatisticsType("Passengers"));
                double destDepartures = airport.Statistics.getStatisticsValue(GameObject.GetInstance().GameTime.Year, airliner.Airliner.Airline, StatisticsTypes.GetStatisticsType("Arrivals"));
                airport.Statistics.setStatisticsValue(GameObject.GetInstance().GameTime.Year, airliner.Airliner.Airline, StatisticsTypes.GetStatisticsType("Passengers%"), (int)(destPassengers / destDepartures));

            }
            airliner.Airliner.Airline.Statistics.addStatisticsValue(GameObject.GetInstance().GameTime.Year, StatisticsTypes.GetStatisticsType("Departures"), 1);
            airliner.Statistics.addStatisticsValue(GameObject.GetInstance().GameTime.Year, StatisticsTypes.GetStatisticsType("Departures"), 1);

            foreach (AirlinerClass aClass in airliner.Airliner.Classes)
            {
                RouteAirlinerClass raClass = airliner.CurrentFlight.Entry.TimeTable.Route.getRouteAirlinerClass(aClass.Type);

                airliner.CurrentFlight.Entry.TimeTable.Route.Statistics.addStatisticsValue(raClass, StatisticsTypes.GetStatisticsType("Departures"), 1);
            }
            double airlinerPassengers = airliner.Statistics.getStatisticsValue(GameObject.GetInstance().GameTime.Year, StatisticsTypes.GetStatisticsType("Passengers"));
            double airlinerDepartures = airliner.Statistics.getStatisticsValue(GameObject.GetInstance().GameTime.Year, StatisticsTypes.GetStatisticsType("Departures"));
            airliner.Statistics.setStatisticsValue(GameObject.GetInstance().GameTime.Year, StatisticsTypes.GetStatisticsType("Passengers%"), (int)(airlinerPassengers / airlinerDepartures));


            double airlinePassengers = airliner.Airliner.Airline.Statistics.getStatisticsValue(GameObject.GetInstance().GameTime.Year, StatisticsTypes.GetStatisticsType("Passengers"));
            double airlineDepartures = airliner.Airliner.Airline.Statistics.getStatisticsValue(GameObject.GetInstance().GameTime.Year, StatisticsTypes.GetStatisticsType("Departures"));
            airliner.Airliner.Airline.Statistics.setStatisticsValue(GameObject.GetInstance().GameTime.Year, StatisticsTypes.GetStatisticsType("Passengers%"), (int)(airlinePassengers / airlineDepartures));

        }
        //simulates a route airliner landing
        private static void SimulateLanding(FleetAirliner airliner)
        {
            TimeSpan flighttime= GameObject.GetInstance().GameTime.Subtract(airliner.CurrentFlight.FlightTime);


            airliner.CurrentPosition = new Coordinates(airliner.CurrentFlight.Entry.Destination.Airport.Profile.Coordinates.Latitude, airliner.CurrentFlight.Entry.Destination.Airport.Profile.Coordinates.Longitude);
            airliner.Status = FleetAirliner.AirlinerStatus.Resting;

            double groundTaxPerPassenger = 5;

            double tax = groundTaxPerPassenger * airliner.CurrentFlight.getTotalPassengers();


            if (airliner.CurrentFlight.Entry.Destination.Airport.Profile.Country.Name != airliner.CurrentFlight.getDepartureAirport().Profile.Country.Name)
                tax = 2 * tax;

            double ticketsIncome = 0;

            foreach (AirlinerClass aClass in airliner.Airliner.Classes)
                ticketsIncome += airliner.CurrentFlight.getFlightAirlinerClass(aClass.Type).Passengers * airliner.CurrentFlight.Entry.TimeTable.Route.getRouteAirlinerClass(aClass.Type).FarePrice;

            Airport dest = Airports.GetAirport(airliner.CurrentPosition);
            Airport dept = airliner.CurrentFlight.getDepartureAirport();

            double dist = MathHelpers.GetDistance(dest.Profile.Coordinates, dept.Profile.Coordinates);

            double feesIncome = 0;
            foreach (FeeType feeType in FeeTypes.GetTypes(FeeType.eFeeType.Fee))
            {
                foreach (AirlinerClass aClass in airliner.Airliner.Classes)
                {
                    double percent = 0.10;
                    double maxValue = Convert.ToDouble(feeType.Percentage) * (1 + percent);
                    double minValue = Convert.ToDouble(feeType.Percentage) * (1 - percent);

                    double value = Convert.ToDouble(rnd.Next((int)minValue, (int)maxValue)) / 100;

                    feesIncome += airliner.CurrentFlight.getFlightAirlinerClass(aClass.Type).Passengers * value * airliner.Airliner.Airline.Fees.getValue(feeType);
                }
            }

            double mealExpenses = 0;
            foreach (AirlinerClass aClass in airliner.Airliner.Classes)
            {
                foreach (RouteFacility facility in airliner.CurrentFlight.Entry.TimeTable.Route.getRouteAirlinerClass(aClass.Type).getFacilities())
                {
                    if (facility.EType == RouteFacility.ExpenseType.Fixed)
                        mealExpenses += airliner.CurrentFlight.getFlightAirlinerClass(aClass.Type).Passengers * facility.ExpensePerPassenger;
                    else
                    {
                        FeeType feeType = facility.FeeType;
                        double percent = 0.10;
                        double maxValue = Convert.ToDouble(feeType.Percentage) * (1 + percent);
                        double minValue = Convert.ToDouble(feeType.Percentage) * (1 - percent);

                        double value = Convert.ToDouble(rnd.Next((int)minValue, (int)maxValue)) / 100;

                        mealExpenses -= airliner.CurrentFlight.getFlightAirlinerClass(aClass.Type).Passengers * value * airliner.Airliner.Airline.Fees.getValue(feeType);

                    }
                }
            }
                /*
                if (airliner.CurrentFlight.Entry.TimeTable.Route.getRouteAirlinerClass(aClass.Type).FoodFacility.EType == RouteFacility.ExpenseType.Fixed)
                {
                    mealExpenses += airliner.CurrentFlight.getFlightAirlinerClass(aClass.Type).Passengers * airliner.CurrentFlight.Entry.TimeTable.Route.getRouteAirlinerClass(aClass.Type).FoodFacility.ExpensePerPassenger;
                }
                else
                {
                    FeeType feeType = airliner.CurrentFlight.Entry.TimeTable.Route.getRouteAirlinerClass(aClass.Type).FoodFacility.FeeType;
                    double percent = 0.10;
                    double maxValue = Convert.ToDouble(feeType.Percentage) * (1 + percent);
                    double minValue = Convert.ToDouble(feeType.Percentage) * (1 - percent);

                    double value = Convert.ToDouble(rnd.Next((int)minValue, (int)maxValue)) / 100;

                    mealExpenses -= airliner.CurrentFlight.getFlightAirlinerClass(aClass.Type).Passengers * value * airliner.Airliner.Airline.Fees.getValue(feeType);
                }
                if (airliner.CurrentFlight.Entry.TimeTable.Route.getRouteAirlinerClass(aClass.Type).DrinksFacility.EType == RouteFacility.ExpenseType.Fixed)
                {
                    mealExpenses += airliner.CurrentFlight.getFlightAirlinerClass(aClass.Type).Passengers * airliner.CurrentFlight.Entry.TimeTable.Route.getRouteAirlinerClass(aClass.Type).DrinksFacility.ExpensePerPassenger;
                }
                else
                {
                    FeeType feeType = airliner.CurrentFlight.Entry.TimeTable.Route.getRouteAirlinerClass(aClass.Type).DrinksFacility.FeeType;
                    double percent = 0.10;
                    double maxValue = Convert.ToDouble(feeType.Percentage) * (1 + percent);
                    double minValue = Convert.ToDouble(feeType.Percentage) * (1 - percent);

                    double value = Convert.ToDouble(rnd.Next((int)minValue, (int)maxValue)) / 100;

                    mealExpenses -= airliner.CurrentFlight.getFlightAirlinerClass(aClass.Type).Passengers * value * airliner.Airliner.Airline.Fees.getValue(feeType);
                }
                null
                if (airliner.CurrentFlight.Entry.TimeTable.Route.getRouteAirlinerClass(aClass.Type).AlcoholicDrinksFacility.EType == RouteFacility.ExpenseType.Fixed)
                {
                    mealExpenses += airliner.CurrentFlight.getFlightAirlinerClass(aClass.Type).Passengers * airliner.CurrentFlight.Entry.TimeTable.Route.getRouteAirlinerClass(aClass.Type).AlcoholicDrinksFacility.ExpensePerPassenger;
    
                }
                else
                {
                    FeeType feeType = airliner.CurrentFlight.Entry.TimeTable.Route.getRouteAirlinerClass(aClass.Type).AlcoholicDrinksFacility.FeeType;
                    double percent = 0.10;
                    double maxValue = Convert.ToDouble(feeType.Percentage) * (1 + percent);
                    double minValue = Convert.ToDouble(feeType.Percentage) * (1 - percent);

                    double value = Convert.ToDouble(rnd.Next((int)minValue, (int)maxValue)) / 100;

                    mealExpenses -= airliner.CurrentFlight.getFlightAirlinerClass(aClass.Type).Passengers * value * airliner.Airliner.Airline.Fees.getValue(feeType);

                }
            }
                */
            
            double fdistance = MathHelpers.GetDistance(airliner.CurrentFlight.getDepartureAirport().Profile.Coordinates, airliner.CurrentPosition);

            double expenses = GameObject.GetInstance().FuelPrice * fdistance * airliner.CurrentFlight.getTotalPassengers() * airliner.Airliner.Type.FuelConsumption + dest.getLandingFee() + tax;

            airliner.Airliner.Airline.Statistics.addStatisticsValue(GameObject.GetInstance().GameTime.Year, StatisticsTypes.GetStatisticsType("Passengers"), airliner.CurrentFlight.getTotalPassengers());
            airliner.Statistics.addStatisticsValue(GameObject.GetInstance().GameTime.Year, StatisticsTypes.GetStatisticsType("Passengers"), airliner.CurrentFlight.getTotalPassengers());
            dest.Statistics.addStatisticsValue(GameObject.GetInstance().GameTime.Year, airliner.Airliner.Airline, StatisticsTypes.GetStatisticsType("Passengers"), airliner.CurrentFlight.getTotalPassengers());
            dest.Statistics.addStatisticsValue(GameObject.GetInstance().GameTime.Year, airliner.Airliner.Airline, StatisticsTypes.GetStatisticsType("Arrivals"), 1);

            if (airliner.CurrentFlight.IsOnTime) airliner.Airliner.Airline.Statistics.addStatisticsValue(GameObject.GetInstance().GameTime.Year, StatisticsTypes.GetStatisticsType("On-Time"), 1);
            airliner.Airliner.Airline.Statistics.addStatisticsValue(GameObject.GetInstance().GameTime.Year, StatisticsTypes.GetStatisticsType("Arrivals"), 1);

            double onTimePercent = airliner.Airliner.Airline.Statistics.getStatisticsValue(GameObject.GetInstance().GameTime.Year, StatisticsTypes.GetStatisticsType("On-Time")) / airliner.Airliner.Airline.Statistics.getStatisticsValue(GameObject.GetInstance().GameTime.Year, StatisticsTypes.GetStatisticsType("Arrivals"));
            airliner.Airliner.Airline.Statistics.setStatisticsValue(GameObject.GetInstance().GameTime.Year, StatisticsTypes.GetStatisticsType("On-Time%"), onTimePercent * 100);


            foreach (AirlinerClass aClass in airliner.Airliner.Classes)
            {
                RouteAirlinerClass raClass = airliner.CurrentFlight.Entry.TimeTable.Route.getRouteAirlinerClass(aClass.Type);

                airliner.CurrentFlight.Entry.TimeTable.Route.Statistics.addStatisticsValue(raClass, StatisticsTypes.GetStatisticsType("Passengers"), airliner.CurrentFlight.getFlightAirlinerClass(raClass.Type).Passengers);
                double routePassengers = airliner.CurrentFlight.Entry.TimeTable.Route.Statistics.getStatisticsValue(raClass, StatisticsTypes.GetStatisticsType("Passengers"));
                double routeDepartures = airliner.CurrentFlight.Entry.TimeTable.Route.Statistics.getStatisticsValue(raClass, StatisticsTypes.GetStatisticsType("Departures"));
                airliner.CurrentFlight.Entry.TimeTable.Route.Statistics.setStatisticsValue(raClass, StatisticsTypes.GetStatisticsType("Passengers%"), (int)(routePassengers / routeDepartures));

                airliner.CurrentFlight.Entry.TimeTable.Route.Statistics.addStatisticsValue(raClass, StatisticsTypes.GetStatisticsType("Capacity"), airliner.Airliner.getAirlinerClass(raClass.Type).SeatingCapacity);
            }

            double airlinerPassengers = airliner.Statistics.getStatisticsValue(GameObject.GetInstance().GameTime.Year, StatisticsTypes.GetStatisticsType("Passengers"));
            double airlinerDepartures = airliner.Statistics.getStatisticsValue(GameObject.GetInstance().GameTime.Year, StatisticsTypes.GetStatisticsType("Departures"));
            airliner.Statistics.setStatisticsValue(GameObject.GetInstance().GameTime.Year, StatisticsTypes.GetStatisticsType("Passengers%"), (int)(airlinerPassengers / airlinerDepartures));

            double destPassengers = dest.Statistics.getStatisticsValue(GameObject.GetInstance().GameTime.Year, airliner.Airliner.Airline, StatisticsTypes.GetStatisticsType("Passengers"));
            double destDepartures = dest.Statistics.getStatisticsValue(GameObject.GetInstance().GameTime.Year, airliner.Airliner.Airline, StatisticsTypes.GetStatisticsType("Arrivals"));
            dest.Statistics.setStatisticsValue(GameObject.GetInstance().GameTime.Year, airliner.Airliner.Airline, StatisticsTypes.GetStatisticsType("Passengers%"), (int)(destPassengers / destDepartures));

            double airlinePassengers = airliner.Airliner.Airline.Statistics.getStatisticsValue(GameObject.GetInstance().GameTime.Year, StatisticsTypes.GetStatisticsType("Passengers"));
            double airlineDepartures = airliner.Airliner.Airline.Statistics.getStatisticsValue(GameObject.GetInstance().GameTime.Year, StatisticsTypes.GetStatisticsType("Departures"));
            airliner.Airliner.Airline.Statistics.setStatisticsValue(GameObject.GetInstance().GameTime.Year, StatisticsTypes.GetStatisticsType("Passengers%"), (int)(airlinePassengers / airlineDepartures));

            //the statistics for destination airport
            dept.addDestinationStatistics(dest, airliner.CurrentFlight.getTotalPassengers());

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

            double wages = airliner.Airliner.Type.CockpitCrew * flighttime.TotalHours * airliner.Airliner.Airline.Fees.getValue(FeeTypes.GetType("Cockpit wage")) + airliner.CurrentFlight.Entry.TimeTable.Route.getTotalCabinCrew() * flighttime.TotalHours * airliner.Airliner.Airline.Fees.getValue(FeeTypes.GetType("Cabin wage"));// +(airliner.CurrentFlight.Entry.TimeTable.Route.getTotalCabinCrew() * airliner.Airliner.Airline.Fees.getValue(FeeTypes.GetType("Cabin kilometer rate")) * fdistance) + (airliner.Airliner.Type.CockpitCrew * airliner.Airliner.Airline.Fees.getValue(FeeTypes.GetType("Cockpit kilometer rate")) * fdistance);
            //wages
            AirlineHelpers.AddAirlineInvoice(airline, GameObject.GetInstance().GameTime, Invoice.InvoiceType.Wages, -wages);

            HolidayYearEvent holiday = HolidayYear.GetHoliday(airline.Profile.Country, GameObject.GetInstance().GameTime);

            if (holiday != null && (holiday.Holiday.Travel == Holiday.TravelType.Both || holiday.Holiday.Travel == Holiday.TravelType.Normal))
                wages = wages * 1.50;

            airliner.CurrentFlight.Entry.TimeTable.Route.addRouteInvoice(new Invoice(GameObject.GetInstance().GameTime, Invoice.InvoiceType.Wages, -wages));

          
            airliner.Statistics.addStatisticsValue(GameObject.GetInstance().GameTime.Year, StatisticsTypes.GetStatisticsType("Airliner_Income"), ticketsIncome - expenses - mealExpenses + feesIncome - wages);

            airliner.Airliner.Flown += fdistance;

            if (airliner.Airliner.Airline.IsHuman && Settings.GetInstance().MailsOnLandings)
                GameObject.GetInstance().NewsBox.addNews(new News(News.NewsType.Flight_News, GameObject.GetInstance().GameTime, string.Format("{0} landed", airliner.Name), string.Format("Your airliner {0} has landed in {1}, {2} with {3} passengers.\nThe airliner flow from {4}, {5}", new object[] { airliner.Name, dest.Profile.Name, dest.Profile.Country.Name, airliner.CurrentFlight.getTotalPassengers(), dept.Profile.Name, dept.Profile.Country.Name })));

            //updates the passengers
            foreach (AirlinerClass airlinerClass in airliner.Airliner.Classes)
            {
                FlightAirlinerClass faClass = airliner.CurrentFlight.getFlightAirlinerClass(airlinerClass.Type);

            }

            CreatePassengersHappiness(airliner);

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

            airliner.CurrentPosition = new Coordinates(airliner.CurrentFlight.Entry.Destination.Airport.Profile.Coordinates.Latitude, airliner.CurrentFlight.Entry.Destination.Airport.Profile.Coordinates.Longitude);
            airliner.Status = FleetAirliner.AirlinerStatus.To_route_start;

            double fdistance = MathHelpers.GetDistance(airliner.CurrentFlight.Entry.Destination.Airport.Profile.Coordinates, airliner.CurrentPosition);
            double expenses = GameObject.GetInstance().FuelPrice * fdistance * airliner.CurrentFlight.getTotalPassengers() * airliner.Airliner.Type.FuelConsumption + Airports.GetAirport(airliner.CurrentPosition).getLandingFee();

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
                airliner.CurrentFlight.Entry.Destination = new RouteEntryDestination(airliner.Homebase, "Service");

            }


        }
        //gets the weather for an airliner
        private static Weather GetAirlinerWeather(FleetAirliner airliner)
        {
            double distance = MathHelpers.GetDistance(airliner.CurrentPosition, airliner.CurrentFlight.Entry.Destination.Airport.Profile.Coordinates);
            Airport dest = airliner.CurrentFlight.Entry.Destination.Airport;
            Airport dept = airliner.CurrentFlight.getDepartureAirport();

            double totalDistance = MathHelpers.GetDistance(dept.Profile.Coordinates, dest.Profile.Coordinates);

            Weather tWeather = new Weather();
            tWeather.WindSpeed = Weather.eWindSpeed.Calm;
            tWeather.Direction = Weather.WindDirection.Head;

            return distance > totalDistance / 2 ? dept.Weather : dest.Weather;
        }
        //sets the next flight for a route airliner
        private static void SetNextFlight(FleetAirliner airliner)
        {

            Route route = GetNextRoute(airliner);
            airliner.CurrentFlight = new Flight(route.TimeTable.getNextEntry(GameObject.GetInstance().GameTime, airliner));
            airliner.Status = FleetAirliner.AirlinerStatus.To_route_start;

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

            var entries = from e in airliner.Routes.Select(r => r.TimeTable.getNextEntry(GameObject.GetInstance().GameTime, airliner)) orderby MathHelpers.ConvertEntryToDate(e) select e;

            return entries.FirstOrDefault().TimeTable.Route;

        }
    }
}
