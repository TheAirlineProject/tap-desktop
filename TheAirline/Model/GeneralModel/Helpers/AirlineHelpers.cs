using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using TheAirline.GUIModel.HelpersModel;
using TheAirline.Model.AirlineModel;
using TheAirline.Model.AirlineModel.SubsidiaryModel;
using TheAirline.Model.AirlinerModel;
using TheAirline.Model.AirportModel;
using TheAirline.Model.GeneralModel.CountryModel;
using TheAirline.Model.GeneralModel.InvoicesModel;
using TheAirline.Model.PassengerModel;
using TheAirline.Model.PilotModel;
using TheAirline.Model.RouteModel;

namespace TheAirline.Model.GeneralModel.Helpers
{
    //the class for some general airline helpers
    public class AirlineHelpers
    {
        //checks for a special contract returns true if not successed or overdue
        public static Boolean CheckSpecialContract(SpecialContract sc)
        {
            Boolean isOk = true;

            foreach (ContractRequirement cr in sc.Type.Requirements)
            {
                foreach (SpecialContractRoute scr in sc.Type.Routes)
                {
                    SpecialContractRoute scr1 = scr;
                    IEnumerable<Route> routes =
                        sc.Routes.Where(
                            r =>
                            r.HasAirliner &&
                            ((r.Destination1 == scr1.Departure && r.Destination2 == scr1.Destination) || (scr1.BothWays && r.Destination2 == scr1.Departure && r.Destination1 == scr1.Destination)));
                    if (cr.Type == ContractRequirement.RequirementType.ClassType)
                    {
                        if (routes.FirstOrDefault(r => ((PassengerRoute) r).GetRouteAirlinerClass(cr.ClassType) != null) == null)
                            isOk = false;
                    }
                    else if (cr.Type == ContractRequirement.RequirementType.Destination)
                    {
                        if (!routes.Any())
                            isOk = false;
                    }
                }
            }

            if (!isOk)
            {
                if (sc.Airline.IsHuman)
                {
                    GameObject.GetInstance()
                              .NewsBox.AddNews(
                                  new News(
                                      News.NewsType.FlightNews,
                                      GameObject.GetInstance().GameTime,
                                      Translator.GetInstance().GetString("News", "1016"),
                                      string.Format(
                                          Translator.GetInstance().GetString("News", "1016", "message"),
                                          sc.Type.Name,
                                          new ValueCurrencyConverter().Convert(sc.Type.Penalty))));
                }

                AddAirlineInvoice(sc.Airline, GameObject.GetInstance().GameTime, Invoice.InvoiceType.AirlineExpenses, -sc.Type.Penalty);
            }

            Boolean overdue = sc.Type.IsFixedDate ? GameObject.GetInstance().GameTime > sc.Type.Period.To : GameObject.GetInstance().GameTime >= sc.Date;

            if (isOk && overdue)
            {
                if (sc.Airline.IsHuman)
                {
                    GameObject.GetInstance()
                              .NewsBox.AddNews(
                                  new News(
                                      News.NewsType.FlightNews,
                                      GameObject.GetInstance().GameTime,
                                      Translator.GetInstance().GetString("News", "1017"),
                                      string.Format(
                                          Translator.GetInstance().GetString("News", "1017", "message"),
                                          sc.Type.Name,
                                          new ValueCurrencyConverter().Convert(sc.Type.Payment))));
                }

                AddAirlineInvoice(sc.Airline, GameObject.GetInstance().GameTime, Invoice.InvoiceType.AirlineExpenses, sc.Type.Payment);

                return true;
            }

            return !isOk;
        }

        //clears the statistics for all routes for all airlines
        public static void ClearRoutesStatistics()
        {
            IEnumerable<Route> routes = Airlines.GetAllAirlines().SelectMany(a => a.Routes);

            foreach (Route route in routes)
                route.Statistics.Clear();
        }

        //clears the statistics for all airlines
        public static void ClearAirlinesStatistics()
        {
            foreach (Airline airline in Airlines.GetAllAirlines())
                airline.Statistics.Clear();
        }

        //creates an airliner for an airline
        public static FleetAirliner CreateAirliner(Airline airline, AirlinerType type)
        {
            Guid id = Guid.NewGuid();

            var airliner = new Airliner(id.ToString(), type, airline.Profile.Country.TailNumbers.GetNextTailNumber(), GameObject.GetInstance().GameTime);
            Airliners.AddAirliner(airliner);

            var fAirliner = new FleetAirliner(FleetAirliner.PurchasedType.Bought, GameObject.GetInstance().GameTime, airline, airliner, airline.Airports[0]);

            airliner.ClearAirlinerClasses();

            AirlinerHelpers.CreateAirlinerClasses(airliner);

            return fAirliner;
        }

        //adds an invoice to an airline
        public static void AddAirlineInvoice(Airline airline, DateTime date, Invoice.InvoiceType type, double amount)
        {
            if (airline.IsHuman && GameObject.GetInstance().HumanAirline == airline)
            {
                GameObject.GetInstance().AddHumanMoney(amount);
                GameObject.GetInstance().HumanAirline.AddInvoice(new Invoice(date, type, amount), false);
            }
            else
                airline.AddInvoice(new Invoice(date, type, amount));
        }

        //buys an airliner to an airline
        public static FleetAirliner BuyAirliner(Airline airline, Airliner airliner, Airport airport)
        {
            return BuyAirliner(airline, airliner, airport, 0);
        }

        public static FleetAirliner BuyAirliner(Airline airline, Airliner airliner, Airport airport, double discount)
        {
            FleetAirliner fAirliner = AddAirliner(airline, airliner, airport, false);

            double price = airliner.GetPrice()*((100 - discount)/100);

            AddAirlineInvoice(airline, GameObject.GetInstance().GameTime, Invoice.InvoiceType.Purchases, -price);

            return fAirliner;
        }

        public static FleetAirliner AddAirliner(Airline airline, Airliner airliner, Airport airport, Boolean leased)
        {
            if (Countries.GetCountryFromTailNumber(airliner.TailNumber).Name != airline.Profile.Country.Name)
            {
                lock (airline.Profile.Country.TailNumbers)
                {
                    airliner.TailNumber = airline.Profile.Country.TailNumbers.GetNextTailNumber();
                }
            }

            var fAirliner = new FleetAirliner(leased ? FleetAirliner.PurchasedType.Leased : FleetAirliner.PurchasedType.Bought, GameObject.GetInstance().GameTime, airline, airliner, airport);

            airline.AddAirliner(fAirliner);

            return fAirliner;
        }

        //orders a number of airliners for an airline
        public static void OrderAirliners(Airline airline, List<AirlinerOrder> orders, Airport airport, DateTime deliveryDate)
        {
            OrderAirliners(airline, orders, airport, deliveryDate, 0);
        }

        //orders a number of airliners for an airline
        public static void OrderAirliners(Airline airline, List<AirlinerOrder> orders, Airport airport, DateTime deliveryDate, double discount)
        {
            Guid id = Guid.NewGuid();

            foreach (AirlinerOrder order in orders)
            {
                for (int i = 0; i < order.Amount; i++)
                {
                    var airliner = new Airliner(id.ToString(), order.Type, airline.Profile.Country.TailNumbers.GetNextTailNumber(), deliveryDate);
                    Airliners.AddAirliner(airliner);

                    const FleetAirliner.PurchasedType pType = FleetAirliner.PurchasedType.Bought;
                    airline.AddAirliner(pType, airliner, airport);

                    airliner.ClearAirlinerClasses();

                    foreach (AirlinerClass aClass in order.Classes)
                    {
                        var tClass = new AirlinerClass(aClass.Type, aClass.SeatingCapacity) {RegularSeatingCapacity = aClass.RegularSeatingCapacity};

                        foreach (AirlinerFacility facility in aClass.GetFacilities())
                            tClass.SetFacility(airline, facility);

                        airliner.AddAirlinerClass(tClass);
                    }
                }
            }

            int totalAmount = orders.Sum(o => o.Amount);
            double price = orders.Sum(o => o.Type.Price*o.Amount);

            double totalPrice = price*((1 - GeneralHelpers.GetAirlinerOrderDiscount(totalAmount)))*((100 - discount)/100);

            AddAirlineInvoice(airline, GameObject.GetInstance().GameTime, Invoice.InvoiceType.Purchases, -totalPrice);
        }

