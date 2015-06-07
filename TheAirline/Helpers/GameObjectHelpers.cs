using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;
using TheAirline.General.Enums;
using TheAirline.General.Models.Countries;
using TheAirline.GUIModel.CustomControlsModel.PopUpWindowsModel;
using TheAirline.GUIModel.HelpersModel;
using TheAirline.GUIModel.ObjectsModel;
using TheAirline.Helpers.Workers;
using TheAirline.Infrastructure;
using TheAirline.Models.Airliners;
using TheAirline.Models.Airlines;
using TheAirline.Models.Airlines.AirlineCooperation;
using TheAirline.Models.Airlines.Subsidiary;
using TheAirline.Models.Airports;
using TheAirline.Models.General;
using TheAirline.Models.General.Countries;
using TheAirline.Models.General.Environment;
using TheAirline.Models.General.Finances;
using TheAirline.Models.General.HistoricEvents;
using TheAirline.Models.General.Holidays;
using TheAirline.Models.General.Statistics;
using TheAirline.Models.Passengers;
using TheAirline.Models.Pilots;
using TheAirline.Models.Routes;
using Settings = TheAirline.Properties.Settings;

namespace TheAirline.Helpers
{
    //the helper class for the game object
    public class GameObjectHelpers
    {
        #region Static Fields

        private static readonly Random Rnd = new Random();

        private static DateTime _lastTime;

        #endregion

        #region Public Methods and Operators

        public static void CreateGame(StartDataObject startData)
        {
            if (startData.RealData)
            {
                List<Airline> notRealAirlines = Airlines.GetAirlines(a => !a.Profile.IsReal && a != startData.Airline);
                List<Manufacturer> notRealManufacturers = Manufacturers.GetManufacturers(m => !m.IsReal);
                List<AirlinerType> notRealAirliners =
                    AirlinerTypes.GetTypes(a => notRealManufacturers.Contains(a.Manufacturer));

                foreach (Airline notRealAirliner in notRealAirlines)
                {
                    Airlines.RemoveAirline(notRealAirliner);
                }

                foreach (AirlinerType airliner in notRealAirliners)
                {
                    AirlinerTypes.RemoveType(airliner);
                }
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
            GameObject.GetInstance().FuelPrice =
                Inflations.GetInflation(GameObject.GetInstance().GameTime.Year).FuelPrice;

            airline.Profile.Country = startData.HomeCountry;
            airline.Profile.CEO = startData.CEO;

            GameObject.GetInstance().SetHumanAirline(airline);
            GameObject.GetInstance().MainAirline = GameObject.GetInstance().HumanAirline;

            if (startData.LocalCurrency)
            {
                GameObject.GetInstance().CurrencyCountry = airline.Profile.Country;
            }
            // AppSettings.GetInstance().resetCurrencyFormat();

            Airport airport = startData.Airport;

            AirportHelpers.RentGates(airport, airline, AirportContract.ContractType.Full,
                                     airline.AirlineRouteFocus == Route.RouteType.Cargo ? Terminal.TerminalType.Cargo : Terminal.TerminalType.Passenger, 2);

            AirportFacility checkinFacility =
                AirportFacilities.GetFacilities(AirportFacility.FacilityType.CheckIn).Find(f => f.TypeLevel == 1);
            AirportFacility facility =
                AirportFacilities.GetFacilities(AirportFacility.FacilityType.Service)
                                 .Find((f => f.TypeLevel == 1));

            airport.AddAirportFacility(
                GameObject.GetInstance().HumanAirline,
                facility,
                GameObject.GetInstance().GameTime);
            airport.AddAirportFacility(
                GameObject.GetInstance().HumanAirline,
                checkinFacility,
                GameObject.GetInstance().GameTime);

            if (continent.Uid != "100" || region.Uid != "100")
            {
                List<Airline> airlines =
                    Airlines.GetAirlines(
                        a =>
                        a.Profile.Country.Region == region
                        || (region.Uid == "100" && continent.HasRegion(a.Profile.Country.Region))
                        && a.Profile.Founded <= startYear && a.Profile.Folded > startYear);
                List<Airport> airports =
                    Airports.GetAirports(
                        a =>
                        a.Profile.Country.Region == region
                        || (region.Uid == "100" && continent.HasRegion(a.Profile.Country.Region))
                        && a.Profile.Period.From.Year <= startYear && a.Profile.Period.To.Year > startYear);

                Airports.Clear();
                foreach (Airport a in airports)
                {
                    Airports.AddAirport(a);
                }

                Airlines.Clear();
                foreach (Airline a in airlines)
                {
                    Airlines.AddAirline(a);
                }
            }

            PassengerHelpers.CreateAirlineDestinationDemand();

            AirlinerHelpers.CreateStartUpAirliners();

            if (startData.RandomOpponents || startData.Opponents == null)
            {
                Setup.SetupMainGame(opponents, startData.SameRegion);
            }
            else
            {
                Setup.SetupMainGame(startData.Opponents, startData.NumberOfOpponents);
            }
            List<Airport> heliports = Airports.GetAirports(a => a.Runways.Exists(r => r.Type == Runway.RunwayType.Helipad));

            if (startData.InternationalAirports)
            {
                List<Airport> intlAirports = Airports.GetAllAirports(a => a.Profile.Type == AirportProfile.AirportType.LongHaulInternational
                                                                          || a.Profile.Type == AirportProfile.AirportType.ShortHaulInternational);

                const int minAirportsPerRegion = 5;
                foreach (Region airportRegion in Regions.GetRegions())
                {
                    IEnumerable<Airport> usedAirports = Airlines.GetAllAirlines().SelectMany(a => a.Airports);

                    int countRegionAirports = intlAirports.Count(a => a.Profile.Country.Region == airportRegion);
                    if (countRegionAirports < minAirportsPerRegion)
                    {
                        IEnumerable<Airport> regionAirports =
                            Airports.GetAirports(airportRegion)
                                    .Where(a => !intlAirports.Contains(a))
                                    .OrderByDescending(a => a.Profile.Size)
                                    .Take(minAirportsPerRegion - countRegionAirports);

                        intlAirports.AddRange(regionAirports);
                    }

                    if (startData.SelectedCountries != null)
                    {
                        foreach (Country country in startData.SelectedCountries)
                        {
                            Country country1 = country;
                            List<Airport> countryAirports =
                                Airports.GetAllAirports(
                                    a => (new CountryCurrentCountryConverter().Convert(a.Profile.Country) as Country) == country1 &&
                                         (a.Profile.Type == AirportProfile.AirportType.Regional || a.Profile.Type == AirportProfile.AirportType.Domestic));

                            foreach (Airport cairport in countryAirports)
                                if (!intlAirports.Contains(cairport))
                                    intlAirports.Add(cairport);
                        }
                    }

                    intlAirports.AddRange(usedAirports);
                    intlAirports.AddRange(heliports);

                    Airports.Clear();

                    foreach (Airport majorAirport in intlAirports.Distinct())
                    {
                        Airports.AddAirport(majorAirport);
                    }
                }
            }
            if (startData.MajorAirports)
            {
                List<Airport> majorAirports =
                    Airports.GetAllAirports(
                        a =>
                        a.Profile.Size == GeneralHelpers.Size.Largest || a.Profile.Size == GeneralHelpers.Size.Large
                        || a.Profile.Size == GeneralHelpers.Size.VeryLarge
                        || a.Profile.Size == GeneralHelpers.Size.Medium);
                IEnumerable<Airport> usedAirports = Airlines.GetAllAirlines().SelectMany(a => a.Airports);

                const int minAirportsPerRegion = 5;
                foreach (Region airportRegion in Regions.GetRegions())
                {
                    int countRegionAirports = majorAirports.Count(a => a.Profile.Country.Region == airportRegion);
                    if (countRegionAirports < minAirportsPerRegion)
                    {
                        IEnumerable<Airport> regionAirports =
                            Airports.GetAirports(airportRegion)
                                    .Where(a => !majorAirports.Contains(a))
                                    .OrderByDescending(a => a.Profile.Size)
                                    .Take(minAirportsPerRegion - countRegionAirports);

                        majorAirports.AddRange(regionAirports);
                    }
                }

                if (startData.SelectedCountries != null)
                {
                    foreach (Country country in startData.SelectedCountries)
                    {
                        Country country1 = country;
                        List<Airport> countryAirports =
                            Airports.GetAllAirports(
                                a => (new CountryCurrentCountryConverter().Convert(a.Profile.Country) as Country) == country1 &&
                                     (a.Profile.Size == GeneralHelpers.Size.Smallest || a.Profile.Size == GeneralHelpers.Size.VerySmall
                                      || a.Profile.Size == GeneralHelpers.Size.Smallest));

                        foreach (Airport cairport in countryAirports)
                            if (!majorAirports.Contains(cairport))
                                majorAirports.Add(cairport);
                    }
                }

                majorAirports.AddRange(usedAirports);
                majorAirports.AddRange(heliports);

                Airports.Clear();

                foreach (Airport majorAirport in majorAirports.Distinct())
                {
                    Airports.AddAirport(majorAirport);
                }
            }

            airline.MarketFocus = startData.Focus;

            GeneralHelpers.CreateHolidays(GameObject.GetInstance().GameTime.Year);

            if (startData.IsPaused)
            {
                GameObjectWorker.GetInstance().StartPaused();
            }
            else
            {
                GameObjectWorker.GetInstance().Start();
            }

            GameObject.GetInstance()
                      .NewsBox.AddNews(
                          new News(
                              News.NewsType.StandardNews,
                              GameObject.GetInstance().GameTime,
                              Translator.GetInstance().GetString("News", "1001"),
                              string.Format(
                                  Translator.GetInstance().GetString("News", "1001", "message"),
                                  GameObject.GetInstance().HumanAirline.Profile.CEO,
                                  GameObject.GetInstance().HumanAirline.Profile.IATACode)));

            Action action = () =>
                {
                    var swPax = new Stopwatch();
                    swPax.Start();

                    PassengerHelpers.CreateDestinationDemand();

                    Console.WriteLine(@"Demand have been created in {0} ms.", swPax.ElapsedMilliseconds);
                    swPax.Stop();
                };

            Task.Factory.StartNew(action);
            //Task.Run(action);
            //Task t2 = Task.Factory.StartNew(action, "passengers");
        }

        public static int GetPassengers(FleetAirliner airliner, AirlinerClass.ClassType type)
        {
            return PassengerHelpers.GetFlightPassengers(airliner, type);
        }

        public static void SetHistoricEventInfluence(HistoricEventInfluence e, Boolean onEndDate)
        {
            double value = onEndDate ? -e.Value : e.Value;

            switch (e.Type)
            {
                case HistoricEventInfluence.InfluenceType.PassengerDemand:
                    PassengerHelpers.ChangePaxDemand(value);
                    break;
                case HistoricEventInfluence.InfluenceType.FuelPrices:
                    double percent = (100 - value)/100;
                    GameObject.GetInstance().FuelPrice = GameObject.GetInstance().FuelPrice*percent;
                    break;
                case HistoricEventInfluence.InfluenceType.Stocks:
                    break;
            }
        }

        public static void SimulateLanding(FleetAirliner airliner)
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

            if (airliner.CurrentFlight.Entry.Destination.Airport.Profile.Country.Name
                != airliner.CurrentFlight.GetDepartureAirport().Profile.Country.Name)
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

            double expenses = fuelExpenses + AirportHelpers.GetLandingFee(dest, airliner.Airliner) + tax;
            if (double.IsNaN(expenses))
            {
                expenses = 0;
            }

            if (double.IsNaN(ticketsIncome) || ticketsIncome < 0)
            {
                ticketsIncome = 0;
            }

            airliner.Data.AddOperatingValue(new OperatingValue("Tickets", GameObject.GetInstance().GameTime.Year, GameObject.GetInstance().GameTime.Month, ticketsIncome));
            airliner.Data.AddOperatingValue(new OperatingValue("In-flight Services", GameObject.GetInstance().GameTime.Year, GameObject.GetInstance().GameTime.Month, feesIncome));

            airliner.Data.AddOperatingValue(new OperatingValue("Fuel Expenses", GameObject.GetInstance().GameTime.Year, GameObject.GetInstance().GameTime.Month, -fuelExpenses));

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

            if (airliner.Airliner.Airline.IsHuman && Infrastructure.Settings.GetInstance().MailsOnLandings)
            {
                GameObject.GetInstance()
                          .NewsBox.AddNews(
                              new News(
                                  News.NewsType.FlightNews,
                                  GameObject.GetInstance().GameTime,
                                  $"{airliner.Name} landed",
                                  $"Your airliner [LI airliner={airliner.Airliner.TailNumber}] has landed in [LI airport={dest.Profile.IATACode}], {dest.Profile.Country.Name} with {airliner.CurrentFlight.GetTotalPassengers()} passengers.\nThe airliner flow from [LI airport={dept.Profile.IATACode}], {dept.Profile.Country.Name}"));
            }

            SetNextFlight(airliner);

            CheckForService(airliner);
        }

