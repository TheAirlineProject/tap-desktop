using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using TheAirline.General.Models.Countries;
using TheAirline.Helpers;
using TheAirline.Infrastructure;
using TheAirline.Models.Airliners;
using TheAirline.Models.Airlines.Subsidiary;
using TheAirline.Models.Airports;
using TheAirline.Models.General;
using TheAirline.Models.General.Countries;
using TheAirline.Models.General.Finances;
using TheAirline.Models.General.Statistics;
using TheAirline.Models.Pilots;
using TheAirline.Models.Routes;

namespace TheAirline.Models.Airlines
{
    [Serializable]
    //the class for an airline
    public class Airline : BaseModel
    {
        private List<KeyValuePair<DateTime, KeyValuePair<double, double>>> _dailyOperatingBalanceHistory;

        #region Constructors and Destructors

        public Airline(
            AirlineProfile profile,
            AirlineMentality mentality,
            AirlineFocus marketFocus,
            AirlineLicense license,
            Route.RouteType routeFocus)
        {
            Scores = new AirlineScores();
            Shares = new List<AirlineShare>();
            Airports = new List<Airport>();
            Fleet = new List<FleetAirliner>();
            Routes = new List<Route>();
            FutureAirlines = new List<FutureSubsidiaryAirline>();
            Subsidiaries = new List<SubsidiaryAirline>();
            Advertisements = new Dictionary<AdvertisementType.AirlineAdvertisementType, AdvertisementType>();
            Codeshares = new List<CodeshareAgreement>();
            Statistics = new GeneralStatistics();
            Facilities = new List<AirlineFacility>();
            Invoices = new Invoices();
            Budget = new AirlineBudget();
            BudgetHistory = new Dictionary<DateTime, AirlineBudget>();
            TestBudget = new Dictionary<DateTime, AirlineBudget>();
            Profile = profile;
            AirlineRouteFocus = routeFocus;
            Loans = new List<Loan>();
            Reputation = 50;
            Alliances = new List<Alliance>();
            Mentality = mentality;
            MarketFocus = marketFocus;
            License = license;
            Policies = new List<AirlinePolicy>();
            EventLog = new List<RandomEvent>();
            Ratings = new AirlineRatings();
            OverallScore = CountedScores = 0;
            GameScores = new Dictionary<DateTime, int>();
            InsuranceClaims = new List<InsuranceClaim>();
            InsurancePolicies = new List<AirlineInsurance>();
            SpecialContracts = new List<SpecialContract>();

            CreateStandardAdvertisement();

            Pilots = new List<Pilot>();
            FlightSchools = new List<FlightSchool>();
            Budget = new AirlineBudget();
        }

        protected Airline(SerializationInfo info, StreamingContext ctxt) : base(info, ctxt)
        {
            Shares = new List<AirlineShare>();

            if (Version == 1)
            {
                AirlineHelpers.CreateStandardAirlineShares(this, 100);
            }
            if (Version < 4)
            {
                Routes = new List<Route>();
                Advertisements =
                    new Dictionary<AdvertisementType.AirlineAdvertisementType, AdvertisementType>();
                CreateStandardAdvertisement();
            }
            if (Version < 5)
                SpecialContracts = new List<SpecialContract>();

            if (Invoices == null)
            {
                Invoices = new Invoices();
            }
        }

        #endregion

        #region Enums

        public enum AirlineFocus
        {
            Global,

            Regional,

            Domestic,

            Local
        }

        public enum AirlineLicense
        {
            Domestic,

            Regional,

            ShortHaul,

            LongHaul
        }

        public enum AirlineMentality
        {
            Safe,

            Moderate,

            Aggressive
        }

        public enum AirlineValue
        {
            VeryLow,

            Low,

            Normal,

            High,

            VeryHigh
        }

        #endregion

        #region Public Properties

        [Versioning("advertisements")]
        public Dictionary<AdvertisementType.AirlineAdvertisementType, AdvertisementType> Advertisements { get; set; }

        [Versioning("routefocus")]
        public Route.RouteType AirlineRouteFocus { get; set; }

        //0-100 with 0-9 as very_low, 10-30 as low, 31-70 as normal, 71-90 as high,91-100 as very_high 

        [Versioning("airports")]
        public List<Airport> Airports { get; set; }

        [Versioning("alliances")]
        public List<Alliance> Alliances { get; set; }

