using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using TheAirline.Model.GeneralModel;
using TheAirline.Model.AirportModel;

namespace TheAirline.Model.AirlinerModel.RouteModel
{
    /*! Flight.
  * This is used for a actually flight.
  * The class needs parameter for the time table entry which the flight flights after
  */
    public class Flight
    {
        public RouteTimeTableEntry Entry { get; set; }
        public List<FlightAirlinerClass> Classes { get; set; }
        public FleetAirliner Airliner { get; set; }
        public Boolean IsOnTime { get; set; }
        public DateTime FlightTime { get; set; }
        public DateTime ExpectedLanding { get { return getExpectedLandingTime(); } set { ;} }
        public Flight(RouteTimeTableEntry entry)
        {
         
            this.Entry = entry;
            this.Classes = new List<FlightAirlinerClass>();

            if (this.Entry != null)
            {
                this.Airliner = this.Entry.Airliner;
                this.FlightTime = MathHelpers.ConvertEntryToDate(this.Entry);
            }
            
            this.IsOnTime = true;

         
        }
        //returns the expected time of landing
        public DateTime getExpectedLandingTime()
        {
            return this.FlightTime.Add(MathHelpers.GetFlightTime(this.Entry.DepartureAirport.Profile.Coordinates, this.Entry.Destination.Airport.Profile.Coordinates, this.Airliner.Airliner.Type));


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
        //returns the next destination
        public Airport getNextDestination()
        {
            return this.Entry.Destination.Airport == this.Entry.TimeTable.Route.Destination1 ? this.Entry.TimeTable.Route.Destination2 : this.Entry.TimeTable.Route.Destination1;
        }
        //returns the departure location
        public Airport getDepartureAirport()
        {
            return getNextDestination();

        }


    }
}
