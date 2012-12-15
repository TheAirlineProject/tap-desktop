using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace TheAirline.Model.AirlineModel
{
    //the start data for an airline
    public class AirlineStartData
    {
        public Airline Airline { get; set; }
        public List<StartDataRoute> Routes { get; private set; }
        public List<StartDataAirliners> Airliners { get; private set; }
        public AirlineStartData(Airline airline)
        {
            this.Airline = airline;
            this.Routes = new List<StartDataRoute>();
            this.Airliners = new List<StartDataAirliners>();
        }
        //adds a route to the list
        public void addRoute(StartDataRoute route)
        {
            this.Routes.Add(route);
        }
        //adds an airliners to the list
        public void addAirliners(StartDataAirliners airliners)
        {
            this.Airliners.Add(airliners);
        }
    }
    //the airliners for the start data
    public class StartDataAirliners
    {
        public int AirlinersEarly { get; set; }
        public int AirlinersLate { get; set; }
        public string Type { get; set; }
        public StartDataAirliners(string type, int airlinersEarly, int airlinersLate)
        {
            this.Type = type;
            this.AirlinersEarly = airlinersEarly;
            this.AirlinersLate = airlinersLate;
        }
    }
    //the route for the start data
    public class StartDataRoute
    {
        public int Opened { get; set; }
        public int Closed { get; set; }
        public string Destination1 { get; set; }
        public string Destination2 { get; set; }
        public StartDataRoute(string destination1, string destination2, int opened, int closed)
        {
            this.Opened = opened;
            this.Closed = closed;
            this.Destination1 = destination1;
            this.Destination2 = destination2;
        }
    }
    //the list of start data
    public class AirlineStartDatas
    {
        private static List<AirlineStartData> startData = new List<AirlineStartData>();
        //adds start data to the list
        public static void AddStartData(AirlineStartData data)
        {
            startData.Add(data);
        }
        //returns the start data for an airline
        public static AirlineStartData GetAirlineStartData(Airline airline)
        {
            return startData.Find(s => s.Airline == airline);
        }
    }
}
