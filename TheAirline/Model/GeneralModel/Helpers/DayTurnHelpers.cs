using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using TheAirline.Model.AirlineModel;
using TheAirline.Model.AirlinerModel;
using TheAirline.Model.AirlinerModel.RouteModel;
using TheAirline.Model.AirportModel;
using TheAirline.Model.GeneralModel.StatisticsModel;
using TheAirline.Model.GeneralModel.HolidaysModel;
using TheAirline.Model.GeneralModel.WeatherModel;
using System.Threading.Tasks;
using System.Diagnostics;
using System.IO;
using TheAirline.Model.GeneralModel.Helpers.WorkersModel;

namespace TheAirline.Model.GeneralModel.Helpers
{
    //the helper class for a turn for a day
    public class DayTurnHelpers
    {
        private static Random rnd = new Random();
        //simulates all flight for an airline for the current day
        public static void SimulateAirlineFlights(Airline airline)
        {

            Stopwatch sw = new Stopwatch();
            sw.Start();
            //foreach (FleetAirliner airliner in airline.Fleet.FindAll(f => f.Status != FleetAirliner.AirlinerStatus.Stopped))
            Parallel.ForEach(airline.Fleet.FindAll(f => f.Status != FleetAirliner.AirlinerStatus.Stopped), airliner =>
            {
             
                if (airliner.CurrentFlight != null)
                {

                    //Boolean stopoverRoute = airliner.CurrentFlight.Entry.MainEntry != null;

                    SimulateLanding(airliner);
                }

                var dayEntries = airliner.Routes.SelectMany(r => r.TimeTable.getEntries(GameObject.GetInstance().GameTime.DayOfWeek)).Where(e => e.Airliner == airliner).OrderBy(e => e.Time);

                if (GameObject.GetInstance().GameTime > airliner.GroundedToDate)
                {

                    foreach (RouteTimeTableEntry entry in dayEntries)
                    {
                    
                        if (entry.TimeTable.Route.HasStopovers)
                            SimulateStopoverFlight(entry);
                        else
                            SimulateFlight(entry);

              
                    }
                    CheckForService(airliner);

                }
      
            });

            sw.Stop();



        }
        //simulates a flight with stopovers
        private static void SimulateStopoverFlight(RouteTimeTableEntry mainEntry)
        {

            //List<Route> routes = mainEntry.TimeTable.Route.Stopovers.SelectMany(s => s.Legs).ToList();

            //Boolean isInbound = mainEntry.DepartureAirport == mainEntry.TimeTable.Route.Destination2;

            SimulateFlight(mainEntry);
            /*
            if (isInbound)
                routes.Reverse();
            TimeSpan time = mainEntry.Time;


            foreach (Route route in routes)
            {
                RouteTimeTable timetable = new RouteTimeTable(route);

                //inbound
                if (isInbound)
                {
                    RouteTimeTableEntry entry = new RouteTimeTableEntry(timetable, mainEntry.Day, time, new RouteEntryDestination(route.Destination1, mainEntry.Destination.FlightCode));

                    time = entry.TimeTable.Route.getFlightTime(mainEntry.Airliner.Airliner.Type).Add(RouteTimeTable.MinTimeBetweenFlights); //getFlightTime ( 737-900ER SBY-BOS-CPH-AAR)
                    entry.Airliner = mainEntry.Airliner;
                    entry.MainEntry = mainEntry;

                    SimulateFlight(entry);
                }
                //outbound
                else
                {
                    RouteTimeTableEntry entry = new RouteTimeTableEntry(timetable, mainEntry.Day, time, new RouteEntryDestination(route.Destination2, mainEntry.Destination.FlightCode));
                    entry.Airliner = mainEntry.Airliner;
                    entry.MainEntry = mainEntry;

                    time = time.Add(entry.TimeTable.Route.getFlightTime(mainEntry.Airliner.Airliner.Type)).Add(RouteTimeTable.MinTimeBetweenFlights);

                    SimulateFlight(entry);
                }



            }
            */

        }
        //simulates a flight
        private static void SimulateFlight(RouteTimeTableEntry entry)
        {
            FleetAirliner airliner = entry.Airliner;

            if (entry.TimeTable.Route.HasStopovers || airliner.CurrentFlight is StopoverFlight)
            {
                if (airliner.CurrentFlight == null || ((StopoverFlight)airliner.CurrentFlight).IsLastTrip)
                    airliner.CurrentFlight = new StopoverFlight(entry);

                ((StopoverFlight)airliner.CurrentFlight).setNextEntry();
            }
            else
                airliner.CurrentFlight = new Flight(entry);

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

                airliner.CurrentFlight = null;
            }
            else
            {
                 airliner.CurrentFlight.addDelayMinutes(delayedMinutes.Value);

                if (airliner.CurrentFlight.Entry.MainEntry == null)
                {
                    if (airliner.CurrentFlight.isPassengerFlight())
                    {
                        var classes = new List<AirlinerClass>(airliner.Airliner.Classes);
                        foreach (AirlinerClass aClass in classes)
                        {
                      
                            airliner.CurrentFlight.Classes.Add(new FlightAirlinerClass(((PassengerRoute)airliner.CurrentFlight.Entry.TimeTable.Route).getRouteAirlinerClass(aClass.Type), PassengerHelpers.GetFlightPassengers(airliner, aClass.Type)));
                            
                            //airliner.CurrentFlight.Classes.Add(new FlightAirlinerClass(((PassengerRoute)airliner.CurrentFlight.Entry.TimeTable.Route).getRouteAirlinerClass(aClass.Type),0));
                        }
                    }
                    if (airliner.CurrentFlight.isCargoFlight())
                        airliner.CurrentFlight.Cargo = PassengerHelpers.GetFlightCargo(airliner);
                }
                //SetTakeoffStatistics(airliner);

                if (airliner.CurrentFlight.ExpectedLanding.ToShortDateString() == GameObject.GetInstance().GameTime.ToShortDateString())
                    SimulateLanding(airliner);

            }
        }
        //simulates the service of a flight
        private static void SimulateService(FleetAirliner airliner)
        {

            double servicePrice = 100000;

            airliner.Airliner.LastServiceCheck = airliner.Airliner.Flown;

            AirlineHelpers.AddAirlineInvoice(airliner.Airliner.Airline, GameObject.GetInstance().GameTime, Invoice.InvoiceType.Maintenances, -servicePrice);

            airliner.Statistics.addStatisticsValue(GameObject.GetInstance().GameTime.Year, StatisticsTypes.GetStatisticsType("Airliner_Income"), -servicePrice);

            airliner.GroundedToDate = GameObject.GetInstance().GameTime.AddDays(90);
        }
        //simulates the landing of a flight
        private static void SimulateLanding(FleetAirliner airliner)
        {

            DateTime landingTime = airliner.CurrentFlight.FlightTime.Add(MathHelpers.GetFlightTime(airliner.CurrentFlight.Entry.DepartureAirport.Profile.Coordinates, airliner.CurrentFlight.Entry.Destination.Airport.Profile.Coordinates, FleetAirlinerHelpers.GetCruisingSpeed(airliner)));
            double fdistance = MathHelpers.GetDistance(airliner.CurrentFlight.getDepartureAirport().Profile.Coordinates, airliner.CurrentPosition);

            TimeSpan flighttime = landingTime.Subtract(airliner.CurrentFlight.FlightTime);
            double groundTaxPerPassenger = 5;

            double tax = 0;

            if (airliner.CurrentFlight.Entry.Destination.Airport.Profile.Country != airliner.CurrentFlight.getDepartureAirport().Profile.Country)
                tax = 2 * tax;

            double ticketsIncome = 0;
            double feesIncome = 0;
            double mealExpenses = 0;
            double fuelExpenses = 0;

            if (airliner.CurrentFlight.isPassengerFlight())
            {
                tax = groundTaxPerPassenger * airliner.CurrentFlight.getTotalPassengers();
                fuelExpenses = GameObject.GetInstance().FuelPrice * fdistance * airliner.CurrentFlight.getTotalPassengers() * airliner.Airliner.Type.FuelConsumption;

                foreach (AirlinerClass aClass in airliner.Airliner.Classes)
                {
                    ticketsIncome += airliner.CurrentFlight.getFlightAirlinerClass(aClass.Type).Passengers * ((PassengerRoute)airliner.CurrentFlight.Entry.TimeTable.Route).getRouteAirlinerClass(aClass.Type).FarePrice;
                }

                FeeType employeeDiscountType = FeeTypes.GetType("Employee Discount");
                double employeesDiscount = airliner.Airliner.Airline.Fees.getValue(employeeDiscountType);

                double totalDiscount = ticketsIncome * (employeeDiscountType.Percentage / 100.0) * (employeesDiscount / 100.0);
                ticketsIncome = ticketsIncome - totalDiscount;

                foreach (FeeType feeType in FeeTypes.GetTypes(FeeType.eFeeType.Fee))
                {
                    if (GameObject.GetInstance().GameTime.Year >= feeType.FromYear)
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
                }

                foreach (AirlinerClass aClass in airliner.Airliner.Classes)
                {
                    foreach (RouteFacility facility in ((PassengerRoute)airliner.CurrentFlight.Entry.TimeTable.Route).getRouteAirlinerClass(aClass.Type).getFacilities())
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



            if (airliner.CurrentFlight is StopoverFlight && !((StopoverFlight)airliner.CurrentFlight).IsLastTrip)
            {
                SimulateFlight(airliner.CurrentFlight.Entry);
            }
            else
                airliner.CurrentFlight = null;

        }
        /*
        //sets the statistics from a take off
        private static void SetTakeoffStatistics(FleetAirliner airliner)
        {
            Airport airport = airliner.CurrentFlight.Entry.DepartureAirport;

            airport.Statistics.addStatisticsValue(GameObject.GetInstance().GameTime.Year, airliner.Airliner.Airline, StatisticsTypes.GetStatisticsType("Departures"), 1);

            double destPassengers = airport.Statistics.getStatisticsValue(GameObject.GetInstance().GameTime.Year, airliner.Airliner.Airline, StatisticsTypes.GetStatisticsType("Passengers"));
            double destDepartures = airport.Statistics.getStatisticsValue(GameObject.GetInstance().GameTime.Year, airliner.Airliner.Airline, StatisticsTypes.GetStatisticsType("Arrivals"));
            airport.Statistics.setStatisticsValue(GameObject.GetInstance().GameTime.Year, airliner.Airliner.Airline, StatisticsTypes.GetStatisticsType("Passengers%"), (int)(destPassengers / destDepartures));


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

        }*/
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
        //checks for an airliner should go to service
        private static void CheckForService(FleetAirliner airliner)
        {
            double serviceCheck = 500000000;
            double sinceLastService = airliner.Airliner.Flown - airliner.Airliner.LastServiceCheck;

            if (sinceLastService > serviceCheck)
                SimulateService(airliner);


        }

    }
}