        //reallocate all gates and facilities from one airport to another - gates, facilities and routes
        public static void ReallocateAirport(Airport oldAirport, Airport newAirport, Airline airline)
        {
            //contract
            List<AirportContract> oldContracts = oldAirport.GetAirlineContracts(airline);

            foreach (AirportContract oldContract in oldContracts)
            {
                oldAirport.RemoveAirlineContract(oldContract);

                oldContract.Airport = newAirport;

                AirportHelpers.AddAirlineContract(oldContract);


                for (int i = 0; i < oldContract.NumberOfGates; i++)
                {
                    Gate newGate = newAirport.Terminals.GetGates().First(g => g.Airline == null);
                    newGate.Airline = airline;

                    Gate oldGate = oldAirport.Terminals.GetGates().First(g => g.Airline == airline);
                    oldGate.Airline = null;
                }
            }
            //routes
            IEnumerable<Route> obsoleteRoutes = (from r in airline.Routes where r.Destination1 == oldAirport || r.Destination2 == oldAirport select r);

            foreach (Route route in obsoleteRoutes)
            {
                if (route.Destination1 == oldAirport) route.Destination1 = newAirport;
                if (route.Destination2 == oldAirport) route.Destination2 = newAirport;


                List<RouteTimeTableEntry> entries = route.TimeTable.Entries.FindAll(e => e.Destination.Airport == oldAirport);

                foreach (RouteTimeTableEntry entry in entries)
                    entry.Destination.Airport = newAirport;

                foreach (FleetAirliner airliner in route.GetAirliners())
                {
                    if (airliner.Homebase == oldAirport)
                        airliner.Homebase = newAirport;
                }
            }

            //facilities
            foreach (AirportFacility facility in oldAirport.GetCurrentAirportFacilities(airline))
            {
                newAirport.AddAirportFacility(airline, facility, GameObject.GetInstance().GameTime);
            }

            oldAirport.ClearFacilities(airline);

            foreach (AirportFacility.FacilityType type in Enum.GetValues(typeof (AirportFacility.FacilityType)))
            {
                AirportFacility noneFacility = AirportFacilities.GetFacilities(type).Find((facility => facility.TypeLevel == 0));

                oldAirport.AddAirportFacility(airline, noneFacility, GameObject.GetInstance().GameTime);
            }
        }

        //returns all route facilities for a given airline and type
        public static List<RouteFacility> GetRouteFacilities(Airline airline, RouteFacility.FacilityType type)
        {
            return RouteFacilities.GetFacilities(type).FindAll(f => f.Requires == null || airline.Facilities.Contains(f.Requires));
        }

        //returns the pay for a codesharing agreement - one wayed
        public static double GetCodesharingPrice(CodeshareAgreement agreement)
        {
            return GetCodesharingPrice(agreement.Airline1, agreement.Airline2);
        }

        //returns the pay for codesharing agreement
        public static double GetCodesharingPrice(Airline airline1, Airline airline2)
        {
            //from airline1 to airline2
            return GeneralHelpers.GetInflationPrice(750);
        }

        //returns if an airline wants to have code sharing with another airline
        public static Boolean AcceptCodesharing(Airline airline, Airline asker, CodeshareAgreement.CodeshareType type)
        {
            double coeff = type == CodeshareAgreement.CodeshareType.OneWay ? 0.25 : 0.40;

            IEnumerable<Country> sameCountries = asker.Airports.Select(a => a.Profile.Country).Distinct().Intersect(airline.Airports.Select(a => a.Profile.Country).Distinct());
            IEnumerable<Airport> sameDestinations = asker.Airports.Distinct().Intersect(airline.Airports);
            IEnumerable<Country> sameCodesharingCountries =
                airline.GetCodesharingAirlines().SelectMany(a => a.Airports).Select(a => a.Profile.Country).Distinct().Intersect(airline.Airports.Select(a => a.Profile.Country).Distinct());

            double airlineDestinations = airline.Airports.Count;
            double airlineRoutes = airline.Routes.Count;
            double airlineCountries = airline.Airports.Select(a => a.Profile.Country).Distinct().Count();
            double airlineCodesharings = airline.Codeshares.Count;
            double airlineAlliances = airline.Alliances.Count;

            //declines if asker is much smaller than the invited airline
            if (airlineRoutes > 3*asker.Routes.Count)
                return false;

            //declines if there is a match for x% of the airlines
            if (sameDestinations.Count() >= airlineDestinations*coeff)
                return false;

            //declines if there is a match for 75% of the airlines
            if (sameCountries.Count() >= airlineCountries*0.75)
                return false;

            //declines if the airline already has a code sharing or alliance in that area
            if (sameCodesharingCountries.Count() >= airlineCountries*coeff)
                return false;

            return true;
        }

        //returns if the airline has training facilities for a specific airliner family
        public static Boolean HasTrainingFacility(Airline airline, string airlinerfamily)
        {
            IEnumerable<AirlineFacility> facilities = airline.Facilities.Where(f => f is PilotTrainingFacility);

            return facilities.SingleOrDefault(f => ((PilotTrainingFacility) f).AirlinerFamily == airlinerfamily) != null;
        }

        //launches a subsidiary to operate on its own
        public static void MakeSubsidiaryAirlineIndependent(SubsidiaryAirline airline)
        {
            airline.Airline.RemoveSubsidiaryAirline(airline);

            airline.Airline = null;

            airline.Profile.CEO = string.Format("{0} {1}", Names.GetInstance().GetRandomFirstName(airline.Profile.Country), Names.GetInstance().GetRandomLastName(airline.Profile.Country));

            if (!Airlines.ContainsAirline(airline))
                Airlines.AddAirline(airline);
        }

        //closes a subsidiary airline for an airline
        public static void CloseSubsidiaryAirline(SubsidiaryAirline airline)
        {
            AddAirlineInvoice(airline.Airline, GameObject.GetInstance().GameTime, Invoice.InvoiceType.AirlineExpenses, airline.Money);

            airline.Airline.RemoveSubsidiaryAirline(airline);
            Airlines.RemoveAirline(airline);

            List<FleetAirliner> fleet = airline.Fleet;

            foreach (FleetAirliner t in fleet)
            {
                t.Airliner.Airline = airline.Airline;
                airline.Airline.AddAirliner(t);
            }

            List<Airport> airports = airline.Airports;

            foreach (Airport t in airports)
            {
                List<AirportContract> contracts = t.GetAirlineContracts(airline);

                foreach (AirportContract t1 in contracts)
                {
                    t1.Airline = airline.Airline;
                }

                if (!airline.Airline.Airports.Contains(t))
                {
                    airline.Airline.AddAirport(t);
                }

                foreach (AirportFacility facility in t.GetCurrentAirportFacilities(airline))
                {
                    if (t.GetAirlineAirportFacility(airline.Airline, facility.Type).Facility.TypeLevel < facility.TypeLevel)
                        t.AddAirportFacility(airline.Airline, facility, GameObject.GetInstance().GameTime);
                }

                t.ClearFacilities(airline);
            }
            //moves the terminals from the subsidiary to the parent airline
            foreach (Airport airport in airline.Airports)
            {
                IEnumerable<Terminal> terminals = airport.Terminals.GetTerminals().Where(t => t.Airline == airline);

                foreach (Terminal terminal in terminals)
                    terminal.Airline = airline.Airline;
            }
        }

