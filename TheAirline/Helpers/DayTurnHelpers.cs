using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using TheAirline.Infrastructure;
using TheAirline.Model.GeneralModel;
using TheAirline.Models.Airliners;
using TheAirline.Models.Airlines;
using TheAirline.Models.Airlines.AirlineCooperation;
using TheAirline.Models.Airports;
using TheAirline.Models.General;
using TheAirline.Models.General.Environment;
using TheAirline.Models.General.Finances;
using TheAirline.Models.General.Holidays;
using TheAirline.Models.General.Statistics;
using TheAirline.Models.Routes;

namespace TheAirline.Helpers
{
    //the helper class for a turn for a day
    public class DayTurnHelpers
    {
        #region Static Fields

        private static readonly Random Rnd = new Random();

        #endregion

        #region Public Methods and Operators

        public static void SimulateAirlineFlights(Airline airline)
        {
            var sw = new Stopwatch();
            sw.Start();

            foreach (
                FleetAirliner airliner in airline.Fleet.FindAll(f => f.Status != FleetAirliner.AirlinerStatus.Stopped)) //Parallel.ForEach(
                //    airline.Fleet.FindAll(f => f.Status != FleetAirliner.AirlinerStatus.Stopped),
                //    airliner =>
            {
                if (airliner.CurrentFlight != null)
                {
                    //Boolean stopoverRoute = airliner.CurrentFlight.Entry.MainEntry != null;

                    SimulateLanding(airliner);
                }

                FleetAirliner airliner1 = airliner;
                IOrderedEnumerable<RouteTimeTableEntry> dayEntries =
                    airliner.Routes.Where(r => r.StartDate <= GameObject.GetInstance().GameTime)
                            .SelectMany(r => r.TimeTable.GetEntries(GameObject.GetInstance().GameTime.DayOfWeek))
                            .Where(
                                e =>
                                e.Airliner == airliner1
                                && (e.TimeTable.Route.Season == Weather.Season.AllYear
                                    || e.TimeTable.Route.Season
                                    == GeneralHelpers.GetSeason(GameObject.GetInstance().GameTime)))
                            .OrderBy(e => e.Time);

                if (GameObject.GetInstance().GameTime > airliner.GroundedToDate)
                {
                    foreach (RouteTimeTableEntry entry in dayEntries)
                    {
                        if (entry.TimeTable.Route.HasStopovers)
                        {
                            SimulateStopoverFlight(entry);
                        }
                        else
                        {
                            SimulateFlight(entry);
                        }
                    }
                    CheckForService(airliner);
                }
            } //);

            sw.Stop();
        }

        #endregion

        #region Methods

        private static void CheckForService(FleetAirliner airliner)
        {
            const double serviceCheck = 500000000;
            double sinceLastService = airliner.Airliner.Flown - airliner.Airliner.LastServiceCheck;

            if (sinceLastService > serviceCheck)
            {
                SimulateService(airliner);
            }
        }

        private static void CreatePassengersHappiness(FleetAirliner airliner)
        {
            const int serviceLevel = 0;
            //airliner.Route.DrinksFacility.ServiceLevel + airliner.Route.FoodFacility.ServiceLevel + airliner.Airliner.Airliner.getFacility(AirlinerFacility.FacilityType.Audio).ServiceLevel + airliner.Airliner.Airliner.getFacility(AirlinerFacility.FacilityType.Seat).ServiceLevel + airliner.Airliner.Airliner.getFacility(AirlinerFacility.FacilityType.Video).ServiceLevel;
            int happyValue = airliner.CurrentFlight.IsOnTime ? 10 : 20;
            happyValue -= (serviceLevel/25);
            for (int i = 0; i < airliner.CurrentFlight.GetTotalPassengers(); i++)
            {
                bool isHappy = Rnd.Next(100) > happyValue;

                if (isHappy)
                {
                    PassengerHelpers.AddPassengerHappiness(airliner.Airliner.Airline);
                }
            }
        }

        //simulates a flight with stopovers

