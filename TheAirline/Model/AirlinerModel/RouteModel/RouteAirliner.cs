using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using TheAirline.Model.GeneralModel;
using TheAirline.Model.AirportModel;

namespace TheAirline.Model.AirlinerModel.RouteModel
{
    //the class for an airliner with a route
    public class RouteAirliner
    {
        public Route Route { get; set; }
        public FleetAirliner Airliner { get; set; }
        public enum AirlinerStatus { Stopped, On_route, On_service, Resting, To_homebase, To_route_start }
        public AirlinerStatus Status { get; set; }
        public Coordinates CurrentPosition { get; set; }
       // public Airport Destination { get; set; }
        public Flight CurrentFlight { get; set; }
        public RouteAirliner(FleetAirliner airliner, Route route)
        {
            this.Route = route;
            this.Airliner = airliner;
            this.Route.Airliner = this;
            airliner.RouteAirliner = this;

            this.Status = AirlinerStatus.Stopped;

            this.CurrentPosition = new Coordinates(this.Airliner.Homebase.Profile.Coordinates.Latitude,this.Airliner.Homebase.Profile.Coordinates.Longitude);

        }
        //returns the next destination
        public Airport getNextDestination()
        {
            return this.CurrentFlight.Entry.Destination.Airport == this.Route.Destination1 ? this.Route.Destination2 : this.Route.Destination1;
        }
        //returns the departure location
        public Airport getDepartureAirport()
        {
            return getNextDestination();

        }
       
    }
    //destination class for a routeTimeTableEntry
    public class RouteEntryDestination : IComparable<RouteEntryDestination>
    {
        public Airport Airport { get; set; }
        public string FlightCode { get; set; }
        public RouteEntryDestination(Airport airport, string flightCode)
        {
            this.Airport = airport;
            this.FlightCode = flightCode;


        }

        public int CompareTo(RouteEntryDestination entry)
        {
            int compare = entry.FlightCode.CompareTo(this.FlightCode);
            if (compare == 0)
                return entry.Airport.Profile.IATACode.CompareTo(this.Airport.Profile.IATACode);
            return compare;
        }
    }
    //the class for an actually flight 
    public class Flight
    {
        public Boolean Delayed { get; set; }
        public RouteTimeTableEntry Entry { get; set; }
        public List<FlightAirlinerClass> Classes { get; set; }
        //public int Passengers { get; set; }
       // public RouteAirliner Airliner { get; set; }
        public Flight(RouteTimeTableEntry entry)
        {
           
            this.Entry = entry;
            this.Delayed = false;
            this.Classes = new List<FlightAirlinerClass>();
         //   this.Airliner = airliner;
           
       
        }
        //returns the expected time of landing
        public DateTime getExpectedLandingTime()
        {
            RouteAirliner airliner = this.Entry.TimeTable.Route.Airliner;
            double distance = MathHelpers.GetDistance(airliner.CurrentPosition, airliner.CurrentFlight.Entry.Destination.Airport.Profile.Coordinates);
            TimeSpan flightTime = MathHelpers.GetFlightTime(airliner.CurrentPosition, airliner.CurrentFlight.Entry.Destination.Airport.Profile.Coordinates, airliner.Airliner.Airliner.Type);

            DateTime time = new DateTime(GameObject.GetInstance().GameTime.Year, GameObject.GetInstance().GameTime.Month, GameObject.GetInstance().GameTime.Day, GameObject.GetInstance().GameTime.Hour, GameObject.GetInstance().GameTime.Minute, 0);

            return time.AddTicks(flightTime.Ticks);
        

        }
        //returns the total number of passengers
        public int getTotalPassengers()
        {
            int passengers = 0;

            foreach (FlightAirlinerClass aClass in this.Classes)
                passengers += aClass.Passengers;

            return passengers;
        }
        //returns the flight airliner class for a specific class type
        public FlightAirlinerClass getFlightAirlinerClass(AirlinerClass.ClassType type)
        {
            return this.Classes.Find((delegate(FlightAirlinerClass c) { return c.AirlinerClass.Type == type; }));
        }

    
    }
    //the class for an airliner class on a flight
    public class FlightAirlinerClass
    {
        public RouteAirlinerClass AirlinerClass { get; set; }
        public int Passengers { get; set; }
        public FlightAirlinerClass(RouteAirlinerClass aClass, int passengers)
        {
            this.AirlinerClass = aClass;
            this.Passengers = passengers;
        }
    }
}
