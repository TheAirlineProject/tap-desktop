using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using TheAirline.Model.GeneralModel;

namespace TheAirline.Model.AirlinerModel.RouteModel
{
    /*! Flight.
  * This is used for a actually flight.
  * The class needs parameter for the time table entry which the flight flights after
  */
    public class Flight
    {
        public Boolean Delayed { get; set; }
        public RouteTimeTableEntry Entry { get; set; }
        public List<FlightAirlinerClass> Classes { get; set; }
        public Flight(RouteTimeTableEntry entry)
        {

            this.Entry = entry;
            this.Delayed = false;
            this.Classes = new List<FlightAirlinerClass>();
  

        }
        //returns the expected time of landing
        public DateTime getExpectedLandingTime()
        {
            FleetAirliner airliner = this.Entry.TimeTable.Route.Airliner;
            double distance = MathHelpers.GetDistance(airliner.CurrentPosition, airliner.CurrentFlight.Entry.Destination.Airport.Profile.Coordinates);
            TimeSpan flightTime = MathHelpers.GetFlightTime(airliner.CurrentPosition, airliner.CurrentFlight.Entry.Destination.Airport.Profile.Coordinates, airliner.Airliner.Type);

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
}