        //simulates a flight
        private static void SimulateFlight(RouteTimeTableEntry entry)
        {
            FleetAirliner airliner = entry.Airliner;

            if (entry.TimeTable.Route.HasStopovers || airliner.CurrentFlight is StopoverFlight)
            {
                if (airliner.CurrentFlight == null || ((StopoverFlight) airliner.CurrentFlight).IsLastTrip)
                {
                    airliner.CurrentFlight = new StopoverFlight(entry);
                }

                ((StopoverFlight) airliner.CurrentFlight).SetNextEntry();
            }
            else
            {
                airliner.CurrentFlight = new Flight(entry);
            }

            KeyValuePair<FleetAirlinerHelpers.DelayType, int> delayedMinutes =
                FleetAirlinerHelpers.GetDelayedMinutes(airliner);

            //cancelled/delay
            if (delayedMinutes.Value
                >= Convert.ToInt16(airliner.Airliner.Airline.GetAirlinePolicy("Cancellation Minutes").PolicyValue))
            {
                if (airliner.Airliner.Airline.IsHuman)
                {
                    Flight flight = airliner.CurrentFlight;

                    switch (delayedMinutes.Key)
                    {
                        case FleetAirlinerHelpers.DelayType.AirlinerProblems:
                            GameObject.GetInstance()
                                      .NewsBox.AddNews(
                                          new News(
                                              News.NewsType.FlightNews,
                                              GameObject.GetInstance().GameTime,
                                              Translator.GetInstance().GetString("News", "1004"),
                                              string.Format(
                                                  Translator.GetInstance().GetString("News", "1004", "message"),
                                                  flight.Entry.Destination.FlightCode,
                                                  flight.Entry.DepartureAirport.Profile.IATACode,
                                                  flight.Entry.Destination.Airport.Profile.IATACode)));
                            break;
                        case FleetAirlinerHelpers.DelayType.BadWeather:
                            GameObject.GetInstance()
                                      .NewsBox.AddNews(
                                          new News(
                                              News.NewsType.FlightNews,
                                              GameObject.GetInstance().GameTime,
                                              Translator.GetInstance().GetString("News", "1005"),
                                              string.Format(
                                                  Translator.GetInstance().GetString("News", "1005", "message"),
                                                  flight.Entry.Destination.FlightCode,
                                                  flight.Entry.DepartureAirport.Profile.IATACode,
                                                  flight.Entry.Destination.Airport.Profile.IATACode)));
                            break;
                    }
                }
                airliner.Airliner.Airline.Statistics.AddStatisticsValue(
                    GameObject.GetInstance().GameTime.Year,
                    StatisticsTypes.GetStatisticsType("Cancellations"),
                    1);

                double cancellationPercent =
                    airliner.Airliner.Airline.Statistics.GetStatisticsValue(
                        GameObject.GetInstance().GameTime.Year,
                        StatisticsTypes.GetStatisticsType("Cancellations"))
                    /(airliner.Airliner.Airline.Statistics.GetStatisticsValue(
                        GameObject.GetInstance().GameTime.Year,
                        StatisticsTypes.GetStatisticsType("Arrivals"))
                      + airliner.Airliner.Airline.Statistics.GetStatisticsValue(
                          GameObject.GetInstance().GameTime.Year,
                          StatisticsTypes.GetStatisticsType("Cancellations")));
                airliner.Airliner.Airline.Statistics.SetStatisticsValue(
                    GameObject.GetInstance().GameTime.Year,
                    StatisticsTypes.GetStatisticsType("Cancellation%"),
                    cancellationPercent*100);

                airliner.CurrentFlight = null;
            }
            else
            {
                airliner.CurrentFlight.AddDelayMinutes(delayedMinutes.Value);

                if (airliner.CurrentFlight.Entry.MainEntry == null)
                {
                    if (airliner.CurrentFlight.IsPassengerFlight())
                    {
                        var classes = new List<AirlinerClass>(airliner.Airliner.Classes);
                        foreach (AirlinerClass aClass in classes)
                        {
                            RouteAirlinerClass rac = ((PassengerRoute) airliner.CurrentFlight.Entry.TimeTable.Route).GetRouteAirlinerClass(aClass.Type);
                            airliner.CurrentFlight.Classes.Add(
                                new FlightAirlinerClass(
                                    ((PassengerRoute) airliner.CurrentFlight.Entry.TimeTable.Route).GetRouteAirlinerClass
                                        (aClass.Type),
                                    PassengerHelpers.GetFlightPassengers(airliner, aClass.Type)));

                            //airliner.CurrentFlight.Classes.Add(new FlightAirlinerClass(((PassengerRoute)airliner.CurrentFlight.Entry.TimeTable.Route).getRouteAirlinerClass(aClass.Type),0));
                        }
                    }
                    if (airliner.CurrentFlight.IsCargoFlight())
                    {
                        airliner.CurrentFlight.Cargo = PassengerHelpers.GetFlightCargo(airliner);
                    }
                }
                //SetTakeoffStatistics(airliner);

                if (airliner.CurrentFlight.ExpectedLanding.ToShortDateString()
                    == GameObject.GetInstance().GameTime.ToShortDateString())
                {
                    SimulateLanding(airliner);
                }
            }
        }

