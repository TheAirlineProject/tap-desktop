
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;
using TheAirline.Model.AirlineModel;
using TheAirline.Model.AirportModel;
using TheAirline.Model.GeneralModel;
using TheAirline.Model.GeneralModel.Helpers;
using TheAirline.Model.GeneralModel.InvoicesModel;
using TheAirline.Model.GeneralModel.WeatherModel;

namespace TheAirline.Model.AirlinerModel.RouteModel
{
    [DataContract]
    [KnownType(typeof(CargoRoute))]
    [KnownType(typeof(PassengerRoute))]
    //the class for a route
    public abstract class Route
    {
        public double Balance { get { return getBalance(); } set { ;} }
        public double FillingDegree { get { return getFillingDegree(); } set { ;} }
        public Boolean IsCargoRoute { get { return this.Type == RouteType.Cargo; } set { ;} }
        public Boolean HasStopovers { get { return this.Stopovers.Count > 0; } set { ;} }
       [DataMember]
      
        public string Id { get; set; }
       [DataMember]
       public Airport Destination1 { get; set; }
       [DataMember]
       public Airport Destination2 { get; set; }
        [DataMember]
        
        public List<StopoverRoute> Stopovers { get; set; }

        [DataMember]
        public RouteTimeTable TimeTable { get; set; }

        [DataMember]
        public Invoices Invoices { get; set; }

        [DataMember]
        public RouteStatistics Statistics { get; set; }

        [DataMember]
        public Boolean Banned { get; set; }

        [DataMember]
        public DateTime LastUpdated { get; set; }
        public Boolean HasAirliner { get { return getAirliners().Count > 0; } set { ;} }

        [DataMember]
        public Weather.Season Season { get; set; }

        [DataMember]
        public Airline Airline { get; set; }

        public enum RouteType { Cargo, Mixed, Passenger }
        [DataMember]
        public RouteType Type { get; set; }
        public Route(RouteType type, string id, Airport destination1, Airport destination2)
        {
            this.Type = type;
            this.Id = id;
            this.Destination1 = destination1;
            this.Destination2 = destination2;

            this.TimeTable = new RouteTimeTable(this);
            this.Invoices = new Invoices();
            this.Statistics = new RouteStatistics();
            this.Banned = false;
            this.Stopovers = new List<StopoverRoute>();

            this.Season = Weather.Season.All_Year;
        }
        //adds a stop over to the route
        public void addStopover(StopoverRoute stopover)
        {
            this.Stopovers.Add(stopover);
        }
        //removes a stop over from the route
        public void removeStopover(Airport stopover)
        {
            this.Stopovers.Remove(this.Stopovers.Find(s => s.Stopover == stopover));
        }

        //returns if the route contains a specific destination
        public Boolean containsDestination(Airport destination)
        {
            return this.Destination1 == destination || this.Destination2 == destination;

        }
        //returns the list of destinations
        public List<Airport> getDestinations()
        {
            List<Airport> dests = new List<Airport>();
            dests.Add(this.Destination1);
            dests.Add(this.Destination2);

            return dests;

        }
        //returns all invoices for the route
        public Invoices getInvoices()
        {
            return this.Invoices;
        }
        //adds an invoice for a route 
        public void addRouteInvoice(Invoice invoice)
        {
            this.Invoices.addInvoice(invoice);

        }
        //sets the invoice to the route
        public void setRouteInvoice(Invoice.InvoiceType type, int year, int month, double amount)
        {
            this.Invoices.addInvoice(type, year, month, amount);
        }
        //returns invoices amount for a specific type for a route
        public double getRouteInvoiceAmount(Invoice.InvoiceType type)
        {
            double amount = 0;
            foreach (StopoverRoute stopover in this.Stopovers)
            {
                amount += stopover.Legs.Sum(l => l.getRouteInvoiceAmount(type));
            }

            if (type == Invoice.InvoiceType.Total)
                amount += this.Invoices.getAmount();
            else
                amount += this.Invoices.getAmount(type);

            return amount;

        }
        //returns the invoices amount for a specific type for a period
        public double getRouteInvoiceAmount(Invoice.InvoiceType type, DateTime startTime, DateTime endTime)
        {
            int startYear = startTime.Year;
            int endYear = endTime.Year;

            int startMonth = startTime.Month;
            int endMonth = endTime.Month;

            int totalMonths = (endMonth - startMonth) + 12 * (endYear - startYear);

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
        //returns the list of invoice types for a route
        public List<Invoice.InvoiceType> getRouteInvoiceTypes()
        {
            List<Invoice.InvoiceType> types = new List<Invoice.InvoiceType>();

            types.Add(Invoice.InvoiceType.Tickets);
            types.Add(Invoice.InvoiceType.OnFlight_Income);
            types.Add(Invoice.InvoiceType.Fees);

            types.Add(Invoice.InvoiceType.Maintenances);
            types.Add(Invoice.InvoiceType.Flight_Expenses);
            types.Add(Invoice.InvoiceType.Wages);

            types.Add(Invoice.InvoiceType.Total);

            return types;
        }
        //returns the balance for the route for a period
        public double getBalance(DateTime startTime, DateTime endTime)
        {
            return getRouteInvoiceAmount(Invoice.InvoiceType.Total, startTime, endTime);
        }
        //get the balance for the route 
        private double getBalance()
        {
            return getRouteInvoiceAmount(Invoice.InvoiceType.Total);
        }
        //returns all airliners assigned to the route
        public List<FleetAirliner> getAirliners()
        {
            var entries = new List<RouteTimeTableEntry>(this.TimeTable.Entries);
            List<FleetAirliner> mainEntries = (from e in entries where e.MainEntry != null && e.MainEntry.Airliner != null select e.MainEntry.Airliner).Distinct().ToList();
            List<FleetAirliner> allAirliners = (from e in entries where e.Airliner != null select e.Airliner).Distinct().ToList();
            allAirliners.AddRange(mainEntries);

            return allAirliners;
        }
        //returns the current airliner on the route
        public FleetAirliner getCurrentAirliner()
        {
            return getAirliners().Find(f => f.CurrentFlight != null && f.CurrentFlight.Entry.TimeTable.Route == this);
        }
        //returns the distance for the route
        public double getDistance()
        {
            if (this.HasStopovers)
                return this.Stopovers.SelectMany(s => s.Legs).Max(l => l.getDistance());
            else
                return MathHelpers.GetDistance(this.Destination1, this.Destination2);

        }
        //returns the flight time for the route for a specific airliner type
        public TimeSpan getFlightTime(AirlinerType type)
        {
            if (this.HasStopovers)
            {
                double totalMinutes = this.Stopovers.SelectMany(s => s.Legs).Sum(l => l.getFlightTime(type).TotalMinutes);
                double totalRestTime = FleetAirlinerHelpers.GetMinTimeBetweenFlights(type).TotalMinutes * (this.Stopovers.SelectMany(s => s.Legs).Count() - 1);
                int time = (int)(totalRestTime + totalMinutes);

                return new TimeSpan(0, time, 0);
            }
            else
                return MathHelpers.GetFlightTime(this.Destination1, this.Destination2, type);


        }
        //returns the filling degree for the route
        public abstract double getFillingDegree();
    }
}
