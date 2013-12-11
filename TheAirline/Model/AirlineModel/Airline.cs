using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using TheAirline.Model.AirportModel;
using TheAirline.Model.AirlinerModel;
using TheAirline.Model.AirlinerModel.RouteModel;
using TheAirline.Model.GeneralModel;
using TheAirline.Model.GeneralModel.StatisticsModel;
using TheAirline.Model.GeneralModel.InvoicesModel;
using TheAirline.Model.AirlineModel.SubsidiaryModel;
using TheAirline.Model.PilotModel;
using System.Runtime.Serialization;


namespace TheAirline.Model.AirlineModel
{
    [DataContract]
    [KnownType(typeof(SubsidiaryAirline))]
    [KnownType(typeof(GeneralStatistics))]
    //the class for an airline
    public class Airline
    {
        public enum AirlineLicense { Domestic, Regional, Short_Haul, Long_Haul }
        public enum AirlineValue { Very_low, Low, Normal, High, Very_high }
        public enum AirlineMentality { Safe,Moderate, Aggressive}
        public enum AirlineFocus { Global, Regional,Domestic, Local }

        [DataMember]
        public Route.RouteType AirlineRouteFocus { get; set; }

        [DataMember]
        public AirlineLicense License { get; set; }

        [DataMember]
        public AirlineFocus MarketFocus { get; set; }

        [DataMember]
        public AirlineMentality Mentality { get; set; }

        [DataMember]
        public int Reputation { get; set; } //0-100 with 0-9 as very_low, 10-30 as low, 31-70 as normal, 71-90 as high,91-100 as very_high 
        [DataMember]
        public List<Airport> Airports { get; set; }
        [DataMember]
        public List<FleetAirliner> Fleet { get; set; }
        [DataMember]
        public List<SubsidiaryAirline> Subsidiaries { get; set; }
          [DataMember]
      
        public AirlineProfile Profile { get; set; }

          [DataMember]
          private List<Route> _Routes;
        public List<Route> Routes { get { return getRoutes(); } set { this._Routes = value; } }

        [DataMember]
        public List<AirlineFacility> Facilities { get; set; }
        [DataMember]
        private Dictionary<AdvertisementType.AirlineAdvertisementType, AdvertisementType> Advertisements;
        [DataMember]
        public GeneralStatistics Statistics { get; set; }
        [DataMember]
        public double Money { get; set; }
        [DataMember]
        public double StartMoney { get; set; }
        public Boolean IsHuman { get { return isHuman(); } set { ;} }
        public Boolean IsSubsidiary { get { return isSubsidiaryAirline(); } set { ;} }
        [DataMember]
        private Invoices Invoices;
        [DataMember]
        public AirlineFees Fees { get; set; }
        [DataMember]
        public List<Loan> Loans { get; set; }
        [DataMember]
        private List<string> FlightCodes;
        public List<FleetAirliner> DeliveredFleet { get { return getDeliveredFleet(); } set { ;} }
        [DataMember]
        public List<Alliance> Alliances { get; set; }
        [DataMember]
        public ManufacturerContract Contract { get; set; }
        [DataMember]
        public List<FutureSubsidiaryAirline> FutureAirlines { get; set; }
        [DataMember]
        public List<AirlinePolicy> Policies { get; set; }
        [DataMember]
        public List<Pilot> Pilots { get; set; }
        [DataMember]
        public AirlineBudget Budget { get; set; }
        [DataMember]
        public List<FlightSchool> FlightSchools { get; set; }
        [DataMember]
        public List<AirlineInsurance> InsurancePolicies { get; set; }
        [DataMember]
        public Int64 AvgFleetValue { get; set; }
        [DataMember]
        public Int64 FleetValue { get; set; }
        [DataMember]
        public List<RandomEvent> EventLog { get; set; }
        [DataMember]
        public IDictionary<DateTime, AirlineBudget> BudgetHistory { get; set; }
        [DataMember]
        public IDictionary<DateTime, AirlineBudget> TestBudget { get; set; }
        [DataMember]
        public AirlineScores Scores { get; set; }
        [DataMember]
        public AirlineRatings Ratings { get; set; }
        [DataMember]
        public int OverallScore { get; set; }
        [DataMember]
        public Dictionary<DateTime, int> GameScores { get; set; }
        [DataMember]
        public int CountedScores { get; set; }
        [DataMember]
        public List<RandomEvent> EventList { get; set; }
        [DataMember]
        public List<InsuranceClaim> InsuranceClaims { get; set; }
        public Airline(AirlineProfile profile, AirlineMentality mentality, AirlineFocus marketFocus, AirlineLicense license, Route.RouteType routeFocus)
        {
            this.Scores = new AirlineScores();
            this.Airports = new List<Airport>();
            this.Fleet = new List<FleetAirliner>();
            this._Routes = new List<Route>();
            this.FutureAirlines = new List<FutureSubsidiaryAirline>();
            this.Subsidiaries = new List<SubsidiaryAirline>();
            this.Advertisements = new Dictionary<AdvertisementType.AirlineAdvertisementType, AdvertisementType>();
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
            this.FlightCodes = new List<string>();
            this.Policies = new List<AirlinePolicy>();
            this.EventLog = new List<RandomEvent>();
            this.Ratings = new AirlineRatings();
            this.OverallScore = this.CountedScores = 0;
            this.GameScores = new Dictionary<DateTime, int>();
            this.InsuranceClaims = new List<InsuranceClaim>();
            this.InsurancePolicies = new List<AirlineInsurance>();
            for (int i = 1; i < 10000; i++)
                this.FlightCodes.Add(string.Format("{0}{1:0000}",this.Profile.IATACode, i));

            createStandardAdvertisement();

            this.Pilots = new List<Pilot>();
            this.FlightSchools = new List<FlightSchool>();
            this.Budget = new AirlineBudget();

         }

