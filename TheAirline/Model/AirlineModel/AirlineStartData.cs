using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using TheAirline.Model.AirlinerModel;
using TheAirline.Model.AirlinerModel.RouteModel;
using TheAirline.Model.GeneralModel;

namespace TheAirline.Model.AirlineModel
{
    
    //the start data for an airline
    public class AirlineStartData
    {
        public Airline Airline { get; set; }
        public List<StartDataRoute> Routes { get; private set; }
        public List<StartDataRoutes> OriginRoutes { get; private set; }
        public List<StartDataAirliners> Airliners { get; private set; }
        public AirlineStartData(Airline airline)
        {
            this.Airline = airline;
            this.Routes = new List<StartDataRoute>();
            this.Airliners = new List<StartDataAirliners>();
            this.OriginRoutes = new List<StartDataRoutes>();
        }
        //adds origin routes to the list
        public void addOriginRoutes(StartDataRoutes route)
        {
            this.OriginRoutes.Add(route);
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
    //the routes for the start data
    public class StartDataRoutes
    {
        public List<Country> Countries { get; set; }
        public string Origin { get; set; }
        public int Destinations { get; set; }
        public GeneralHelpers.Size MinimumSize { get; set; }
        public Route.RouteType RouteType { get; set; }
        public StartDataRoutes(string origin, int destinations, GeneralHelpers.Size minimumsize,Route.RouteType routetype)
        {
            this.Countries = new List<Country>();
            this.Origin = origin;
            this.Destinations = destinations;
            this.MinimumSize = minimumsize;
            this.RouteType = routetype;
        }
        //adds a country to the routes
        public void addCountry(Country country)
        {
            this.Countries.Add(country);
        }
    }
    //the route for the start data
    public class StartDataRoute
    {
        public int Opened { get; set; }
        public int Closed { get; set; }
        public string Destination1 { get; set; }
        public string Destination2 { get; set; }
        public AirlinerType Type { get; set; }
        public Route.RouteType RouteType { get; set; }
        public StartDataRoute(string destination1, string destination2, int opened, int closed, Route.RouteType routetype)
        {
            this.Opened = opened;
            this.Closed = closed;
            this.Destination1 = destination1;
            this.Destination2 = destination2;
            this.RouteType = routetype;
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