        //adds a subsidiary airline to an airline
        public static void AddSubsidiaryAirline(Airline airline, SubsidiaryAirline sAirline, double money, Airport airportHomeBase)
        {
            Terminal.TerminalType terminaltype = sAirline.AirlineRouteFocus == Route.RouteType.Cargo ? Terminal.TerminalType.Cargo : Terminal.TerminalType.Passenger;
            AddAirlineInvoice(airline, GameObject.GetInstance().GameTime, Invoice.InvoiceType.AirlineExpenses, -money);
            sAirline.Money = money;
            sAirline.StartMoney = money;

            sAirline.Fees = new AirlineFees();

            airline.AddSubsidiaryAirline(sAirline);

            if (!AirportHelpers.HasFreeGates(airportHomeBase, sAirline, terminaltype) && airportHomeBase.Terminals.GetFreeGates(terminaltype) > 1)
            {
                AirportHelpers.RentGates(airportHomeBase, sAirline, AirportContract.ContractType.Full, terminaltype, 2);
                //sets all the facilities at an airport to none for all airlines
                foreach (Airport airport in Airports.GetAllAirports())
                {
                    foreach (AirportFacility.FacilityType type in Enum.GetValues(typeof (AirportFacility.FacilityType)))
                    {
                        AirportFacility noneFacility = AirportFacilities.GetFacilities(type).Find((facility => facility.TypeLevel == 0));

                        airport.AddAirportFacility(sAirline, noneFacility, GameObject.GetInstance().GameTime);
                    }
                }


                AirportFacility serviceFacility = AirportFacilities.GetFacilities(AirportFacility.FacilityType.Service).Find(f => f.TypeLevel == 1);
                AirportFacility checkinFacility = AirportFacilities.GetFacilities(AirportFacility.FacilityType.CheckIn).Find(f => f.TypeLevel == 1);

                airportHomeBase.AddAirportFacility(sAirline, serviceFacility, GameObject.GetInstance().GameTime);
                airportHomeBase.AddAirportFacility(sAirline, checkinFacility, GameObject.GetInstance().GameTime);
            }

            foreach (AirlinePolicy policy in airline.Policies)
                sAirline.AddAirlinePolicy(policy);

            Airlines.AddAirline(sAirline);
        }


        //creates a subsidiary airline for an airline
        public static SubsidiaryAirline CreateSubsidiaryAirline(Airline airline, double money, string name, string iata, Airline.AirlineMentality mentality, Airline.AirlineFocus market,
                                                                Route.RouteType routefocus, Airport homebase)
        {
            var profile = new AirlineProfile(name, iata, airline.Profile.Color, airline.Profile.CEO, true, GameObject.GetInstance().GameTime.Year, 2199) {Country = homebase.Profile.Country};

            var sAirline = new SubsidiaryAirline(airline, profile, mentality, market, airline.License, routefocus);

            AddSubsidiaryAirline(airline, sAirline, money, homebase);

            return sAirline;
        }

        //hires the pilots for a specific airliner
        public static void HireAirlinerPilots(FleetAirliner airliner)
        {
            if (Pilots.GetNumberOfUnassignedPilots() < 10)
                GeneralHelpers.CreatePilots(50);

            while (airliner.Airliner.Type.CockpitCrew > airliner.NumberOfPilots)
            {
                List<Pilot> pilots = Pilots.GetUnassignedPilots(p => p.Profile.Town.Country == airliner.Airliner.Airline.Profile.Country && p.Aircrafts.Contains(airliner.Airliner.Type.AirlinerFamily));

                if (pilots.Count == 0)
                    pilots =
                        Pilots.GetUnassignedPilots(p => p.Profile.Town.Country.Region == airliner.Airliner.Airline.Profile.Country.Region && p.Aircrafts.Contains(airliner.Airliner.Type.AirlinerFamily));

                if (pilots.Count == 0)
                    pilots = Pilots.GetUnassignedPilots(p => p.Aircrafts.Contains(airliner.Airliner.Type.AirlinerFamily));

                if (pilots.Count == 0)
                {
                    GeneralHelpers.CreatePilots(4, airliner.Airliner.Type.AirlinerFamily);
                    HireAirlinerPilots(airliner);
                }

                Pilot pilot = pilots.OrderByDescending(p => p.Rating.CostIndex).FirstOrDefault();

                if (pilot != null)
                {
                    airliner.Airliner.Airline.AddPilot(pilot);

                    pilot.Airliner = airliner;
                    airliner.AddPilot(pilot);
                }
                else
                    GeneralHelpers.CreatePilots(50);
            }
        }

        //returns the price for training a pilot
        public static double GetTrainingPrice(Pilot pilot, string airlinerfamily)
        {
            double dayTrainingPrice = GeneralHelpers.GetInflationPrice(750);

            int days = GetTrainingDays(pilot, airlinerfamily);

            if (HasTrainingFacility(pilot.Airline, airlinerfamily))
                return dayTrainingPrice;
            return days*dayTrainingPrice;
        }

        //returns the number of training days a pilot 
        public static int GetTrainingDays(Pilot pilot, string airlinerfamily)
        {
            Manufacturer manufacturer = AirlinerTypes.GetTypes(t => t.AirlinerFamily == airlinerfamily).Select(t => t.Manufacturer).FirstOrDefault();

            Boolean hasManufacturer = false;
            if (manufacturer != null)
            {
                foreach (string family in pilot.Aircrafts)
                {
                    string family1 = family;
                    Manufacturer tManufacturer = AirlinerTypes.GetTypes(t => t.AirlinerFamily == family1).Select(t => t.Manufacturer).FirstOrDefault();

                    if (tManufacturer != null && tManufacturer.ShortName == manufacturer.ShortName)
                        hasManufacturer = true;
                }
            }

            if (hasManufacturer)
                return 2;
            return 14;
        }

        //send a pilot for an airline on training
        public static void SendForTraining(Airline airline, Pilot pilot, string airlinerfamily, int trainingdays, double price)
        {
            AddAirlineInvoice(airline, GameObject.GetInstance().GameTime, Invoice.InvoiceType.AirlineExpenses, -price);

            pilot.Training = new PilotTraining(airlinerfamily, GameObject.GetInstance().GameTime.AddDays(trainingdays));
        }

        //returns the discount factor for a manufactorer for an airline and for a period
        public static double GetAirlineManufactorerDiscountFactor(Airline airline, int length, Boolean forReputation)
        {
            double score;

            if (forReputation)
                score = 0.3*(1 + (int) airline.GetReputation());
            else
                score = 0.005*(1 + (int) airline.GetValue());

            double discountFactor = (Convert.ToDouble(length)/20) + (score);
            double discount = Math.Pow(discountFactor, 5);

            if (discount > 30)
                discount = length*3;

            return discount;
        }

        //returns if an airline can create a hub at an airport
        public static Boolean CanCreateHub(Airline airline, Airport airport, HubType type)
        {
            Terminal.TerminalType terminaltype = airline.AirlineRouteFocus == Route.RouteType.Cargo ? Terminal.TerminalType.Cargo : Terminal.TerminalType.Passenger;

            Boolean airlineHub = airport.GetHubs().Exists(h => h.Airline == airline);

            int airlineValue = (int) airline.GetAirlineValue() + 1;

            int totalAirlineHubs = airline.GetHubs().Count; // 'Airports.GetAllActiveAirports().Sum(a => a.Hubs.Count(h => h.Airline == airline));
            double airlineGatesPercent = Convert.ToDouble(airport.Terminals.GetNumberOfGates(airline))/Convert.ToDouble(airport.Terminals.GetNumberOfGates(terminaltype))*100;

            switch (type.Type)
            {
                case HubType.TypeOfHub.FocusCity:
                    return !airlineHub && airline.Money > AirportHelpers.GetHubPrice(airport, type);
                case HubType.TypeOfHub.RegionalHub:
                    return !airlineHub && airline.Money > AirportHelpers.GetHubPrice(airport, type) &&
                           (airport.Profile.Size == GeneralHelpers.Size.Large || airport.Profile.Size == GeneralHelpers.Size.Medium) && airport.GetHubs().Count < 7;
                case HubType.TypeOfHub.FortressHub:
                    return !airlineHub && airline.Money > AirportHelpers.GetHubPrice(airport, type) && (airport.Profile.Size > GeneralHelpers.Size.Medium) && airlineGatesPercent > 70 &&
                           (totalAirlineHubs < airlineValue);
                case HubType.TypeOfHub.Hub:
                    return !airlineHub && airline.Money > AirportHelpers.GetHubPrice(airport, type) && (airlineGatesPercent > 20) && (totalAirlineHubs < airlineValue) &&
                           (airport.GetHubs(HubType.TypeOfHub.Hub).Count < (int) airport.Profile.Size);
            }

            return false;
        }