        //simulates a "turn"
        public static void SimulateTurn()
        {
            if (GameObject.GetInstance().DayRoundEnabled)
            {
                var sw = new Stopwatch();
                sw.Start();

                GameObject.GetInstance().GameTime = GameObject.GetInstance().GameTime.AddDays(1);

                DoDailyUpdate();

                if (MathHelpers.IsNewMonth(GameObject.GetInstance().GameTime))
                {
                    DoMonthlyUpdate();
                }

                if (MathHelpers.IsNewYear(GameObject.GetInstance().GameTime))
                {
                    DoYearlyUpdate();
                }
                Parallel.ForEach(
                    Airlines.GetAllAirlines(),
                    airline =>
                        {
                            double balance = airline.Money;

                            if (!airline.IsHuman)
                            {
                                AIHelpers.UpdateCPUAirline(airline);
                            }

                            DayTurnHelpers.SimulateAirlineFlights(airline);

                            double income =
                                airline.Invoices.MonthlyInvoices.Where(
                                    i =>
                                    i.Day == GameObject.GetInstance().GameTime.Day && i.Month == GameObject.GetInstance().GameTime.Month && i.Year == GameObject.GetInstance().GameTime.Year &&
                                    i.Amount > 0).Sum(i => i.Amount);
                            double expenses =
                                airline.Invoices.MonthlyInvoices.Where(
                                    i =>
                                    i.Day == GameObject.GetInstance().GameTime.Day && i.Month == GameObject.GetInstance().GameTime.Month && i.Year == GameObject.GetInstance().GameTime.Year &&
                                    i.Amount < 0).Sum(i => i.Amount);

                            airline.DailyOperatingBalanceHistory.Add(
                                new KeyValuePair<DateTime, KeyValuePair<double, double>>(
                                    GameObject.GetInstance().GameTime,
                                    new KeyValuePair<double, double>(Math.Abs(income), Math.Abs(expenses))));
                        });

                sw.Stop();
            }
            else
            {
                var sw = new Stopwatch();
                sw.Start();
                GameObject.GetInstance().GameTime =
                    GameObject.GetInstance().GameTime.AddMinutes(Infrastructure.Settings.GetInstance().MinutesPerTurn);

                CalibrateTime();

                if (MathHelpers.IsNewDay(GameObject.GetInstance().GameTime))
                {
                    DoDailyUpdate();
                }

                if (MathHelpers.IsNewYear(GameObject.GetInstance().GameTime))
                {
                    DoYearlyUpdate();
                }

                Parallel.ForEach(
                    Airlines.GetAllAirlines(),
                    airline =>
                        {
                            if (GameObject.GetInstance().GameTime.Minute == 0
                                && GameObject.GetInstance().GameTime.Hour == airline.Airports.Count%24)
                            {
                                if (!airline.IsHuman)
                                {
                                    AIHelpers.UpdateCPUAirline(airline);
                                }
                            }

                            foreach (FleetAirliner airliner in airline.Fleet)
                            {
                                UpdateAirliner(airliner);
                            }
                        });

                if (MathHelpers.IsNewMonth(GameObject.GetInstance().GameTime))
                {
                    DoMonthlyUpdate();
                }
                sw.Stop();
            }
        }

        #endregion

        #region Methods

        private static void CalibrateTime()
        {
            if (Infrastructure.Settings.GetInstance().MinutesPerTurn == 60 && GameObject.GetInstance().GameTime.Minute != 0)
            {
                GameObject.GetInstance().GameTime =
                    GameObject.GetInstance().GameTime.AddMinutes(-GameObject.GetInstance().GameTime.Minute);
            }

            if (Infrastructure.Settings.GetInstance().MinutesPerTurn == 30 && GameObject.GetInstance().GameTime.Minute == 15)
            {
                GameObject.GetInstance().GameTime = GameObject.GetInstance().GameTime.AddMinutes(15);
            }
        }

        private static void CheckForService(FleetAirliner airliner)
        {
            const double serviceCheck = 500000000;
            double sinceLastService = airliner.Airliner.Flown - airliner.Airliner.LastServiceCheck;

            if (sinceLastService > serviceCheck)
            {
                airliner.Status = FleetAirliner.AirlinerStatus.OnService;
                airliner.CurrentFlight.Entry.Destination = new RouteEntryDestination(airliner.Homebase, "Service", null);
            }
        }

        private static void ClearAllUsedStats()
        {
            Airports.GetAllAirports().ForEach(a => a.ClearDestinationPassengerStatistics());
            Airports.GetAllAirports().ForEach(a => a.ClearDestinationCargoStatistics());
            AirlineHelpers.ClearRoutesStatistics();
            AirlineHelpers.ClearAirlinesStatistics();
            AirportHelpers.ClearAirportStatistics();
        }

        //do the daily update

        //creates the monthly summary report for the human airline
        private static void CreateMontlySummary()
        {
            Airline airline = GameObject.GetInstance().HumanAirline;

            string monthName = GameObject.GetInstance()
                                         .GameTime.AddMonths(-1)
                                         .ToString("MMMM", CultureInfo.InvariantCulture);

            string summary = "[HEAD=Routes Summary]\n";

            IOrderedEnumerable<Route> routes =
                airline.Routes.OrderByDescending(
                    r =>
                    r.GetBalance(GameObject.GetInstance().GameTime.AddMonths(-1), GameObject.GetInstance().GameTime));
            Airport homeAirport = airline.Airports[0];

            foreach (Route route in routes)
            {
                double monthBalance = route.GetBalance(
                    GameObject.GetInstance().GameTime.AddMonths(-1),
                    GameObject.GetInstance().GameTime);
                summary +=
                    $"[WIDTH=300 {route.Destination1.Profile.Name}-{route.Destination2.Profile.Name}]Balance in month: {new ValueCurrencyConverter().Convert(monthBalance)}\n";
            }

            summary += "\n\n";

            summary += "[HEAD=Destinations Advice]\n";

            Airport largestDestination;

            if (airline.AirlineRouteFocus == Route.RouteType.Cargo)
            {
                largestDestination =
                    homeAirport.GetDestinationDemands()
                               .Where(
                                   a =>
                                   a != null && GeneralHelpers.IsAirportActive(a)
                                   && !airline.Routes.Exists(
                                       r =>
                                       (r.Destination1 == homeAirport && r.Destination2 == a)
                                       || (r.Destination2 == homeAirport && r.Destination1 == a)))
                               .OrderByDescending(homeAirport.GetDestinationCargoRate)
                               .FirstOrDefault();
            }
            else
            {
                largestDestination =
                    homeAirport.GetDestinationDemands()
                               .Where(
                                   a =>
                                   a != null && GeneralHelpers.IsAirportActive(a)
                                   && !airline.Routes.Exists(
                                       r =>
                                       (r.Destination1 == homeAirport && r.Destination2 == a)
                                       || (r.Destination2 == homeAirport && r.Destination1 == a)))
                               .OrderByDescending(
                                   a => homeAirport.GetDestinationPassengersRate(a, AirlinerClass.ClassType.EconomyClass))
                               .FirstOrDefault();
            }

            if (largestDestination != null)
            {
                summary +=
                    $"The largest destination in terms of demand from [LI airport={homeAirport.Profile.IATACode}] where you don't have a route, is [LI airport={largestDestination.Profile.IATACode}]";
            }

            summary += "\n[HEAD=Fleet Summary]\n";

            int fleetSize = GameObject.GetInstance().HumanAirline.DeliveredFleet.Count;
            int inorderFleetSize = GameObject.GetInstance().HumanAirline.Fleet.Count - fleetSize;

            int airlinersWithoutRoute = GameObject.GetInstance().HumanAirline.DeliveredFleet.Count(f => !f.HasRoute);

            summary += $"[WIDTH=300 Fleet Size:]{fleetSize}\n";
            summary += $"[WIDTH=300 Airliners in Order:]{inorderFleetSize}\n";
            summary += $"[WIDTH=300 Airliners Without Routes:]{airlinersWithoutRoute}\n";

            GameObject.GetInstance()
                      .NewsBox.AddNews(
                          new News(
                              News.NewsType.AirlineNews,
                              GameObject.GetInstance().GameTime,
                              $"{monthName} {GameObject.GetInstance().GameTime.AddMonths(-1).Year} Summary",
                              summary));
            // Translator.GetInstance().GetString("News", "1003"), string.Format(Translator.GetInstance().GetString("News", "1003", "message"), airliner.Airliner.TailNumber, airport.Profile.IATACode)));
        }

        //updates an airliner

        //creates the happiness for a landed route airliner
        private static void CreatePassengersHappiness(FleetAirliner airliner)
        {
            const int serviceLevel = 0;
            //airliner.Route.DrinksFacility.ServiceLevel + airliner.Route.FoodFacility.ServiceLevel + airliner.Airliner.Airliner.getFacility(AirlinerFacility.FacilityType.Audio).ServiceLevel + airliner.Airliner.Airliner.getFacility(AirlinerFacility.FacilityType.Seat).ServiceLevel + airliner.Airliner.Airliner.getFacility(AirlinerFacility.FacilityType.Video).ServiceLevel;
            int happyValue = airliner.CurrentFlight.IsOnTime ? 10 : 20;
            happyValue -= (serviceLevel/25);
            for (int i = 0; i < airliner.CurrentFlight.GetTotalPassengers(); i++)
            {
                Boolean isHappy = Rnd.Next(100) > happyValue;

                if (isHappy)
                {
                    PassengerHelpers.AddPassengerHappiness(airliner.Airliner.Airline);
                }
            }
        }