        //stores a budget
        public void storeBudget(AirlineBudget budget)
        {
            this.BudgetHistory.Add(GameObject.GetInstance().GameTime, budget);
        }
        //adds a pilot to the airline
        public void addPilot(Pilot pilot)
        {
            if (pilot == null)
                throw new NullReferenceException("Pilot is null at Airline.cs/addPilot");

            lock (this.Pilots)
            {
                this.Pilots.Add(pilot);
                pilot.Airline = this;
            }
        }
        //removes a pilot from the airline
        public void removePilot(Pilot pilot)
        {
            this.Pilots.Remove(pilot);
            pilot.Airline = null;

            if (pilot.Airliner != null)
                pilot.Airliner.removePilot(pilot);
        }
        //adds a flight school to the airline
        public void addFlightSchool(FlightSchool school)
        {
            this.FlightSchools.Add(school);
        }
        public void removeFlightSchool(FlightSchool school)
        {
            this.FlightSchools.Remove(school);

            foreach (Instructor instructor in school.Instructors)
                instructor.FlightSchool = null;
        }
        //adds a route to the airline
        public void addRoute(Route route)
        {

            var routes = new List<Route>(this.Routes);

           
            lock (this._Routes)
            {
                this._Routes.Add(route);
                route.Airline = this;

                foreach (string flightCode in route.TimeTable.Entries.Select(e => e.Destination.FlightCode).Distinct())
                    this.FlightCodes.Remove(flightCode);
            }
         
        }
        //removes a route from the airline
        public void removeRoute(Route route)
        {
            lock (this._Routes)
            {
                this._Routes.Remove(route);

                foreach (string flightCode in route.TimeTable.Entries.Select(e => e.Destination.FlightCode).Distinct())
                    this.FlightCodes.Add(flightCode);
     
            }
       
        }
        //get routes for the airline
        private List<Route> getRoutes()
        {

            List<Route> routes;
            lock (this._Routes)
            {
                routes = new List<Route>(this._Routes);
            }

            return routes;
        }
       