        //returns the possible home bases for an airline
        public static List<Airport> GetHomebases(Airline airline)
        {
            //var curentFacility = 
            return
                airline.Airports.FindAll(
                    a =>
                    (a.HasContractType(airline, AirportContract.ContractType.FullService) ||
                     airline.Fleet.Count(ar => ar.Homebase == a) < a.GetCurrentAirportFacility(airline, AirportFacility.FacilityType.Service).ServiceLevel));
        }

        public static List<Airport> GetHomebases(Airline airline, AirlinerType type)
        {
            return GetHomebases(airline, type.MinRunwaylength);
        }

        public static List<Airport> GetHomebases(Airline airline, long minrunway)
        {
            return GetHomebases(airline).Where(h => h.GetMaxRunwayLength() >= minrunway).ToList();
        }

        //update the damage scores for an airline
        public static void UpdateMaintList(Airline airline)
        {
            foreach (FleetAirliner a in airline.Fleet)
            {
                airline.Scores.Maintenance.Add((int) a.Airliner.Condition);
            }

            if (airline.Scores.Maintenance.Count > (airline.Fleet.Count*2))
            {
                airline.Scores.Maintenance.RemoveRange(0, airline.Fleet.Count);
            }
        }

        //updates the airlines ratings for an airline
        public static void UpdateRatings(Airline airline)
        {
            airline.Ratings.SafetyRating = (int) airline.Scores.Safety.Average();
            airline.Ratings.SecurityRating = (int) airline.Scores.Security.Average();
            airline.Ratings.CustomerHappinessRating = (int) airline.Scores.CHR.Average();
            airline.Ratings.EmployeeHappinessRating = (int) airline.Scores.EHR.Average();
            airline.Ratings.MaintenanceRating = (int) airline.Scores.Maintenance.Average();
        }

        //returns the max loan sum for an airline
        public static double GetMaxLoanAmount(Airline airline)
        {
            return airline.GetValue()*500000;
        }

        //returns if an airline can apply for a loan
        public static Boolean CanApplyForLoan(Airline airline, Loan loan)
        {
            double loans = airline.Loans.Sum(l => l.Amount);

            return loan.Amount + loans < GetMaxLoanAmount(airline);
        }

        //returns if an airline has licens for flying between two airports
        public static Boolean HasAirlineLicens(Airline airline, Airport airport1, Airport airport2)
        {
            Continent continent1 = Continents.GetContinent(airport1.Profile.Country.Region);
            Continent continent2 = Continents.GetContinent(airport2.Profile.Country.Region);

            Boolean isInUnion =
                Unions.GetUnions(airport1.Profile.Country, GameObject.GetInstance().GameTime).Intersect(Unions.GetUnions(airport2.Profile.Country, GameObject.GetInstance().GameTime)).Any();

            if (airline.License == Airline.AirlineLicense.LongHaul)
                return true;

            if (airline.License == Airline.AirlineLicense.ShortHaul && (MathHelpers.GetDistance(airport1, airport2) < 2000 || isInUnion))
                return true;

            if (airline.License == Airline.AirlineLicense.Domestic && airport1.Profile.Country == airport2.Profile.Country)
                return true;

            if (airline.License == Airline.AirlineLicense.Regional && (continent1 == continent2 || isInUnion))
                return true;

            return false;
        }

        //returns the monthly payroll for an airline
        public static double GetMonthlyPayroll(Airline airline)
        {
            double instructorFee = airline.FlightSchools.Sum(f => f.NumberOfInstructors)*airline.Fees.GetValue(FeeTypes.GetType("Instructor Base Salary"));

            double cockpitCrewFee = airline.Pilots.Count*airline.Fees.GetValue(FeeTypes.GetType("Cockpit Wage"));

            double cabinCrewFee = airline.Routes.Where(r => r.Type == Route.RouteType.Passenger).Sum(r => ((PassengerRoute) r).GetTotalCabinCrew())*
                                  airline.Fees.GetValue(FeeTypes.GetType("Cabin Wage"));

            double serviceCrewFee =
                airline.Airports.SelectMany(a => a.GetCurrentAirportFacilities(airline)).Where(a => a.EmployeeType == AirportFacility.EmployeeTypes.Support).Sum(a => a.NumberOfEmployees)*
                airline.Fees.GetValue(FeeTypes.GetType("Support Wage"));
            double maintenanceCrewFee =
                airline.Airports.SelectMany(a => a.GetCurrentAirportFacilities(airline)).Where(a => a.EmployeeType == AirportFacility.EmployeeTypes.Maintenance).Sum(a => a.NumberOfEmployees)*
                airline.Fees.GetValue(FeeTypes.GetType("Maintenance Wage"));

            return instructorFee + cockpitCrewFee + cabinCrewFee + serviceCrewFee + maintenanceCrewFee;
        }

        //returns the number of routes out of an airport for an airline
        public static int GetAirportOutboundRoutes(Airline airline, Airport airport)
        {
            var routes = new List<Route>(airline.Routes);
            return routes.Count(r => r.Destination1 == airport || r.Destination2 == airport);
        }

        //checks for any insurance claims up for settlement
        public static void CheckInsuranceSettlements(Airline airline)
        {
            foreach (InsuranceClaim claim in airline.InsuranceClaims)
            {
                if (claim.SettlementDate <= GameObject.GetInstance().GameTime)
                {
                    AirlineInsuranceHelpers.ReceiveInsurancePayout(airline, claim.Policy, claim);
                }
            }
        }

        //returns the current price per share for an airline
        public static double GetPricePerAirlineShare(Airline airline)
        {
            var rnd = new Random();

            double price = 0;
            Airline.AirlineValue value = airline.GetAirlineValue();

            switch (value)
            {
                case Airline.AirlineValue.Low:
                    price = 15 + (rnd.NextDouble()*10);
                    break;
                case Airline.AirlineValue.VeryLow:
                    price = 5 + (rnd.NextDouble()*10);
                    break;
                case Airline.AirlineValue.Normal:
                    price = 25 + (rnd.NextDouble()*10);
                    break;
                case Airline.AirlineValue.High:
                    price = 40 + (rnd.NextDouble()*10);
                    break;
                case Airline.AirlineValue.VeryHigh:
                    price = 55 + (rnd.NextDouble()*10);
                    break;
            }

            return GeneralHelpers.GetInflationPrice(price);
        }

        //adds a number of shares to an airline
        public static void AddAirlineShares(Airline airline, int shares, double sharePrice)
        {
            for (int i = 0; i < shares; i++)
            {
                var share = new AirlineShare(null, sharePrice);
                airline.Shares.Add(share);
            }
        }

        //sets a number of shares to an airline
        public static void SetAirlineShares(Airline airline, Airline shareAirline, int shares)
        {
            for (int i = 0; i < shares; i++)
            {
                AirlineShare share = airline.Shares.First(s => s.Airline == null);
                share.Airline = shareAirline;
            }
        }