        private static void DoDailyUpdate()
        {
            //Clear stats when it on daily update
            if (Infrastructure.Settings.GetInstance().ClearStats == Intervals.Daily)
            {
                ClearAllUsedStats();
            }

            //Auto save when it on daily
            if (Infrastructure.Settings.GetInstance().AutoSave == Intervals.Daily)
            {
                SerializedLoadSaveHelpers.SaveGame("autosave");
            }

            //Clearing stats as an RAM work-a-round
            Airports.GetAllAirports().ForEach(a => a.ClearDestinationPassengerStatistics());
            Airports.GetAllAirports().ForEach(a => a.ClearDestinationCargoStatistics());

            List<Airline> humanAirlines = Airlines.GetAirlines(a => a.IsHuman);

            //Console.WriteLine(GameObject.GetInstance().GameTime.ToShortDateString() + ": " + DateTime.Now.Subtract(LastTime).TotalMilliseconds + " ms." + " : routes: " + totalRoutes + " airliners on route: " + totalAirlinersOnRoute);

            _lastTime = DateTime.Now;
            //changes the fuel prices 
            double fuelDiff = Inflations.GetInflation(GameObject.GetInstance().GameTime.Year + 1).FuelPrice
                              - Inflations.GetInflation(GameObject.GetInstance().GameTime.Year).FuelPrice;
            double fuelPrice = (Rnd.NextDouble()*(fuelDiff/4));

            GameObject.GetInstance().FuelPrice =
                Inflations.GetInflation(GameObject.GetInstance().GameTime.Year).FuelPrice + fuelPrice;
            //checks for airports due to close in 14 days
            List<Airport> closingAirports =
                Airports.GetAllAirports(
                    a =>
                    a.Profile.Period.To.ToShortDateString()
                    == GameObject.GetInstance().GameTime.AddDays(14).ToShortDateString());
            List<Airport> openingAirports =
                Airports.GetAllAirports(
                    a =>
                    a.Profile.Period.From.ToShortDateString()
                    == GameObject.GetInstance().GameTime.AddDays(14).ToShortDateString());

            foreach (Airport airport in closingAirports)
            {
                Airport reallocatedAirport = openingAirports.Find(a => a.Profile.Town == airport.Profile.Town);

                if (reallocatedAirport == null)
                {
                    GameObject.GetInstance()
                              .NewsBox.AddNews(
                                  new News(
                                      News.NewsType.AirportNews,
                                      GameObject.GetInstance().GameTime,
                                      "Airport closing",
                                      $"The airport [LI airport={airport.Profile.IATACode}]({new AirportCodeConverter().Convert(airport)}) is closing in 14 days.\n\rPlease move all routes to another destination."));
                }
                else
                {
                    GameObject.GetInstance()
                              .NewsBox.AddNews(
                                  new News(
                                      News.NewsType.AirportNews,
                                      GameObject.GetInstance().GameTime,
                                      "Airport closing",
                                      string.Format(
                                          "The airport [LI airport={0}]({1}) is closing in 14 days.\n\rThe airport will be replaced by {2}({3}) and all gates and routes from {0} will be reallocated to {2}.",
                                          airport.Profile.IATACode,
                                          new AirportCodeConverter().Convert(airport),
                                          reallocatedAirport.Profile.Name,
                                          new AirportCodeConverter().Convert(reallocatedAirport))));
                }

                CalendarItems.AddCalendarItem(
                    new CalendarItem(
                        CalendarItem.ItemType.AirportClosing,
                        airport.Profile.Period.To,
                        "Airport closing",
                        $"{airport.Profile.Name}, {((Country) new CountryCurrentCountryConverter().Convert(airport.Profile.Country)).Name}"));
            }

            foreach (Airport airport in openingAirports)
            {
                GameObject.GetInstance()
                          .NewsBox.AddNews(
                              new News(
                                  News.NewsType.AirportNews,
                                  GameObject.GetInstance().GameTime,
                                  "Airport opening",
                                  $"A new airport {airport.Profile.Name}({new AirportCodeConverter().Convert(airport)}) is opening in 14 days in {airport.Profile.Town.Name}, {((Country) new CountryCurrentCountryConverter().Convert(airport.Profile.Country)).Name}."));
                CalendarItems.AddCalendarItem(
                    new CalendarItem(
                        CalendarItem.ItemType.AirportOpening,
                        airport.Profile.Period.From,
                        "Airport opening",
                        $"{airport.Profile.Name}, {((Country) new CountryCurrentCountryConverter().Convert(airport.Profile.Country)).Name}"));
            }
            //checks for new airports which are opening
            List<Airport> openedAirports =
                Airports.GetAllAirports(
                    a =>
                    a.Profile.Period.From.ToShortDateString()
                    == GameObject.GetInstance().GameTime.ToShortDateString());
            List<Airport> closedAirports =
                Airports.GetAllAirports(
                    a =>
                    a.Profile.Period.To.ToShortDateString() == GameObject.GetInstance().GameTime.ToShortDateString());

            //checks for airports which are closing down
            foreach (Airport airport in closedAirports)
            {
                //check for airport which are reallocated 
                Airport reallocatedAirport = openedAirports.Find(a => a.Profile.Town == airport.Profile.Town);

                if (reallocatedAirport != null)
                {
                    IEnumerable<Airline> airlines =
                        new List<Airline>(from c in airport.AirlineContracts select c.Airline).Distinct();
                    foreach (Airline airline in airlines)
                    {
                        AirlineHelpers.ReallocateAirport(airport, reallocatedAirport, airline);

                        if (airline.IsHuman)
                        {
                            GameObject.GetInstance()
                                      .NewsBox.AddNews(
                                          new News(
                                              News.NewsType.AirportNews,
                                              GameObject.GetInstance().GameTime,
                                              "Airport operations changed",
                                              $"All your gates, routes and facilities has been moved from {airport.Profile.Name}({new AirportCodeConverter().Convert(airport)}) to [LI airport={reallocatedAirport.Profile.IATACode}]({new AirportCodeConverter().Convert(reallocatedAirport)})"));
                        }
                    }
                }

                GameObject.GetInstance()
                          .NewsBox.AddNews(
                              new News(
                                  News.NewsType.AirportNews,
                                  GameObject.GetInstance().GameTime,
                                  "Airport closed",
                                  $"The airport {airport.Profile.Name}({new AirportCodeConverter().Convert(airport)}) has now been closed. \n\rAll routes to and from the airports has been cancelled."));

                Airport airport1 = airport;
                IEnumerable<Route> obsoleteRoutes = (from r in Airlines.GetAllAirlines().SelectMany(a => a.Routes)
                                                     where r.Destination1 == airport1 || r.Destination2 == airport1
                                                     select r);

                foreach (Route route in obsoleteRoutes)
                {
                    route.Banned = true;

                    foreach (FleetAirliner airliner in route.GetAirliners())
                    {
                        if (airliner.Homebase == airport)
                        {
                            if (airliner.Airliner.Airline.IsHuman)
                            {
                                airliner.Homebase = (Airport) PopUpNewAirlinerHomebase.ShowPopUp(airliner);
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
            foreach (
                AirlinerType aType in
                    AirlinerTypes.GetTypes(
                        a =>
                        a.Produced.From.ToShortDateString() == GameObject.GetInstance().GameTime.ToShortDateString())
                )
            {
                GameObject.GetInstance()
                          .NewsBox.AddNews(
                              new News(
                                  News.NewsType.AirlinerNews,
                                  GameObject.GetInstance().GameTime,
                                  "New airliner type available",
                                  $"{aType.Manufacturer.Name} has finished the design of {aType.Name} and it is now available for purchase"));

                if (
                    !AirlineFacilities.GetFacilities(f => f is PilotTrainingFacility)
                                      .Exists(f => ((PilotTrainingFacility) f).AirlinerFamily == aType.AirlinerFamily))
                {
                    AirlineFacilities.AddFacility(
                        new PilotTrainingFacility(
                            "airlinefacilities",
                            aType.AirlinerFamily,
                            9000,
                            1000,
                            GameObject.GetInstance().GameTime.Year,
                            0,
                            0,
                            aType.AirlinerFamily));
                }
            }
            //checks for airliner types which are out of production
            foreach (
                AirlinerType aType in
                    AirlinerTypes.GetTypes(
                        a => a.Produced.To.ToShortDateString() == GameObject.GetInstance().GameTime.ToShortDateString())
                )
            {
                GameObject.GetInstance()
                          .NewsBox.AddNews(
                              new News(
                                  News.NewsType.AirlinerNews,
                                  GameObject.GetInstance().GameTime,
                                  "Airliner type out of production",
                                  $"{aType.Manufacturer.Name} has taken {aType.Name} out of production"));

                Boolean lastFromManufacturer =
                    AirlinerTypes.GetAllTypes().Count(t => (t.Manufacturer == aType.Manufacturer
                                                            && t.Produced.To > GameObject.GetInstance().GameTime)) == 0;

                if (lastFromManufacturer)
                {
                    AirlinerType type = aType;
                    IEnumerable<Airline> manufacturerContracts =
                        Airlines.GetAllAirlines()
                                .Where(a => a.Contract != null && a.Contract.Manufacturer == type.Manufacturer);

                    foreach (Airline contractedAirline in manufacturerContracts)
                    {
                        contractedAirline.Contract = null;
                    }
                }
            }
            //checks for airport facilities for the human airline
            IEnumerable<AirlineAirportFacility> humanAirportFacilities =
                (from f in humanAirlines.SelectMany(ai => ai.Airports.SelectMany(a => a.GetAirportFacilities(ai)))
                 where f.FinishedDate.ToShortDateString() == GameObject.GetInstance().GameTime.ToShortDateString()
                 select f);

            foreach (AirlineAirportFacility facility in humanAirportFacilities)
            {
                GameObject.GetInstance()
                          .NewsBox.AddNews(
                              new News(
                                  News.NewsType.AirportNews,
                                  GameObject.GetInstance().GameTime,
                                  "Airport facility",
                                  $"Your airport facility {facility.Facility.Name} at [LI airport={facility.Airport.Profile.IATACode}] is now finished building"));
                facility.FinishedDate = GameObject.GetInstance().GameTime;
            }
            //checks for changed flight restrictions
            foreach (
                FlightRestriction restriction in
                    FlightRestrictions.GetRestrictions()
                                      .FindAll(
                                          r =>
                                          r.StartDate.ToShortDateString() == GameObject.GetInstance().GameTime.ToShortDateString()
                                          || r.EndDate.ToShortDateString()
                                          == GameObject.GetInstance().GameTime.ToShortDateString()))
            {
                string restrictionNewsText = "";
                if (restriction.Type == FlightRestriction.RestrictionType.Flights)
                {
                    restrictionNewsText = string.Format(restriction.StartDate.ToShortDateString()
                                                        == GameObject.GetInstance().GameTime.ToShortDateString() ? "All flights from {0} to {1} have been banned" : "The ban for all flights from {0} to {1} have been lifted", restriction.From.Name, restriction.To.Name);
                }
                if (restriction.Type == FlightRestriction.RestrictionType.Airlines)
                {
                    restrictionNewsText = string.Format(restriction.StartDate.ToShortDateString()
                                                        == GameObject.GetInstance().GameTime.ToShortDateString() ? "All airlines flying from {0} flying to {1} have been blacklisted" : "The blacklist on all airlines from {0} flying to {1} have been lifted", restriction.From.Name, restriction.To.Name);
                }
                if (restriction.StartDate.ToShortDateString() == GameObject.GetInstance().GameTime.ToShortDateString())
                {
                    if (restriction.Type == FlightRestriction.RestrictionType.Flights)
                    {
                        IEnumerable<Route> bannedRoutes = (from r in Airlines.GetAllAirlines().SelectMany(a => a.Routes)
                                                           where
                                                               FlightRestrictions.HasRestriction(
                                                                   r.Destination1.Profile.Country,
                                                                   r.Destination2.Profile.Country,
                                                                   GameObject.GetInstance().GameTime)
                                                           select r);

                        foreach (Route route in bannedRoutes)
                        {
                            route.Banned = true;
                        }
                    }
                }
                GameObject.GetInstance()
                          .NewsBox.AddNews(
                              new News(
                                  News.NewsType.StandardNews,
                                  GameObject.GetInstance().GameTime,
                                  "Flight restriction",
                                  restrictionNewsText));
            }
            //checks for historic events
            foreach (HistoricEvent e in HistoricEvents.GetHistoricEvents(GameObject.GetInstance().GameTime))
            {
                GameObject.GetInstance()
                          .NewsBox.AddNews(
                              new News(News.NewsType.StandardNews, GameObject.GetInstance().GameTime, e.Name, e.Text));

                foreach (HistoricEventInfluence influence in e.Influences)
                {
                    SetHistoricEventInfluence(influence, false);
                }
            }
            //checks for historic events influences ending
            foreach (
                HistoricEventInfluence influence in
                    HistoricEvents.GetHistoricEventInfluences(GameObject.GetInstance().GameTime))
            {
                SetHistoricEventInfluence(influence, true);
            }

            //updates airports
            //Parallel.ForEach(
            //    Airports.GetAllActiveAirports(),
            //    airport =>
            foreach (Airport airport in Airports.GetAllActiveAirports())
            {
                //AirportHelpers.CreateAirportWeather(airport);

                if (Infrastructure.Settings.GetInstance().MailsOnBadWeather
                    && humanAirlines.SelectMany(a => a.Airports.FindAll(aa => aa == airport)).Any()
                    && (airport.Weather[airport.Weather.Length - 1].WindSpeed == Weather.eWindSpeed.ViolentStorm
                        || airport.Weather[airport.Weather.Length - 1].WindSpeed == Weather.eWindSpeed.Hurricane))
                {
                    GameObject.GetInstance()
                              .NewsBox.AddNews(
                                  new News(
                                      News.NewsType.AirportNews,
                                      GameObject.GetInstance().GameTime,
                                      Translator.GetInstance().GetString("News", "1002"),
                                      string.Format(
                                          Translator.GetInstance().GetString("News", "1002", "message"),
                                          airport.Profile.IATACode,
                                          GameObject.GetInstance().GameTime.AddDays(airport.Weather.Length - 1).DayOfWeek)));
                }
                // chs, 2011-01-11 changed for delivery of terminals
                foreach (Terminal terminal in airport.Terminals.GetTerminals())
                {
                    if (terminal.DeliveryDate.Year == GameObject.GetInstance().GameTime.Year
                        && terminal.DeliveryDate.Month == GameObject.GetInstance().GameTime.Month
                        && terminal.DeliveryDate.Day == GameObject.GetInstance().GameTime.Day)
                    {
                        if (terminal.Airline == null)
                        {
                            GameObject.GetInstance()
                                      .NewsBox.AddNews(
                                          new News(
                                              News.NewsType.AirportNews,
                                              GameObject.GetInstance().GameTime,
                                              "Construction of terminal",
                                              $"[LI airport={airport.Profile.IATACode}], {airport.Profile.Country.Name} has build a new terminal with {terminal.Gates.NumberOfGates} gates"));
                        }

                        if (terminal.Airline != null && terminal.Airline.IsHuman)
                        {
                            GameObject.GetInstance()
                                      .NewsBox.AddNews(
                                          new News(
                                              News.NewsType.AirportNews,
                                              GameObject.GetInstance().GameTime,
                                              "Construction of terminal",
                                              $"Your terminal at [LI airport={airport.Profile.IATACode}], {airport.Profile.Country.Name} is now finished and ready for use."));
                        }

                        if (terminal.Airline != null)
                        {
                            Terminal terminal1 = terminal;
                            IEnumerable<AirportContract> oldContracts = new List<AirportContract>(airport.GetAirlineContracts(terminal.Airline)).Where(c => c.TerminalType == terminal1.Type);

                            var airportContracts = oldContracts as AirportContract[] ?? oldContracts.ToArray();
                            if (airportContracts.Any())
                            {
                                int totalGates = airportContracts.Sum(c => c.NumberOfGates);

                                int gatesDiff = totalGates - terminal.Gates.NumberOfGates;

                                if (gatesDiff > 0)
                                {
                                    int length = airportContracts.Max(c => c.Length);
                                    var newContract = new AirportContract(
                                        terminal.Airline,
                                        airport,
                                        AirportContract.ContractType.Full,
                                        terminal.Type,
                                        GameObject.GetInstance().GameTime,
                                        gatesDiff,
                                        length,
                                        AirportHelpers.GetYearlyContractPayment(
                                            airport,
                                            AirportContract.ContractType.Full,
                                            gatesDiff,
                                            length)/2,
                                        true);

                                    AirportHelpers.AddAirlineContract(newContract);
                                }

                                foreach (AirportContract oldContract in airportContracts)
                                {
                                    airport.RemoveAirlineContract(oldContract);

                                    for (int i = 0; i < oldContract.NumberOfGates; i++)
                                    {
                                        Gate oldGate =
                                            airport.Terminals.GetGates().First(g => terminal != null && g.Airline == terminal.Airline);
                                        oldGate.Airline = null;
                                    }
                                }
                            }
                            double yearlyPayment = AirportHelpers.GetYearlyContractPayment(
                                airport,
                                AirportContract.ContractType.Full,
                                terminal.Gates.NumberOfGates,
                                20);

                            AirportHelpers.AddAirlineContract(
                                new AirportContract(
                                    terminal.Airline,
                                    airport,
                                    AirportContract.ContractType.Full,
                                    terminal.Type,
                                    GameObject.GetInstance().GameTime,
                                    terminal.Gates.NumberOfGates,
                                    20,
                                    yearlyPayment*0.75,
                                    true));

                            if (
                                terminal.Airport.GetAirportFacility(
                                    terminal.Airline,
                                    AirportFacility.FacilityType.CheckIn).TypeLevel == 0)
                            {
                                AirportFacility checkinFacility =
                                    AirportFacilities.GetFacilities(AirportFacility.FacilityType.CheckIn)
                                                     .Find(f => f.TypeLevel == 1);

                                terminal.Airport.AddAirportFacility(
                                    terminal.Airline,
                                    checkinFacility,
                                    GameObject.GetInstance().GameTime);
                            }
                        }
                    }
                        //new gates in an existing terminal
                    else
                    {
                        int numberOfNewGates =
                            terminal.Gates.GetGates()
                                    .Count(
                                        g =>
                                        g.DeliveryDate.ToShortDateString()
                                        == GameObject.GetInstance().GameTime.ToShortDateString());

                        if (numberOfNewGates > 0)
                        {
                            GameObject.GetInstance()
                                      .NewsBox.AddNews(
                                          new News(
                                              News.NewsType.AirportNews,
                                              GameObject.GetInstance().GameTime,
                                              "Expansion of terminal",
                                              $"[LI airport={airport.Profile.IATACode}], {airport.Profile.Country.Name} has expanded {terminal.Name} with {numberOfNewGates} gates"));

                            double yearlyPayment = AirportHelpers.GetYearlyContractPayment(
                                airport,
                                AirportContract.ContractType.Full,
                                numberOfNewGates + terminal.Gates.NumberOfGates,
                                20);

                            AirportContract terminalContract =
                                airport.AirlineContracts.Find(c => c.Terminal != null && c.Terminal == terminal);

                            if (terminalContract != null)
                            {
                                terminalContract.NumberOfGates += numberOfNewGates;
                                terminalContract.YearlyPayment = yearlyPayment;

                                for (int i = 0; i < numberOfNewGates; i++)
                                {
                                    Gate newGate = airport.Terminals.GetGates().First(g => g.Airline == null);
                                    newGate.Airline = terminalContract.Airline;
                                }
                            }
                        }
                    }
                    //expired contracts
                    var airlineContracts =
                        new List<AirportContract>(
                            airport.AirlineContracts.FindAll(
                                c =>
                                c.ExpireDate.ToShortDateString()
                                == GameObject.GetInstance().GameTime.ToShortDateString()));

                    foreach (AirportContract contract in airlineContracts)
                    {
                        if (contract.AutoRenew)
                        {
                            contract.ContractDate = GameObject.GetInstance().GameTime;
                            contract.ExpireDate = GameObject.GetInstance().GameTime.AddYears(contract.Length);

                            if (contract.Airline.IsHuman)
                            {
                                GameObject.GetInstance()
                                          .NewsBox.AddNews(
                                              new News(
                                                  News.NewsType.AirportNews,
                                                  GameObject.GetInstance().GameTime,
                                                  "Airport contract renewed",
                                                  $"Your contract for {contract.NumberOfGates} gates at [LI airport={contract.Airport.Profile.IATACode}], {contract.Airport.Profile.Country.Name} is now been renewed"));
                            }
                        }
                        else
                        {
                            for (int i = 0; i < contract.NumberOfGates; i++)
                            {
                                Gate gate =
                                    airport.Terminals.GetGates().First(g => contract != null && g.Airline == contract.Airline);
                                gate.Airline = null;
                            }

                            if (contract.Airline.IsHuman)
                            {
                                int totalContractGates =
                                    airport.AirlineContracts.Where(c => c.Airline.IsHuman).Sum(c => c.NumberOfGates);

                                var airlineRoutes =
                                    new List<Route>(AirportHelpers.GetAirportRoutes(airport, contract.Airline));

                                var remainingContracts =
                                    new List<AirportContract>(
                                        airport.AirlineContracts.FindAll(
                                            c => c.Airline == contract.Airline && c != contract));

                                Boolean canFillRoutes = AirportHelpers.CanFillRoutesEntries(
                                    airport,
                                    contract.Airline,
                                    remainingContracts,
                                    Weather.Season.AllYear);

                                if (!canFillRoutes)
                                {
                                    GameObject.GetInstance()
                                              .NewsBox.AddNews(
                                                  new News(
                                                      News.NewsType.AirportNews,
                                                      GameObject.GetInstance().GameTime,
                                                      "Airport contract expired",
                                                      $"Your contract for {contract.NumberOfGates} gates at [LI airport={contract.Airport.Profile.IATACode}], {contract.Airport.Profile.Country.Name} is now expired, and a number of routes has been cancelled"));

                                    int currentRoute = 0;
                                    while (!canFillRoutes)
                                    {
                                        Route routeToDelete = airlineRoutes[currentRoute];

                                        foreach (FleetAirliner fAirliner in routeToDelete.GetAirliners())
                                        {
                                            fAirliner.Status = FleetAirliner.AirlinerStatus.Stopped;
                                            fAirliner.RemoveRoute(routeToDelete);
                                        }

                                        contract.Airline.RemoveRoute(routeToDelete);

                                        currentRoute++;

                                        canFillRoutes = AirportHelpers.CanFillRoutesEntries(
                                            airport,
                                            contract.Airline,
                                            remainingContracts,
                                            Weather.Season.AllYear);
                                    }
                                }
                                else
                                {
                                    GameObject.GetInstance()
                                              .NewsBox.AddNews(
                                                  new News(
                                                      News.NewsType.AirportNews,
                                                      GameObject.GetInstance().GameTime,
                                                      "Airport contract expired",
                                                      $"Your contract for {contract.NumberOfGates} gates at [LI airport={contract.Airport.Profile.IATACode}], {contract.Airport.Profile.Country.Name} is now expired"));
                                }

                                airport.RemoveAirlineContract(contract);
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
                                {
                                    airport.RemoveAirlineContract(contract);
                                }
                            }
                        }
                    }
                }
                //checks for airport expansions
                foreach (AirportExpansion expansion in airport.Profile.Expansions.Where(e => e.Date.ToShortDateString() == GameObject.GetInstance().GameTime.ToShortDateString()))
                {
                    AirportHelpers.SetAirportExpansion(airport, expansion);
                }
            } //);
            //checks for airliners for the human airline
            foreach (
                FleetAirliner airliner in
                    humanAirlines.SelectMany(
                        a =>
                        a.Fleet.FindAll(
                            f =>
                            f.Airliner.BuiltDate == GameObject.GetInstance().GameTime
                            && f.Purchased != FleetAirliner.PurchasedType.BoughtDownPayment)))
            {
                if (airliner.Airliner.Airline == GameObject.GetInstance().HumanAirline)
                {
                    GameObject.GetInstance()
                              .NewsBox.AddNews(
                                  new News(
                                      News.NewsType.FleetNews,
                                      GameObject.GetInstance().GameTime,
                                      "Delivery of airliner",
                                      $"Your new airliner [LI airliner={airliner.Airliner.TailNumber}] as been delivered to your fleet.\nThe airliner is currently at [LI airport={airliner.Homebase.Profile.IATACode}], {airliner.Homebase.Profile.Country.Name}"));
                }
                else
                {
                    GameObject.GetInstance()
                              .NewsBox.AddNews(
                                  new News(
                                      News.NewsType.FleetNews,
                                      GameObject.GetInstance().GameTime,
                                      "Delivery of airliner",
                                      $"The new airliner [LI airliner={airliner.Airliner.TailNumber}] as been delivered for [LI airline={airliner.Airliner.Airline.Profile.IATACode}].\nThe airliner is currently at [LI airport={airliner.Homebase.Profile.IATACode}], {airliner.Homebase.Profile.Country.Name}"));
                }
            }

            //Parallel.ForEach(
            //    Airlines.GetAllAirlines(),
            //    airline =>
            foreach (Airline airline in Airlines.GetAllAirlines())

            {
                lock (airline.Fleet)
                {
                    var fleet = new List<FleetAirliner>(airline.Fleet);
                    foreach (FleetAirliner airliner in
                        fleet.FindAll(
                            a =>
                            a != null
                            && a.Airliner.BuiltDate.ToShortDateString()
                               == GameObject.GetInstance().GameTime.ToShortDateString()
                            && a.Purchased == FleetAirliner.PurchasedType.BoughtDownPayment))
                    {
                        if (airline.Money >= airliner.Airliner.Type.Price)
                        {
                            AirlineHelpers.AddAirlineInvoice(
                                airline,
                                GameObject.GetInstance().GameTime,
                                Invoice.InvoiceType.Purchases,
                                -airliner.Airliner.Type.Price);
                            airliner.Purchased = FleetAirliner.PurchasedType.Bought;
                        }
                        else
                        {
                            airline.RemoveAirliner(airliner);

                            if (airline.IsHuman)
                            {
                                GameObject.GetInstance()
                                          .NewsBox.AddNews(
                                              new News(
                                                  News.NewsType.FleetNews,
                                                  GameObject.GetInstance().GameTime,
                                                  "Delivery of airliner",
                                                  $"Your new airliner {airliner.Name} can't be delivered to your fleet.\nYou don't have enough money to purchase it."));
                            }
                        }
                    }
                }

                if (airline.Contract != null
                    && airline.Contract.ExpireDate.ToShortDateString()
                    == GameObject.GetInstance().GameTime.ToShortDateString())
                {
                    int missingAirliners = airline.Contract.Airliners - airline.Contract.PurchasedAirliners;

                    if (missingAirliners > 0)
                    {
                        double missingFee = (airline.Contract.GetTerminationFee()/(airline.Contract.Length*2))
                                            *missingAirliners;
                        AirlineHelpers.AddAirlineInvoice(
                            airline,
                            GameObject.GetInstance().GameTime,
                            Invoice.InvoiceType.Purchases,
                            -missingFee);

                        if (airline.IsHuman)
                        {
                            GameObject.GetInstance()
                                      .NewsBox.AddNews(
                                          new News(
                                              News.NewsType.FleetNews,
                                              GameObject.GetInstance().GameTime,
                                              "Contract expired",
                                              $"Your contract with {airline.Contract.Manufacturer.Name} has now expired.\nYou didn't purchased enough airliners with costs a fee of {missingFee:C} for missing {missingAirliners} airliners"));
                        }
                    }
                    else if (airline.IsHuman)
                    {
                        GameObject.GetInstance()
                                  .NewsBox.AddNews(
                                      new News(
                                          News.NewsType.FleetNews,
                                          GameObject.GetInstance().GameTime,
                                          "Contract expired",
                                          $"Your contract with {airline.Contract.Manufacturer.Name} has now expired."));
                    }

                    airline.Contract = null;
                }
                //checks for students educated
                IEnumerable<PilotStudent> educatedStudents =
                    airline.FlightSchools.SelectMany(
                        f =>
                        f.Students.FindAll(
                            s =>
                            s.EndDate.ToShortDateString()
                            == GameObject.GetInstance().GameTime.ToShortDateString()));

                foreach (PilotStudent student in educatedStudents)
                {
                    var pilot = new Pilot(student.Profile, GameObject.GetInstance().GameTime, student.Rating);

                    if (student.AirlinerFamily != "")
                    {
                        pilot.AddAirlinerFamily(student.AirlinerFamily);
                    }

                    student.Instructor.RemoveStudent(student);
                    student.Instructor.FlightSchool.RemoveStudent(student);
                    student.Instructor = null;

                    airline.AddPilot(pilot);

                    if (airline.IsHuman)
                    {
                        GameObject.GetInstance()
                                  .NewsBox.AddNews(
                                      new News(
                                          News.NewsType.FlightNews,
                                          GameObject.GetInstance().GameTime,
                                          Translator.GetInstance().GetString("News", "1006"),
                                          string.Format(
                                              Translator.GetInstance().GetString("News", "1006", "message"),
                                              pilot.Profile.Name)));
                    }
                }

                IEnumerable<Pilot> trainedPilots =
                    airline.Pilots.Where(
                        p =>
                        p.Training != null
                        && p.Training.EndDate.ToShortDateString()
                        == GameObject.GetInstance().GameTime.ToShortDateString());

                foreach (Pilot pilot in trainedPilots)
                {
                    pilot.AddAirlinerFamily(pilot.Training.AirlinerFamily);

                    if (airline.IsHuman)
                    {
                        GameObject.GetInstance()
                                  .NewsBox.AddNews(
                                      new News(
                                          News.NewsType.FlightNews,
                                          GameObject.GetInstance().GameTime,
                                          Translator.GetInstance().GetString("News", "1015"),
                                          string.Format(
                                              Translator.GetInstance().GetString("News", "1015", "message"),
                                              pilot.Profile.Name,
                                              pilot.Training.AirlinerFamily)));
                    }

                    pilot.Training = null;
                }
            } //);
            //checks for mergers
            foreach (AirlineMerger merger in AirlineMergers.GetAirlineMergers(GameObject.GetInstance().GameTime))
            {
                if (merger.Type == AirlineMerger.MergerType.Merger)
                {
                    AirlineHelpers.SwitchAirline(merger.Airline2, merger.Airline1);

                    Airlines.RemoveAirline(merger.Airline2);

                    if (merger.NewName != null && merger.NewName.Length > 1)
                    {
                        merger.Airline1.Profile.Name = merger.NewName;
                    }
                }
                if (merger.Type == AirlineMerger.MergerType.Subsidiary)
                {
                    string oldLogo = merger.Airline2.Profile.Logo;

                    var sAirline = new SubsidiaryAirline(
                        merger.Airline1,
                        merger.Airline2.Profile,
                        merger.Airline2.Mentality,
                        merger.Airline2.MarketFocus,
                        merger.Airline2.License,
                        merger.Airline2.AirlineRouteFocus);

                    AirlineHelpers.SwitchAirline(merger.Airline2, merger.Airline1);

                    merger.Airline1.AddSubsidiaryAirline(sAirline);

                    Airlines.RemoveAirline(merger.Airline2);

                    sAirline.Profile.Logos = merger.Airline2.Profile.Logos;
                    sAirline.Profile.Color = merger.Airline2.Profile.Color;
                }

                GameObject.GetInstance()
                          .NewsBox.AddNews(
                              new News(
                                  News.NewsType.AirlineNews,
                                  GameObject.GetInstance().GameTime,
                                  "Airline merger",
                                  merger.Name));
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

                var airliners = new List<FleetAirliner>(a.Fleet);
                foreach (FleetAirliner airliner in airliners)
                {
                    if (airliner != null)
                    {
                        FleetAirlinerHelpers.DoMaintenance(airliner);
                        FleetAirlinerHelpers.RestoreMaintRoutes(airliner);
                    }
                }

                //checks for special contracts
                var sContracts = new List<SpecialContract>(a.SpecialContracts.Where(s => s.Date <= GameObject.GetInstance().GameTime));

                foreach (SpecialContract sc in sContracts)
                {
                    if (sc.Date.ToShortDateString() == GameObject.GetInstance().GameTime.ToShortDateString() && sc.Type.IsFixedDate)
                    {
                        foreach (SpecialContractRoute route in sc.Type.Routes)
                        {
                            PassengerHelpers.ChangePaxDemand(route.Departure, route.Destination, (int) route.PassengersPerDay);

                            if (route.BothWays)
                                PassengerHelpers.ChangePaxDemand(route.Destination, route.Departure, (int) route.PassengersPerDay);
                        }
                    }
                    if (AirlineHelpers.CheckSpecialContract(sc))
                    {
                        a.SpecialContracts.Remove(sc);

                        if (sc.Type.IsFixedDate)
                        {
                            foreach (SpecialContractRoute route in sc.Type.Routes)
                            {
                                PassengerHelpers.ChangePaxDemand(route.Departure, route.Destination, -(int) route.PassengersPerDay);

                                if (route.BothWays)
                                    PassengerHelpers.ChangePaxDemand(route.Destination, route.Departure, -(int) route.PassengersPerDay);
                            }
                        }
                    }
                }
            }

            if (GameObject.GetInstance().GameTime.Day%7 == 0)
            {
                foreach (Airline airline in Airlines.GetAllAirlines())
                    airline.OverallScore += StatisticsHelpers.GetWeeklyScore(airline);
                // GameObject.GetInstance().HumanAirline.OverallScore +=
                //   StatisticsHelpers.GetWeeklyScore(GameObject.GetInstance().HumanAirline);
            }
            //checks for new special contract types
            List<SpecialContractType> randomSpecialContracts = SpecialContractTypes.GetRandomTypes();
            List<SpecialContractType> fixedSpecialContracts =
                SpecialContractTypes.GetTypes().FindAll(s => s.IsFixedDate && GameObject.GetInstance().GameTime.ToShortDateString() == s.Period.From.AddMonths(-1).ToShortDateString());

            var existingContracts = new List<SpecialContractType>(GameObject.GetInstance().Contracts);

            foreach (SpecialContractType sct in existingContracts)
            {
                if (sct.LastDate.AddMonths(1).ToShortDateString() == GameObject.GetInstance().GameTime.ToShortDateString())
                    GameObject.GetInstance().Contracts.Remove(sct);
                else
                {
                    Boolean taken = false;
                    int i = 0;

                    while (!taken && i < Airlines.GetAirlines(a => !a.IsHuman).Count)
                    {
                        Airline airline = Airlines.GetAirlines(a => !a.IsHuman)[i];
                        taken = AIHelpers.WillTakeSpecialContract(airline, sct);

                        if (taken)
                        {
                            DateTime startdate = sct.IsFixedDate ? sct.Period.From : GameObject.GetInstance().GameTime;

                            var sc = new SpecialContract(sct, startdate, airline);
                            airline.SpecialContracts.Add(sc);

                            GameObject.GetInstance().Contracts.Remove(sct);
                        }

                        i++;
                    }
                }
            }

            foreach (SpecialContractType sct in fixedSpecialContracts)
            {
                if (sct.LastDate == DateTime.MinValue)
                    sct.LastDate = GameObject.GetInstance().GameTime;

                GameObject.GetInstance().Contracts.Add(sct);
            }

            foreach (SpecialContractType sct in randomSpecialContracts)
            {
                if (sct.LastDate == DateTime.MinValue)
                    sct.LastDate = GameObject.GetInstance().GameTime;

                int monthsSinceLast = MathHelpers.GetAgeMonths(sct.LastDate);

                int monthsFrequency = 12/sct.Frequency;

                //mf = 12, ms = 1 => procent = lille, mf = 6, ms = 6 => procent = medium, mf = 1, ms = 12 => procent = høj

                int value = 100 - (monthsSinceLast - monthsFrequency);

                Boolean createContract = !GameObject.GetInstance().Contracts.Contains(sct) && Rnd.Next(value) == 0;

                if (createContract)
                {
                    sct.LastDate = GameObject.GetInstance().GameTime;

                    GameObject.GetInstance().Contracts.Add(sct);
                }
            }
        }

        private static void DoMonthlyUpdate()
        {
            //Clear stats when it on monthly
            if (Infrastructure.Settings.GetInstance().ClearStats == Intervals.Monthly)
            {
                ClearAllUsedStats();
            }

            //Auto save when it on monthly
            if (Infrastructure.Settings.GetInstance().AutoSave == Intervals.Monthly)
            {
                SerializedLoadSaveHelpers.SaveGame("autosave");
            }

            //deletes all used airliners older than 30 years
            var oldAirliners =
                new List<Airliner>(
                    Airliners.GetAirlinersForSale(a => a.BuiltDate.Year <= GameObject.GetInstance().GameTime.Year - 30));

            /*
           
            //Set the amount if planes that should be made its decreased alot over time
            int upper = (Airlines.GetAllAirlines().Count - (gametime * 5)) / 2;
            int lower = (Airlines.GetAllAirlines().Count - (gametime * 5)) / 4;
            if (upper <= 0) { upper = 3; }
            if (lower <= 0) { lower = 1; }
            int airliners = rnd.Next(lower, upper);

            for (int i = 0; i < airliners; i++)
            {
                Airliners.AddAirliner(AirlinerHelpers.CreateAirlinerFromYear(GameObject.GetInstance().GameTime.Year - rnd.Next(1, 10)));
            }*/
            //creates some new used airliners

            foreach (Airliner airliner in oldAirliners)
            {
                Airliners.RemoveAirliner(airliner);
            }

            int numberOfAirliners = oldAirliners.Count + 2*Airlines.GetNumberOfAirlines();

            for (int i = 0; i < numberOfAirliners; i++)
            {
                Airliners.AddAirliner(
                    AirlinerHelpers.CreateAirlinerFromYear(GameObject.GetInstance().GameTime.Year - Rnd.Next(1, 10)));
            }
            //checks for new airports which are opening
            List<Airport> openedAirports =
                Airports.GetAllAirports(
                    a =>
                    a.Profile.Period.From.ToShortDateString()
                    == GameObject.GetInstance().GameTime.ToShortDateString());
            List<Airport> closedAirports =
                Airports.GetAllAirports(
                    a =>
                    a.Profile.Period.To.ToShortDateString() == GameObject.GetInstance().GameTime.ToShortDateString());

            foreach (Airport airport in openedAirports)
            {
                if (closedAirports.Find(a => a.Profile.Town == airport.Profile.Town) != null)
                {
                    AirportHelpers.ReallocateAirport(
                        closedAirports.Find(a => a.Profile.Town == airport.Profile.Town),
                        airport);
                }
                else
                {
                    PassengerHelpers.CreateDestinationPassengers(airport);
                }

                Airport airport1 = airport;
                foreach (
                    Airport dAirport in
                        Airports.GetAirports(
                            a =>
                            a != airport1 && a.Profile.Town != airport1.Profile.Town
                            && MathHelpers.GetDistance(
                                a.Profile.Coordinates.ConvertToGeoCoordinate(),
                                airport1.Profile.Coordinates.ConvertToGeoCoordinate()) > 25))
                {
                    PassengerHelpers.CreateDestinationPassengers(dAirport, airport);
                }

                Airport airport2 = airport;
                int count =
                    Airports.GetAirports(
                        a =>
                        a.Profile.Town == airport2.Profile.Town && airport2 != a
                        && a.Terminals.GetNumberOfGates(GameObject.GetInstance().MainAirline) > 0).Count;

                if (count == 1)
                {
                    Airport airport3 = airport;
                    Airport allocateFromAirport =
                        Airports.GetAirports(
                            a =>
                            a.Profile.Town == airport3.Profile.Town && airport3 != a
                            && a.Terminals.GetNumberOfGates(GameObject.GetInstance().HumanAirline) > 0).First();
                    GameObject.GetInstance()
                              .NewsBox.AddNews(
                                  new News(
                                      News.NewsType.AirportNews,
                                      GameObject.GetInstance().GameTime,
                                      "New airport opened",
                                      $"A new airport [LI airport={airport.Profile.IATACode}]({new AirportCodeConverter().Convert(airport)}) is opened in {airport.Profile.Town.Name}, {((Country) new CountryCurrentCountryConverter().Convert(airport.Profile.Country)).Name}.\n\rYou can reallocate all your operations from {allocateFromAirport.Profile.Name}({new AirportCodeConverter().Convert(allocateFromAirport)}) for free within the next 30 days"));
                }
                else
                {
                    GameObject.GetInstance()
                              .NewsBox.AddNews(
                                  new News(
                                      News.NewsType.AirportNews,
                                      GameObject.GetInstance().GameTime,
                                      "New airport opened",
                                      $"A new airport [LI airport={airport.Profile.IATACode}]({new AirportCodeConverter().Convert(airport)}) is opened in {airport.Profile.Town.Name}, {((Country) new CountryCurrentCountryConverter().Convert(airport.Profile.Country)).Name}"));
                }
            }
            //pays all codeshare agreements (one way)
            IEnumerable<CodeshareAgreement> agreements =
                Airlines.GetAllAirlines()
                        .SelectMany(a => a.Codeshares)
                        .Where(c => c.Type == CodeshareAgreement.CodeshareType.OneWay);

            foreach (CodeshareAgreement agreement in agreements)
            {
                double amount = AirlineHelpers.GetCodesharingPrice(agreement);
                AirlineHelpers.AddAirlineInvoice(
                    agreement.Airline1,
                    GameObject.GetInstance().GameTime,
                    Invoice.InvoiceType.AirlineExpenses,
                    -amount);
                AirlineHelpers.AddAirlineInvoice(
                    agreement.Airline2,
                    GameObject.GetInstance().GameTime,
                    Invoice.InvoiceType.AirlineExpenses,
                    amount);
            }
            //check if pilots are retireing
            const int retirementAge = Pilot.RetirementAge;

            //Parallel.ForEach(
            //    Airlines.GetAllAirlines(),
            //    airline =>
            foreach (Airline airline in Airlines.GetAllAirlines())
            {
                List<Pilot> pilotsToRetire =
                    airline.Pilots.FindAll(
                        p =>
                        p.Profile.Birthdate.AddYears(retirementAge).AddMonths(-1)
                        < GameObject.GetInstance().GameTime);
                var pilotsToRetirement =
                    new List<Pilot>(
                        airline.Pilots.FindAll(
                            p => p.Profile.Birthdate.AddYears(retirementAge) < GameObject.GetInstance().GameTime));

                IEnumerable<Instructor> instructorsToRetire =
                    airline.FlightSchools.SelectMany(f => f.Instructors)
                           .Where(
                               i =>
                               i.Profile.Birthdate.AddYears(retirementAge).AddMonths(-1)
                               < GameObject.GetInstance().GameTime);
                var instructorsToRetirement =
                    new List<Instructor>(
                        airline.FlightSchools.SelectMany(f => f.Instructors)
                               .Where(i => i.Profile.Birthdate.AddYears(retirementAge) < GameObject.GetInstance().GameTime));

                foreach (Pilot pilot in pilotsToRetire)
                {
                    if (airline.IsHuman)
                    {
                        GameObject.GetInstance()
                                  .NewsBox.AddNews(
                                      new News(
                                          News.NewsType.FlightNews,
                                          GameObject.GetInstance().GameTime,
                                          Translator.GetInstance().GetString("News", "1007"),
                                          string.Format(
                                              Translator.GetInstance().GetString("News", "1007", "message"),
                                              pilot.Profile.Name,
                                              pilot.Profile.Age + 1)));
                    }
                    else
                    {
                        if (pilot.Airliner != null)
                        {
                            if (Pilots.GetNumberOfUnassignedPilots() == 0)
                            {
                                GeneralHelpers.CreatePilots(10);
                            }

                            Pilot newPilot =
                                Pilots.GetUnassignedPilots()
                                      .Find(
                                          p =>
                                          p.Rating == pilot.Rating
                                          && p.Aircrafts.Contains(pilot.Airliner.Airliner.Type.AirlinerFamily));

                            if (newPilot == null)
                            {
                                GeneralHelpers.CreatePilots(4, pilot.Airliner.Airliner.Type.AirlinerFamily);
                                Pilot pilot1 = pilot;
                                newPilot =
                                    Pilots.GetUnassignedPilots(
                                        p => p.Aircrafts.Contains(pilot1.Airliner.Airliner.Type.AirlinerFamily))[0];
                            }

                            airline.AddPilot(newPilot);

                            newPilot.Airliner = pilot.Airliner;
                            newPilot.Airliner.AddPilot(newPilot);

                            pilot.Airliner.RemovePilot(pilot);
                            pilot.Airliner = null;
                        }
                    }
                }

                foreach (Instructor instructor in instructorsToRetire)
                {
                    if (airline.IsHuman)
                    {
                        GameObject.GetInstance()
                                  .NewsBox.AddNews(
                                      new News(
                                          News.NewsType.FlightNews,
                                          GameObject.GetInstance().GameTime,
                                          Translator.GetInstance().GetString("News", "1008"),
                                          string.Format(
                                              Translator.GetInstance().GetString("News", "1008", "message"),
                                              instructor.Profile.Name,
                                              instructor.Profile.Age + 1)));
                    }
                }

                foreach (Pilot pilot in pilotsToRetirement)
                {
                    if (pilot.Airliner != null)
                    {
                        //pilot.Airliner.Status = FleetAirliner.AirlinerStatus.Stopped;

                        if (airline.IsHuman)
                        {
                            GameObject.GetInstance()
                                      .NewsBox.AddNews(
                                          new News(
                                              News.NewsType.FlightNews,
                                              GameObject.GetInstance().GameTime,
                                              Translator.GetInstance().GetString("News", "1010"),
                                              string.Format(
                                                  Translator.GetInstance().GetString("News", "1010", "message"),
                                                  pilot.Profile.Name,
                                                  pilot.Airliner.Name)));
                            if (pilot.Airliner != null)
                            {
                                if (Pilots.GetNumberOfUnassignedPilots() == 0)
                                {
                                    GeneralHelpers.CreatePilots(10);
                                }

                                if (GameObject.GetInstance().HumanAirline.Pilots.Exists(p => p.Airliner == null))
                                {
                                    Pilot newPilot =
                                        GameObject.GetInstance().HumanAirline.Pilots.Find(p => p.Airliner == null);

                                    newPilot.Airliner = pilot.Airliner;
                                    newPilot.Airliner.AddPilot(newPilot);
                                }
                                else
                                {
                                    Pilot pilot1 = pilot;
                                    List<Pilot> pilots =
                                        Pilots.GetUnassignedPilots(
                                            p =>
                                            p.Profile.Town.Country
                                            == pilot1.Airliner.Airliner.Airline.Profile.Country
                                            && p.Aircrafts.Contains(pilot1.Airliner.Airliner.Type.AirlinerFamily));

                                    if (pilots.Count == 0)
                                    {
                                        Pilot pilot2 = pilot;
                                        pilots =
                                            Pilots.GetUnassignedPilots(
                                                p =>
                                                p.Profile.Town.Country.Region
                                                == pilot2.Airliner.Airliner.Airline.Profile.Country.Region
                                                && p.Aircrafts.Contains(pilot2.Airliner.Airliner.Type.AirlinerFamily));
                                    }

                                    if (pilots.Count == 0)
                                    {
                                        GeneralHelpers.CreatePilots(4, pilot.Airliner.Airliner.Type.AirlinerFamily);
                                        Pilot pilot2 = pilot;
                                        pilots =
                                            Pilots.GetUnassignedPilots(
                                                p => p.Aircrafts.Contains(pilot2.Airliner.Airliner.Type.AirlinerFamily));
                                    }

                                    Pilot newPilot = pilots.First();

                                    pilot.Airliner.Airliner.Airline.AddPilot(newPilot);
                                    newPilot.Airliner = pilot.Airliner;
                                    newPilot.Airliner.AddPilot(newPilot);
                                }
                            }
                        }
                        pilot.Airliner.RemovePilot(pilot);
                    }
                    else
                    {
                        if (airline.IsHuman)
                        {
                            GameObject.GetInstance()
                                      .NewsBox.AddNews(
                                          new News(
                                              News.NewsType.FlightNews,
                                              GameObject.GetInstance().GameTime,
                                              Translator.GetInstance().GetString("News", "1009"),
                                              string.Format(
                                                  Translator.GetInstance().GetString("News", "1009", "message"),
                                                  pilot.Profile.Name)));
                        }
                    }
                    airline.RemovePilot(pilot);
                    pilot.Airline = null;
                    Pilots.RemovePilot(pilot);
                }

                foreach (Instructor instructor in instructorsToRetirement)
                {
                    int studentsCapacity = (instructor.FlightSchool.Instructors.Count - 1)
                                           *FlightSchool.MaxNumberOfStudentsPerInstructor;

                    if (instructor.Students.Count > 0)
                    {
                        if (instructor.FlightSchool.NumberOfStudents > studentsCapacity)
                        {
                            Instructor newInstructor;

                            if (airline.IsHuman)
                            {
                                newInstructor =
                                    Instructors.GetUnassignedInstructors().OrderBy(i => i.Rating).ToList()[0];
                            }
                            else
                            {
                                newInstructor =
                                    Instructors.GetUnassignedInstructors()[
                                        Rnd.Next(Instructors.GetUnassignedInstructors().Count)];
                            }

                            foreach (PilotStudent student in instructor.Students)
                            {
                                student.Instructor = newInstructor;
                            }

                            newInstructor.FlightSchool = instructor.FlightSchool;
                            newInstructor.FlightSchool.AddInstructor(newInstructor);
                            instructor.Students.Clear();

                            if (airline.IsHuman)
                            {
                                GameObject.GetInstance()
                                          .NewsBox.AddNews(
                                              new News(
                                                  News.NewsType.FlightNews,
                                                  GameObject.GetInstance().GameTime,
                                                  Translator.GetInstance().GetString("News", "1012"),
                                                  string.Format(
                                                      Translator.GetInstance().GetString("News", "1012", "message"),
                                                      instructor.Profile.Name,
                                                      newInstructor.Profile.Name)));
                            }
                        }
                        else
                        {
                            while (instructor.Students.Count > 0)
                            {
                                PilotStudent student = instructor.Students[0];

                                Instructor newInstructor =
                                    instructor.FlightSchool.Instructors.Find(
                                        i =>
                                        i.Students.Count < FlightSchool.MaxNumberOfStudentsPerInstructor
                                        && i != instructor);
                                newInstructor.AddStudent(student);
                                student.Instructor = newInstructor;

                                instructor.RemoveStudent(student);
                            }

                            if (airline.IsHuman)
                            {
                                GameObject.GetInstance()
                                          .NewsBox.AddNews(
                                              new News(
                                                  News.NewsType.FlightNews,
                                                  GameObject.GetInstance().GameTime,
                                                  Translator.GetInstance().GetString("News", "1011"),
                                                  string.Format(
                                                      Translator.GetInstance().GetString("News", "1011", "message"),
                                                      instructor.Profile.Name)));
                            }
                        }
                    }
                    else
                    {
                        if (airline.IsHuman)
                        {
                            GameObject.GetInstance()
                                      .NewsBox.AddNews(
                                          new News(
                                              News.NewsType.FlightNews,
                                              GameObject.GetInstance().GameTime,
                                              Translator.GetInstance().GetString("News", "1011"),
                                              string.Format(
                                                  Translator.GetInstance().GetString("News", "1011", "message"),
                                                  instructor.Profile.Name)));
                        }
                    }

                    instructor.FlightSchool.RemoveInstructor(instructor);
                    instructor.FlightSchool = null;

                    Instructors.RemoveInstructor(instructor);
                }

                //wages
                foreach (Pilot pilot in airline.Pilots)
                {
                    double salary = AirlineHelpers.GetPilotSalary(airline, pilot);
                    AirlineHelpers.AddAirlineInvoice(
                        airline,
                        GameObject.GetInstance().GameTime,
                        Invoice.InvoiceType.Wages,
                        -salary);

                    pilot.Airliner?.Data.AddOperatingValue(new OperatingValue("Salary", GameObject.GetInstance().GameTime.Year, GameObject.GetInstance().GameTime.Month, -salary));
                }

                foreach (Instructor instructor in airline.FlightSchools.SelectMany(f => f.Instructors))
                {
                    double salary = instructor.Rating.CostIndex
                                    *airline.Fees.GetValue(FeeTypes.GetType("Instructor Base Salary"));
                    AirlineHelpers.AddAirlineInvoice(
                        airline,
                        GameObject.GetInstance().GameTime,
                        Invoice.InvoiceType.Wages,
                        -salary);
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
                {
                    AirlineHelpers.AddAirlineInvoice(
                        airline,
                        GameObject.GetInstance().GameTime,
                        Invoice.InvoiceType.AirlineExpenses,
                        -facility.MonthlyCost);
                }

                foreach (FleetAirliner airliner in
                    airline.Fleet.FindAll(
                        (a => a.Purchased == FleetAirliner.PurchasedType.Leased)))
                {
                    AirlineHelpers.AddAirlineInvoice(
                        airline,
                        GameObject.GetInstance().GameTime,
                        Invoice.InvoiceType.Rents,
                        -airliner.Airliner.LeasingPrice);

                    if (airliner.Airliner.Owner != null)
                    {
                        AirlineHelpers.AddAirlineInvoice(
                            airliner.Airliner.Owner,
                            GameObject.GetInstance().GameTime,
                            Invoice.InvoiceType.Rents,
                            airliner.Airliner.LeasingPrice);
                    }

                    airliner.Data.AddOperatingValue(new OperatingValue("Leasing Expenses", GameObject.GetInstance().GameTime.Year, GameObject.GetInstance().GameTime.Month,
                                                                       -airliner.Airliner.LeasingPrice));
                }

                foreach (Airport airport in airline.Airports)
                {
                    var contracts = new List<AirportContract>(airport.GetAirlineContracts(airline));

                    double contractPrice = contracts.Sum(c => c.YearlyPayment)/12;

                    AirlineHelpers.AddAirlineInvoice(
                        airline,
                        GameObject.GetInstance().GameTime,
                        Invoice.InvoiceType.Rents,
                        -contractPrice);

                    long airportIncome = Convert.ToInt64(contractPrice);
                    airport.Income += airportIncome;

                    //expired contracts
                    var airlineContracts =
                        new List<AirportContract>(
                            airport.AirlineContracts.FindAll(
                                c => c.ExpireDate < GameObject.GetInstance().GameTime.AddMonths(2)));

                    foreach (AirportContract contract in airlineContracts)
                    {
                        if (contract.Airline.IsHuman)
                        {
                            GameObject.GetInstance()
                                      .NewsBox.AddNews(
                                          new News(
                                              News.NewsType.AirportNews,
                                              GameObject.GetInstance().GameTime,
                                              "Airport contract expires",
                                              $"Your contract for {contract.NumberOfGates} gates at [LI airport={contract.Airport.Profile.IATACode}], {contract.Airport.Profile.Country.Name} expires soon. Please extend the contracts."));
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
                            {
                                airport.RemoveAirlineContract(contract);
                            }
                        }
                    }

                    //wages
                    foreach (AirportFacility facility in airport.GetCurrentAirportFacilities(airline))
                    {
                        double wage = 0;
                        int employees = facility.NumberOfEmployees;

                        if (facility.Type == AirportFacility.FacilityType.Service)
                        {
                            //the more aircrafts the more employees
                            int aircrafts = airline.Fleet.Count(a => a.Homebase == airport);
                            employees += aircrafts;
                        }

                        if (facility.EmployeeType == AirportFacility.EmployeeTypes.Maintenance)
                        {
                            wage = airline.Fees.GetValue(FeeTypes.GetType("Maintenance Wage"));
                        }

                        if (facility.EmployeeType == AirportFacility.EmployeeTypes.Support)
                        {
                            wage = airline.Fees.GetValue(FeeTypes.GetType("Support Wage"));
                        }

                        double facilityWage = employees*wage*(40*4.33);
                        //40 hours per week and 4.33 weeks per month

                        AirlineHelpers.AddAirlineInvoice(
                            airline,
                            GameObject.GetInstance().GameTime,
                            Invoice.InvoiceType.Wages,
                            -facilityWage);
                    }
                    //passenger growth if ticket office
                    //checking if someone in the alliance has a ticket office for the route (if in an alliance).
                    if (airline.Alliances.Count > 0)
                    {
                        int highest =
                            airline.Alliances.SelectMany(a => a.Members)
                                   .Select(m => m.Airline)
                                   .Max(
                                       m =>
                                       airport.GetAirlineAirportFacility(m, AirportFacility.FacilityType.TicketOffice)
                                              .Facility.ServiceLevel);
                        Airport airport1 = airport;
                        bool hasTicketOffice =
                            airline.Alliances.SelectMany(a => a.Members)
                                   .Select(m => m.Airline)
                                   .Where(
                                       m =>
                                       airport1.GetAirlineAirportFacility(m, AirportFacility.FacilityType.TicketOffice)
                                              .Facility.TypeLevel > 0) != null;

                        //If there is an service level update the routes
                        if (hasTicketOffice)
                        {
                            Airport airport2 = airport;
                            foreach (Route route in
                                airline.Routes.Where(r => r.Destination1 == airport2 || r.Destination2 == airport2))
                            {
                                Airport destination = airport == route.Destination1
                                                          ? route.Destination2
                                                          : route.Destination1;
                                airport.AddDestinationPassengersRate(destination, (ushort) highest);
                            }
                        }
                    }


                        //not in an alliance so we will look for the airline only
                    else
                    {
                        if (
                            airport.GetAirlineAirportFacility(airline, AirportFacility.FacilityType.TicketOffice)
                                   .Facility.TypeLevel > 0
                            || airport.HasContractType(airline, AirportContract.ContractType.FullService)
                            || airport.HasContractType(airline, AirportContract.ContractType.MediumService))
                        {
                            Airport airport1 = airport;
                            foreach (Route route in
                                airline.Routes.Where(r => r.Destination1 == airport1 || r.Destination2 == airport1))
                            {
                                Airport destination = airport == route.Destination1
                                                          ? route.Destination2
                                                          : route.Destination1;

                                airport.AddDestinationPassengersRate(
                                    destination,
                                    (ushort)
                                    airport.GetAirlineAirportFacility(
                                        airline,
                                        AirportFacility.FacilityType.TicketOffice).Facility.ServiceLevel);
                            }
                        }
                    }
                }
                //passenger demand
                int advertisementFactor = airline.GetAirlineAdvertisements().Sum(a => a.ReputationLevel);

                foreach (Route route in airline.Routes)
                {
                    route.Destination1.AddDestinationPassengersRate(
                        route.Destination2,
                        (ushort) (5*advertisementFactor));
                    route.Destination2.AddDestinationPassengersRate(
                        route.Destination1,
                        (ushort) (5*advertisementFactor));
                }
                foreach (Loan loan in airline.Loans)
                {
                    if (loan.IsActive)
                    {
                        double amount = Math.Min(loan.GetMonthlyPayment(), loan.PaymentLeft);

                        AirlineHelpers.AddAirlineInvoice(
                            airline,
                            GameObject.GetInstance().GameTime,
                            Invoice.InvoiceType.Loans,
                            -amount);

                        loan.PaymentLeft -= amount;
                    }
                }
                // chs, 2011-17-10 change so it only looks at the advertisement types which are invented at the time
                foreach (AdvertisementType.AirlineAdvertisementType type in
                    Enum.GetValues(typeof (AdvertisementType.AirlineAdvertisementType)))
                {
                    if (GameObject.GetInstance().GameTime.Year >= (int) type)
                    {
                        AirlineHelpers.AddAirlineInvoice(
                            airline,
                            GameObject.GetInstance().GameTime,
                            Invoice.InvoiceType.AirlineExpenses,
                            -airline.GetAirlineAdvertisement(type).Price);

                        if (airline.Reputation < 100)
                        {
                            airline.Reputation += airline.GetAirlineAdvertisement(type).ReputationLevel;
                        }
                    }
                }
                //maintenance, insurance, and ratings

                AirlineInsuranceHelpers.CheckExpiredInsurance(airline);
                if (airline.InsurancePolicies != null)
                {
                    AirlineInsuranceHelpers.MakeInsurancePayment(airline);
                }

                RandomEvent.CheckExpired();
            } //);

            if (Pilots.GetNumberOfUnassignedPilots() < 25)
            {
                GeneralHelpers.CreatePilots(100);
            }

            if (Instructors.GetNumberOfUnassignedInstructors() < 20)
            {
                GeneralHelpers.CreateInstructors(75);
            }

            //checks if the airport will increase the number of gates either by new terminal or by extending existing

            //Parallel.ForEach(
            //    Airports.GetAllAirports(a => a.Terminals.getInusePercent() > 90),
            //    airport => { AirportHelpers.CheckForExtendGates(airport); });
            foreach (Airport airport in Airports.GetAllAirports(a => a.Terminals.GetInusePercent(Terminal.TerminalType.Passenger) > 90))
            {
                AirportHelpers.CheckForExtendGates(airport);
            }

            long longestRequiredRunwayLenght =
                AirlinerTypes.GetTypes(
                    a =>
                    a.Produced.From <= GameObject.GetInstance().GameTime
                    && a.Produced.To >= GameObject.GetInstance().GameTime).Max(a => a.MinRunwaylength);


            foreach (
                Airport airport in
                    Airports.GetAllAirports(
                        a =>
                        a.Runways.Count > 0
                        && a.Runways.Select(r => r.Length).Max() < longestRequiredRunwayLenght/2))

                //Parallel.ForEach(
                //    Airports.GetAllAirports(
                //        a => a.Runways.Count > 0 && a.Runways.Select(r => r.Length).Max() < longestRequiredRunwayLenght / 2),
                //    airport =>
            {
                AirportHelpers.CheckForExtendRunway(airport);

                foreach (Cooperation cooperation in airport.Cooperations)
                {
                    AirlineHelpers.AddAirlineInvoice(
                        cooperation.Airline,
                        GameObject.GetInstance().GameTime,
                        Invoice.InvoiceType.Purchases,
                        -cooperation.Type.MonthlyPrice);
                }
            } //);

            foreach (Airport airport in Airports.GetAllAirports(a => AirportHelpers.GetAirportRoutes(a).Count > 0))
            {
                double airportRoutes = AirportHelpers.GetAirportRoutes(airport).Count;
                double growth = Math.Min(0.5*airportRoutes, 2);

                PassengerHelpers.ChangePaxDemand(airport, growth);
            }

            if (GameObject.GetInstance().Scenario != null)
            {
                ScenarioHelpers.UpdateScenario(GameObject.GetInstance().Scenario);
            }

            CreateMontlySummary();
        }

        private static void DoYearlyUpdate()
        {
            //Clear stats when it on yearly
            if (Infrastructure.Settings.GetInstance().ClearStats == Intervals.Yearly)
            {
                ClearAllUsedStats();
            }

            //Auto save when it on yearly
            if (Infrastructure.Settings.GetInstance().AutoSave == Intervals.Yearly)
            {
                SerializedLoadSaveHelpers.SaveGame("autosave");
            }

            AirlineHelpers.ClearRoutesStatistics();
            //updates holidays 
            GeneralHelpers.CreateHolidays(GameObject.GetInstance().GameTime.Year);

            const double yearlyRaise = 1.03; //3 % 

            foreach (Airline airline in Airlines.GetAllAirlines())
            {
                foreach (FleetAirliner airliner in airline.Fleet)
                {
                    AirlineHelpers.AddAirlineInvoice(
                        airline,
                        GameObject.GetInstance().GameTime,
                        Invoice.InvoiceType.Maintenances,
                        -airliner.Airliner.Type.GetMaintenance());
                }

                foreach (FeeType feeType in FeeTypes.GetTypes())
                {
                    airline.Fees.SetValue(feeType, airline.Fees.GetValue(feeType)*yearlyRaise);
                }
            }

            //increases the passenger demand between airports with up to 5%
            //Parallel.ForEach(
            //    Airports.GetAllActiveAirports(),
            //    airport =>
            foreach (Airport airport in Airports.GetAllActiveAirports())
            {
                foreach (DestinationDemand destPax in airport.GetDestinationsPassengers())
                {
                    destPax.Rate = (ushort) (destPax.Rate*MathHelpers.GetRandomDoubleNumber(0.97, 1.05));
                }
            } //);

            //removes the oldest pilots/instructors and creates some new ones
            List<Pilot> oldPilots = Pilots.GetUnassignedPilots().OrderByDescending(p => p.Profile.Age).ToList();
            List<Instructor> oldInstructors =
                Instructors.GetUnassignedInstructors().OrderByDescending(i => i.Profile.Age).ToList();

            for (int i = 0; i < Math.Min(15, oldPilots.Count); i++)
            {
                Pilots.RemovePilot(oldPilots[i]);
            }

            for (int i = 0; i < Math.Min(10, oldInstructors.Count); i++)
            {
                Instructors.RemoveInstructor(oldInstructors[i]);
            }

            GeneralHelpers.CreatePilots(15);
            GeneralHelpers.CreateInstructors(10);

            foreach (Airline airline in Airlines.GetAllAirlines())
            {
                EventsHelpers.GenerateEvents(airline);
            }
        }

        //simualtes an airliner for service

        //gets the weather for an airliner
        private static Weather GetAirlinerWeather(FleetAirliner airliner)
        {
            double distance = MathHelpers.GetDistance(
                airliner.CurrentPosition,
                airliner.CurrentFlight.Entry.Destination.Airport);
            Airport dest = airliner.CurrentFlight.Entry.Destination.Airport;
            Airport dept = airliner.CurrentFlight.GetDepartureAirport();

            double totalDistance = MathHelpers.GetDistance(
                dept.Profile.Coordinates.ConvertToGeoCoordinate(),
                dest.Profile.Coordinates.ConvertToGeoCoordinate());

            return distance > totalDistance/2 ? dept.Weather[0] : dest.Weather[0];
        }

        //sets the next flight for a route airliner

        //finds the next flight time for an airliner - checks also for delay
        private static DateTime GetNextFlightTime(FleetAirliner airliner)
        {
            if (airliner.CurrentFlight == null)
            {
                SetNextFlight(airliner);
                if (airliner.CurrentFlight != null) return airliner.CurrentFlight.FlightTime;
            }
            if (airliner.CurrentFlight != null && airliner.CurrentFlight.Entry.TimeTable.Route.Banned)
            {
                SetNextFlight(airliner);
                if (airliner.CurrentFlight.Entry.TimeTable.Route.Banned)
                {
                    airliner.Status = FleetAirliner.AirlinerStatus.Stopped;
                    return new DateTime(2500, 1, 1);
                }
                return airliner.CurrentFlight.FlightTime;
            }
            return airliner.CurrentFlight?.FlightTime ?? new DateTime();
        }

        //returns the next route for an airliner 
        private static Route GetNextRoute(FleetAirliner airliner)
        {
            IOrderedEnumerable<RouteTimeTableEntry> entries =
                from e in
                    airliner.Routes.Where(r => r.StartDate <= GameObject.GetInstance().GameTime)
                            .Select(r => r.TimeTable.GetNextEntry(GameObject.GetInstance().GameTime, airliner))
                where e != null
                orderby MathHelpers.ConvertEntryToDate(e)
                select e;

            if (entries.Any())
            {
                return entries.First().TimeTable.Route;
            }
            return null;
        }

        private static int GetWindInfluence(FleetAirliner airliner)
        {
            double direction =
                MathHelpers.GetDirection(
                    airliner.CurrentPosition.Profile.Coordinates.ConvertToGeoCoordinate(),
                    airliner.CurrentFlight.GetNextDestination().Profile.Coordinates.ConvertToGeoCoordinate());

            Weather.WindDirection windDirection = MathHelpers.GetWindDirectionFromDirection(direction);

            Weather currentWeather = GetAirlinerWeather(airliner);
            //W+E = 0+4= 5, N+S=2+6 - = Abs(Count/2) -> Head, Abs(0) -> Tail -> if ends/starts with same => tail, indexof +-1 -> tail, (4+(indexof))+-1 -> head 

            int windDirectionLenght = Enum.GetValues(typeof (Weather.WindDirection)).Length;
            int indexCurrentPosition = Array.IndexOf(Enum.GetValues(typeof (Weather.WindDirection)), windDirection);
            //int indexWeather = Array.IndexOf(Enum.GetValues(typeof(Weather.WindDirection)),currentWeather.WindSpeed);

            //check for tail wind
            Weather.WindDirection windTailLeft = indexCurrentPosition > 0
                                                     ? (Weather.WindDirection) indexCurrentPosition - 1
                                                     : (Weather.WindDirection) windDirectionLenght - 1;
            Weather.WindDirection windTailRight = indexCurrentPosition < windDirectionLenght - 1
                                                      ? (Weather.WindDirection) indexCurrentPosition + 1
                                                      : 0;

            if (windTailLeft == currentWeather.Direction || windTailRight == currentWeather.Direction
                || windDirection == currentWeather.Direction)
            {
                return 1;
            }

            Weather.WindDirection windOpposite = indexCurrentPosition - (windDirectionLenght/2) > 0
                                                     ? (Weather.WindDirection) indexCurrentPosition - (windDirectionLenght/2)
                                                     : (Weather.WindDirection) windDirectionLenght - 1 - indexCurrentPosition - (windDirectionLenght/2);
            int indexOpposite = Array.IndexOf(Enum.GetValues(typeof (Weather.WindDirection)), windOpposite);

            Weather.WindDirection windHeadLeft = indexOpposite > 0
                                                     ? (Weather.WindDirection) indexOpposite - 1
                                                     : (Weather.WindDirection) windDirectionLenght - 1;
            Weather.WindDirection windHeadRight = indexOpposite < windDirectionLenght - 1
                                                      ? (Weather.WindDirection) indexOpposite + 1
                                                      : 0;

            if (windHeadLeft == currentWeather.Direction || windHeadRight == currentWeather.Direction
                || windOpposite == currentWeather.Direction)
            {
                return -1;
            }

            return 0;
        }

        private static void SetNextFlight(FleetAirliner airliner)
        {
            Route route = GetNextRoute(airliner);

            if ((airliner.CurrentFlight == null && route != null && route.HasStopovers)
                || (airliner.CurrentFlight is StopoverFlight
                    && ((StopoverFlight) airliner.CurrentFlight).IsLastTrip)
                || (airliner.CurrentFlight != null && airliner.CurrentFlight.Entry.MainEntry == null && route != null
                    && route.HasStopovers))
            {
                if (route != null)
                    airliner.CurrentFlight = airliner.GroundedToDate > GameObject.GetInstance().GameTime ? new StopoverFlight(route.TimeTable.GetNextEntry(airliner.GroundedToDate, airliner)) : new StopoverFlight(route.TimeTable.GetNextEntry(GameObject.GetInstance().GameTime, airliner));
            }

            airliner.Status = FleetAirliner.AirlinerStatus.ToRouteStart;

            if (airliner.CurrentFlight is StopoverFlight && !((StopoverFlight) airliner.CurrentFlight).IsLastTrip)
            {
                ((StopoverFlight) airliner.CurrentFlight).SetNextEntry();
                //airliner.CurrentFlight.FlightTime = new DateTime(Math.Max(GameObject.GetInstance().GameTime.Add(RouteTimeTable.MinTimeBetweenFlights).Ticks, airliner.CurrentFlight.FlightTime.Ticks));
            }
            else
            {
                if (route != null)
                {
                    airliner.CurrentFlight = airliner.GroundedToDate > GameObject.GetInstance().GameTime ? new Flight(route.TimeTable.GetNextEntry(airliner.GroundedToDate, airliner)) : new Flight(route.TimeTable.GetNextEntry(GameObject.GetInstance().GameTime, airliner));
                }
                else
                {
                    airliner.Status = FleetAirliner.AirlinerStatus.ToHomebase;
                }
            }
        }

        private static void SimulateService(FleetAirliner airliner)
        {
            double servicePrice = 10000;

            airliner.CurrentPosition = airliner.CurrentFlight.Entry.Destination.Airport;
            // new GeoCoordinate(airliner.CurrentFlight.Entry.Destination.Airport.Profile.Coordinates.Latitude, airliner.CurrentFlight.Entry.Destination.Airport.Profile.Coordinates.Longitude);
            airliner.Status = FleetAirliner.AirlinerStatus.ToRouteStart;

            double fdistance = MathHelpers.GetDistance(
                airliner.CurrentFlight.Entry.Destination.Airport,
                airliner.CurrentPosition);
            double expenses = AirportHelpers.GetFuelPrice(airliner.CurrentPosition)*fdistance
                              *airliner.CurrentFlight.GetTotalPassengers()*airliner.Airliner.FuelConsumption
                              + AirportHelpers.GetLandingFee(airliner.CurrentPosition, airliner.Airliner);

            servicePrice += expenses;

            airliner.Airliner.Flown += fdistance;

            airliner.Airliner.LastServiceCheck = airliner.Airliner.Flown;

            AirlineHelpers.AddAirlineInvoice(
                airliner.Airliner.Airline,
                GameObject.GetInstance().GameTime,
                Invoice.InvoiceType.Maintenances,
                -servicePrice);

            airliner.CurrentFlight.Entry.TimeTable.Route.AddRouteInvoice(
                new Invoice(GameObject.GetInstance().GameTime, Invoice.InvoiceType.Maintenances, -servicePrice));
            airliner.Statistics.AddStatisticsValue(
                GameObject.GetInstance().GameTime.Year,
                StatisticsTypes.GetStatisticsType("Airliner_Income"),
                -servicePrice);

            SetNextFlight(airliner);
        }

        private static void SimulateTakeOff(FleetAirliner airliner)
        {
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

                SetNextFlight(airliner);
            }
            else
            {
                airliner.CurrentFlight.AddDelayMinutes(delayedMinutes.Value);

                if (delayedMinutes.Value > 0)
                {
                    airliner.CurrentFlight.IsOnTime = false;
                }
            }

            if (airliner.CurrentFlight.FlightTime <= GameObject.GetInstance().GameTime)
            {
                if (AirportHelpers.HasBadWeather(airliner.CurrentFlight.Entry.DepartureAirport)
                    || AirportHelpers.HasBadWeather(airliner.CurrentFlight.Entry.Destination.Airport))
                {
                    if (airliner.Airliner.Airline.IsHuman)
                    {
                        Airport airport = AirportHelpers.HasBadWeather(airliner.CurrentFlight.Entry.Destination.Airport)
                                              ? airliner.CurrentFlight.Entry.Destination.Airport
                                              : airliner.CurrentFlight.Entry.DepartureAirport;
                        GameObject.GetInstance()
                                  .NewsBox.AddNews(
                                      new News(
                                          News.NewsType.FlightNews,
                                          GameObject.GetInstance().GameTime,
                                          Translator.GetInstance().GetString("News", "1003"),
                                          string.Format(
                                              Translator.GetInstance().GetString("News", "1003", "message"),
                                              airliner.Airliner.TailNumber,
                                              airport.Profile.IATACode)));
                    }
                    SetNextFlight(airliner);
                }
                else
                {
                    airliner.Status = FleetAirliner.AirlinerStatus.OnRoute;

                    if (airliner.CurrentFlight.Entry.MainEntry == null)
                    {
                        if (airliner.CurrentFlight.IsPassengerFlight())
                        {
                            var classes = new List<AirlinerClass>(airliner.Airliner.Classes);

                            foreach (AirlinerClass aClass in classes)
                            {
                                airliner.CurrentFlight.Classes.Add(
                                    new FlightAirlinerClass(
                                        ((PassengerRoute) airliner.CurrentFlight.Entry.TimeTable.Route)
                                            .GetRouteAirlinerClass(aClass.Type),
                                        GetPassengers(airliner, aClass.Type)));
                            }
                        }
                        if (airliner.CurrentFlight.IsCargoFlight())
                        {
                            airliner.CurrentFlight.Cargo = PassengerHelpers.GetFlightCargo(airliner);
                        }
                    }
                    else
                    {
                        airliner.Status = FleetAirliner.AirlinerStatus.OnRoute;
                    }
                }
            }
        }

        private static void SimulateToHomebase(FleetAirliner airliner)
        {
            airliner.CurrentPosition = airliner.CurrentFlight.Entry.Destination.Airport;
            //new GeoCoordinate(airliner.CurrentFlight.Entry.Destination.Airport.Profile.Coordinates.Latitude, airliner.CurrentFlight.Entry.Destination.Airport.Profile.Coordinates.Longitude);
            Airport airport = airliner.CurrentPosition;
            airliner.Status = FleetAirliner.AirlinerStatus.ToHomebase;

            if (
                !airliner.CurrentFlight.Entry.Destination.Airport.Profile.Coordinates.Equals(
                    airliner.Homebase.Profile.Coordinates))
            {
                airliner.Status = FleetAirliner.AirlinerStatus.Stopped;
            }
            else
            {
                airliner.CurrentFlight =
                    new Flight(
                        new RouteTimeTableEntry(
                            airliner.CurrentFlight.Entry.TimeTable,
                            GameObject.GetInstance().GameTime.DayOfWeek,
                            GameObject.GetInstance().GameTime.TimeOfDay,
                            new RouteEntryDestination(airliner.Homebase, "Service", null)));
            }
        }

        private static void UpdateAirliner(FleetAirliner airliner)
        {
            if (airliner.HasRoute)
            {
                switch (airliner.Status)
                {
                    case FleetAirliner.AirlinerStatus.OnRoute:
                        UpdateOnRouteAirliner(airliner);
                        break;
                    case FleetAirliner.AirlinerStatus.OnService:
                        UpdateOnRouteAirliner(airliner);
                        break;
                    case FleetAirliner.AirlinerStatus.ToHomebase:
                        UpdateOnRouteAirliner(airliner);
                        break;
                    case FleetAirliner.AirlinerStatus.ToRouteStart:
                        UpdateToRouteStartAirliner(airliner);
                        break;

                    case FleetAirliner.AirlinerStatus.Resting:
                        DateTime nextFlightTime = GetNextFlightTime(airliner);
                        if (nextFlightTime <= GameObject.GetInstance().GameTime)
                        {
                            SimulateTakeOff(airliner);
                        }
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

                if (route?.TimeTable.GetNextEntry(GameObject.GetInstance().GameTime, airliner.CurrentPosition) == null)
                {
                    airliner.CurrentFlight = null;
                }
                else
                {
                    airliner.CurrentFlight =
                        new Flight(
                            route.TimeTable.GetNextEntry(GameObject.GetInstance().GameTime, airliner.CurrentPosition));
                }
            }
            if (airliner.CurrentFlight != null)
            {
                double adistance = airliner.CurrentFlight.DistanceToDestination;
                //airliner.CurrentPosition.GetDistanceTo(airliner.CurrentFlight.Entry.Destination.Airport.Profile.Coordinates);

                double speed = airliner.Airliner.CruisingSpeed;

                /* Leaving it out until we get a position implemented agian
                Weather currentWeather = GetAirlinerWeather(airliner);
                int wind = GetWindInfluence(airliner) * (int)currentWeather.WindSpeed;
                speed = airliner.Airliner.Type.CruisingSpeed  + wind;
                */

                if (adistance > 4)
                {
                    MathHelpers.MoveObject(airliner, speed);
                }

                double distance =
                    MathHelpers.GetDistance(
                        airliner.CurrentPosition.Profile.Coordinates.ConvertToGeoCoordinate(),
                        airliner.CurrentFlight.Entry.Destination.Airport.Profile.Coordinates.ConvertToGeoCoordinate());

                if (airliner.CurrentFlight.DistanceToDestination < 5)
                {
                    if (airliner.Status == FleetAirliner.AirlinerStatus.OnRoute)
                    {
                        SimulateLanding(airliner);
                    }
                    else if (airliner.Status == FleetAirliner.AirlinerStatus.OnService)
                    {
                        SimulateService(airliner);
                    }
                    else if (airliner.Status == FleetAirliner.AirlinerStatus.ToHomebase)
                    {
                        SimulateToHomebase(airliner);
                    }
                    //else if (airliner.Status == FleetAirliner.AirlinerStatus.To_route_start)
                    //  SimulateRouteStart(airliner);
                }
            }
            else
            {
                airliner.Status = FleetAirliner.AirlinerStatus.ToRouteStart;
            }
        }

        //the method for updating a route airliner with status toroutestart
        private static void UpdateToRouteStartAirliner(FleetAirliner airliner)
        {
            if (airliner.CurrentFlight == null)
            {
                Route route = GetNextRoute(airliner);

                if (route == null)
                {
                    airliner.Status = FleetAirliner.AirlinerStatus.ToHomebase;
                }
                else
                {
                    airliner.CurrentFlight =
                        new Flight(route.TimeTable.GetNextEntry(GameObject.GetInstance().GameTime, airliner));
                }
            }
            if (airliner.CurrentFlight != null)
            {
                double adistance = airliner.CurrentFlight.DistanceToDestination;

                double speed = airliner.Airliner.CruisingSpeed;

                /* leaving it out until we get position working agian
                if (airliner.CurrentFlight != null)
                {
                    Weather currentWeather = GetAirlinerWeather(airliner);

                    int wind = GetWindInfluence(airliner) * (int)currentWeather.WindSpeed;
                    speed = airliner.Airliner.Type.CruisingSpeed  + wind;

                } */

                if (adistance > 4)
                {
                    MathHelpers.MoveObject(airliner, speed);
                }

                if (airliner.CurrentFlight.DistanceToDestination < 5)
                {
                    airliner.Status = FleetAirliner.AirlinerStatus.Resting;
                    airliner.CurrentPosition = airliner.CurrentFlight.Entry.DepartureAirport;
                    // new GeoCoordinate(destination.Latitude, destination.Longitude);
                }
            }
            else
            {
                airliner.Status = FleetAirliner.AirlinerStatus.ToHomebase;
            }
        }

        #endregion

        //calibrates the time if needed

        //handles an influence for a historic event
    }
}