        [Versioning("avgfleetvalue")]
        public long AvgFleetValue { get; set; }

        [Versioning("budget")]
        public AirlineBudget Budget { get; set; }

        [Versioning("budgethistory")]
        public IDictionary<DateTime, AirlineBudget> BudgetHistory { get; set; }

        [Versioning("codeshares", Version = 3)]
        public List<CodeshareAgreement> Codeshares { get; set; }

        [Versioning("contract")]
        public ManufacturerContract Contract { get; set; }

        [Versioning("countedscores")]
        public int CountedScores { get; set; }

        public List<FleetAirliner> DeliveredFleet => GetDeliveredFleet();

        [Versioning("eventlists")]
        public List<RandomEvent> EventList { get; set; }

        [Versioning("eventlog")]
        public List<RandomEvent> EventLog { get; set; }

        [Versioning("facilities")]
        public List<AirlineFacility> Facilities { get; set; }

        [Versioning("fees")]
        public AirlineFees Fees { get; set; }

        [Versioning("fleet")]
        public List<FleetAirliner> Fleet { get; set; }

        [Versioning("fleetvalue")]
        public long FleetValue { get; set; }

        [Versioning("flightschools")]
        public List<FlightSchool> FlightSchools { get; set; }

        [Versioning("futureairlines")]
        public List<FutureSubsidiaryAirline> FutureAirlines { get; set; }

        [Versioning("gamescores")]
        public Dictionary<DateTime, int> GameScores { get; set; }

        [Versioning("insuranceclaims")]
        public List<InsuranceClaim> InsuranceClaims { get; set; }

        [Versioning("insurancepolicies")]
        public List<AirlineInsurance> InsurancePolicies { get; set; }

        [Versioning("invoices")]
        public Invoices Invoices { get; set; }

        public bool IsHuman => isHuman();

        public bool IsSubsidiary => IsSubsidiaryAirline();

        [Versioning("license")]
        public AirlineLicense License { get; set; }

        [Versioning("loans")]
        public List<Loan> Loans { get; set; }

        [Versioning("marketfocus")]
        public AirlineFocus MarketFocus { get; set; }

        [Versioning("mentality")]
        public AirlineMentality Mentality { get; set; }

        [Versioning("money")]
        public double Money { get; set; }

        [Versioning("overallscore")]
        public int OverallScore { get; set; }

        [Versioning("pilots")]
        public List<Pilot> Pilots { get; set; }

        [Versioning("policies")]
        public List<AirlinePolicy> Policies { get; set; }

        [Versioning("profile")]
        public AirlineProfile Profile { get; set; }

        [Versioning("ratings")]
        public AirlineRatings Ratings { get; set; }

        [Versioning("reputation")]
        public int Reputation { get; set; }

        public List<Route> Routes
        {
            get { return GetRoutes(); }
            set { _Routes = value; }
        }

        [Versioning("scores")]
        public AirlineScores Scores { get; set; }

        public List<AirlineShare> Shares { get; set; }

        [Versioning("startmoney")]
        public double StartMoney { get; set; }

        [Versioning("statistics")]
        public GeneralStatistics Statistics { get; set; }

        [Versioning("subsidiaries")]
        public List<SubsidiaryAirline> Subsidiaries { get; set; }

        public IDictionary<DateTime, AirlineBudget> TestBudget { get; set; }

        [Versioning("routes")]
        public List<Route> _Routes { get; set; }

        [Versioning("scontracts", Version = 5)]
        public List<SpecialContract> SpecialContracts { get; set; }


        [Versioning("dailyoperatingbalancehistory")]
        public List<KeyValuePair<DateTime, KeyValuePair<double, double>>> DailyOperatingBalanceHistory
        {
            get { return _dailyOperatingBalanceHistory ?? (_dailyOperatingBalanceHistory = new List<KeyValuePair<DateTime, KeyValuePair<double, double>>>()); }
            protected set
            {
                if (value != null)
                {
                    _dailyOperatingBalanceHistory = value;
                }
            }
        }

        #endregion

        #region Public Methods and Operators

        public override void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            info.AddValue("version", 5);

            base.GetObjectData(info, context);
        }

        public void AddAirlinePolicy(AirlinePolicy policy)
        {
            Policies.Add(policy);
        }