        //adds an alliance to the airline
        public void addAlliance(Alliance alliance)
        {
            this.Alliances.Add(alliance);
        }
        //removes an alliance
        public void removeAlliance(Alliance alliance)
        {
            lock (this.Alliances)
            {
                if (this.Alliances.Contains(alliance))
                    this.Alliances.Remove(alliance);
            }
        }
        //adds an airliner to the airlines fleet
        public void addAirliner(FleetAirliner.PurchasedType type, Airliner airliner,  Airport homeBase)
        {
            addAirliner(new FleetAirliner(type,GameObject.GetInstance().GameTime, this,airliner, homeBase));
        }
        //adds a fleet airliner to the airlines fleet
        public void addAirliner(FleetAirliner airliner)
        {
            lock (this.Fleet)
            {
                this.Fleet.Add(airliner);
            }
        }
        //remove a fleet airliner from the airlines fleet
        public void removeAirliner(FleetAirliner airliner)
        {
            this.Fleet.Remove(airliner);

            airliner.Airliner.Airline = null; 
        }

        //adds an airport to the airline
        public void addAirport(Airport airport)
        {
            lock (this.Airports)
            {
                if (airport != null)
                    this.Airports.Add(airport);
            }
        }
        //removes an airport from the airline
        public void removeAirport(Airport airport)
        {
            this.Airports.Remove(airport);
        }
        //returns all hubs airports for the airline
        public List<Airport> getHubs()
        {
            List<Airport> hubs = new List<Airport>();
            lock (this.Airports)
            {
                hubs = (from a in this.Airports where a.getHubs().Exists(h=>h.Airline == this) select a).ToList();
            }
            return hubs;
        }
        //adds a facility to the airline
        public void addFacility(AirlineFacility facility)
        {
            this.Facilities.Add(facility);
        }
        //removes a facility from the airline
        public void removeFacility(AirlineFacility facility)
        {
            this.Facilities.Remove(facility);
        }
        //returns all the invoices
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

            int totalMonths = (endMonth - startMonth) + 12 * (endYear - startYear) +1;

            double totalAmount = 0;

            DateTime date = new DateTime(startYear, startMonth, 1);

            for (int i = 0; i < totalMonths; i++)
            {
                if (type == Invoice.InvoiceType.Total)
                    totalAmount += this.Invoices.getAmount(date.Year, date.Month);
                else
                    totalAmount += this.Invoices.getAmount(type, date.Year, date.Month);

                date = date.AddMonths(1);
            }

            return totalAmount;
        }
        public double getInvoicesAmountYear(int year, Invoice.InvoiceType type)
        {
            if (type == Invoice.InvoiceType.Total)
                return this.Invoices.getYearlyAmount(year);
            else
                return this.Invoices.getYearlyAmount(type, year);
          
        }
        public double getInvoicesAmountMonth(int year,int month, Invoice.InvoiceType type)
        {
            if (type == Invoice.InvoiceType.Total)
                return this.Invoices.getAmount(year, month);
            else
                return this.Invoices.getAmount(type, year, month);
           
        }
        // chs, 2011-13-10 added function to add an invoice without have to pay for it. Used for loading of saved game
        //sets an invoice to the airline - no payment is made
        public void setInvoice(Invoice invoice)
        {
            this.Invoices.addInvoice(invoice);
        }
        public void setInvoice(Invoice.InvoiceType type, int year, int month, double amount)
        {
            this.Invoices.addInvoice(type, year, month, amount);
        }
        //adds an invoice for the airline - both incoming and expends - if updateMoney == true the money is updated as well
        public void addInvoice(Invoice invoice, Boolean updateMoney = true)
        {

            this.Invoices.addInvoice(invoice);

            if (updateMoney)
                this.Money += invoice.Amount;


        }
   
        //returns the reputation for the airline
        public AirlineValue getReputation()
        {
            //0-100 with 0-10 as very_low, 11-30 as low, 31-70 as normal, 71-90 as high,91-100 as very_high 
            if (this.Reputation < 11)
                return AirlineValue.Very_low;
            if (this.Reputation > 10 && this.Reputation < 31)
                return AirlineValue.Low;
            if (this.Reputation > 30 && this.Reputation < 71)
                return AirlineValue.Normal;
            if (this.Reputation > 70 && this.Reputation < 91)
                return AirlineValue.High;
            if (this.Reputation > 90)
                return AirlineValue.Very_high;
            return AirlineValue.Normal;
        }
     
       
        //returns the value of the airline in "money"
        public long getValue()
        {
            double value = 0;
            value += this.Money;

            var fleet = new List<FleetAirliner>(this.Fleet.FindAll(f=>f.Purchased != FleetAirliner.PurchasedType.Leased));

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
                    value += facility.Facility.Price;
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
                value += subAirline.getValue();

            value = value / 1000000;

            if (double.IsNaN(value))
                return 0;
            else
                return Convert.ToInt64(value);
           
        }
       