        //creates the standard number of shares for an airline
        public static void CreateStandardAirlineShares(Airline airline, double sharePrice)
        {
            var rnd = new Random();

            const int numberOfShares = 10000;

            int airlinePercentShares = rnd.Next(55, 65);

            airline.Shares = new List<AirlineShare>();

            int airlineShares = (numberOfShares/100)*airlinePercentShares;

            //airline shares
            lock (airline.Shares)
            {
                for (int i = 0; i < airlineShares; i++)
                {
                    var share = new AirlineShare(airline, sharePrice);

                    airline.Shares.Add(share);
                }

                //'free' shares
                for (int i = airlineShares; i < numberOfShares; i++)
                {
                    var share = new AirlineShare(null, sharePrice);

                    airline.Shares.Add(share);
                }
            }
        }

        public static void CreateStandardAirlineShares(Airline airline)
        {
            double sharePrice = GetPricePerAirlineShare(airline);

            CreateStandardAirlineShares(airline, sharePrice);
        }

        //switches from one airline to another airline
        public static void SwitchAirline(Airline airlineFrom, Airline airlineTo)
        {
            while (airlineFrom.Alliances.Count > 0)
            {
                Alliance alliance = airlineFrom.Alliances[0];
                alliance.RemoveMember(airlineFrom);
                alliance.AddMember(new AllianceMember(airlineTo, GameObject.GetInstance().GameTime));
            }
            while (airlineFrom.Facilities.Count > 0)
            {
                AirlineFacility airlineFacility = airlineFrom.Facilities[0];
                airlineFrom.RemoveFacility(airlineFacility);
                airlineTo.AddFacility(airlineFacility);
            }


            while (airlineFrom.GetFleetSize() > 0)
            {
                FleetAirliner airliner = airlineFrom.Fleet[0];
                airlineFrom.RemoveAirliner(airliner);
                airlineTo.AddAirliner(airliner);
                airliner.Airliner.Airline = airlineTo;
            }

            while (airlineFrom.Routes.Count > 0)
            {
                Route route = airlineFrom.Routes[0];
                route.Airline = airlineTo;

                airlineFrom.RemoveRoute(route);
                airlineTo.AddRoute(route);
            }
            while (airlineFrom.Pilots.Count > 0)
            {
                Pilot pilot = airlineFrom.Pilots[0];
                airlineFrom.RemovePilot(pilot);

                pilot.Airline = airlineTo;
                airlineTo.AddPilot(pilot);
            }
            while (airlineFrom.Airports.Count > 0)
            {
                Airport airport = airlineFrom.Airports[0];
                airport.Terminals.SwitchAirline(airlineFrom, airlineTo);

                foreach (AirportFacility facility in airport.GetCurrentAirportFacilities(airlineFrom))
                {
                    if (facility.TypeLevel > airport.GetCurrentAirportFacility(airlineTo, facility.Type).TypeLevel)
                        airport.AddAirportFacility(airlineTo, facility, GameObject.GetInstance().GameTime);

                    AirportFacility noneFacility = AirportFacilities.GetFacilities(facility.Type).Find(f => f.TypeLevel == 0);

                    airport.AddAirportFacility(airlineFrom, noneFacility, GameObject.GetInstance().GameTime);
                }
            }
        }

        //returns if a route can be created
        public static Boolean IsRouteDestinationsOk(Airline airline, Airport destination1, Airport destination2, Route.RouteType routeType, Airport stopover1 = null, Airport stopover2 = null)
        {
            Terminal.TerminalType type = routeType == Route.RouteType.Cargo ? Terminal.TerminalType.Cargo : Terminal.TerminalType.Passenger;

            var distances = new List<double>();

            Boolean stopoverOk = (stopover1 == null || routeType == Route.RouteType.Cargo || AirportHelpers.HasFreeGates(stopover1, airline, type)) &&
                                 (stopover2 == null || routeType == Route.RouteType.Cargo || AirportHelpers.HasFreeGates(stopover2, airline, type));

            if ((AirportHelpers.HasFreeGates(destination1, airline, type) && AirportHelpers.HasFreeGates(destination2, airline, type) && stopoverOk) || routeType == Route.RouteType.Cargo)
            {
                var routeOkStatus = RouteOkStatus.Ok;

                if (stopover1 == null && stopover2 == null)
                {
                    distances.Add(MathHelpers.GetDistance(destination1, destination2));
                    routeOkStatus = GetRouteStatus(destination1, destination2, routeType);
                }

                if (stopover1 == null && stopover2 != null)
                {
                    distances.Add(MathHelpers.GetDistance(destination1, stopover2));
                    distances.Add(MathHelpers.GetDistance(stopover2, destination2));
                    routeOkStatus = GetRouteStatus(destination1, stopover2, routeType);

                    if (routeOkStatus == RouteOkStatus.Ok)
                        routeOkStatus = GetRouteStatus(stopover2, destination2, routeType);
                }

                if (stopover1 != null && stopover2 == null)
                {
                    distances.Add(MathHelpers.GetDistance(destination1, stopover1));
                    distances.Add(MathHelpers.GetDistance(stopover1, destination2));
                    routeOkStatus = GetRouteStatus(destination1, stopover1, routeType);

                    if (routeOkStatus == RouteOkStatus.Ok)
                        routeOkStatus = GetRouteStatus(stopover1, destination2, routeType);
                }

                if (stopover1 != null && stopover2 != null)
                {
                    distances.Add(MathHelpers.GetDistance(destination1, stopover1));
                    distances.Add(MathHelpers.GetDistance(stopover1, stopover2));
                    distances.Add(MathHelpers.GetDistance(stopover2, destination2));
                    routeOkStatus = GetRouteStatus(destination1, stopover1, routeType);

                    if (routeOkStatus == RouteOkStatus.Ok)
                        routeOkStatus = GetRouteStatus(stopover1, stopover2, routeType);

                    if (routeOkStatus == RouteOkStatus.Ok)
                        routeOkStatus = GetRouteStatus(stopover2, destination2, routeType);
                }

                double maxDistance = distances.Max();
                double minDistance = distances.Min();

                IEnumerable<long> query = from a in AirlinerTypes.GetTypes(t => t.Produced.From < GameObject.GetInstance().GameTime)
                                          select a.Range;

                double maxFlightDistance = query.Max();

                if (minDistance <= Route.MinRouteDistance || maxDistance > maxFlightDistance)
                    routeOkStatus = RouteOkStatus.WrongDistance;

                if (routeOkStatus == RouteOkStatus.Ok)
                    return true;
                if (routeOkStatus == RouteOkStatus.AppropriateType)
                    throw new Exception("3002");
                if (routeOkStatus == RouteOkStatus.WrongDistance)
                    throw new Exception("3001");
                if (routeOkStatus == RouteOkStatus.MissingCargo)
                    throw new Exception("3003");
                if (routeOkStatus == RouteOkStatus.Restrictions)
                    throw new Exception("3005");
                if (routeOkStatus == RouteOkStatus.MissingLicense)
                    throw new Exception("3004");
                throw new Exception("3000");
            }
            throw new Exception("3000");
        }

        //returns if two airports can have route between them and if the airline has license for the route
        private static Boolean CheckRouteOk(Airport airport1, Airport airport2, Route.RouteType routeType)
        {
            Boolean isCargoRouteOk = true;
            if (routeType == Route.RouteType.Cargo)
                isCargoRouteOk = AIHelpers.IsCargoRouteDestinationsCorrect(airport1, airport2, GameObject.GetInstance().HumanAirline);

            return isCargoRouteOk && HasAirlineLicens(GameObject.GetInstance().HumanAirline, airport1, airport2) && AIHelpers.IsRouteInCorrectArea(airport1, airport2) &&
                   !FlightRestrictions.HasRestriction(airport1.Profile.Country, airport2.Profile.Country, GameObject.GetInstance().GameTime, FlightRestriction.RestrictionType.Flights) &&
                   !FlightRestrictions.HasRestriction(airport2.Profile.Country, airport1.Profile.Country, GameObject.GetInstance().GameTime, FlightRestriction.RestrictionType.Flights) &&
                   !FlightRestrictions.HasRestriction(GameObject.GetInstance().HumanAirline, airport1.Profile.Country, airport2.Profile.Country, GameObject.GetInstance().GameTime);
        }

