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
                // if (airline != Game.GetInstance().HumanAirline)
                //   AIHelpers.UpdateCPUAirline(airline);
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

            }
            foreach (FleetAirliner airliner in GameObject.GetInstance().HumanAirline.Fleet.FindAll((delegate(FleetAirliner a) { return a.Airliner.BuiltDate == GameObject.GetInstance().GameTime && a.Purchased != FleetAirliner.PurchasedType.BoughtDownPayment; })))
                GameObject.GetInstance().NewsBox.addNews(new News(News.NewsType.Fleet_News, GameObject.GetInstance().GameTime, "Delivery of airliner", string.Format("Your new airliner {0} as been delivered to your fleet.\nThe airliner is currently at {1}, {2}.", airliner.Name, airliner.Homebase.Profile.Name, airliner.Homebase.Profile.Country.Name)));
            foreach (Airline airline in Airlines.GetAirlines())
                foreach (FleetAirliner airliner in airline.Fleet.FindAll((delegate(FleetAirliner a) { return a.Airliner.BuiltDate == GameObject.GetInstance().GameTime && a.Purchased == FleetAirliner.PurchasedType.BoughtDownPayment; })))
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
                foreach (FleetAirliner airliner in airline.Fleet.FindAll((delegate(FleetAirliner a) { return a.Purchased == FleetAirliner.PurchasedType.Leased; })))
                    airline.addInvoice(new Invoice(GameObject.GetInstance().GameTime, Invoice.InvoiceType.Rents, -airliner.Airliner.LeasingPrice));

                foreach (Airport airport in airline.Airports)
                {
                    int gates = airport.Gates.getNumberOfGates(airline);
                    airline.addInvoice(new Invoice(GameObject.GetInstance().GameTime, Invoice.InvoiceType.Rents, -airport.getGatePrice() * gates));
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
            if (airliner.RouteAirliner != null)
            {
                switch (airliner.RouteAirliner.Status)
                {
                    case RouteAirliner.AirlinerStatus.On_route:
                        UpdateOnRouteAirliner(airliner.RouteAirliner);
                        break;
                    case RouteAirliner.AirlinerStatus.On_service:
                        UpdateOnRouteAirliner(airliner.RouteAirliner);
                        break;
                    case RouteAirliner.AirlinerStatus.To_homebase:
                        UpdateOnRouteAirliner(airliner.RouteAirliner);
                        break;
                    case RouteAirliner.AirlinerStatus.To_route_start:
                        UpdateOnRouteAirliner(airliner.RouteAirliner);
                        break;

                    case RouteAirliner.AirlinerStatus.Resting:
                        DateTime newFlightTime = GetNextFlightTime(airliner.RouteAirliner);
                        if (newFlightTime <= GameObject.GetInstance().GameTime)
                            SimulateTakeOff(airliner.RouteAirliner);
                        break;
                }
            }


        }


        //the method for updating a route airliner
        private static void UpdateOnRouteAirliner(RouteAirliner airliner)
        {
            if (airliner.CurrentFlight == null)
            {
                Airport a = Airports.GetAirport(airliner.CurrentPosition);
                airliner.CurrentFlight = new Flight(airliner.Route.TimeTable.getNextEntry(GameObject.GetInstance().GameTime, airliner.CurrentPosition));
            }
            double adistance = MathHelpers.GetDistance(airliner.CurrentPosition, airliner.CurrentFlight.Entry.Destination.Airport.Profile.Coordinates);

            double speed = airliner.Airliner.Airliner.Type.CruisingSpeed / 4;
            if (airliner.CurrentFlight != null)
            {
                Weather currentWeather = GetAirlinerWeather(airliner);
                int wind = currentWeather.Direction == Weather.WindDirection.Tail ? (int)currentWeather.WindSpeed / 4 : -(int)currentWeather.WindSpeed / 4;
                speed = airliner.Airliner.Airliner.Type.CruisingSpeed / 4 + wind;

            }
            if (adistance > 4)
                MathHelpers.MoveObject(airliner.CurrentPosition, airliner.CurrentFlight.Entry.Destination.Airport.Profile.Coordinates, Math.Min(speed, MathHelpers.GetDistance(airliner.CurrentPosition, airliner.CurrentFlight.Entry.Destination.Airport.Profile.Coordinates)));

            double distance = MathHelpers.GetDistance(airliner.CurrentPosition, airliner.CurrentFlight.Entry.Destination.Airport.Profile.Coordinates);

            if (MathHelpers.GetDistance(airliner.CurrentPosition, airliner.CurrentFlight.Entry.Destination.Airport.Profile.Coordinates) < 5)
            {
                if (airliner.Status == RouteAirliner.AirlinerStatus.On_route)
                    SimulateLanding(airliner);
                else if (airliner.Status == RouteAirliner.AirlinerStatus.On_service)
                    SimulateService(airliner);
                else if (airliner.Status == RouteAirliner.AirlinerStatus.To_homebase)
                    SimulateToHomebase(airliner);
                else if (airliner.Status == RouteAirliner.AirlinerStatus.To_route_start)
                    SimulateRouteStart(airliner);
            }


        }

        //simulates a route airliner going to homebase
        private static void SimulateToHomebase(RouteAirliner airliner)
        {
            airliner.CurrentPosition = new Coordinates(airliner.CurrentFlight.Entry.Destination.Airport.Profile.Coordinates.Latitude, airliner.CurrentFlight.Entry.Destination.Airport.Profile.Coordinates.Longitude);
            Airport airport = Airports.GetAirport(airliner.CurrentPosition);
            airliner.Status = RouteAirliner.AirlinerStatus.To_homebase;

            if (airliner.CurrentFlight.Entry.Destination.Airport.Profile.Coordinates.CompareTo(airliner.Airliner.Homebase.Profile.Coordinates) == 0)
                airliner.Status = RouteAirliner.AirlinerStatus.Stopped;
            else
                airliner.CurrentFlight = new Flight(new RouteTimeTableEntry(airliner.CurrentFlight.Entry.TimeTable, GameObject.GetInstance().GameTime.DayOfWeek, GameObject.GetInstance().GameTime.TimeOfDay, new RouteEntryDestination(airliner.Airliner.Homebase, "Service")));
            //        airliner.CurrentFlight.Entry.Destination.Airport = airliner.Airliner.Homebase;
        }
        //simulates the start for routing
        private static void SimulateRouteStart(RouteAirliner airliner)
        {

        
            airliner.CurrentFlight = new Flight(airliner.Route.TimeTable.getNextEntry(GameObject.GetInstance().GameTime, airliner.CurrentPosition));

            airliner.Status = RouteAirliner.AirlinerStatus.Resting;

        }
        //simulates a route airliner taking off
        private static void SimulateTakeOff(RouteAirliner airliner)
        {


            airliner.Status = RouteAirliner.AirlinerStatus.On_route;

            foreach (AirlinerClass aClass in airliner.Airliner.Airliner.Classes)
            {
                if (airliner.Route.containsDestination(Airports.GetAirport(airliner.CurrentPosition)))
                    airliner.CurrentFlight.Classes.Add(new FlightAirlinerClass(airliner.Route.getRouteAirlinerClass(aClass.Type), GetPassengers(airliner, aClass.Type)));
                else
                    airliner.CurrentFlight.Classes.Add(new FlightAirlinerClass(airliner.Route.getRouteAirlinerClass(aClass.Type), 0));
            }

            Airport airport = Airports.GetAirport(airliner.CurrentPosition);

            if (airport != null)
            {
                airport.Statistics.addStatisticsValue(airliner.Airliner.Airline, StatisticsTypes.GetStatisticsType("Departures"), 1);

                double destPassengers = airport.Statistics.getStatisticsValue(airliner.Airliner.Airline, StatisticsTypes.GetStatisticsType("Passengers"));
                double destDepartures = airport.Statistics.getStatisticsValue(airliner.Airliner.Airline, StatisticsTypes.GetStatisticsType("Arrivals"));
                airport.Statistics.setStatisticsValue(airliner.Airliner.Airline, StatisticsTypes.GetStatisticsType("Passengers%"), (int)(destPassengers / destDepartures));

            }
            airliner.Airliner.Airline.Statistics.addStatisticsValue(StatisticsTypes.GetStatisticsType("Departures"), 1);
            airliner.Airliner.Statistics.addStatisticsValue(StatisticsTypes.GetStatisticsType("Departures"), 1);

            foreach (AirlinerClass aClass in airliner.Airliner.Airliner.Classes)
            {
                RouteAirlinerClass raClass = airliner.Route.getRouteAirlinerClass(aClass.Type);

                airliner.Route.Statistics.addStatisticsValue(raClass, StatisticsTypes.GetStatisticsType("Departures"), 1);
            }
            double airlinerPassengers = airliner.Airliner.Statistics.getStatisticsValue(StatisticsTypes.GetStatisticsType("Passengers"));
            double airlinerDepartures = airliner.Airliner.Statistics.getStatisticsValue(StatisticsTypes.GetStatisticsType("Departures"));
            airliner.Airliner.Statistics.setStatisticsValue(StatisticsTypes.GetStatisticsType("Passengers%"), (int)(airlinerPassengers / airlinerDepartures));


            double airlinePassengers = airliner.Airliner.Airline.Statistics.getStatisticsValue(StatisticsTypes.GetStatisticsType("Passengers"));
            double airlineDepartures = airliner.Airliner.Airline.Statistics.getStatisticsValue(StatisticsTypes.GetStatisticsType("Departures"));
            airliner.Airliner.Airline.Statistics.setStatisticsValue(StatisticsTypes.GetStatisticsType("Passengers%"), (int)(airlinePassengers / airlineDepartures));

         }
        //simulates a route airliner landing
        private static void SimulateLanding(RouteAirliner airliner)
        {
            DateTime date = GameObject.GetInstance().GameTime;


            airliner.CurrentPosition = new Coordinates(airliner.CurrentFlight.Entry.Destination.Airport.Profile.Coordinates.Latitude, airliner.CurrentFlight.Entry.Destination.Airport.Profile.Coordinates.Longitude);
            airliner.Status = RouteAirliner.AirlinerStatus.Resting;
     
            double groundTaxPerPassenger = 5;

            double tax = groundTaxPerPassenger * airliner.CurrentFlight.getTotalPassengers();

            if (airliner.CurrentFlight.Entry.Destination.Airport.Profile.Country.Name != airliner.getDepartureAirport().Profile.Country.Name)
                tax = 2 * tax;

            double ticketsIncome = 0;

            foreach (AirlinerClass aClass in airliner.Airliner.Airliner.Classes)
                ticketsIncome += airliner.CurrentFlight.getFlightAirlinerClass(aClass.Type).Passengers * airliner.Route.getRouteAirlinerClass(aClass.Type).FarePrice;

            Airport dest = Airports.GetAirport(airliner.CurrentPosition);
            Airport dept = airliner.getDepartureAirport();



            double dist = MathHelpers.GetDistance(dest.Profile.Coordinates, dept.Profile.Coordinates);

            double feesIncome = 0;
            foreach (FeeType feeType in FeeTypes.GetTypes(FeeType.eFeeType.Fee))
            {
                foreach (AirlinerClass aClass in airliner.Airliner.Airliner.Classes)
                {
                    double percent = 0.10;
                    double maxValue = Convert.ToDouble(feeType.Percentage) * (1 + percent);
                    double minValue = Convert.ToDouble(feeType.Percentage) * (1 - percent);

                    double value = Convert.ToDouble(rnd.Next((int)minValue, (int)maxValue)) / 100;

                    feesIncome += airliner.CurrentFlight.getFlightAirlinerClass(aClass.Type).Passengers * value * airliner.Airliner.Airline.Fees.getValue(feeType);
                }
            }

            double mealExpenses = 0;
            foreach (AirlinerClass aClass in airliner.Airliner.Airliner.Classes)
            {
                if (airliner.Route.getRouteAirlinerClass(aClass.Type).FoodFacility.EType == RouteFacility.ExpenseType.Fixed)
                {
                    mealExpenses += airliner.CurrentFlight.getFlightAirlinerClass(aClass.Type).Passengers * airliner.Route.getRouteAirlinerClass(aClass.Type).FoodFacility.ExpensePerPassenger;
                }
                else
                {
                    FeeType feeType = airliner.Route.getRouteAirlinerClass(aClass.Type).FoodFacility.FeeType;
                    double percent = 0.10;
                    double maxValue = Convert.ToDouble(feeType.Percentage) * (1 + percent);
                    double minValue = Convert.ToDouble(feeType.Percentage) * (1 - percent);

                    double value = Convert.ToDouble(rnd.Next((int)minValue, (int)maxValue)) / 100;

                    mealExpenses -= airliner.CurrentFlight.getFlightAirlinerClass(aClass.Type).Passengers * value * airliner.Airliner.Airline.Fees.getValue(feeType);
                }
                if (airliner.Route.getRouteAirlinerClass(aClass.Type).DrinksFacility.EType == RouteFacility.ExpenseType.Fixed)
                {
                    mealExpenses += airliner.CurrentFlight.getFlightAirlinerClass(aClass.Type).Passengers * airliner.Route.getRouteAirlinerClass(aClass.Type).DrinksFacility.ExpensePerPassenger;
                }
                else
                {
                    FeeType feeType = airliner.Route.getRouteAirlinerClass(aClass.Type).DrinksFacility.FeeType;
                    double percent = 0.10;
                    double maxValue = Convert.ToDouble(feeType.Percentage) * (1 + percent);
                    double minValue = Convert.ToDouble(feeType.Percentage) * (1 - percent);

                    double value = Convert.ToDouble(rnd.Next((int)minValue, (int)maxValue)) / 100;

                    mealExpenses -= airliner.CurrentFlight.getFlightAirlinerClass(aClass.Type).Passengers * value * airliner.Airliner.Airline.Fees.getValue(feeType);
                }
            }


            double fdistance = MathHelpers.GetDistance(airliner.getDepartureAirport().Profile.Coordinates, airliner.CurrentPosition);

            double expenses = GameObject.GetInstance().FuelPrice * fdistance * airliner.CurrentFlight.getTotalPassengers() * airliner.Airliner.Airliner.Type.FuelConsumption + Airports.GetAirport(airliner.CurrentPosition).getLandingFee() + tax;

            airliner.Airliner.Airline.Statistics.addStatisticsValue(StatisticsTypes.GetStatisticsType("Passengers"), airliner.CurrentFlight.getTotalPassengers());
            airliner.Airliner.Statistics.addStatisticsValue(StatisticsTypes.GetStatisticsType("Passengers"), airliner.CurrentFlight.getTotalPassengers());
            dest.Statistics.addStatisticsValue(airliner.Airliner.Airline, StatisticsTypes.GetStatisticsType("Passengers"), airliner.CurrentFlight.getTotalPassengers());
            dest.Statistics.addStatisticsValue(airliner.Airliner.Airline, StatisticsTypes.GetStatisticsType("Arrivals"), 1);

            foreach (AirlinerClass aClass in airliner.Airliner.Airliner.Classes)
            {
                RouteAirlinerClass raClass = airliner.Route.getRouteAirlinerClass(aClass.Type);

                airliner.Route.Statistics.addStatisticsValue(raClass, StatisticsTypes.GetStatisticsType("Passengers"), airliner.CurrentFlight.getTotalPassengers());
                double routePassengers = airliner.Route.Statistics.getStatisticsValue(raClass, StatisticsTypes.GetStatisticsType("Passengers"));
                double routeDepartures = airliner.Route.Statistics.getStatisticsValue(raClass, StatisticsTypes.GetStatisticsType("Departures"));
                airliner.Route.Statistics.setStatisticsValue(raClass, StatisticsTypes.GetStatisticsType("Passengers%"), (int)(routePassengers / routeDepartures));
            }

            double airlinerPassengers = airliner.Airliner.Statistics.getStatisticsValue(StatisticsTypes.GetStatisticsType("Passengers"));
            double airlinerDepartures = airliner.Airliner.Statistics.getStatisticsValue(StatisticsTypes.GetStatisticsType("Departures"));
            airliner.Airliner.Statistics.setStatisticsValue(StatisticsTypes.GetStatisticsType("Passengers%"), (int)(airlinerPassengers / airlinerDepartures));

            double destPassengers = dest.Statistics.getStatisticsValue(airliner.Airliner.Airline, StatisticsTypes.GetStatisticsType("Passengers"));
            double destDepartures = dest.Statistics.getStatisticsValue(airliner.Airliner.Airline, StatisticsTypes.GetStatisticsType("Arrivals"));
            dest.Statistics.setStatisticsValue(airliner.Airliner.Airline, StatisticsTypes.GetStatisticsType("Passengers%"), (int)(destPassengers / destDepartures));

            double airlinePassengers = airliner.Airliner.Airline.Statistics.getStatisticsValue(StatisticsTypes.GetStatisticsType("Passengers"));
            double airlineDepartures = airliner.Airliner.Airline.Statistics.getStatisticsValue(StatisticsTypes.GetStatisticsType("Departures"));
            airliner.Airliner.Airline.Statistics.setStatisticsValue(StatisticsTypes.GetStatisticsType("Passengers%"), (int)(airlinePassengers / airlineDepartures));

            airliner.Airliner.Airline.addInvoice(new Invoice(GameObject.GetInstance().GameTime, Invoice.InvoiceType.Flight_Expenses, -expenses));
            airliner.Airliner.Airline.addInvoice(new Invoice(GameObject.GetInstance().GameTime, Invoice.InvoiceType.Tickets, ticketsIncome));
            airliner.Airliner.Airline.addInvoice(new Invoice(GameObject.GetInstance().GameTime, Invoice.InvoiceType.OnFlight_Income, -mealExpenses));
            airliner.Airliner.Airline.addInvoice(new Invoice(GameObject.GetInstance().GameTime, Invoice.InvoiceType.Fees, feesIncome));

            airliner.Route.addRouteInvoice(new Invoice(GameObject.GetInstance().GameTime, Invoice.InvoiceType.Flight_Expenses, -expenses));
            airliner.Route.addRouteInvoice(new Invoice(GameObject.GetInstance().GameTime, Invoice.InvoiceType.Tickets, ticketsIncome));
            airliner.Route.addRouteInvoice(new Invoice(GameObject.GetInstance().GameTime, Invoice.InvoiceType.OnFlight_Income, -mealExpenses));
            airliner.Route.addRouteInvoice(new Invoice(GameObject.GetInstance().GameTime, Invoice.InvoiceType.Fees, feesIncome));

            //wages
            airliner.Airliner.Airline.addInvoice(new Invoice(GameObject.GetInstance().GameTime, Invoice.InvoiceType.Wages, -airliner.Airliner.Airliner.Type.CockpitCrew * airliner.Airliner.Airline.Fees.getValue(FeeTypes.GetType("Cockpit wage"))));
            airliner.Airliner.Airline.addInvoice(new Invoice(GameObject.GetInstance().GameTime, Invoice.InvoiceType.Wages, -airliner.Route.getTotalCabinCrew() * airliner.Airliner.Airline.Fees.getValue(FeeTypes.GetType("Cabin wage"))));
            airliner.Airliner.Airline.addInvoice(new Invoice(GameObject.GetInstance().GameTime, Invoice.InvoiceType.Wages, -(airliner.Route.getTotalCabinCrew()) * airliner.Airliner.Airline.Fees.getValue(FeeTypes.GetType("Cabin kilometer rate")) * fdistance));
            airliner.Airliner.Airline.addInvoice(new Invoice(GameObject.GetInstance().GameTime, Invoice.InvoiceType.Wages, -(airliner.Airliner.Airliner.Type.CockpitCrew) * airliner.Airliner.Airline.Fees.getValue(FeeTypes.GetType("Cockpit kilometer rate")) * fdistance));


            airliner.Route.addRouteInvoice(new Invoice(GameObject.GetInstance().GameTime, Invoice.InvoiceType.Wages, -airliner.Airliner.Airliner.Type.CockpitCrew * airliner.Airliner.Airline.Fees.getValue(FeeTypes.GetType("Cockpit wage"))));
            airliner.Route.addRouteInvoice(new Invoice(GameObject.GetInstance().GameTime, Invoice.InvoiceType.Wages, -airliner.Route.getTotalCabinCrew() * airliner.Airliner.Airline.Fees.getValue(FeeTypes.GetType("Cabin wage"))));
            airliner.Route.addRouteInvoice(new Invoice(GameObject.GetInstance().GameTime, Invoice.InvoiceType.Wages, -(airliner.Route.getTotalCabinCrew()) * airliner.Airliner.Airline.Fees.getValue(FeeTypes.GetType("Cabin kilometer rate")) * fdistance));
            airliner.Route.addRouteInvoice(new Invoice(GameObject.GetInstance().GameTime, Invoice.InvoiceType.Wages, -(airliner.Airliner.Airliner.Type.CockpitCrew) * airliner.Airliner.Airline.Fees.getValue(FeeTypes.GetType("Cockpit kilometer rate")) * fdistance));


            airliner.Airliner.Airliner.Flown += fdistance;

            if (airliner.Airliner.Airline.IsHuman && GameObject.GetInstance().NewsBox.MailsOnLandings)
                GameObject.GetInstance().NewsBox.addNews(new News(News.NewsType.Flight_News, GameObject.GetInstance().GameTime, string.Format("{0} landed", airliner.Airliner.Name), string.Format("Your airliner {0} has landed in {1}, {2} with {3} passengers.\nThe airliner flow from {4}, {5}", new object[] { airliner.Airliner.Name, dest.Profile.Name, dest.Profile.Country.Name, airliner.CurrentFlight.getTotalPassengers(), dept.Profile.Name, dept.Profile.Country.Name })));

            CreatePassengersHappiness(airliner);

            SetNextFlight(airliner);

            CheckForService(airliner);


        }
        //creates the happiness for a landed route airliner
        private static void CreatePassengersHappiness(RouteAirliner airliner)
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
        private static void SimulateService(RouteAirliner airliner)
        {
            double servicePrice = 10000;

            airliner.CurrentPosition = new Coordinates(airliner.CurrentFlight.Entry.Destination.Airport.Profile.Coordinates.Latitude, airliner.CurrentFlight.Entry.Destination.Airport.Profile.Coordinates.Longitude);
            airliner.Status = RouteAirliner.AirlinerStatus.To_route_start;
    
            double fdistance = MathHelpers.GetDistance(airliner.CurrentFlight.Entry.Destination.Airport.Profile.Coordinates, airliner.CurrentPosition);
            double expenses = GameObject.GetInstance().FuelPrice * fdistance * airliner.CurrentFlight.getTotalPassengers() * airliner.Airliner.Airliner.Type.FuelConsumption + Airports.GetAirport(airliner.CurrentPosition).getLandingFee();

            servicePrice += expenses;

            airliner.Airliner.Airliner.Flown += fdistance;

            airliner.Airliner.Airliner.LastServiceCheck = airliner.Airliner.Airliner.Flown;

            airliner.Airliner.Airline.addInvoice(new Invoice(GameObject.GetInstance().GameTime, Invoice.InvoiceType.Maintenances, -servicePrice));

            airliner.Route.addRouteInvoice(new Invoice(GameObject.GetInstance().GameTime, Invoice.InvoiceType.Maintenances, -servicePrice));

        }
        //checks for an airliner should go to service
        private static void CheckForService(RouteAirliner airliner)
        {
            double serviceCheck = 500000000;
            double sinceLastService = airliner.Airliner.Airliner.Flown - airliner.Airliner.Airliner.LastServiceCheck;


            if (sinceLastService > serviceCheck)
            {
                airliner.Status = RouteAirliner.AirlinerStatus.On_service;
                airliner.CurrentFlight.Entry.Destination = new RouteEntryDestination(airliner.Airliner.Homebase, "Service");
    
            }


        }
        //gets the weather for an airliner
        private static Weather GetAirlinerWeather(RouteAirliner airliner)
        {
            double distance = MathHelpers.GetDistance(airliner.CurrentPosition, airliner.CurrentFlight.Entry.Destination.Airport.Profile.Coordinates);
            Airport dest = airliner.CurrentFlight.Entry.Destination.Airport;
            Airport dept = airliner.getDepartureAirport();

            double totalDistance = MathHelpers.GetDistance(dept.Profile.Coordinates, dest.Profile.Coordinates);

            Weather tWeather = new Weather();
            tWeather.WindSpeed = Weather.eWindSpeed.Calm;
            tWeather.Direction = Weather.WindDirection.Head;

            return distance > totalDistance / 2 ? dept.Weather : dest.Weather;
        }
        //sets the next flight for a route airliner
        private static void SetNextFlight(RouteAirliner airliner)
        {



            RouteTimeTableEntry entry = airliner.Route.TimeTable.getNextEntry(airliner.CurrentFlight.Entry);

            airliner.CurrentFlight = new Flight(entry);


        }

        //finds the next flight time for an airliner - checks also for delay
        private static DateTime GetNextFlightTime(RouteAirliner airliner)
        {
         
            RouteTimeTableEntry entry = airliner.CurrentFlight.Entry;

            return MathHelpers.ConvertEntryToDate(entry);

        }
        //returns the passengers for an airliner
        public static int GetPassengers(RouteAirliner airliner, AirlinerClass.ClassType type)
        {


            return PassengerHelpers.GetFlightPassengers(airliner, type);
        }

    }
}