        //returns the "value" of the airline
        public AirlineValue getAirlineValue()
        {
            double value =GeneralHelpers.GetInflationPrice(getValue() * 1000000);
            double startMoney = GeneralHelpers.GetInflationPrice(this.StartMoney);
            
            if (value <= startMoney)
                return AirlineValue.Very_low;
            if (value > startMoney && value < startMoney * 3)
                return AirlineValue.Low;
            if (value >= startMoney * 3 && value < startMoney * 9)
                return AirlineValue.Normal;
            if (value >= startMoney * 9 && value < startMoney * 18)
                return AirlineValue.High;
            if (value >=startMoney * 18)
                return AirlineValue.Very_high;

            return AirlineValue.Normal;
        }
        //adds a loan to the airline
        public void addLoan(Loan loan)
        {
            this.Loans.Add(loan);
        }
        //removes a loan 
        public void removeLoan(Loan loan)
        {
            this.Loans.Remove(loan);
        }
        // chs, 2011-11-17 changed so the airline gets a "new" flight code each time
        //returns the next flight code for the airline
        public string getNextFlightCode(int n)
        {
            return getFlightCodes()[n];
        }
        //returns the list of flight codes for the airline
        public List<string> getFlightCodes()
        {
            /*
            List<string> codes = new List<string>(this.FlightCodes);

            var routes = new List<Route>(this.Routes);

            var entries = new List<RouteTimeTableEntry>(routes.SelectMany(r => r.TimeTable.Entries));
            
            foreach (RouteTimeTableEntry entry in entries)
            {
                if (codes.Contains(entry.Destination.FlightCode))
                    codes.Remove(entry.Destination.FlightCode);
                   
            }

            codes.Sort(delegate(string s1, string s2) { return s1.CompareTo(s2); });
            


            return codes;
             *  * */
            return this.FlightCodes;
        }
        //adds an insurance to the airline
        public void addInsurance(AirlineInsurance insurance)
        {
            this.InsurancePolicies.Add(insurance);
        }
        //removes a policy
        public void removeInsurance(AirlineInsurance insurance)
        {
            this.InsurancePolicies.Remove(insurance);
        }
        //returns all airliners which are delivered
        private List<FleetAirliner> getDeliveredFleet()
        {
            return this.Fleet.FindAll((delegate(FleetAirliner a) { return a.Airliner.BuiltDate <= GameObject.GetInstance().GameTime; }));
        }
        // chs, 2011-14-10 added functions for airline advertisement
        //sets an Advertisement to the airline
        public void setAirlineAdvertisement(AdvertisementType type)
        {
            if (!this.Advertisements.ContainsKey(type.Type))
                this.Advertisements.Add(type.Type, type);
            else
                this.Advertisements[type.Type] = type;
        }
        //returns all advertisements for the airline
        public List<AdvertisementType> getAirlineAdvertisements()
        {
            return this.Advertisements.Values.ToList();
        }
        //returns the advertisement for the airline for a specific type
        public AdvertisementType getAirlineAdvertisement(AdvertisementType.AirlineAdvertisementType type)
        {
            return this.Advertisements[type];
        }
        //creates the standard Advertisement for the airline
        private void createStandardAdvertisement()
        {
            foreach (AdvertisementType.AirlineAdvertisementType type in Enum.GetValues(typeof(AdvertisementType.AirlineAdvertisementType)))
            {
                setAirlineAdvertisement(AdvertisementTypes.GetBasicType(type));
            }
        
        }
        // chs, 2011-18-10 added to handle the different statistics for the airline
        /*! returns the total profit for the airline
         */
        public double getProfit()
        {
            return this.Money - this.StartMoney;
        }
        /*! returns the fleet size
         */
        public double getFleetSize()
        {
            return this.Fleet.Count;
        }
        /*! returns the average age for the fleet
         */
        public double getAverageFleetAge()
        {
            if (this.Fleet.Count > 0)
                return this.Fleet.Average(f => f.Airliner.Age);
            else
                return 0;
        }