        private static RouteOkStatus GetRouteStatus(Airport airport1, Airport airport2, Route.RouteType routeType)
        {
            var status = RouteOkStatus.Ok;

            if (routeType == Route.RouteType.Cargo || routeType == Route.RouteType.Mixed)
                if (!AIHelpers.IsCargoRouteDestinationsCorrect(airport1, airport2, GameObject.GetInstance().HumanAirline))
                    status = RouteOkStatus.MissingCargo;

            if (status == RouteOkStatus.Ok)
            {
                if (HasAirlineLicens(GameObject.GetInstance().HumanAirline, airport1, airport2))
                {
                    if (AIHelpers.IsRouteInCorrectArea(airport1, airport2))
                    {
                        if (!FlightRestrictions.HasRestriction(airport1.Profile.Country, airport2.Profile.Country, GameObject.GetInstance().GameTime, FlightRestriction.RestrictionType.Flights) &&
                            !FlightRestrictions.HasRestriction(airport2.Profile.Country, airport1.Profile.Country, GameObject.GetInstance().GameTime, FlightRestriction.RestrictionType.Flights) &&
                            !FlightRestrictions.HasRestriction(GameObject.GetInstance().HumanAirline, airport1.Profile.Country, airport2.Profile.Country, GameObject.GetInstance().GameTime))
                            status = RouteOkStatus.Ok;
                        else
                            status = RouteOkStatus.Restrictions;
                    }
                    else
                        status = RouteOkStatus.AppropriateType;
                }
                else
                    status = RouteOkStatus.MissingLicense;
            }

            return status;
        }

        //returns the salary for a pilot at an airline
        public static double GetPilotSalary(Airline airline, Pilot pilot)
        {
            double pilotBasePrice = airline.Fees.GetValue(FeeTypes.GetType("Pilot Base Salary")); //GeneralHelpers.GetInflationPrice(133.53);<

            double pilotExperienceFee = pilot.Aircrafts.Count*GeneralHelpers.GetInflationPrice(20.3);

            return pilot.Rating.CostIndex*pilotBasePrice + pilotExperienceFee;
        }

        private enum RouteOkStatus
        {
            Ok,
            MissingCargo,
            WrongDistance,
            AppropriateType,
            Restrictions,
            MissingLicense
        };
    }