        //adds an airliner to the airlines fleet
        public void AddAirliner(FleetAirliner.PurchasedType type, Airliner airliner, Airport homeBase)
        {
            AddAirliner(new FleetAirliner(type, GameObject.GetInstance().GameTime, this, airliner, homeBase));
        }

        //adds a fleet airliner to the airlines fleet
        public void AddAirliner(FleetAirliner airliner)
        {
            lock (Fleet)
            {
                Fleet.Add(airliner);
            }
        }

        //remove a fleet airliner from the airlines fleet

        //adds an airport to the airline
        public void AddAirport(Airport airport)
        {
            lock (Airports)
            {
                if (airport != null)
                {
                    Airports.Add(airport);
                }
            }
        }

        public void AddAlliance(Alliance alliance)
        {
            Alliances.Add(alliance);
        }

        public void AddCodeshareAgreement(CodeshareAgreement share)
        {
            Codeshares.Add(share);
        }

        //removes an airport from the airline

        //adds a facility to the airline
        public void AddFacility(AirlineFacility facility)
        {
            Facilities.Add(facility);
        }

        public void AddFlightSchool(FlightSchool school)
        {
            FlightSchools.Add(school);
        }

        public void AddInsurance(AirlineInsurance insurance)
        {
            InsurancePolicies.Add(insurance);
        }

        //removes a facility from the airline

        //adds an invoice for the airline - both incoming and expends - if updateMoney == true the money is updated as well
        public void AddInvoice(Invoice invoice, bool updateMoney = true)
        {
            Invoices.AddInvoice(invoice);

            if (updateMoney)
            {
                Money += invoice.Amount;
            }
        }

        //returns the reputation for the airline

        //adds a loan to the airline
        public void AddLoan(Loan loan)
        {
            lock (Loans)
            {
                Loans.Add(loan);
            }
        }

        public void AddPilot(Pilot pilot)
        {
            if (pilot == null)
            {
                throw new NullReferenceException("Pilot is null at Airline.cs/addPilot");
            }

            lock (Pilots)
            {
                Pilots.Add(pilot);
                pilot.Airline = this;
            }
        }

        public void AddRoute(Route route)
        {
            var routes = new List<Route>(Routes);

            lock (_Routes)
            {
                _Routes.Add(route);
                route.Airline = this;
            }
            /*
                foreach (string flightCode in route.TimeTable.Entries.Select(e => e.Destination.FlightCode).Distinct())
                    this.FlightCodes.Remove(flightCode);
          
         */
        }

        public void AddSubsidiaryAirline(SubsidiaryAirline subsidiary)
        {
            Subsidiaries.Add(subsidiary);
        }

        //removes a loan 

        //returns the advertisement for the airline for a specific type
        public AdvertisementType GetAirlineAdvertisement(AdvertisementType.AirlineAdvertisementType type)
        {
            return Advertisements[type];
        }

        public List<AdvertisementType> GetAirlineAdvertisements()
        {
            return Advertisements.Values.ToList();
        }

        public AirlinePolicy GetAirlinePolicy(string name)
        {
            return Policies.Find(p => p.Name == name);
        }

        public AirlineValue GetAirlineValue()
        {
            double value = GeneralHelpers.GetInflationPrice(GetValue()*1000000);
            double startMoney = GeneralHelpers.GetInflationPrice(StartMoney);

            if (value <= startMoney)
            {
                return AirlineValue.VeryLow;
            }
            if (value > startMoney && value < startMoney*3)
            {
                return AirlineValue.Low;
            }
            if (value >= startMoney*3 && value < startMoney*9)
            {
                return AirlineValue.Normal;
            }
            if (value >= startMoney*9 && value < startMoney*18)
            {
                return AirlineValue.High;
            }
            if (value >= startMoney*18)
            {
                return AirlineValue.VeryHigh;
            }

            return AirlineValue.Normal;
        }

        //creates the standard Advertisement for the airline

        /*! returns the average age for the fleet
         */

        public double GetAverageFleetAge()
        {
            if (Fleet.Count > 0)
            {
                return Fleet.Average(f => f.Airliner.Age);
            }
            return 0;
        }

        //returns total current value of fleet

        //returns the average value of an airliner in the fleet
        public long GetAvgFleetValue()
        {
            return GetFleetValue()/Fleet.Count();
        }

