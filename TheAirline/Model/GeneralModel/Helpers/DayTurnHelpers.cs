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

namespace TheAirline.Model.GeneralModel.Helpers
{
    //the helper class for a turn for a day
    public class DayTurnHelpers
    {
        private static Random rnd = new Random();
        //simulates all flight for an airline for the current day
        public static void SimulateAirlineFlights(Airline airline)
        {
            foreach (FleetAirliner airliner in airline.Fleet.FindAll(f => f.Status != FleetAirliner.AirlinerStatus.Stopped))
            {
                if (airliner.CurrentFlight != null)
                    SimulateLanding(airliner);

                var dayEntries = airliner.Routes.SelectMany(r => r.TimeTable.getEntries(GameObject.GetInstance().GameTime.DayOfWeek)).Where(e=>e.Airliner == airliner).OrderBy(e=>e.Time);

                foreach (RouteTimeTableEntry entry in dayEntries)
                    SimulateFlight(entry);
            }
        }
        //simulates a flight
        private static void SimulateFlight(RouteTimeTableEntry entry)
        {
            FleetAirliner airliner = entry.Airliner;
            entry.Airliner.CurrentFlight = new Flight(entry);

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
                 airliner.CurrentFlight = null;
             }
             else
             {
                 airliner.CurrentFlight.FlightTime = airliner.CurrentFlight.FlightTime.AddMinutes(delayedMinutes.Value);
                 foreach (AirlinerClass aClass in airliner.Airliner.Classes)
                 {
                     airliner.CurrentFlight.Classes.Add(new FlightAirlinerClass(airliner.CurrentFlight.Entry.TimeTable.Route.getRouteAirlinerClass(aClass.Type), PassengerHelpers.GetFlightPassengers(airliner, aClass.Type)));
                 }

                 SetTakeoffStatistics(airliner);

                 if (airliner.CurrentFlight.ExpectedLanding.ToShortDateString() == GameObject.GetInstance().GameTime.ToShortDateString())
                     SimulateLanding(airliner);
             }
        }
        //simulates the landing of a flight
        private static void SimulateLanding(FleetAirliner airliner)
        {

            DateTime landingTime = airliner.CurrentFlight.FlightTime.Add(MathHelpers.GetFlightTime(airliner.CurrentFlight.Entry.DepartureAirport.Profile.Coordinates, airliner.CurrentFlight.Entry.Destination.Airport.Profile.Coordinates, airliner.Airliner.Type));
            landingTime.Add(GetFlightWindInfluence(airliner));

            TimeSpan flighttime = landingTime.Subtract(airliner.CurrentFlight.FlightTime);
            double groundTaxPerPassenger = 5;

            double tax = groundTaxPerPassenger * airliner.CurrentFlight.getTotalPassengers();

            if (airliner.CurrentFlight.Entry.Destination.Airport.Profile.Country.Name != airliner.CurrentFlight.getDepartureAirport().Profile.Country.Name)
                tax = 2 * tax;

            double ticketsIncome = 0;

            foreach (AirlinerClass aClass in airliner.Airliner.Classes)
                ticketsIncome += airliner.CurrentFlight.getFlightAirlinerClass(aClass.Type).Passengers * airliner.CurrentFlight.Entry.TimeTable.Route.getRouteAirlinerClass(aClass.Type).FarePrice;

            //employees discount
            FeeType employeeDiscountType = FeeTypes.GetType("Employee Discount");
            double employeesDiscount = airliner.Airliner.Airline.Fees.getValue(employeeDiscountType);

            double totalDiscount = ticketsIncome * (employeeDiscountType.Percentage / 100.0) * (employeesDiscount / 100.0);
            ticketsIncome = ticketsIncome - totalDiscount;

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

            double fdistance = MathHelpers.GetDistance(airliner.CurrentFlight.getDepartureAirport().Profile.Coordinates, airliner.CurrentPosition);

            double expenses = GameObject.GetInstance().FuelPrice * fdistance * airliner.CurrentFlight.getTotalPassengers() * airliner.Airliner.Type.FuelConsumption + dest.getLandingFee() + tax;

            airliner.Airliner.Airline.Statistics.addStatisticsValue(GameObject.GetInstance().GameTime.Year, StatisticsTypes.GetStatisticsType("Passengers"), airliner.CurrentFlight.getTotalPassengers());
            airliner.Statistics.addStatisticsValue(GameObject.GetInstance().GameTime.Year, StatisticsTypes.GetStatisticsType("Passengers"), airliner.CurrentFlight.getTotalPassengers());
            dest.Statistics.addStatisticsValue(GameObject.GetInstance().GameTime.Year, airliner.Airliner.Airline, StatisticsTypes.GetStatisticsType("Passengers"), airliner.CurrentFlight.getTotalPassengers());
            dest.Statistics.addStatisticsValue(GameObject.GetInstance().GameTime.Year, airliner.Airliner.Airline, StatisticsTypes.GetStatisticsType("Arrivals"), 1);

            //canellation and ontime-percent
            double cancellationPercent = airliner.Airliner.Airline.Statistics.getStatisticsValue(GameObject.GetInstance().GameTime.Year, StatisticsTypes.GetStatisticsType("Cancellations")) / (airliner.Airliner.Airline.Statistics.getStatisticsValue(GameObject.GetInstance().GameTime.Year, StatisticsTypes.GetStatisticsType("Arrivals")) + airliner.Airliner.Airline.Statistics.getStatisticsValue(GameObject.GetInstance().GameTime.Year, StatisticsTypes.GetStatisticsType("Cancellations")));
            airliner.Airliner.Airline.Statistics.setStatisticsValue(GameObject.GetInstance().GameTime.Year, StatisticsTypes.GetStatisticsType("Cancellation%"), cancellationPercent * 100);

            Boolean isOnTime = airliner.CurrentFlight.IsOnTime;//airliner.CurrentFlight.ExpectedLanding >= GameObject.GetInstance().GameTime;

            if (isOnTime)
                airliner.Airliner.Airline.Statistics.addStatisticsValue(GameObject.GetInstance().GameTime.Year, StatisticsTypes.GetStatisticsType("On-Time"), 1);

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

            int cabinCrew = ((AirlinerPassengerType)airliner.Airliner.Type).CabinCrew;

            double wages = airliner.Airliner.Type.CockpitCrew * flighttime.TotalHours * airliner.Airliner.Airline.Fees.getValue(FeeTypes.GetType("Cockpit Wage")) + cabinCrew * flighttime.TotalHours * airliner.Airliner.Airline.Fees.getValue(FeeTypes.GetType("Cabin Wage"));// +(airliner.CurrentFlight.Entry.TimeTable.Route.getTotalCabinCrew() * airliner.Airliner.Airline.Fees.getValue(FeeTypes.GetType("Cabin kilometer rate")) * fdistance) + (airliner.Airliner.Type.CockpitCrew * airliner.Airliner.Airline.Fees.getValue(FeeTypes.GetType("Cockpit kilometer rate")) * fdistance);
            //wages
            AirlineHelpers.AddAirlineInvoice(airline, GameObject.GetInstance().GameTime, Invoice.InvoiceType.Wages, -wages);

            HolidayYearEvent holiday = HolidayYear.GetHoliday(airline.Profile.Country, GameObject.GetInstance().GameTime);

            if (holiday != null && (holiday.Holiday.Travel == Holiday.TravelType.Both || holiday.Holiday.Travel == Holiday.TravelType.Normal))
                wages = wages * 1.50;

            airliner.CurrentFlight.Entry.TimeTable.Route.addRouteInvoice(new Invoice(GameObject.GetInstance().GameTime, Invoice.InvoiceType.Wages, -wages));


            airliner.Statistics.addStatisticsValue(GameObject.GetInstance().GameTime.Year, StatisticsTypes.GetStatisticsType("Airliner_Income"), ticketsIncome - expenses - mealExpenses + feesIncome - wages);

            airliner.Airliner.Flown += fdistance;

            if (airliner.Airliner.Airline.IsHuman && Settings.GetInstance().MailsOnLandings)
                GameObject.GetInstance().NewsBox.addNews(new News(News.NewsType.Flight_News, GameObject.GetInstance().GameTime, string.Format("{0} landed", airliner.Name), string.Format("Your airliner [LI airliner={0}] has landed in [LI airport={1}], {2} with {3} passengers.\nThe airliner flow from [LI airport={4}], {5}", new object[] { airliner.Airliner.TailNumber, dest.Profile.IATACode, dest.Profile.Country.Name, airliner.CurrentFlight.getTotalPassengers(), dept.Profile.IATACode, dept.Profile.Country.Name })));

            airliner.CurrentFlight = null;
            CreatePassengersHappiness(airliner);




        }
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
        //returns the wind influence for a flight
        private static TimeSpan GetFlightWindInfluence(FleetAirliner airliner)
        {
            double distance = MathHelpers.GetDistance(airliner.CurrentFlight.Entry.DepartureAirport.Profile.Coordinates,airliner.CurrentFlight.Entry.Destination.Airport.Profile.Coordinates);
            Airport dest = airliner.CurrentFlight.Entry.Destination.Airport;
            Airport dept = airliner.CurrentFlight.getDepartureAirport();

            double totalDistance = MathHelpers.GetDistance(dept.Profile.Coordinates, dest.Profile.Coordinates);

            double windFirstHalf = ((int)dept.Weather[0].WindSpeed) * (distance / 2) / 100 * GameObjectHelpers.GetWindInfluence(airliner);

            

            double windSecondHalf = ((int)dest.Weather[0].WindSpeed) * (distance / 2) / 100 * GameObjectHelpers.GetWindInfluence(airliner);

            //return new TimeSpan(0,(int)(windFirstHalf + windSecondHalf),0);<

            return new TimeSpan(0, 0, 0);
    
        }
    }
}