        //returns total current value of fleet
        public long getFleetValue()
        {
            long fleetValue = 0;
            foreach (FleetAirliner airliner in this.Fleet)
            {
                fleetValue += getValue();
            }
            return fleetValue;
        }

        //returns the average value of an airliner in the fleet
        public long getAvgFleetValue()
        {
            return getFleetValue() / this.Fleet.Count();
        }

        //returns if the airline is a subsidiary airline
        public virtual Boolean isSubsidiaryAirline()
        {
            return false;
        }
        //returns if it is the human airline
        public virtual Boolean isHuman()
        {
            return this == GameObject.GetInstance().HumanAirline || this == GameObject.GetInstance().MainAirline;
        }
        //adds a subsidiary airline to the airline
        public void addSubsidiaryAirline(SubsidiaryAirline subsidiary)
        {
            this.Subsidiaries.Add(subsidiary);
        }
        //removes a subsidary airline from the airline 
        public void removeSubsidiaryAirline(SubsidiaryAirline subsidiary)
        {
            this.Subsidiaries.Remove(subsidiary);
        }
        //adds a policy to the airline
        public void addAirlinePolicy(AirlinePolicy policy)
        {
            this.Policies.Add(policy);
        }
        //sets the policy for the airline
        public void setAirlinePolicy(string name, object value)
        {
            this.Policies.Find(p => p.Name == name).PolicyValue = value;
        }
        //returns a policy for the airline
        public AirlinePolicy getAirlinePolicy(string name)
        {
            return this.Policies.Find(p => p.Name == name);
        }
        //returns the number of employees
        public int getNumberOfEmployees()
        {
            int instructors = this.FlightSchools.Sum(f => f.NumberOfInstructors);
          
            int cockpitCrew = this.Pilots.Count;
            int cabinCrew = this.Routes.Where(r => r.Type == Route.RouteType.Passenger).Sum(r => ((PassengerRoute)r).getTotalCabinCrew());

            int serviceCrew = this.Airports.SelectMany(a => a.getCurrentAirportFacilities(this)).Where(a => a.EmployeeType == AirportFacility.EmployeeTypes.Support).Sum(a => a.NumberOfEmployees);
            int maintenanceCrew = this.Airports.SelectMany(a => a.getCurrentAirportFacilities(this)).Where(a => a.EmployeeType == AirportFacility.EmployeeTypes.Maintenance).Sum(a => a.NumberOfEmployees);

            return cockpitCrew + cabinCrew + serviceCrew + maintenanceCrew + instructors;
        }
        
     
       
    }
    //the list of airlines
    public class Airlines
    {
        private static List<Airline> airlines = new List<Airline>();
        //clears the list
        public static void Clear()
        {
            airlines = new List<Airline>();
        }
        //adds an airline to the collection
        public static void AddAirline(Airline airline)
        {
            airlines.Add(airline);
        }
        //returns an airline
        public static Airline GetAirline(string iata)
        {
            return airlines.Find(a => a.Profile.IATACode == iata);
        }
        //returns all airlines
        public static List<Airline> GetAllAirlines()
        {
            List<Airline> tAirlines;
            lock (airlines)
            {
                tAirlines = new List<Airline>(airlines);
            }

            return tAirlines;
        }
        //returns the number of airlines
        public static int GetNumberOfAirlines()
        {
            return airlines.Count;
        }
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
        //returns all human airlines
        public static List<Airline> GetHumanAirlines()
        {
            return airlines.FindAll(a => a.IsHuman);
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
        //returns if the list of airlines contains an airline
        public static Boolean ContainsAirline(Airline airline)
        {
            return airlines.Contains(airline);
        }
           
      
    }    
   
}
