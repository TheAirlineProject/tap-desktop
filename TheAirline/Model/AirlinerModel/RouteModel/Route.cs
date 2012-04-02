using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using TheAirline.Model.AirportModel;
using TheAirline.Model.GeneralModel;
using TheAirline.Model.GeneralModel.StatisticsModel;

namespace TheAirline.Model.AirlinerModel.RouteModel
{
    //the class for a route
    public class Route
    {
        public string Id { get; set; }
        public Airport Destination1 { get; set; }
        public Airport Destination2 { get; set; }
        public RouteAirliner Airliner { get; set; }
        public List<RouteAirlinerClass> Classes { get; set; }
        public RouteTimeTable TimeTable { get; set; }
        public List<Invoice> Invoices { get; set; }
        public RouteStatistics Statistics { get; set; }
        public string FlightCodes { get { return getFlightCodes();} set { ;} }
        public double Balance { get { return getBalance(); } set { ;} }
        public double FillingDegree { get { return getFillingDegree(); } set { ;} }
        public double IncomePerPassenger { get { return getIncomePerPassenger(); } set { ;} }
        public DateTime LastUpdated { get; set; }
        public Route(string id, Airport destination1, Airport destination2, double farePrice,string flightCode1, string flightCode2)
        {
            this.Id = id;
            this.Destination1 = destination1;
            this.Destination2 = destination2;
            //this.FarePrice = farePrice;
            this.TimeTable = new RouteTimeTable(this);
            this.Invoices = new List<Invoice>();
            this.Statistics = new RouteStatistics();

            createTimetable(flightCode1,flightCode2);

            this.Classes = new List<RouteAirlinerClass>();

            foreach (AirlinerClass.ClassType type in Enum.GetValues(typeof(AirlinerClass.ClassType)))
            {
                RouteAirlinerClass cl = new RouteAirlinerClass(type, RouteAirlinerClass.SeatingType.Reserved_Seating, farePrice);
                cl.CabinCrew = 1;

                this.Classes.Add(cl);
            }

        }
        //creates the "dummy" time table
        private void createTimetable(string flightCode1, string flightCode2)
        {
            Random rnd = new Random();

            var query = from a in AirlinerTypes.GetTypes().FindAll((delegate(AirlinerType t) { return t.Produced.From<GameObject.GetInstance().GameTime.Year; }))
                        select a.CruisingSpeed;

            double maxSpeed= query.Max();

            TimeSpan minFlightTime = MathHelpers.GetFlightTime(this.Destination1.Profile.Coordinates, this.Destination2.Profile.Coordinates, maxSpeed).Add(RouteTimeTable.MinTimeBetweenFlights);

            if (minFlightTime.Hours < 12 && minFlightTime.Days<1)
            {

                TimeSpan tFlightTime = minFlightTime.Subtract(new TimeSpan(minFlightTime.Ticks / 2));


                this.TimeTable.addDailyEntries(new RouteEntryDestination(this.Destination2, flightCode2), new TimeSpan(12, 0, 0).Subtract(tFlightTime));
                this.TimeTable.addDailyEntries(new RouteEntryDestination(this.Destination1, flightCode1), new TimeSpan(12, 0, 0).Add(tFlightTime));
            }
            else
            {
                DayOfWeek day = 0;

                int outTime = 15 * rnd.Next(-12,12);
                int homeTime = 15 * rnd.Next(-12,12);
                
                for (int i = 0; i < 3; i++)
                {
                    this.TimeTable.addEntry(new RouteTimeTableEntry(this.TimeTable,day,new TimeSpan(12,0,0).Add(new TimeSpan(0,outTime,0)),new RouteEntryDestination(this.Destination2, flightCode2)));
         
                    day += 2;
                }

                day = (DayOfWeek)1;
                
                for (int i = 0; i < 3; i++)
                {
                    this.TimeTable.addEntry(new RouteTimeTableEntry(this.TimeTable, day, new TimeSpan(12, 0, 0).Add(new TimeSpan(0, homeTime, 0)), new RouteEntryDestination(this.Destination1, flightCode1)));
         
                    day += 2;
                }
                
            }



        }
        //returns if the route contains a specific destination
        public Boolean containsDestination(Airport destination)
        {
            return this.Destination1 == destination || this.Destination2 == destination;

        }
        //adds a route airliner class to the route
        public void addRouteAirlinerClass(RouteAirlinerClass aClass)
        {
            this.Classes.Add(aClass);
        }
        //returns the route airliner class for a specific class type
        public RouteAirlinerClass getRouteAirlinerClass(AirlinerClass.ClassType type)
        {
            RouteAirlinerClass rac = this.Classes.Find(cl => cl.Type == type);
            return this.Classes.Find((delegate(RouteAirlinerClass c) { return c.Type == type; }));
        }
        //returns the total number of cabin crew for the route based on airliner
        public int getTotalCabinCrew()
        {


            int cabinCrew = 0;

            foreach (AirlinerClass aClass in this.Airliner.Airliner.Airliner.Classes)
                if (getRouteAirlinerClass(aClass.Type).CabinCrew > cabinCrew)
                    cabinCrew = getRouteAirlinerClass(aClass.Type).CabinCrew;
         
            return cabinCrew;
        }
        //adds an invoice for a route 
        public void addRouteInvoice(Invoice invoice)
        {
            this.Invoices.Add(invoice);

        }
        //returns the flightcodes for the codes
        private string getFlightCodes()
        {
           
        
            return string.Format("{1}/{0}", this.TimeTable.getRouteEntryDestinations()[0].FlightCode, this.TimeTable.getRouteEntryDestinations()[1].FlightCode);
           
        }
        //returns invoices amount for a specific type for a route
        public double getRouteInvoiceAmount(Invoice.InvoiceType type)
        {
            List<Invoice> tInvoices = this.Invoices;

            if (type != Invoice.InvoiceType.Total)
                tInvoices = this.Invoices.FindAll(delegate(Invoice i) { return i.Type == type; });

            double amount = 0;
            foreach (Invoice invoice in tInvoices)
                amount += invoice.Amount;

            return amount;
        }
        //returns the invoices amount for a specific type for a period
        public double getRouteInvoiceAmount(Invoice.InvoiceType type, DateTime startTime, DateTime endTime)
        {
            List<Invoice> tInvoices;

            if (type != Invoice.InvoiceType.Total)
                tInvoices = this.Invoices.FindAll(i => i.Type == type && i.Date >= startTime && i.Date <= endTime);
            else
                tInvoices = this.Invoices.FindAll(i => i.Date >= startTime && i.Date <= endTime);

            return tInvoices.Sum(i => i.Amount);
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
        //get the degree of filling
        private double getFillingDegree()
        {
            double avgPassengers = Convert.ToDouble(this.Statistics.getTotalValue(StatisticsTypes.GetStatisticsType("Passengers"))) / Convert.ToDouble(this.Statistics.getStatisticsValue(this.Classes[0],StatisticsTypes.GetStatisticsType("Departures")));

            double totalPassengers = Convert.ToDouble(this.Airliner.Airliner.Airliner.getTotalSeatCapacity());

            double fillingDegree = avgPassengers / totalPassengers;

            if (fillingDegree > 1)
            {
                string s = "";
                s += this.Destination1.Profile.Name + " <-> " + this.Destination2.Profile.Name;
            }

            return avgPassengers / totalPassengers;
        }
       
        //gets the income per passenger
        private double getIncomePerPassenger()
        {
            double totalPassengers = Convert.ToDouble(this.Airliner.Airliner.Airliner.getTotalSeatCapacity());

            return getBalance() / totalPassengers;
        }


    }
   
}
