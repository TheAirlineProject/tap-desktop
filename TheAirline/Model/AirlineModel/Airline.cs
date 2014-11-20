namespace TheAirline.Model.AirlineModel
{
    using System;
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using System.Linq;
    using System.Reflection;
    using System.Runtime.Caching;
    using System.Runtime.CompilerServices;
    using System.Runtime.Serialization;
    using TheAirline.Model.AirlineModel.SubsidiaryModel;
    using TheAirline.Model.AirlinerModel;
    using TheAirline.Model.AirlinerModel.RouteModel;
    using TheAirline.Model.AirportModel;
    using TheAirline.Model.GeneralModel;
    using TheAirline.Model.GeneralModel.Helpers;
    using TheAirline.Model.GeneralModel.InvoicesModel;
    using TheAirline.Model.GeneralModel.StatisticsModel;
    using TheAirline.Model.PilotModel;

    [Serializable]
    //the class for an airline
    public class Airline : ISerializable
    {

        private List<KeyValuePair<DateTime, KeyValuePair<double, double>>> dailyOperatingBalanceHistory;

        #region Constructors and Destructors

        public Airline(
            AirlineProfile profile,
            AirlineMentality mentality,
            AirlineFocus marketFocus,
            AirlineLicense license,
            Route.RouteType routeFocus)
        {
            this.Scores = new AirlineScores();
            this.Shares = new AirlineShare[10];
            this.Airports = new List<Airport>();
            this.Fleet = new List<FleetAirliner>();
            this.Routes = new List<Route>();
            this.FutureAirlines = new List<FutureSubsidiaryAirline>();
            this.Subsidiaries = new List<SubsidiaryAirline>();
            this.Advertisements = new Dictionary<AdvertisementType.AirlineAdvertisementType, AdvertisementType>();
            this.Codeshares = new List<CodeshareAgreement>();
            this.Statistics = new GeneralStatistics();
            this.Facilities = new List<AirlineFacility>();
            this.Invoices = new Invoices();
            this.Budget = new AirlineBudget();
            this.BudgetHistory = new Dictionary<DateTime, AirlineBudget>();
            this.TestBudget = new Dictionary<DateTime, AirlineBudget>();
            this.Profile = profile;
            this.AirlineRouteFocus = routeFocus;
            this.Loans = new List<Loan>();
            this.Reputation = 50;
            this.Alliances = new List<Alliance>();
            this.Mentality = mentality;
            this.MarketFocus = marketFocus;
            this.License = license;
            this.Policies = new List<AirlinePolicy>();
            this.EventLog = new List<RandomEvent>();
            this.Ratings = new AirlineRatings();
            this.OverallScore = this.CountedScores = 0;
            this.GameScores = new Dictionary<DateTime, int>();
            this.InsuranceClaims = new List<InsuranceClaim>();
            this.InsurancePolicies = new List<AirlineInsurance>();
            this.SpecialContracts = new List<SpecialContract>();
            this.MaintenanceCenters = new List<MaintenanceCenter>();
            this.Maintenances = new Dictionary<AirlinerMaintenanceType, AirlinerMaintenanceCenter>();
        
            this.createStandardAdvertisement();

            this.Pilots = new List<Pilot>();
            this.FlightSchools = new List<FlightSchool>();
            this.Budget = new AirlineBudget();
        }

        protected Airline(SerializationInfo info, StreamingContext ctxt)
        {
            try
            {
                int version = info.GetInt16("version");

                IEnumerable<FieldInfo> fields =
                    this.GetType()
                        .GetFields(BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public)
                        .Where(p => p.GetCustomAttribute(typeof(Versioning)) != null);

                IList<PropertyInfo> props =
                    new List<PropertyInfo>(
                        this.GetType()
                            .GetProperties(BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public)
                            .Where(p => p.GetCustomAttribute(typeof(Versioning)) != null));

                IEnumerable<MemberInfo> propsAndFields = props.Cast<MemberInfo>().Union(fields.Cast<MemberInfo>());

                foreach (SerializationEntry entry in info)
                {
                    MemberInfo prop =
                        propsAndFields.FirstOrDefault(
                            p => ((Versioning)p.GetCustomAttribute(typeof(Versioning))).Name == entry.Name);

                    if (prop != null)
                    {
                        if (prop is FieldInfo)
                        {
                            ((FieldInfo)prop).SetValue(this, entry.Value);
                        }
                        else
                        {
                            ((PropertyInfo)prop).SetValue(this, entry.Value);
                        }
                    }
                }

                IEnumerable<MemberInfo> notSetProps =
                    propsAndFields.Where(p => ((Versioning)p.GetCustomAttribute(typeof(Versioning))).Version > version);

                foreach (MemberInfo notSet in notSetProps)
                {
                    var ver = (Versioning)notSet.GetCustomAttribute(typeof(Versioning));

                    if (ver.AutoGenerated)
                    {
                        if (notSet is FieldInfo)
                        {
                            ((FieldInfo)notSet).SetValue(this, ver.DefaultValue);
                        }
                        else
                        {
                            ((PropertyInfo)notSet).SetValue(this, ver.DefaultValue);
                        }
                    }
                }

                if (version == 1)
                {
                    AirlineHelpers.CreateStandardAirlineShares(this, 100);
                }
                if (version < 4)
                {
                    this.Routes = new List<Route>();
                    this.Advertisements =
                        new Dictionary<AdvertisementType.AirlineAdvertisementType, AdvertisementType>();
                    this.createStandardAdvertisement();
                }
                if (version < 5)
                    this.SpecialContracts = new List<SpecialContract>();

                if (version < 6)
                    this.Shares = new AirlineShare[10];

                if (version < 7)
                    this.MaintenanceCenters = new List<MaintenanceCenter>();

                if (version < 8)
                    this.Maintenances = new Dictionary<AirlinerMaintenanceType, AirlinerMaintenanceCenter>();

                if (this.Maintenances == null)
                    this.Maintenances = new Dictionary<AirlinerMaintenanceType, AirlinerMaintenanceCenter>();

                if (this.Invoices == null)
                {
                    this.Invoices = new Invoices();
                }
                if (this.SpecialContracts == null)
                    this.SpecialContracts = new List<SpecialContract>();

                if (this.MaintenanceCenters == null)
                    this.MaintenanceCenters = new List<MaintenanceCenter>();
            }
            catch (Exception e)
            {
                string s = e.ToString();
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

            Short_Haul,

            Long_Haul
        }

        public enum AirlineMentality
        {
            Safe,

            Moderate,

            Aggressive
        }

        public enum AirlineValue
        {
            Very_low,

            Low,

            Normal,

            High,

            Very_high
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

        [Versioning("maintenances",Version=8)]
        public Dictionary<AirlinerMaintenanceType,AirlinerMaintenanceCenter> Maintenances { get; set; } 

        [Versioning("alliances")]
        public List<Alliance> Alliances { get; set; }

        [Versioning("avgfleetvalue")]
        public Int64 AvgFleetValue { get; set; }

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

        public List<FleetAirliner> DeliveredFleet
        {
            get
            {
                return this.getDeliveredFleet();
            }
            set
            {
                ;
            }
        }

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
        public Int64 FleetValue { get; set; }

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

        public Boolean IsHuman
        {
            get
            {
                return this.isHuman();
            }
            set
            {
                ;
            }
        }

        public Boolean IsSubsidiary
        {
            get
            {
                return this.isSubsidiaryAirline();
            }
            set
            {
                ;
            }
        }

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
            get
            {
                return this.getRoutes();
            }
            set
            {
                this._Routes = value;
            }
        }

        [Versioning("scores")]
        public AirlineScores Scores { get; set; }

        [Versioning("shares", Version = 6)]
        public AirlineShare[] Shares { get; set; }

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

        [Versioning("centers", Version = 7)]
        public List<MaintenanceCenter> MaintenanceCenters { get; set; }

        [Versioning("dailyoperatingbalancehistory")]
        public List<KeyValuePair<DateTime, KeyValuePair<double, double>>> DailyOperatingBalanceHistory
        {
            get
            {
                if (this.dailyOperatingBalanceHistory == null)
                {
                    this.dailyOperatingBalanceHistory = new List<KeyValuePair<DateTime, KeyValuePair<double, double>>>();

                }

                return this.dailyOperatingBalanceHistory;
            }
            protected set
            {
                if (value != null)
                {
                    this.dailyOperatingBalanceHistory = value;


                }
            }
        }

        #endregion

        #region Public Methods and Operators

        public virtual void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            info.AddValue("version", 8);

            Type myType = this.GetType();

            IEnumerable<FieldInfo> fields =
                myType.GetFields(BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public)
                    .Where(p => p.GetCustomAttribute(typeof(Versioning)) != null);

            IList<PropertyInfo> props =
                new List<PropertyInfo>(
                    myType.GetProperties(BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public)
                        .Where(p => p.GetCustomAttribute(typeof(Versioning)) != null));

            IEnumerable<MemberInfo> propsAndFields = props.Cast<MemberInfo>().Union(fields.Cast<MemberInfo>());

            foreach (MemberInfo member in propsAndFields)
            {
                object propValue;

               

                if (member is FieldInfo)
                {
                    propValue = ((FieldInfo)member).GetValue(this);
                }
                else
                {
                    propValue = ((PropertyInfo)member).GetValue(this, null);
                }

                var att = (Versioning)member.GetCustomAttribute(typeof(Versioning));

                info.AddValue(att.Name, propValue);
            }

        }

        public void addAirlinePolicy(AirlinePolicy policy)
        {
            this.Policies.Add(policy);
        }

        //adds an airliner to the airlines fleet
        public void addAirliner(FleetAirliner.PurchasedType type, Airliner airliner, Airport homeBase)
        {
            airliner.Airline = this;

            this.addAirliner(new FleetAirliner(type, GameObject.GetInstance().GameTime, this, airliner, homeBase));
        }

        //adds a fleet airliner to the airlines fleet
        public void addAirliner(FleetAirliner airliner)
        {
            lock (this.Fleet)
            {
                this.Fleet.Add(airliner);
            }
            foreach (AirlinerMaintenanceType type in this.Maintenances.Keys)
            {
                AirlinerMaintenanceCheck check = airliner.Maintenance.Checks.Find(c=>c.Type == type);

                if (check != null)
                {
                    check.CheckCenter = new AirlinerMaintenanceCenter(type);

                    if (this.Maintenances[type].Airport == null)
                        check.CheckCenter.Center = this.Maintenances[type].Center;
                    else
                        check.CheckCenter.Airport = this.Maintenances[type].Airport;
                }

            }
            
        }

        //remove a fleet airliner from the airlines fleet

        //adds an airport to the airline
        public void addAirport(Airport airport)
        {
            lock (this.Airports)
            {
                if (airport != null)
                {
                    this.Airports.Add(airport);
                }
            }
        }

        public void addAlliance(Alliance alliance)
        {
            this.Alliances.Add(alliance);
        }

        public void addCodeshareAgreement(CodeshareAgreement share)
        {
            this.Codeshares.Add(share);
        }

        //removes an airport from the airline

        //adds a facility to the airline
        public void addFacility(AirlineFacility facility)
        {
            this.Facilities.Add(facility);
        }

        public void addFlightSchool(FlightSchool school)
        {
            this.FlightSchools.Add(school);
        }

        public void addInsurance(AirlineInsurance insurance)
        {
            this.InsurancePolicies.Add(insurance);
        }

        //removes a facility from the airline

        //adds an invoice for the airline - both incoming and expends - if updateMoney == true the money is updated as well
        public void addInvoice(Invoice invoice, Boolean updateMoney = true)
        {
            this.Invoices.addInvoice(invoice);

            if (updateMoney)
            {
                this.Money += invoice.Amount;
            }
        }

        //returns the reputation for the airline

        //adds a loan to the airline
        public void addLoan(Loan loan)
        {
            lock (this.Loans)
            {
                this.Loans.Add(loan);
            }
        }

        public void addPilot(Pilot pilot)
        {
            if (pilot == null)
            {
                throw new NullReferenceException("Pilot is null at Airline.cs/addPilot");
            }

            lock (this.Pilots)
            {
                this.Pilots.Add(pilot);
                pilot.Airline = this;
            }
        }

        public void addRoute(Route route)
        {
            var routes = new List<Route>(this.Routes);

            lock (this._Routes)
            {
                this._Routes.Add(route);
                route.Airline = this;
            }
            /*
                foreach (string flightCode in route.TimeTable.Entries.Select(e => e.Destination.FlightCode).Distinct())
                    this.FlightCodes.Remove(flightCode);
          
         */
        }

        public void addSubsidiaryAirline(SubsidiaryAirline subsidiary)
        {
            this.Subsidiaries.Add(subsidiary);
        }

        //removes a loan 

        //returns the advertisement for the airline for a specific type
        public AdvertisementType getAirlineAdvertisement(AdvertisementType.AirlineAdvertisementType type)
        {
            return this.Advertisements[type];
        }

        public List<AdvertisementType> getAirlineAdvertisements()
        {
            return this.Advertisements.Values.ToList();
        }

        public AirlinePolicy getAirlinePolicy(string name)
        {
            return this.Policies.Find(p => p.Name == name);
        }

        public AirlineValue getAirlineValue()
        {
            double value = GeneralHelpers.GetInflationPrice(this.getValue() * 1000000);
            double startMoney = GeneralHelpers.GetInflationPrice(this.StartMoney);

            if (value <= startMoney)
            {
                return AirlineValue.Very_low;
            }
            if (value > startMoney && value < startMoney * 3)
            {
                return AirlineValue.Low;
            }
            if (value >= startMoney * 3 && value < startMoney * 9)
            {
                return AirlineValue.Normal;
            }
            if (value >= startMoney * 9 && value < startMoney * 18)
            {
                return AirlineValue.High;
            }
            if (value >= startMoney * 18)
            {
                return AirlineValue.Very_high;
            }

            return AirlineValue.Normal;
        }

        //creates the standard Advertisement for the airline

        /*! returns the average age for the fleet
         */

        public double getAverageFleetAge()
        {
            if (this.Fleet.Count > 0)
            {
                return this.Fleet.Average(f => f.Airliner.Age);
            }
            return 0;
        }

        //returns total current value of fleet

        //returns the average value of an airliner in the fleet
        public long getAvgFleetValue()
        {
            return this.getFleetValue() / this.Fleet.Count();
        }

        public List<Airline> getCodesharingAirlines()
        {
            return this.Codeshares.Select(c => c.Airline1 == this ? c.Airline2 : c.Airline1).ToList();
        }

        public double getFleetSize()
        {
            return this.Fleet.Count;
        }

        public long getFleetValue()
        {
            long fleetValue = 0;
            foreach (FleetAirliner airliner in this.Fleet)
            {
                fleetValue += this.getValue();
            }
            return fleetValue;
        }

        public List<string> getFlightCodes()
        {
            var codes = new List<string>();

            IEnumerable<string> rCodes =
                this.Routes.SelectMany(r => r.TimeTable.Entries).Select(e => e.Destination.FlightCode).Distinct();

            for (int i = 0; i < 1000; i++)
            {
                string code = string.Format("{0}{1:0000}", this.Profile.IATACode, i);

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

        public List<Airport> getHubs()
        {
            var hubs = new List<Airport>();
            lock (this.Airports)
            {
                hubs = (from a in this.Airports where a.getHubs().Exists(h => h.Airline == this) select a).ToList();
            }
            return hubs;
        }

        public Invoices getInvoices()
        {
            return this.Invoices;
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
        public double getInvoicesAmount(DateTime startTime, DateTime endTime, Invoice.InvoiceType type)
        {
            int startYear = startTime.Year;
            int endYear = endTime.Year;

            int startMonth = startTime.Month;
            int endMonth = endTime.Month;

            int totalMonths = (endMonth - startMonth) + 12 * (endYear - startYear) + 1;

            double totalAmount = 0;

            var date = new DateTime(startYear, startMonth, 1);

            for (int i = 0; i < totalMonths; i++)
            {
                if (type == Invoice.InvoiceType.Total)
                {
                    totalAmount += this.Invoices.getAmount(date.Year, date.Month);
                }
                else
                {
                    totalAmount += this.Invoices.getAmount(type, date.Year, date.Month);
                }

                date = date.AddMonths(1);
            }

            return totalAmount;
        }

        public double getInvoicesAmountMonth(int year, int month, Invoice.InvoiceType type)
        {
            if (type == Invoice.InvoiceType.Total)
            {
                return this.Invoices.getAmount(year, month);
            }
            return this.Invoices.getAmount(type, year, month);
        }

        public double getInvoicesAmountYear(int year, Invoice.InvoiceType type)
        {
            if (type == Invoice.InvoiceType.Total)
            {
                return this.Invoices.getYearlyAmount(year);
            }
            return this.Invoices.getYearlyAmount(type, year);
        }

        public string getNextFlightCode(int n)
        {
            return this.getFlightCodes()[n];
        }

        public int getNumberOfEmployees()
        {
            int instructors = this.FlightSchools.Sum(f => f.NumberOfInstructors);

            int cockpitCrew = this.Pilots.Count;
            int cabinCrew =
                this.Routes.Where(r => r.Type == Route.RouteType.Passenger)
                    .Sum(r => ((PassengerRoute)r).getTotalCabinCrew());

            int serviceCrew =
                this.Airports.SelectMany(a => a.getCurrentAirportFacilities(this))
                    .Where(a => a.EmployeeType == AirportFacility.EmployeeTypes.Support)
                    .Sum(a => a.NumberOfEmployees);
            int maintenanceCrew =
                this.Airports.SelectMany(a => a.getCurrentAirportFacilities(this))
                    .Where(a => a.EmployeeType == AirportFacility.EmployeeTypes.Maintenance)
                    .Sum(a => a.NumberOfEmployees);

            return cockpitCrew + cabinCrew + serviceCrew + maintenanceCrew + instructors;
        }

        public double getProfit()
        {
            return this.Money - this.StartMoney;
        }

        public AirlineValue getReputation()
        {
            //0-100 with 0-10 as very_low, 11-30 as low, 31-70 as normal, 71-90 as high,91-100 as very_high 
            if (this.Reputation < 11)
            {
                return AirlineValue.Very_low;
            }
            if (this.Reputation > 10 && this.Reputation < 31)
            {
                return AirlineValue.Low;
            }
            if (this.Reputation > 30 && this.Reputation < 71)
            {
                return AirlineValue.Normal;
            }
            if (this.Reputation > 70 && this.Reputation < 91)
            {
                return AirlineValue.High;
            }
            if (this.Reputation > 90)
            {
                return AirlineValue.Very_high;
            }
            return AirlineValue.Normal;
        }

        //returns the value of the airline in "money"
        public long getValue()
        {
            double value = 0;
            value += this.Money;

            var fleet =
                new List<FleetAirliner>(this.Fleet.FindAll(f => f.Purchased != FleetAirliner.PurchasedType.Leased));

            foreach (FleetAirliner airliner in fleet)
            {
                value += airliner.Airliner.getPrice();
            }

            var facilities = new List<AirlineFacility>(this.Facilities);
            foreach (AirlineFacility facility in facilities)
            {
                value += facility.Price;
            }

            var airports = new List<Airport>(this.Airports);
            foreach (Airport airport in airports)
            {
                var aFacilities = new List<AirlineAirportFacility>(airport.getAirportFacilities(this));
                foreach (AirlineAirportFacility facility in aFacilities)
                {
                    value += facility.Facility.Price;
                }
            }

            lock (this.Loans)
            {
                var loans = new List<Loan>(this.Loans);
                foreach (Loan loan in loans)
                {
                    value -= loan.PaymentLeft;
                }
            }
            var subs = new List<SubsidiaryAirline>(this.Subsidiaries);
            foreach (SubsidiaryAirline subAirline in subs)
            {
                value += subAirline.getValue();
            }

            value = value / 1000000;

            if (double.IsNaN(value))
            {
                return 0;
            }
            return Convert.ToInt64(value);
        }

        //returns if the airline is a subsidiary airline

        //returns if it is the human airline
        public virtual Boolean isHuman()
        {
            return this == GameObject.GetInstance().HumanAirline || this == GameObject.GetInstance().MainAirline;
        }

        public virtual Boolean isSubsidiaryAirline()
        {
            return false;
        }

        public void removeAirliner(FleetAirliner airliner)
        {
            this.Fleet.Remove(airliner);

            airliner.Airliner.Airline = null;
        }

        public void removeAirport(Airport airport)
        {
            this.Airports.Remove(airport);

            airport.Cooperations.RemoveAll(r => r.Airline == this);
        }

        public void removeAlliance(Alliance alliance)
        {
            lock (this.Alliances)
            {
                if (this.Alliances.Contains(alliance))
                {
                    this.Alliances.Remove(alliance);
                }
            }
        }

        //adds a subsidiary airline to the airline

        //removes a code share agreement from the airline
        public void removeCodeshareAgreement(CodeshareAgreement share)
        {
            this.Codeshares.Remove(share);
        }

        public void removeFacility(AirlineFacility facility)
        {
            this.Facilities.Remove(facility);
        }

        public void removeFlightSchool(FlightSchool school)
        {
            this.FlightSchools.Remove(school);

            foreach (Instructor instructor in school.Instructors)
            {
                instructor.FlightSchool = null;
            }
        }

        public void removeInsurance(AirlineInsurance insurance)
        {
            this.InsurancePolicies.Remove(insurance);
        }

        public void removeLoan(Loan loan)
        {
            this.Loans.Remove(loan);
        }

        public void removePilot(Pilot pilot)
        {
            this.Pilots.Remove(pilot);
            pilot.Airline = null;

            if (pilot.Airliner != null)
            {
                pilot.Airliner.removePilot(pilot);
            }
        }

        public void removeRoute(Route route)
        {
            lock (this._Routes)
            {
                this._Routes.Remove(route);

                /*
                foreach (string flightCode in route.TimeTable.Entries.Select(e => e.Destination.FlightCode).Distinct())
                    this.FlightCodes.Add(flightCode);*/
            }
        }

        public void removeSubsidiaryAirline(SubsidiaryAirline subsidiary)
        {
            this.Subsidiaries.Remove(subsidiary);
        }

        public void setAirlineAdvertisement(AdvertisementType type)
        {
            if (!this.Advertisements.ContainsKey(type.Type))
            {
                this.Advertisements.Add(type.Type, type);
            }
            else
            {
                this.Advertisements[type.Type] = type;
            }
        }

        //adds a policy to the airline

        //sets the policy for the airline
        public void setAirlinePolicy(string name, object value)
        {
            this.Policies.Find(p => p.Name == name).PolicyValue = value;
        }

        public void setInvoice(Invoice invoice)
        {
            this.Invoices.addInvoice(invoice);
        }

        public void setInvoice(Invoice.InvoiceType type, int year, int month, int day, double amount)
        {
            this.Invoices.addInvoice(type, year, month, day, amount);
        }

        public void storeBudget(AirlineBudget budget)
        {
            this.BudgetHistory.Add(GameObject.GetInstance().GameTime, budget);
        }

        #endregion

        #region Methods
      
        private void createStandardAdvertisement()
        {
            if (this.Advertisements == null)
                this.Advertisements = new Dictionary<AdvertisementType.AirlineAdvertisementType, AdvertisementType>();

            foreach (
                AdvertisementType.AirlineAdvertisementType type in
                    Enum.GetValues(typeof(AdvertisementType.AirlineAdvertisementType)))
            {
                this.setAirlineAdvertisement(AdvertisementTypes.GetBasicType(type));
            }
        }

        private List<FleetAirliner> getDeliveredFleet()
        {
            return
                this.Fleet.FindAll(
                    (delegate(FleetAirliner a) { return a.Airliner.BuiltDate <= GameObject.GetInstance().GameTime; }));
        }

        private List<Route> getRoutes()
        {
            var routes = new List<Route>(this._Routes);

             return routes;
        }

        #endregion

        //returns a policy for the airline
        public bool hasRouteTo(Airport airport)
        {
            return
                Routes.Any(
                    r =>
                        ((r.Destination1.Profile.IATACode == airport.Profile.IATACode)
                         || (r.Destination2.Profile.IATACode == airport.Profile.IATACode)));
        }

        //returns a policy for the airline
        public bool hasAirplaneOnRouteTo(Airport airport)
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

        private static List<Airline> airlines = new List<Airline>();

        #endregion

        //clears the list

        //adds an airline to the collection

        #region Public Methods and Operators

        public static void AddAirline(Airline airline)
        {
     
            if (airline != null)
                airlines.Add(airline);
        }

        public static void Clear()
        {
            airlines = new List<Airline>();
        }

        public static Boolean ContainsAirline(Airline airline)
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

        //returns if the list of airlines contains an airline
    }
}