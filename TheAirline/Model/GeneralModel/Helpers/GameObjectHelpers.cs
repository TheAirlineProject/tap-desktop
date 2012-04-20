using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using TheAirline.Model.AirlinerModel.RouteModel;
using TheAirline.Model.AirlinerModel;
using TheAirline.Model.AirlineModel;
using TheAirline.Model.AirportModel;
using TheAirline.Model.GeneralModel.StatisticsModel;

namespace TheAirline.Model.GeneralModel.Helpers
{
    //the helper class for the game object
    public class GameObjectHelpers
    {
        private static Random rnd = new Random();
        //simulates a "turn" (one hour) now 1/4 hour
        public static void SimulateTurn()
        {
            GameObject.GetInstance().GameTime = GameObject.GetInstance().GameTime.AddMinutes(15);

            if (MathHelpers.IsNewDay(GameObject.GetInstance().GameTime)) DoDailyUpdate();

            if (MathHelpers.IsNewMonth(GameObject.GetInstance().GameTime)) DoMonthlyUpdate();

            if (MathHelpers.IsNewYear(GameObject.GetInstance().GameTime)) DoYearlyUpdate();

            foreach (Airline airline in Airlines.GetAirlines())
            {
                if (airline != GameObject.GetInstance().HumanAirline)
                    AIHelpers.UpdateCPUAirline(airline);
                foreach (FleetAirliner airliner in airline.Fleet)
                {
                    UpdateAirliner(airliner);
                }

            }
        }
        //do the daily update
        private static void DoDailyUpdate()
        {
            foreach (Airport airport in Airports.GetAirports())
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
                            GameObject.GetInstance().NewsBox.addNews(new News(News.NewsType.Airport_News, GameObject.GetInstance().GameTime, "Construction of terminal", string.Format("Your terminal at {0}, {1} is now finished and ready for use.", airport.Profile.Name,airport.Profile.Country.Name)));

                        //moves the "old" rented gates into the new terminal
                        foreach (Terminal tTerminal in airport.Terminals.getTerminals().FindAll((delegate(Terminal t) { return t.Airline == null; })))
                        {
                            foreach (Gate gate in tTerminal.Gates.getGates(terminal.Airline))
                            {
                                Gate nGate = terminal.Gates.getEmptyGate(terminal.Airline);
                                if (nGate != null)
                                {
                                    nGate.Route = gate.Route;

                                    gate.Airline = null;
                                    gate.Route = null;
                                }

                                
                            }
                          
                        }
                     
                        

                    }
                }
                

            }
            foreach (FleetAirliner airliner in GameObject.GetInstance().HumanAirline.Fleet.FindAll((delegate(FleetAirliner a) { return a.Airliner.BuiltDate == GameObject.GetInstance().GameTime && a.Purchased != FleetAirliner.PurchasedType.BoughtDownPayment; })))
                GameObject.GetInstance().NewsBox.addNews(new News(News.NewsType.Fleet_News, GameObject.GetInstance().GameTime, "Delivery of airliner", string.Format("Your new airliner {0} as been delivered to your fleet.\nThe airliner is currently at {1}, {2}.", airliner.Name, airliner.Homebase.Profile.Name, airliner.Homebase.Profile.Country.Name)));
            foreach (Airline airline in Airlines.GetAirlines())
                foreach (FleetAirliner airliner in airline.Fleet.FindAll(a=> a.Airliner.BuiltDate == GameObject.GetInstance().GameTime && a.Purchased == FleetAirliner.PurchasedType.BoughtDownPayment))
                {
                    airliner.Purchased = FleetAirliner.PurchasedType.Bought;
                    if (airline.Money >= airliner.Airliner.Type.Price)
                    {
                        airline.addInvoice(new Invoice(GameObject.GetInstance().GameTime, Invoice.InvoiceType.Purchases, -airliner.Airliner.Type.Price));
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
            foreach (Airline airline in Airlines.GetAirlines())
            {
                foreach (FleetAirliner airliner in airline.Fleet)
                    airline.addInvoice(new Invoice(GameObject.GetInstance().GameTime, Invoice.InvoiceType.Maintenances, -airliner.Airliner.Type.getMaintenance()));
            }
        }
        //do the monthly update
        private static void DoMonthlyUpdate()
        {
            foreach (Airline airline in Airlines.GetAirlines())
            {
                foreach (AirlineFacility facility in airline.Facilities)
                    airline.addInvoice(new Invoice(GameObject.GetInstance().GameTime, Invoice.InvoiceType.Airline_Expenses, -facility.MonthlyCost));
                foreach (FleetAirliner airliner in airline.Fleet.FindAll((delegate(FleetAirliner a) { return a.Purchased == FleetAirliner.PurchasedType.Leased; })))
                    airline.addInvoice(new Invoice(GameObject.GetInstance().GameTime, Invoice.InvoiceType.Rents, -airliner.Airliner.LeasingPrice));

                // chs, 2011-28-10 changed so a terminal only costs 75% of gate price
                foreach (Airport airport in airline.Airports)
                {
                    foreach (Terminal terminal in airport.Terminals.getDeliveredTerminals())
                    {
                        double gates = Convert.ToDouble(terminal.Gates.getNumberOfGates(airline));
                        double gatePrice = airline == terminal.Airline ? airport.getGatePrice() * 0.75 : airport.getGatePrice();

                        airline.addInvoice(new Invoice(GameObject.GetInstance().GameTime, Invoice.InvoiceType.Rents, -gatePrice * gates));

                    }
                }
                foreach (Loan loan in airline.Loans)
                {
                    if (loan.IsActive)
                    {

                        double amount = Math.Min(loan.getMonthlyPayment(), loan.PaymentLeft);

                        airline.addInvoice(new Invoice(GameObject.GetInstance().GameTime, Invoice.InvoiceType.Loans, -amount));

                        loan.PaymentLeft -= amount;

                    }

                }
                // chs, 2011-17-10 change so it only looks at the advertisement types which are invented at the time
                foreach (AdvertisementType.AirlineAdvertisementType type in Enum.GetValues(typeof(AdvertisementType.AirlineAdvertisementType)))
                {
                    if (GameObject.GetInstance().GameTime.Year >= (int)type)
                    {
                        airline.addInvoice(new Invoice(GameObject.GetInstance().GameTime, Invoice.InvoiceType.Airline_Expenses, -airline.getAirlineAdvertisement(type).Price));
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
                        UpdateOnRouteAirliner(airliner);
                        break;

                    case FleetAirliner.AirlinerStatus.Resting:
                        DateTime newFlightTime = GetNextFlightTime(airliner);
                        if (newFlightTime <= GameObject.GetInstance().GameTime)
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
                //airliner.CurrentFlight = new Flight(airliner.Route.TimeTable.getNextEntry(GameObject.GetInstance().GameTime, airliner.CurrentPosition));
            }
            double adistance = MathHelpers.GetDistance(airliner.CurrentPosition, airliner.CurrentFlight.Entry.Destination.Airport.Profile.Coordinates);

            double speed = airliner.Airliner.Type.CruisingSpeed / 4;
            if (airliner.CurrentFlight != null)
            {
                Weather currentWeather = GetAirlinerWeather(airliner);
                int wind = currentWeather.Direction == Weather.WindDirection.Tail ? (int)currentWeather.WindSpeed / 4 : -(int)currentWeather.WindSpeed / 4;
                speed = airliner.Airliner.Type.CruisingSpeed / 4 + wind;

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
                else if (airliner.Status == FleetAirliner.AirlinerStatus.To_route_start)
                    SimulateRouteStart(airliner);
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
            //        airliner.CurrentFlight.Entry.Destination.Airport = airliner.Airliner.Homebase;
        }
        //simulates the start for routing
        private static void SimulateRouteStart(FleetAirliner airliner)
        {

            Route route = GetNextRoute(airliner);
            airliner.CurrentFlight = new Flight(route.TimeTable.getNextEntry(GameObject.GetInstance().GameTime, airliner.CurrentPosition));

            airliner.Status = FleetAirliner.AirlinerStatus.Resting;

        }
        //simulates a route airliner taking off
        private static void SimulateTakeOff(FleetAirliner airliner)
        {


            airliner.Status = FleetAirliner.AirlinerStatus.On_route;

            foreach (AirlinerClass aClass in airliner.Airliner.Classes)
            {
                if (airliner.CurrentFlight.Entry.TimeTable.Route.containsDestination(Airports.GetAirport(airliner.CurrentPosition)))
                    airliner.CurrentFlight.Classes.Add(new FlightAirlinerClass(airliner.CurrentFlight.Entry.TimeTable.Route.getRouteAirlinerClass(aClass.Type), GetPassengers(airliner, aClass.Type)));
                else
                    airliner.CurrentFlight.Classes.Add(new FlightAirlinerClass(airliner.CurrentFlight.Entry.TimeTable.Route.getRouteAirlinerClass(aClass.Type), 0));
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
            DateTime date = GameObject.GetInstance().GameTime;


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
            }


            double fdistance = MathHelpers.GetDistance(airliner.CurrentFlight.getDepartureAirport().Profile.Coordinates, airliner.CurrentPosition);

            double expenses = GameObject.GetInstance().FuelPrice * fdistance * airliner.CurrentFlight.getTotalPassengers() * airliner.Airliner.Type.FuelConsumption + Airports.GetAirport(airliner.CurrentPosition).getLandingFee() + tax;

            airliner.Airliner.Airline.Statistics.addStatisticsValue(GameObject.GetInstance().GameTime.Year, StatisticsTypes.GetStatisticsType("Passengers"), airliner.CurrentFlight.getTotalPassengers());
            airliner.Statistics.addStatisticsValue(GameObject.GetInstance().GameTime.Year, StatisticsTypes.GetStatisticsType("Passengers"), airliner.CurrentFlight.getTotalPassengers());
            dest.Statistics.addStatisticsValue(GameObject.GetInstance().GameTime.Year, airliner.Airliner.Airline, StatisticsTypes.GetStatisticsType("Passengers"), airliner.CurrentFlight.getTotalPassengers());
            dest.Statistics.addStatisticsValue(GameObject.GetInstance().GameTime.Year, airliner.Airliner.Airline, StatisticsTypes.GetStatisticsType("Arrivals"), 1);

            if (airliner.IsOnTime) airliner.Airliner.Airline.Statistics.addStatisticsValue(GameObject.GetInstance().GameTime.Year,StatisticsTypes.GetStatisticsType("On-Time"),1);
            airliner.Airliner.Airline.Statistics.addStatisticsValue(GameObject.GetInstance().GameTime.Year, StatisticsTypes.GetStatisticsType("Arrivals"),1);
          
            double onTimePercent = airliner.Airliner.Airline.Statistics.getStatisticsValue(GameObject.GetInstance().GameTime.Year, StatisticsTypes.GetStatisticsType("On-Time")) / airliner.Airliner.Airline.Statistics.getStatisticsValue(GameObject.GetInstance().GameTime.Year, StatisticsTypes.GetStatisticsType("Arrivals"));
            airliner.Airliner.Airline.Statistics.setStatisticsValue(GameObject.GetInstance().GameTime.Year, StatisticsTypes.GetStatisticsType("On-Time%"), onTimePercent*100);
          

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

            airliner.Airliner.Airline.addInvoice(new Invoice(GameObject.GetInstance().GameTime, Invoice.InvoiceType.Flight_Expenses, -expenses));
            airliner.Airliner.Airline.addInvoice(new Invoice(GameObject.GetInstance().GameTime, Invoice.InvoiceType.Tickets, ticketsIncome));
            airliner.Airliner.Airline.addInvoice(new Invoice(GameObject.GetInstance().GameTime, Invoice.InvoiceType.OnFlight_Income, -mealExpenses));
            airliner.Airliner.Airline.addInvoice(new Invoice(GameObject.GetInstance().GameTime, Invoice.InvoiceType.Fees, feesIncome));

            airliner.CurrentFlight.Entry.TimeTable.Route.addRouteInvoice(new Invoice(GameObject.GetInstance().GameTime, Invoice.InvoiceType.Flight_Expenses, -expenses));
            airliner.CurrentFlight.Entry.TimeTable.Route.addRouteInvoice(new Invoice(GameObject.GetInstance().GameTime, Invoice.InvoiceType.Tickets, ticketsIncome));
            airliner.CurrentFlight.Entry.TimeTable.Route.addRouteInvoice(new Invoice(GameObject.GetInstance().GameTime, Invoice.InvoiceType.OnFlight_Income, -mealExpenses));
            airliner.CurrentFlight.Entry.TimeTable.Route.addRouteInvoice(new Invoice(GameObject.GetInstance().GameTime, Invoice.InvoiceType.Fees, feesIncome));

            //wages
            airliner.Airliner.Airline.addInvoice(new Invoice(GameObject.GetInstance().GameTime, Invoice.InvoiceType.Wages, -airliner.Airliner.Type.CockpitCrew * airliner.Airliner.Airline.Fees.getValue(FeeTypes.GetType("Cockpit wage"))));
            airliner.Airliner.Airline.addInvoice(new Invoice(GameObject.GetInstance().GameTime, Invoice.InvoiceType.Wages, -airliner.CurrentFlight.Entry.TimeTable.Route.getTotalCabinCrew() * airliner.Airliner.Airline.Fees.getValue(FeeTypes.GetType("Cabin wage"))));
            airliner.Airliner.Airline.addInvoice(new Invoice(GameObject.GetInstance().GameTime, Invoice.InvoiceType.Wages, -(airliner.CurrentFlight.Entry.TimeTable.Route.getTotalCabinCrew()) * airliner.Airliner.Airline.Fees.getValue(FeeTypes.GetType("Cabin kilometer rate")) * fdistance));
            airliner.Airliner.Airline.addInvoice(new Invoice(GameObject.GetInstance().GameTime, Invoice.InvoiceType.Wages, -(airliner.Airliner.Type.CockpitCrew) * airliner.Airliner.Airline.Fees.getValue(FeeTypes.GetType("Cockpit kilometer rate")) * fdistance));


            airliner.CurrentFlight.Entry.TimeTable.Route.addRouteInvoice(new Invoice(GameObject.GetInstance().GameTime, Invoice.InvoiceType.Wages, -airliner.Airliner.Type.CockpitCrew * airliner.Airliner.Airline.Fees.getValue(FeeTypes.GetType("Cockpit wage"))));
            airliner.CurrentFlight.Entry.TimeTable.Route.addRouteInvoice(new Invoice(GameObject.GetInstance().GameTime, Invoice.InvoiceType.Wages, -airliner.CurrentFlight.Entry.TimeTable.Route.getTotalCabinCrew() * airliner.Airliner.Airline.Fees.getValue(FeeTypes.GetType("Cabin wage"))));
            airliner.CurrentFlight.Entry.TimeTable.Route.addRouteInvoice(new Invoice(GameObject.GetInstance().GameTime, Invoice.InvoiceType.Wages, -(airliner.CurrentFlight.Entry.TimeTable.Route.getTotalCabinCrew()) * airliner.Airliner.Airline.Fees.getValue(FeeTypes.GetType("Cabin kilometer rate")) * fdistance));
            airliner.CurrentFlight.Entry.TimeTable.Route.addRouteInvoice(new Invoice(GameObject.GetInstance().GameTime, Invoice.InvoiceType.Wages, -(airliner.Airliner.Type.CockpitCrew) * airliner.Airliner.Airline.Fees.getValue(FeeTypes.GetType("Cockpit kilometer rate")) * fdistance));

            double wages = airliner.Airliner.Type.CockpitCrew * airliner.Airliner.Airline.Fees.getValue(FeeTypes.GetType("Cockpit wage")) + airliner.CurrentFlight.Entry.TimeTable.Route.getTotalCabinCrew() * airliner.Airliner.Airline.Fees.getValue(FeeTypes.GetType("Cabin wage")) + (airliner.CurrentFlight.Entry.TimeTable.Route.getTotalCabinCrew() * airliner.Airliner.Airline.Fees.getValue(FeeTypes.GetType("Cabin kilometer rate")) * fdistance) +(airliner.Airliner.Type.CockpitCrew * airliner.Airliner.Airline.Fees.getValue(FeeTypes.GetType("Cockpit kilometer rate")) * fdistance);

            airliner.Statistics.addStatisticsValue(GameObject.GetInstance().GameTime.Year, StatisticsTypes.GetStatisticsType("Airliner_Income"), ticketsIncome - expenses - mealExpenses + feesIncome - wages);
  
            airliner.Airliner.Flown += fdistance;

            if (airliner.Airliner.Airline.IsHuman && Settings.GetInstance().MailsOnLandings)
                GameObject.GetInstance().NewsBox.addNews(new News(News.NewsType.Flight_News, GameObject.GetInstance().GameTime, string.Format("{0} landed", airliner.Name), string.Format("Your airliner {0} has landed in {1}, {2} with {3} passengers.\nThe airliner flow from {4}, {5}", new object[] { airliner.Name, dest.Profile.Name, dest.Profile.Country.Name, airliner.CurrentFlight.getTotalPassengers(), dept.Profile.Name, dept.Profile.Country.Name })));

            CreatePassengersHappiness(airliner);

            SetNextFlight(airliner);

            CheckForService(airliner);


        }
        //creates the happiness for a landed route airliner
        private static void CreatePassengersHappiness(FleetAirliner airliner)
        {
            int serviceLevel = 0;//airliner.Route.DrinksFacility.ServiceLevel + airliner.Route.FoodFacility.ServiceLevel + airliner.Airliner.Airliner.getFacility(AirlinerFacility.FacilityType.Audio).ServiceLevel + airliner.Airliner.Airliner.getFacility(AirlinerFacility.FacilityType.Seat).ServiceLevel + airliner.Airliner.Airliner.getFacility(AirlinerFacility.FacilityType.Video).ServiceLevel;
            int happyValue = airliner.CurrentFlight.Delayed ? 20 : 10;
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

            airliner.Airliner.Airline.addInvoice(new Invoice(GameObject.GetInstance().GameTime, Invoice.InvoiceType.Maintenances, -servicePrice));

            airliner.CurrentFlight.Entry.TimeTable.Route.addRouteInvoice(new Invoice(GameObject.GetInstance().GameTime, Invoice.InvoiceType.Maintenances, -servicePrice));
            airliner.Statistics.addStatisticsValue(GameObject.GetInstance().GameTime.Year, StatisticsTypes.GetStatisticsType("Airliner_Income"), -servicePrice);
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

            RouteTimeTableEntry currentEntry = airliner.CurrentFlight.Entry;
            RouteTimeTableEntry entry = currentEntry.TimeTable.getNextEntry(currentEntry);

            airliner.CurrentFlight = null;
            
            entry.Airliner.CurrentFlight = new Flight(entry);


        }

        //finds the next flight time for an airliner - checks also for delay
        private static DateTime GetNextFlightTime(FleetAirliner airliner)
        {

            RouteTimeTableEntry entry = airliner.CurrentFlight.Entry;

            return MathHelpers.ConvertEntryToDate(entry);

        }
        //returns the passengers for an airliner
        public static int GetPassengers(FleetAirliner airliner, AirlinerClass.ClassType type)
        {


            return PassengerHelpers.GetFlightPassengers(airliner, type);
        }
        //returns the next route for an airliner 
        private static Route GetNextRoute(FleetAirliner airliner)
        {
            var entries = from e in airliner.Routes.Select(r=>r.TimeTable.getNextEntry(GameObject.GetInstance().GameTime,airliner.CurrentPosition)) orderby MathHelpers.ConvertEntryToDate(e) select e;
            
            return entries.FirstOrDefault().TimeTable.Route;          
          
        }
    }
}
