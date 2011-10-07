using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using TheAirlineV2.Model.AirportModel;
using TheAirlineV2.Model.AirlinerModel;
using TheAirlineV2.Model.AirlinerModel.RouteModel;
using TheAirlineV2.Model.GeneralModel;
using TheAirlineV2.Model.GeneralModel.StatisticsModel;

namespace TheAirlineV2.Model.AirlineModel
{
    //the class for an airline
    public class Airline
    {
        public enum AirlineValue { Very_low, Low, Normal, High, Very_high }
        public int Reputation { get; set; } //0-100 with 0-9 as very_low, 10-30 as low, 31-70 as normal, 71-90 as high,91-100 as very_high 
        public List<Airport> Airports { get; set; }
        public List<FleetAirliner> Fleet { get; set; }
        public AirlineProfile Profile { get; set; }
        public List<Route> Routes { get; set; }
        public List<AirlineFacility> Facilities { get; set; }
        public GeneralStatistics Statistics { get; set; }
        public double Money { get; set; }
        public Boolean IsHuman { get { return this == GameObject.GetInstance().HumanAirline; } set { ;} }
        private List<Invoice> Invoices;
        public AirlineFees Fees { get; set; }
        public List<Loan> Loans { get; set; }
        private List<string> FlightCodes;
        public List<FleetAirliner> DeliveredFleet { get { return getDeliveredFleet(); } set { ;} }
        public Airline(AirlineProfile profile)
        {
            this.Airports = new List<Airport>();
            this.Fleet = new List<FleetAirliner>();
            this.Routes = new List<Route>();
            this.Statistics = new GeneralStatistics();
            this.Facilities = new List<AirlineFacility>();
            this.Invoices = new List<Invoice>();
            this.Profile = profile;
            this.Money = GameObject.GetInstance().StartMoney;
            this.Fees = new AirlineFees();
            this.Loans = new List<Loan>();
            this.Reputation = 50;

            this.FlightCodes = new List<string>();

            for (int i = 1; i < 10000; i++)
                this.FlightCodes.Add(string.Format("{0}{1:0000}",this.Profile.IATACode, i));
  
        }
        //adds a route to the airline
        public void addRoute(Route route)
        {
            this.Routes.Add(route);

            foreach (RouteEntryDestination dest in route.TimeTable.getRouteEntryDestinations())
                this.FlightCodes.Remove(dest.FlightCode);

         
        }
        //removes a route from the airline
        public void removeRoute(Route route)
        {
            this.Routes.Remove(route);

            foreach (RouteEntryDestination dest in route.TimeTable.getRouteEntryDestinations())
                this.FlightCodes.Add(dest.FlightCode);

            this.FlightCodes.Sort(delegate(string s1, string s2) { return s1.CompareTo(s2); });
  
       
        }
        //adds an airliner to the airlines fleet
        public void addAirliner(FleetAirliner.PurchasedType type, Airliner airliner, string name, Airport homeBase)
        {
            addAirliner(new FleetAirliner(type,this,airliner, name, homeBase));
        }
        //adds a fleet airliner to the airlines fleet
        public void addAirliner(FleetAirliner airliner)
        {
            this.Fleet.Add(airliner);
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
            this.Airports.Add(airport);
        }
        //removes an airport from the airline
        public void removeAirport(Airport airport)
        {
            this.Airports.Remove(airport);
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
        public List<Invoice> getInvoices()
        {
            return this.Invoices;
        }
        //returns all the invoices in a specific period
        public List<Invoice> getInvoices(DateTime start, DateTime end)
        {
            return this.Invoices.FindAll(delegate(Invoice i) { return i.Date >= start && i.Date <= end; });

        }
        //returns the amount of all the invoices in a specific period of a specific type
        public double getInvoicesAmount(DateTime start, DateTime end, Invoice.InvoiceType type)
        {
            List<Invoice> tInvoices = new List<Invoice>();
            
            if (type == Invoice.InvoiceType.Total)
                tInvoices = this.Invoices.FindAll(delegate(Invoice i) { return i.Date >= start && i.Date <= end; });
            else
                tInvoices = this.Invoices.FindAll(delegate(Invoice i) { return i.Date >= start && i.Date <= end && i.Type == type; });

            double amount = 0;
            foreach (Invoice invoice in tInvoices)
                amount += invoice.Amount;
            //return Math.Abs(amount);
            return amount;
        }
        public double getInvoicesAmountYear(int year, Invoice.InvoiceType type)
        {
            List<Invoice> tInvoices = new List<Invoice>();

            if (type == Invoice.InvoiceType.Total)
                tInvoices = this.Invoices.FindAll(delegate(Invoice i) { return i.Date.Year == year; });
            else
                tInvoices = this.Invoices.FindAll(delegate(Invoice i) { return i.Date.Year == year && i.Type == type; });

            double amount = 0;
            foreach (Invoice invoice in tInvoices)
                amount += invoice.Amount;
            //return Math.Abs(amount);
            return amount;
        }
        public double getInvoicesAmountMonth(int month, Invoice.InvoiceType type)
        {
            List<Invoice> tInvoices = new List<Invoice>();

            if (type == Invoice.InvoiceType.Total)
                tInvoices = this.Invoices.FindAll(delegate(Invoice i) { return i.Date.Month == month; });
            else
                tInvoices = this.Invoices.FindAll(delegate(Invoice i) { return i.Date.Month == month && i.Type == type; });

            double amount = 0;
            foreach (Invoice invoice in tInvoices)
                amount += invoice.Amount;
            //return Math.Abs(amount);
            return amount;
        }
        //adds an invoice for the airline - both incoming and expends
        public void addInvoice(Invoice invoice)
        {
            
            this.Invoices.Add(invoice);
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
        //returns the "value" of the airline
        public AirlineValue getValue()
        {
            double value = 0;
            value += this.Money;
            foreach (FleetAirliner airliner in this.Fleet)
            {
                value += airliner.Airliner.getPrice();
            }
            foreach (AirlineFacility facility in this.Facilities)
            {
                value += facility.Price; 
            }
            foreach (Airport airport in this.Airports)
            {
                foreach (AirportFacility facility in airport.getAirportFacilities(this))
                    value += facility.Price;
                

            }
           
            double startMoney = GameObject.GetInstance().StartMoney;
            
            if (value < startMoney / 4)
                return AirlineValue.Very_low;
            if (value >= startMoney / 4 && value < startMoney / 2)
                return AirlineValue.Low;
            if (value >= startMoney / 2 && value < startMoney * 2)
                return AirlineValue.Normal;
            if (value >= startMoney * 2 && value < startMoney * 4)
                return AirlineValue.High;
            if (value >= startMoney * 4)
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
        //returns the next flight code for the airline
        public string getNextFlightCode()
        {
            return this.FlightCodes[0];
        }
        //returns the list of flight codes for the airline
        public List<string> getFlightCodes()
        {
            return this.FlightCodes;
        }
        //returns all airliners which are delivered
        private List<FleetAirliner> getDeliveredFleet()
        {
            return this.Fleet.FindAll((delegate(FleetAirliner a) { return a.Airliner.BuiltDate <= GameObject.GetInstance().GameTime; }));
        }
     
       
    }
    //the collection of airlines
    public class Airlines
    {
        private static Dictionary<string, Airline> airlines = new Dictionary<string, Airline>();
        //clears the list
        public static void Clear()
        {
            airlines = new Dictionary<string, Airline>();
        }
        //adds an airline to the collection
        public static void AddAirline(Airline airline)
        {
            airlines.Add(airline.Profile.IATACode, airline);
        }
        //returns an airline
        public static Airline GetAirline(string iata)
        {
            return airlines[iata];
        }
        //returns the list of airlines
        public static List<Airline> GetAirlines()
        {
            return airlines.Values.ToList();
        }
        //removes an airline from the list
        public static void RemoveAirline(Airline airline)
        {
            airlines.Remove(airline.Profile.IATACode);
        }
    }
   
    //the class for fees for an airline
    public class AirlineFees
    {
        private Dictionary<FeeType, double> Fees;
        public AirlineFees()
        {
            this.Fees = new Dictionary<FeeType, double>();

            foreach (FeeType type in FeeTypes.GetTypes())
                this.Fees.Add(type, type.DefaultValue);

        }
        //returns the value of a specific fee type
        public double getValue(FeeType type)
        {
            return this.Fees[type];
        }
        //sets the value of a specific fee type
        public void setValue(FeeType type, double value)
        {
            this.Fees[type] = value;
        }
    }
    //the list of fee types
    public class FeeTypes
    {
        private static Dictionary<string, FeeType> types = new Dictionary<string, FeeType>();
        //adds a type to the list
        public static void AddType(FeeType type)
        {
            types.Add(type.Name, type);
        }
        //clears the list
        public static void Clear()
        {
            types = new Dictionary<string, FeeType>();
        }
        //returns the list of fees of a specific type
        public static List<FeeType> GetTypes(FeeType.eFeeType type)
        {
            return GetTypes().FindAll(delegate(FeeType t) { return t.Type == type; });
        }
        //returns the list of fee types
        public static List<FeeType> GetTypes()
        {
            return types.Values.ToList();
        }
        //returns a fee type
        public static FeeType GetType(string name)
        {
            return types[name];
        }

    }
    //the class for a fee type
    public class FeeType
    {
        public enum eFeeType { Fee, Wage, FoodDrinks }
        public eFeeType Type { get; set; }
        public double MinValue { get; set; }
        public double MaxValue { get; set; }
        public string Name { get; set; }
        public double DefaultValue { get; set; }
        public int Percentage { get; set; }
        public FeeType(eFeeType type, string name, double defaultValue, double minValue, double maxValue, int percentage)
        {
            this.Type = type;
            this.MinValue = minValue;
            this.MaxValue = maxValue;
            this.DefaultValue = defaultValue;
            this.Name = name;
            this.Percentage = percentage;
        }
    }
    
   
}