        public List<Airline> GetCodesharingAirlines()
        {
            return Codeshares.Select(c => c.Airline1 == this ? c.Airline2 : c.Airline1).ToList();
        }

        public double GetFleetSize()
        {
            return Fleet.Count;
        }

        public long GetFleetValue()
        {
            return Fleet.Sum(airliner => GetValue());
        }

        public List<string> GetFlightCodes()
        {
            var codes = new List<string>();

            IEnumerable<string> rCodes =
                Routes.SelectMany(r => r.TimeTable.Entries).Select(e => e.Destination.FlightCode).Distinct().ToList();

            for (int i = 0; i < 1000; i++)
            {
                string code = $"{Profile.IATACode}{i:0000}";

                if (!rCodes.Contains(code))
                {
                    codes.Add(code);
                }
            }

            return codes;

            /*
            var routes = new List<Route>(this.Routes);

            var entries = new List<RouteTimeTableEntry>(routes.SelectMany(r => r.TimeTable.Entries));
            
            foreach (RouteTimeTableEntry entry in entries)
            {
                if (codes.Contains(entry.Destination.FlightCode))
                    codes.Remove(entry.Destination.FlightCode);
                   
            }

            codes.Sort(delegate(string s1, string s2) { return s1.CompareTo(s2); });
            

            
            return codes;
         */
        }

        public List<Airport> GetHubs()
        {
            var hubs = new List<Airport>();
            lock (Airports)
            {
                hubs = (from a in Airports where a.GetHubs().Exists(h => h.Airline == this) select a).ToList();
            }
            return hubs;
        }

        public Invoices GetInvoices()
        {
            return Invoices;
        }

        /*
        //returns all invoices with type
        public List<Invoice> getInvoices(DateTime start, DateTime end, Invoice.InvoiceType type)
        
        {
            return this.Invoices.FindAll(i=>i.Date>=start && i.Date <=end && i.Type == type);
        }
        //returns all the invoices in a specific period
        public List<Invoice> getInvoices(DateTime start, DateTime end)
        {
           return this.Invoices.FindAll(delegate(Invoice i) { return i.Date >= start && i.Date <= end; });

        }
       */
        //returns the amount of all the invoices in a specific period of a specific type
        public double GetInvoicesAmount(DateTime startTime, DateTime endTime, Invoice.InvoiceType type)
        {
            int startYear = startTime.Year;
            int endYear = endTime.Year;

            int startMonth = startTime.Month;
            int endMonth = endTime.Month;

            int totalMonths = (endMonth - startMonth) + 12*(endYear - startYear) + 1;

            double totalAmount = 0;

            var date = new DateTime(startYear, startMonth, 1);

            for (int i = 0; i < totalMonths; i++)
            {
                if (type == Invoice.InvoiceType.Total)
                {
                    totalAmount += Invoices.GetAmount(date.Year, date.Month);
                }
                else
                {
                    totalAmount += Invoices.GetAmount(type, date.Year, date.Month);
                }

                date = date.AddMonths(1);
            }

            return totalAmount;
        }

        public double GetInvoicesAmountMonth(int year, int month, Invoice.InvoiceType type)
        {
            if (type == Invoice.InvoiceType.Total)
            {
                return Invoices.GetAmount(year, month);
            }
            return Invoices.GetAmount(type, year, month);
        }

        public double GetInvoicesAmountYear(int year, Invoice.InvoiceType type)
        {
            if (type == Invoice.InvoiceType.Total)
            {
                return Invoices.GetYearlyAmount(year);
            }
            return Invoices.GetYearlyAmount(type, year);
        }

        public string GetNextFlightCode(int n)
        {
            return GetFlightCodes()[n];
        }

        public int GetNumberOfEmployees()
        {
            int instructors = FlightSchools.Sum(f => f.NumberOfInstructors);

            int cockpitCrew = Pilots.Count;
            int cabinCrew =
                Routes.Where(r => r.Type == Route.RouteType.Passenger)
                      .Sum(r => ((PassengerRoute) r).GetTotalCabinCrew());

            int serviceCrew =
                Airports.SelectMany(a => a.GetCurrentAirportFacilities(this))
                        .Where(a => a.EmployeeType == AirportFacility.EmployeeTypes.Support)
                        .Sum(a => a.NumberOfEmployees);
            int maintenanceCrew =
                Airports.SelectMany(a => a.GetCurrentAirportFacilities(this))
                        .Where(a => a.EmployeeType == AirportFacility.EmployeeTypes.Maintenance)
                        .Sum(a => a.NumberOfEmployees);

            return cockpitCrew + cabinCrew + serviceCrew + maintenanceCrew + instructors;
        }