        //simulates the service of a flight

        //simulates the landing of a flight
        private static void SimulateLanding(FleetAirliner airliner)
        {
            DateTime landingTime =
                airliner.CurrentFlight.FlightTime.Add(
                    MathHelpers.GetFlightTime(
                        airliner.CurrentFlight.Entry.DepartureAirport.Profile.Coordinates.ConvertToGeoCoordinate(),
                        airliner.CurrentFlight.Entry.Destination.Airport.Profile.Coordinates.ConvertToGeoCoordinate(),
                        airliner.Airliner.Type));
            double fdistance = MathHelpers.GetDistance(
                airliner.CurrentFlight.GetDepartureAirport(),
                airliner.CurrentFlight.Entry.Destination.Airport);

            TimeSpan flighttime = landingTime.Subtract(airliner.CurrentFlight.FlightTime);
            const double groundTaxPerPassenger = 5;

            double tax = 0;

            if (airliner.CurrentFlight.Entry.Destination.Airport.Profile.Country
                != airliner.CurrentFlight.GetDepartureAirport().Profile.Country)
            {
                tax = 2*tax;
            }

            double ticketsIncome = 0;
            double feesIncome = 0;
            double mealExpenses = 0;
            double fuelExpenses = 0;

            if (airliner.CurrentFlight.IsPassengerFlight())
            {
                tax = groundTaxPerPassenger*airliner.CurrentFlight.GetTotalPassengers();
                fuelExpenses = FleetAirlinerHelpers.GetFuelExpenses(airliner, fdistance);

                ticketsIncome += airliner.CurrentFlight.Classes.Sum(fac => fac.Passengers*((PassengerRoute) airliner.CurrentFlight.Entry.TimeTable.Route).GetRouteAirlinerClass(fac.AirlinerClass.Type).FarePrice);

                FeeType employeeDiscountType = FeeTypes.GetType("Employee Discount");
                double employeesDiscount = airliner.Airliner.Airline.Fees.GetValue(employeeDiscountType);

                double totalDiscount = ticketsIncome*(employeeDiscountType.Percentage/100.0)
                                       *(employeesDiscount/100.0);
                ticketsIncome = ticketsIncome - totalDiscount;

                feesIncome += (from feeType in FeeTypes.GetTypes(FeeType.EFeeType.Fee) where GameObject.GetInstance().GameTime.Year >= feeType.FromYear from fac in airliner.CurrentFlight.Classes let percent = 0.10 let maxValue = Convert.ToDouble(feeType.Percentage)*(1 + percent) let minValue = Convert.ToDouble(feeType.Percentage)*(1 - percent) let value = Convert.ToDouble(Rnd.Next((int) minValue, (int) maxValue))/100 select fac.Passengers*value*airliner.Airliner.Airline.Fees.GetValue(feeType)).Sum();

                foreach (FlightAirlinerClass fac in airliner.CurrentFlight.Classes)
                {
                    foreach (
                        RouteFacility facility in
                            ((PassengerRoute) airliner.CurrentFlight.Entry.TimeTable.Route).GetRouteAirlinerClass(
                                fac.AirlinerClass.Type).GetFacilities())
                    {
                        if (facility.EType == RouteFacility.ExpenseType.Fixed)
                        {
                            mealExpenses += fac.Passengers*facility.ExpensePerPassenger;
                        }
                        else
                        {
                            FeeType feeType = facility.FeeType;
                            const double percent = 0.10;
                            double maxValue = Convert.ToDouble(feeType.Percentage)*(1 + percent);
                            double minValue = Convert.ToDouble(feeType.Percentage)*(1 - percent);

                            double value = Convert.ToDouble(Rnd.Next((int) minValue, (int) maxValue))/100;

                            mealExpenses -= fac.Passengers*value*airliner.Airliner.Airline.Fees.GetValue(feeType);
                        }
                    }
                }
            }
            if (airliner.CurrentFlight.IsCargoFlight())
            {
                tax = groundTaxPerPassenger*airliner.CurrentFlight.Cargo;
                fuelExpenses = FleetAirlinerHelpers.GetFuelExpenses(airliner, fdistance);

                ticketsIncome = airliner.CurrentFlight.Cargo*airliner.CurrentFlight.GetCargoPrice();
            }

            Airport dest = airliner.CurrentFlight.Entry.Destination.Airport;
            Airport dept = airliner.CurrentFlight.Entry.DepartureAirport;

            double dist = MathHelpers.GetDistance(
                dest.Profile.Coordinates.ConvertToGeoCoordinate(),
                dept.Profile.Coordinates.ConvertToGeoCoordinate());

            airliner.Data.AddOperatingValue(new OperatingValue("Tickets", GameObject.GetInstance().GameTime.Year, GameObject.GetInstance().GameTime.Month, ticketsIncome));
            airliner.Data.AddOperatingValue(new OperatingValue("In-flight Services", GameObject.GetInstance().GameTime.Year, GameObject.GetInstance().GameTime.Month, feesIncome));

            airliner.Data.AddOperatingValue(new OperatingValue("Fuel Expenses", GameObject.GetInstance().GameTime.Year, GameObject.GetInstance().GameTime.Month, -fuelExpenses));


            double expenses = fuelExpenses + AirportHelpers.GetLandingFee(dest, airliner.Airliner) + tax;

            if (double.IsNaN(expenses))
            {
                expenses = 0;
            }

            if (double.IsNaN(ticketsIncome) || ticketsIncome < 0)
            {
                ticketsIncome = 0;
            }

            FleetAirlinerHelpers.SetFlightStats(airliner);

            long airportIncome = Convert.ToInt64(AirportHelpers.GetLandingFee(dest, airliner.Airliner));
            dest.Income += airportIncome;

            Airline airline = airliner.Airliner.Airline;

            IEnumerable<CodeshareAgreement> agreements =
                airline.Codeshares.Where(
                    c => c.Airline1 == airline || c.Type == CodeshareAgreement.CodeshareType.BothWays);

            foreach (CodeshareAgreement agreement in agreements)
            {
                Airline tAirline = agreement.Airline1 == airline ? agreement.Airline2 : agreement.Airline1;

                double agreementIncome = ticketsIncome*(CodeshareAgreement.TicketSalePercent/100);

                AirlineHelpers.AddAirlineInvoice(
                    tAirline,
                    GameObject.GetInstance().GameTime,
                    Invoice.InvoiceType.Tickets,
                    agreementIncome);
            }

            AirlineHelpers.AddAirlineInvoice(
                airline,
                GameObject.GetInstance().GameTime,
                Invoice.InvoiceType.FlightExpenses,
                -expenses);
            AirlineHelpers.AddAirlineInvoice(
                airline,
                GameObject.GetInstance().GameTime,
                Invoice.InvoiceType.Tickets,
                ticketsIncome);
            AirlineHelpers.AddAirlineInvoice(
                airline,
                GameObject.GetInstance().GameTime,
                Invoice.InvoiceType.OnFlightIncome,
                -mealExpenses);
            AirlineHelpers.AddAirlineInvoice(
                airline,
                GameObject.GetInstance().GameTime,
                Invoice.InvoiceType.Fees,
                feesIncome);

            airliner.CurrentFlight.Entry.TimeTable.Route.AddRouteInvoice(
                new Invoice(GameObject.GetInstance().GameTime, Invoice.InvoiceType.FlightExpenses, -expenses));
            airliner.CurrentFlight.Entry.TimeTable.Route.AddRouteInvoice(
                new Invoice(GameObject.GetInstance().GameTime, Invoice.InvoiceType.Tickets, ticketsIncome));
            airliner.CurrentFlight.Entry.TimeTable.Route.AddRouteInvoice(
                new Invoice(GameObject.GetInstance().GameTime, Invoice.InvoiceType.OnFlightIncome, -mealExpenses));
            airliner.CurrentFlight.Entry.TimeTable.Route.AddRouteInvoice(
                new Invoice(GameObject.GetInstance().GameTime, Invoice.InvoiceType.Fees, feesIncome));

            double wages = 0;

            if (airliner.CurrentFlight.IsPassengerFlight())
            {
                int cabinCrew = ((AirlinerPassengerType) airliner.Airliner.Type).CabinCrew;

                wages = cabinCrew*flighttime.TotalHours
                        *airliner.Airliner.Airline.Fees.GetValue(FeeTypes.GetType("Cabin Wage"));
                // +(airliner.CurrentFlight.Entry.TimeTable.Route.getTotalCabinCrew() * airliner.Airliner.Airline.Fees.getValue(FeeTypes.GetType("Cabin kilometer rate")) * fdistance) + (airliner.Airliner.Type.CockpitCrew * airliner.Airliner.Airline.Fees.getValue(FeeTypes.GetType("Cockpit kilometer rate")) * fdistance);
                //wages
                AirlineHelpers.AddAirlineInvoice(
                    airline,
                    GameObject.GetInstance().GameTime,
                    Invoice.InvoiceType.Wages,
                    -wages);

                HolidayYearEvent holiday = HolidayYear.GetHoliday(
                    airline.Profile.Country,
                    GameObject.GetInstance().GameTime);

                if (holiday != null
                    && (holiday.Holiday.Travel == Holiday.TravelType.Both
                        || holiday.Holiday.Travel == Holiday.TravelType.Normal))
                {
                    wages = wages*1.50;
                }

                airliner.CurrentFlight.Entry.TimeTable.Route.AddRouteInvoice(
                    new Invoice(GameObject.GetInstance().GameTime, Invoice.InvoiceType.Wages, -wages));

                CreatePassengersHappiness(airliner);
            }

            airliner.Statistics.AddStatisticsValue(
                GameObject.GetInstance().GameTime.Year,
                StatisticsTypes.GetStatisticsType("Airliner_Income"),
                ticketsIncome - expenses - mealExpenses + feesIncome - wages);

            airliner.Airliner.Flown += fdistance;

            if (airliner.CurrentFlight.IsPassengerFlight())
            {
                foreach (
                    Cooperation cooperation in
                        airliner.CurrentFlight.Entry.Destination.Airport.Cooperations.Where(c => c.Airline == airline))
                {
                    double incomePerPax = MathHelpers.GetRandomDoubleNumber(
                        cooperation.Type.IncomePerPax*0.9,
                        cooperation.Type.IncomePerPax*1.1);

                    double incomeFromCooperation = Convert.ToDouble(airliner.CurrentFlight.GetTotalPassengers())
                                                   *incomePerPax;

                    AirlineHelpers.AddAirlineInvoice(
                        airline,
                        GameObject.GetInstance().GameTime,
                        Invoice.InvoiceType.OnFlightIncome,
                        incomeFromCooperation);
                }
            }

            if (airliner.Airliner.Airline.IsHuman && Settings.GetInstance().MailsOnLandings)
            {
                GameObject.GetInstance()
                          .NewsBox.AddNews(
                              new News(
                                  News.NewsType.FlightNews,
                                  GameObject.GetInstance().GameTime,
                                  $"{airliner.Name} landed",
                                  $"Your airliner [LI airliner={airliner.Airliner.TailNumber}] has landed in [LI airport={dest.Profile.IATACode}], {dest.Profile.Country.Name} with {airliner.CurrentFlight.GetTotalPassengers()} passengers.\nThe airliner flow from [LI airport={dept.Profile.IATACode}], {dept.Profile.Country.Name}"));
            }

            if (airliner.CurrentFlight is StopoverFlight && !((StopoverFlight) airliner.CurrentFlight).IsLastTrip)
            {
                SimulateFlight(airliner.CurrentFlight.Entry);
            }
            else
            {
                airliner.CurrentFlight = null;
            }
        }

        private static void SimulateService(FleetAirliner airliner)
        {
            const double servicePrice = 100000;

            airliner.Airliner.LastServiceCheck = airliner.Airliner.Flown;

            AirlineHelpers.AddAirlineInvoice(
                airliner.Airliner.Airline,
                GameObject.GetInstance().GameTime,
                Invoice.InvoiceType.Maintenances,
                -servicePrice);

            airliner.Statistics.AddStatisticsValue(
                GameObject.GetInstance().GameTime.Year,
                StatisticsTypes.GetStatisticsType("Airliner_Income"),
                -servicePrice);

            airliner.GroundedToDate = GameObject.GetInstance().GameTime.AddDays(90);
        }

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

        #endregion

        //simulates all flight for an airline for the current day

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
    }
}