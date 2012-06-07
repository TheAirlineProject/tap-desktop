using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using TheAirline.Model.AirportModel;
using TheAirline.Model.GeneralModel;
using TheAirline.Model.AirlinerModel;

namespace TheAirline.Model.PassengerModel
{
    //the class for a passenger
    public class Passenger
    {
        public enum PassengerType { Business, Tourist, Other }
        public PassengerType CurrentType { get; set; }
        public PassengerType PrimaryType { get; set; }
        public string ID { get; set; }
        public Airport Destination { get; set; }
        public Airport HomeAirport { get; set; }
        public DateTime Updated { get; set; }
        public AirlinerClass.ClassType PreferedClass { get; set; }
        public List<Airport> Route { get; set; }
        public int Factor { get; set; } //how many passengers does the passenger count for
        public Passenger(string id, PassengerType type, Airport homeAirport, AirlinerClass.ClassType preferedClass)
        {
            this.ID = id;
            this.CurrentType = type;
            this.PrimaryType = type;
            this.HomeAirport = homeAirport;
            this.PreferedClass = preferedClass;
       
        }
        //returns the next destination on the route for the passenger
        public Airport getNextRouteDestination(Airport currentDestination)
        {
            int index = this.Route.IndexOf(currentDestination);

            if (index >= 0 && index < this.Route.Count - 1)
                return this.Route[index + 1];
            else
                return null;
        }
    }
    //the list of passengers 
    public class Passengers
    {
        private static Dictionary<string, Passenger> passengers = new Dictionary<string, Passenger>();
        //adds a passengers to the list
        public static void AddPassenger(Passenger passenger)
        {
           passengers.Add(passenger.ID,passenger);
        }
        //returns the list of passengers
        public static List<Passenger> GetPassengers()
        {
            return passengers.Values.ToList();
        }
        //clears the list of passengers
        public static void Clear() 
        {
            passengers.Clear();
        }
        //returns a passenger with an id
        public static Passenger GetPassenger(string id)
        {
            return passengers[id];
        }
    }
}