        public double GetProfit()
        {
            return Money - StartMoney;
        }

        public AirlineValue GetReputation()
        {
            //0-100 with 0-10 as very_low, 11-30 as low, 31-70 as normal, 71-90 as high,91-100 as very_high 
            if (Reputation < 11)
            {
                return AirlineValue.VeryLow;
            }
            if (Reputation > 10 && Reputation < 31)
            {
                return AirlineValue.Low;
            }
            if (Reputation > 30 && Reputation < 71)
            {
                return AirlineValue.Normal;
            }
            if (Reputation > 70 && Reputation < 91)
            {
                return AirlineValue.High;
            }
            if (Reputation > 90)
            {
                return AirlineValue.VeryHigh;
            }
            return AirlineValue.Normal;
        }

        //returns the value of the airline in "money"
        public long GetValue()
        {
            double value = 0;
            value += Money;

            var fleet =
                new List<FleetAirliner>(Fleet.FindAll(f => f.Purchased != FleetAirliner.PurchasedType.Leased));

            value = fleet.Aggregate(value, (current, airliner) => current + airliner.Airliner.GetPrice());

            var facilities = new List<AirlineFacility>(Facilities);
            value += facilities.Sum(facility => facility.Price);

            var airports = new List<Airport>(Airports);
            value += airports.SelectMany(airport => new List<AirlineAirportFacility>(airport.GetAirportFacilities(this))).Sum(facility => facility.Facility.Price);

            lock (Loans)
            {
                var loans = new List<Loan>(Loans);
                value = loans.Aggregate(value, (current, loan) => current - loan.PaymentLeft);
            }
            var subs = new List<SubsidiaryAirline>(Subsidiaries);
            value = subs.Aggregate(value, (current, subAirline) => current + subAirline.GetValue());

            value = value/1000000;

            if (double.IsNaN(value))
            {
                return 0;
            }
            return Convert.ToInt64(value);
        }

        //returns if the airline is a subsidiary airline

        //returns if it is the human airline
        public virtual bool isHuman()
        {
            return this == GameObject.GetInstance().HumanAirline || this == GameObject.GetInstance().MainAirline;
        }

        public virtual bool IsSubsidiaryAirline()
        {
            return false;
        }

        public void RemoveAirliner(FleetAirliner airliner)
        {
            Fleet.Remove(airliner);

            airliner.Airliner.Airline = null;
        }

        public void RemoveAirport(Airport airport)
        {
            Airports.Remove(airport);

            airport.Cooperations.RemoveAll(r => r.Airline == this);
        }

        public void RemoveAlliance(Alliance alliance)
        {
            lock (Alliances)
            {
                if (Alliances.Contains(alliance))
                {
                    Alliances.Remove(alliance);
                }
            }
        }

        //adds a subsidiary airline to the airline

        //removes a code share agreement from the airline
        public void RemoveCodeshareAgreement(CodeshareAgreement share)
        {
            Codeshares.Remove(share);
        }

        public void RemoveFacility(AirlineFacility facility)
        {
            Facilities.Remove(facility);
        }

        public void RemoveFlightSchool(FlightSchool school)
        {
            FlightSchools.Remove(school);

            foreach (Instructor instructor in school.Instructors)
            {
                instructor.FlightSchool = null;
            }
        }

        public void RemoveInsurance(AirlineInsurance insurance)
        {
            InsurancePolicies.Remove(insurance);
        }

        public void RemoveLoan(Loan loan)
        {
            Loans.Remove(loan);
        }

        public void RemovePilot(Pilot pilot)
        {
            Pilots.Remove(pilot);
            pilot.Airline = null;

            pilot.Airliner?.RemovePilot(pilot);
        }

        public void RemoveRoute(Route route)
        {
            lock (_Routes)
            {
                _Routes.Remove(route);

                /*
                foreach (string flightCode in route.TimeTable.Entries.Select(e => e.Destination.FlightCode).Distinct())
                    this.FlightCodes.Add(flightCode);*/
            }
        }