    //airline insurance helpers
    public class AirlineInsuranceHelpers
    {
        //add insurance policy
        public static AirlineInsurance CreatePolicy(Airline airline, AirlineInsurance.InsuranceType type, AirlineInsurance.InsuranceScope scope, AirlineInsurance.PaymentTerms terms, bool allAirliners,
                                                    double length, int amount)
        {
            #region Method Setup

            var rnd = new Random();
            double modifier = GetRatingModifier(airline);
            double hub = airline.GetHubs().Count()*0.1;
            var policy = new AirlineInsurance(type, scope, terms, amount)
                {
                    InsuranceEffective = GameObject.GetInstance().GameTime,
                    InsuranceExpires = GameObject.GetInstance().GameTime.AddYears((int) length),
                    PolicyIndex = GameObject.GetInstance().GameTime.ToString(CultureInfo.InvariantCulture) + airline,
                    TermLength = length
                };
            switch (policy.InsTerms)
            {
                case AirlineInsurance.PaymentTerms.Monthly:
                    policy.RemainingPayments = length*12;
                    break;
                case AirlineInsurance.PaymentTerms.Quarterly:
                    policy.RemainingPayments = length*4;
                    break;
                case AirlineInsurance.PaymentTerms.Biannual:
                    policy.RemainingPayments = length*2;
                    break;
                case AirlineInsurance.PaymentTerms.Annual:
                    policy.RemainingPayments = length;
                    break;
            }
            //sets up multipliers based on the type and scope of insurance policy
            var typeMultipliers = new Dictionary<AirlineInsurance.InsuranceType, double>();
            var scopeMultipliers = new Dictionary<AirlineInsurance.InsuranceScope, double>();
            double typeMPublic = modifier;
            double typeMPassenger = modifier + 0.2;
            double typeMCSL = modifier + 0.5;
            double typeMFull = modifier + 1;

            double scMAirport = modifier;
            double scMDomestic = modifier + 0.2;
            double scMHub = modifier + hub + 0.5;
            double scMGlobal = modifier + hub + 1;

            #endregion

            #region Domestic/Int'l Airport Counter

            int i = 0;
            int j = 0;
            foreach (Airport airport in GameObject.GetInstance().HumanAirline.Airports)
            {
                if (airport.Profile.Country != GameObject.GetInstance().HumanAirline.Profile.Country)
                {
                    i++;
                }
                else j++;
            }

            #endregion

            // all the decision making for monthly payment amounts and deductibles

            #region Public Liability

            switch (type)
            {
                case AirlineInsurance.InsuranceType.PublicLiability:
                    switch (scope)
                    {
                        case AirlineInsurance.InsuranceScope.Airport:
                            policy.Deductible = amount*0.005;
                            policy.PaymentAmount = policy.InsuredAmount*(4/10)*typeMPublic*scMAirport;
                            if (terms == AirlineInsurance.PaymentTerms.Annual) policy.PaymentAmount = policy.InsuredAmount/length;
                            if (terms == AirlineInsurance.PaymentTerms.Biannual) policy.PaymentAmount = policy.InsuredAmount/length/2;
                            if (terms == AirlineInsurance.PaymentTerms.Quarterly) policy.PaymentAmount = policy.InsuredAmount/length/4;
                            if (terms == AirlineInsurance.PaymentTerms.Monthly) policy.PaymentAmount = policy.InsuredAmount/length/12;

                            break;

                        case AirlineInsurance.InsuranceScope.Domestic:
                            policy.Deductible = amount*0.001;
                            policy.PaymentAmount = policy.InsuredAmount*(4/10)*typeMPublic*scMDomestic;
                            if (terms == AirlineInsurance.PaymentTerms.Annual) policy.PaymentAmount = policy.InsuredAmount/length;
                            if (terms == AirlineInsurance.PaymentTerms.Biannual) policy.PaymentAmount = policy.InsuredAmount/length/2;
                            if (terms == AirlineInsurance.PaymentTerms.Quarterly) policy.PaymentAmount = policy.InsuredAmount/length/4;
                            if (terms == AirlineInsurance.PaymentTerms.Monthly) policy.PaymentAmount = policy.InsuredAmount/length/12;
                            break;

                        case AirlineInsurance.InsuranceScope.Hub:
                            policy.Deductible = amount*0.001;
                            policy.PaymentAmount = policy.InsuredAmount*(4/10)*typeMPublic*scMHub;
                            if (terms == AirlineInsurance.PaymentTerms.Annual) policy.PaymentAmount = policy.InsuredAmount/length;
                            if (terms == AirlineInsurance.PaymentTerms.Biannual) policy.PaymentAmount = policy.InsuredAmount/length/2;
                            if (terms == AirlineInsurance.PaymentTerms.Quarterly) policy.PaymentAmount = policy.InsuredAmount/length/4;
                            if (terms == AirlineInsurance.PaymentTerms.Monthly) policy.PaymentAmount = policy.InsuredAmount/length/12;
                            break;

                        case AirlineInsurance.InsuranceScope.Global:
                            policy.Deductible = amount*0.001;
                            policy.PaymentAmount = policy.InsuredAmount*(4/10)*typeMPublic*scMGlobal;
                            if (terms == AirlineInsurance.PaymentTerms.Annual) policy.PaymentAmount = policy.InsuredAmount/length;
                            if (terms == AirlineInsurance.PaymentTerms.Biannual) policy.PaymentAmount = policy.InsuredAmount/length/2;
                            if (terms == AirlineInsurance.PaymentTerms.Quarterly) policy.PaymentAmount = policy.InsuredAmount/length/4;
                            if (terms == AirlineInsurance.PaymentTerms.Monthly) policy.PaymentAmount = policy.InsuredAmount/length/12;
                            break;
                    }
                    break;

                    #endregion

                    #region Passenger Liability

                case AirlineInsurance.InsuranceType.PassengerLiability:
                    switch (scope)
                    {
                        case AirlineInsurance.InsuranceScope.Airport:
                            policy.Deductible = amount*0.005;
                            policy.PaymentAmount = policy.InsuredAmount*(4/10)*typeMPassenger*scMAirport;
                            if (terms == AirlineInsurance.PaymentTerms.Annual) policy.PaymentAmount = policy.InsuredAmount/length;
                            if (terms == AirlineInsurance.PaymentTerms.Biannual) policy.PaymentAmount = policy.InsuredAmount/length/2;
                            if (terms == AirlineInsurance.PaymentTerms.Quarterly) policy.PaymentAmount = policy.InsuredAmount/length/4;
                            if (terms == AirlineInsurance.PaymentTerms.Monthly) policy.PaymentAmount = policy.InsuredAmount/length/12;
                            break;

                        case AirlineInsurance.InsuranceScope.Domestic:
                            policy.Deductible = amount*0.001;
                            policy.PaymentAmount = policy.InsuredAmount*(4/10)*typeMPassenger*scMDomestic;
                            if (terms == AirlineInsurance.PaymentTerms.Annual) policy.PaymentAmount = policy.InsuredAmount/length;
                            if (terms == AirlineInsurance.PaymentTerms.Biannual) policy.PaymentAmount = policy.InsuredAmount/length/2;
                            if (terms == AirlineInsurance.PaymentTerms.Quarterly) policy.PaymentAmount = policy.InsuredAmount/length/4;
                            if (terms == AirlineInsurance.PaymentTerms.Monthly) policy.PaymentAmount = policy.InsuredAmount/length/12;
                            break;

                        case AirlineInsurance.InsuranceScope.Hub:
                            policy.Deductible = amount*0.001;
                            policy.PaymentAmount = policy.InsuredAmount*(4/10)*typeMPassenger*scMHub;
                            if (terms == AirlineInsurance.PaymentTerms.Annual) policy.PaymentAmount = policy.InsuredAmount/length;
                            if (terms == AirlineInsurance.PaymentTerms.Biannual) policy.PaymentAmount = policy.InsuredAmount/length/2;
                            if (terms == AirlineInsurance.PaymentTerms.Quarterly) policy.PaymentAmount = policy.InsuredAmount/length/4;
                            if (terms == AirlineInsurance.PaymentTerms.Monthly) policy.PaymentAmount = policy.InsuredAmount/length/12;
                            break;

                        case AirlineInsurance.InsuranceScope.Global:
                            policy.Deductible = amount*0.001;
                            policy.PaymentAmount = policy.InsuredAmount*(4/10)*typeMPassenger*scMGlobal;
                            if (terms == AirlineInsurance.PaymentTerms.Annual) policy.PaymentAmount = policy.InsuredAmount/length;
                            if (terms == AirlineInsurance.PaymentTerms.Biannual) policy.PaymentAmount = policy.InsuredAmount/length/2;
                            if (terms == AirlineInsurance.PaymentTerms.Quarterly) policy.PaymentAmount = policy.InsuredAmount/length/4;
                            if (terms == AirlineInsurance.PaymentTerms.Monthly) policy.PaymentAmount = policy.InsuredAmount/length/12;
                            break;
                    }
                    break;

                    #endregion

                    #region Combined Single Limit

                case AirlineInsurance.InsuranceType.CombinedSingleLimit:
                    switch (scope)
                    {
                        case AirlineInsurance.InsuranceScope.Airport:
                            policy.Deductible = amount*0.005;
                            policy.PaymentAmount = policy.InsuredAmount*(4/10)*typeMCSL*scMAirport;
                            if (terms == AirlineInsurance.PaymentTerms.Annual) policy.PaymentAmount = policy.InsuredAmount/length;
                            if (terms == AirlineInsurance.PaymentTerms.Biannual) policy.PaymentAmount = policy.InsuredAmount/length/2;
                            if (terms == AirlineInsurance.PaymentTerms.Quarterly) policy.PaymentAmount = policy.InsuredAmount/length/4;
                            if (terms == AirlineInsurance.PaymentTerms.Monthly) policy.PaymentAmount = policy.InsuredAmount/length/12;
                            break;

                        case AirlineInsurance.InsuranceScope.Domestic:
                            policy.Deductible = amount*0.001;
                            policy.PaymentAmount = policy.InsuredAmount*(4/10)*typeMCSL*scMDomestic;
                            if (terms == AirlineInsurance.PaymentTerms.Annual) policy.PaymentAmount = policy.InsuredAmount/length;
                            if (terms == AirlineInsurance.PaymentTerms.Biannual) policy.PaymentAmount = policy.InsuredAmount/length/2;
                            if (terms == AirlineInsurance.PaymentTerms.Quarterly) policy.PaymentAmount = policy.InsuredAmount/length/4;
                            if (terms == AirlineInsurance.PaymentTerms.Monthly) policy.PaymentAmount = policy.InsuredAmount/length/12;
                            break;

                        case AirlineInsurance.InsuranceScope.Hub:
                            policy.Deductible = amount*0.001;
                            policy.PaymentAmount = policy.InsuredAmount*(4/10)*typeMCSL*scMHub;
                            if (terms == AirlineInsurance.PaymentTerms.Annual) policy.PaymentAmount = policy.InsuredAmount/length;
                            if (terms == AirlineInsurance.PaymentTerms.Biannual) policy.PaymentAmount = policy.InsuredAmount/length/2;
                            if (terms == AirlineInsurance.PaymentTerms.Quarterly) policy.PaymentAmount = policy.InsuredAmount/length/4;
                            if (terms == AirlineInsurance.PaymentTerms.Monthly) policy.PaymentAmount = policy.InsuredAmount/length/12;
                            break;

                        case AirlineInsurance.InsuranceScope.Global:
                            policy.Deductible = amount*0.001;
                            policy.PaymentAmount = policy.InsuredAmount*(4/10)*typeMCSL*scMGlobal;
                            if (terms == AirlineInsurance.PaymentTerms.Annual) policy.PaymentAmount = policy.InsuredAmount/length;
                            if (terms == AirlineInsurance.PaymentTerms.Biannual) policy.PaymentAmount = policy.InsuredAmount/length/2;
                            if (terms == AirlineInsurance.PaymentTerms.Quarterly) policy.PaymentAmount = policy.InsuredAmount/length/4;
                            if (terms == AirlineInsurance.PaymentTerms.Monthly) policy.PaymentAmount = policy.InsuredAmount/length/12;
                            break;
                    }
                    break;

                    #endregion

                    #region Full Coverage

                case AirlineInsurance.InsuranceType.FullCoverage:
                    switch (scope)
                    {
                        case AirlineInsurance.InsuranceScope.Airport:
                            policy.Deductible = amount*0.005;
                            policy.PaymentAmount = policy.InsuredAmount*(4/10)*typeMFull*scMAirport;
                            if (terms == AirlineInsurance.PaymentTerms.Annual) policy.PaymentAmount = policy.InsuredAmount/length;
                            if (terms == AirlineInsurance.PaymentTerms.Biannual) policy.PaymentAmount = policy.InsuredAmount/length/2;
                            if (terms == AirlineInsurance.PaymentTerms.Quarterly) policy.PaymentAmount = policy.InsuredAmount/length/4;
                            if (terms == AirlineInsurance.PaymentTerms.Monthly) policy.PaymentAmount = policy.InsuredAmount/length/12;
                            break;

                        case AirlineInsurance.InsuranceScope.Domestic:
                            policy.Deductible = amount*0.001;
                            policy.PaymentAmount = policy.InsuredAmount*(4/10)*typeMFull*scMDomestic;
                            if (terms == AirlineInsurance.PaymentTerms.Annual) policy.PaymentAmount = policy.InsuredAmount/length;
                            if (terms == AirlineInsurance.PaymentTerms.Biannual) policy.PaymentAmount = policy.InsuredAmount/length/2;
                            if (terms == AirlineInsurance.PaymentTerms.Quarterly) policy.PaymentAmount = policy.InsuredAmount/length/4;
                            if (terms == AirlineInsurance.PaymentTerms.Monthly) policy.PaymentAmount = policy.InsuredAmount/length/12;
                            break;

                        case AirlineInsurance.InsuranceScope.Hub:
                            policy.Deductible = amount*0.001;
                            policy.PaymentAmount = policy.InsuredAmount*(4/10)*typeMFull*scMHub;
                            if (terms == AirlineInsurance.PaymentTerms.Annual) policy.PaymentAmount = policy.InsuredAmount/length;
                            if (terms == AirlineInsurance.PaymentTerms.Biannual) policy.PaymentAmount = policy.InsuredAmount/length/2;
                            if (terms == AirlineInsurance.PaymentTerms.Quarterly) policy.PaymentAmount = policy.InsuredAmount/length/4;
                            if (terms == AirlineInsurance.PaymentTerms.Monthly) policy.PaymentAmount = policy.InsuredAmount/length/12;
                            break;

                        case AirlineInsurance.InsuranceScope.Global:
                            policy.Deductible = amount*0.001;
                            policy.PaymentAmount = policy.InsuredAmount*(4/10)*typeMFull*scMGlobal;
                            if (terms == AirlineInsurance.PaymentTerms.Annual) policy.PaymentAmount = policy.InsuredAmount/length;
                            if (terms == AirlineInsurance.PaymentTerms.Biannual) policy.PaymentAmount = policy.InsuredAmount/length/2;
                            if (terms == AirlineInsurance.PaymentTerms.Quarterly) policy.PaymentAmount = policy.InsuredAmount/length/4;
                            if (terms == AirlineInsurance.PaymentTerms.Monthly) policy.PaymentAmount = policy.InsuredAmount/length/12;
                            break;
                    }

                    #endregion

                    break;
            }

            if (allAirliners)
            {
                amount *= airline.Fleet.Count();
                policy.PaymentAmount *= (airline.Fleet.Count()*0.95);
            }
            return policy;
        }