        public void RemoveSubsidiaryAirline(SubsidiaryAirline subsidiary)
        {
            Subsidiaries.Remove(subsidiary);
        }

        public void SetAirlineAdvertisement(AdvertisementType type)
        {
            if (!Advertisements.ContainsKey(type.Type))
            {
                Advertisements.Add(type.Type, type);
            }
            else
            {
                Advertisements[type.Type] = type;
            }
        }

        //adds a policy to the airline

        //sets the policy for the airline
        public void SetAirlinePolicy(string name, object value)
        {
            Policies.Find(p => p.Name == name).PolicyValue = value;
        }

        public void SetInvoice(Invoice invoice)
        {
            Invoices.AddInvoice(invoice);
        }

        public void SetInvoice(Invoice.InvoiceType type, int year, int month, int day, double amount)
        {
            Invoices.AddInvoice(type, year, month, day, amount);
        }

        public void StoreBudget(AirlineBudget budget)
        {
            BudgetHistory.Add(GameObject.GetInstance().GameTime, budget);
        }

        #endregion

        #region Methods

        private void CreateStandardAdvertisement()
        {
            foreach (
                AdvertisementType.AirlineAdvertisementType type in
                    Enum.GetValues(typeof (AdvertisementType.AirlineAdvertisementType)))
            {
                SetAirlineAdvertisement(AdvertisementTypes.GetBasicType(type));
            }
        }

        private List<FleetAirliner> GetDeliveredFleet()
        {
            return
                Fleet.FindAll(
                    (a => a.Airliner.BuiltDate <= GameObject.GetInstance().GameTime));
        }

        private List<Route> GetRoutes()
        {
            var routes = new List<Route>();
            lock (_Routes)
            {
                routes = new List<Route>(_Routes);
            }

            return routes;
        }

        #endregion

        //returns a policy for the airline
        public bool HasRouteTo(Airport airport)
        {
            return
                Routes.Any(
                    r =>
                    ((r.Destination1.Profile.IATACode == airport.Profile.IATACode)
                     || (r.Destination2.Profile.IATACode == airport.Profile.IATACode)));
        }

        //returns a policy for the airline
        public bool HasAirplaneOnRouteTo(Airport airport)
        {
            return
                Routes.Any(
                    r =>
                    ((r.Destination1.Profile.IATACode == airport.Profile.IATACode)
                     || (r.Destination2.Profile.IATACode == airport.Profile.IATACode)) && r.HasAirliner);
        }
    }

    //the list of airlines
    public class Airlines
    {
        #region Static Fields

        private static readonly List<Airline> airlines = new List<Airline>();

        #endregion

        #region Public Methods and Operators

        public static void AddAirline(Airline airline)
        {
            airlines.Add(airline);
        }

        public static void Clear()
        {
            airlines.Clear();
        }

        public static bool ContainsAirline(Airline airline)
        {
            return airlines.Contains(airline);
        }

        //returns an airline
        public static Airline GetAirline(string iata)
        {
            return airlines.Find(a => a.Profile.IATACode == iata);
        }

        public static Airline GetAirline(string iata, int year)
        {
            return airlines.Find(a => a.Profile.IATACode == iata && a.Profile.Founded <= year && a.Profile.Folded >= year);
        }

        //returns all airlines

        //returns all airlines for a specific region
        public static List<Airline> GetAirlines(Region region)
        {
            return airlines.FindAll(a => a.Profile.Country.Region == region);
        }

        //returns a list of airlines
        public static List<Airline> GetAirlines(Predicate<Airline> match)
        {
            List<Airline> tAirlines;
            lock (airlines)
            {
                tAirlines = new List<Airline>(airlines.FindAll(match));
            }
            return tAirlines;
        }

        public static List<Airline> GetAllAirlines()
        {
            return airlines;
        }

        //returns all human airlines
        public static List<Airline> GetHumanAirlines()
        {
            return airlines.FindAll(a => a.IsHuman);
        }

        public static int GetNumberOfAirlines()
        {
            return airlines.Count;
        }

        //removes an airline from the list
        public static void RemoveAirline(Airline airline)
        {
            airlines.Remove(airline);
        }

        //removes airlines from the list
        public static void RemoveAirlines(Predicate<Airline> match)
        {
            airlines.RemoveAll(match);
        }

        #endregion

        //clears the list

        //adds an airline to the collection

        //returns if the list of airlines contains an airline
    }
}