        //gets insurance rate modifiers based on security, safety, and aircraft state of maintenance
        public static double GetRatingModifier(Airline airline)
        {
            double mod = 1;
            mod += (100 - airline.Ratings.MaintenanceRating)/100.0;
            mod += (100 - airline.Ratings.SafetyRating)/150.0;
            mod += (100 - airline.Ratings.SecurityRating)/100.0;
            return mod;
        }


        //extend or modify policy
        public static void ModifyPolicy(Airline airline, string index, AirlineInsurance newPolicy)
        {
            //AirlineInsurance oldPolicy = airline.InsurancePolicies[index];
            //use the index to compare the new policy passed in to the existing one and make changes
        }

        public static void CheckExpiredInsurance(Airline airline)
        {
            DateTime date = GameObject.GetInstance().GameTime;
            foreach (AirlineInsurance policy in airline.InsurancePolicies)
            {
                if (policy.InsuranceExpires < date)
                {
                    airline.RemoveInsurance(policy);
                }
            }
        }

        public static void MakeInsurancePayment(Airline airline)
        {
            foreach (AirlineInsurance policy in airline.InsurancePolicies)
            {
                if (policy.RemainingPayments > 0)
                {
                    if (policy.NextPaymentDue.Month == GameObject.GetInstance().GameTime.Month)
                    {
                        airline.Money -= policy.PaymentAmount;
                        AirlineHelpers.AddAirlineInvoice(airline, GameObject.GetInstance().GameTime, Invoice.InvoiceType.Maintenances, -policy.PaymentAmount);
                        policy.RemainingPayments--;
                        switch (policy.InsTerms)
                        {
                            case AirlineInsurance.PaymentTerms.Monthly:
                                policy.NextPaymentDue = GameObject.GetInstance().GameTime.AddMonths(1);
                                break;
                            case AirlineInsurance.PaymentTerms.Quarterly:
                                policy.NextPaymentDue = GameObject.GetInstance().GameTime.AddMonths(3);
                                break;
                            case AirlineInsurance.PaymentTerms.Biannual:
                                policy.NextPaymentDue = GameObject.GetInstance().GameTime.AddMonths(6);
                                break;
                            case AirlineInsurance.PaymentTerms.Annual:
                                policy.NextPaymentDue = GameObject.GetInstance().GameTime.AddMonths(12);
                                break;
                        }
                    }
                }
            }
        }


        //for general damage or claims
        public static void FileInsuranceClaim(Airline airline, AirlineInsurance policy, int damage)
        {
            var claim = new InsuranceClaim(airline, null, null, GameObject.GetInstance().GameTime, damage);
            airline.InsuranceClaims.Add(claim);
            var news = new News(News.NewsType.AirlineNews, GameObject.GetInstance().GameTime, "Insurance Claim Filed", "You have filed an insurance claim. Reference: " + claim.Index);
        }

        //for damage and claims involving an airport or airport facility
        public static void FileInsuranceClaim(Airline airline, Airport airport, int damage)
        {
            var claim = new InsuranceClaim(airline, null, airport, GameObject.GetInstance().GameTime, damage);
            airline.InsuranceClaims.Add(claim);
            var news = new News(News.NewsType.AirlineNews, GameObject.GetInstance().GameTime, "Insurance Claim Filed", "You have filed an insurance claim. Reference: " + claim.Index);
        }


        //for damage and claims involving an airliner
        public static void FileInsuranceClaim(Airline airline, FleetAirliner airliner, int damage)
        {
            var claim = new InsuranceClaim(airline, airliner, null, GameObject.GetInstance().GameTime, damage);
            airline.InsuranceClaims.Add(claim);
            var news = new News(News.NewsType.AirlineNews, GameObject.GetInstance().GameTime, "Insurance Claim Filed", "You have filed an insurance claim. Reference: " + claim.Index);
        }

        //for damage and claims involving an airliner and airport or airport facility
        public static void FileInsuranceClaim(Airline airline, FleetAirliner airliner, Airport airport, int damage)
        {
            var claim = new InsuranceClaim(airline, airliner, airport, GameObject.GetInstance().GameTime, damage);
            airline.InsuranceClaims.Add(claim);
            var news = new News(News.NewsType.AirlineNews, GameObject.GetInstance().GameTime, "Insurance Claim Filed", "You have filed an insurance claim. Reference: " + claim.Index);
        }


        public static void ReceiveInsurancePayout(Airline airline, AirlineInsurance policy, InsuranceClaim claim)
        {
            if (claim.Damage > policy.Deductible)
            {
                claim.Damage -= (int) policy.Deductible;
                airline.Money -= policy.Deductible;
                policy.Deductible = 0;
                policy.InsuredAmount -= claim.Damage;
                airline.Money += claim.Damage;
                var news = new News(News.NewsType.AirlineNews, GameObject.GetInstance().GameTime, "Insurance Claim Payout",
                                    "You have received an insurance payout in the amount of $" + claim.Damage.ToString(CultureInfo.InvariantCulture) + ". This was for claim number " + claim.Index);
            }

            else if (claim.Damage < policy.Deductible)
            {
                policy.Deductible -= claim.Damage;
                //Warnings.AddWarning("Low Damage", "The damage incurred was less than your deductible, so you will not receive an insurance payout for this claim! \n Reference: " + claim.Index);
            }
        }
    }
}