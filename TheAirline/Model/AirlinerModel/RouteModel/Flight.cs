using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using TheAirline.Model.GeneralModel;
using TheAirline.Model.AirportModel;
using ProtoBuf;

namespace TheAirline.Model.AirlinerModel.RouteModel
{
    /*! Flight.
  * This is used for a actually flight.
  * The class needs parameter for the time table entry which the flight flights after
  */
    [ProtoContract]
    [ProtoInclude(100,typeof(StopoverFlight))]
    public class Flight
    {
        [ProtoMember(1)]
        public RouteTimeTableEntry Entry { get; set; }
        [ProtoMember(2)]
        public List<FlightAirlinerClass> Classes { get; set; }
        [ProtoMember(3)]
        public FleetAirliner Airliner { get; set; }
        [ProtoMember(4)]
        public Boolean IsOnTime { get; set; }
        [ProtoMember(5)]
        public DateTime FlightTime { get; set; }
        [ProtoMember(6)]
        public DateTime ScheduledFlightTime { get; set; }
        public DateTime ExpectedLanding { get { return getExpectedLandingTime(); } set { ;} }
        [ProtoMember(7)]
        public double Cargo { get; set; }
        public Flight(RouteTimeTableEntry entry)
        {
         
            
            this.Entry = entry;

            if (this.Entry.TimeTable.Route.Type == Route.RouteType.Passenger || this.Entry.TimeTable.Route.Type == Route.RouteType.Mixed)
            {
                this.Classes = new List<FlightAirlinerClass>();

                if (this.Entry != null)
                {
                    this.Airliner = this.Entry.Airliner;
                    this.FlightTime = MathHelpers.ConvertEntryToDate(this.Entry);
                    this.ScheduledFlightTime = this.FlightTime;
                }

                this.IsOnTime = true;
            }
            if (this.Entry.TimeTable.Route.Type == Route.RouteType.Cargo || this.Entry.TimeTable.Route.Type == Route.RouteType.Mixed)
            {

                if (this.Entry != null)
                {
                    this.Airliner = this.Entry.Airliner;
                    this.FlightTime = MathHelpers.ConvertEntryToDate(this.Entry);
                    this.ScheduledFlightTime = this.FlightTime;
                }

                this.IsOnTime = true;
            }
           
         
        }
        //adds some delay minutes to the flight
        public virtual void addDelayMinutes(int minutes)
        {
           this.FlightTime = this.FlightTime.AddMinutes(minutes);
        }
        //returns the scheduled expected landing time
        public DateTime getScheduledLandingTime()
        {
            return this.ScheduledFlightTime.Add(MathHelpers.GetFlightTime(this.Entry.DepartureAirport.Profile.Coordinates, this.Entry.Destination.Airport.Profile.Coordinates, this.Airliner.Airliner.Type));

        }
        //returns the expected time of landing
        public DateTime getExpectedLandingTime()
        {
            return this.FlightTime.Add(MathHelpers.GetFlightTime(this.Entry.DepartureAirport.Profile.Coordinates, this.Entry.Destination.Airport.Profile.Coordinates, this.Airliner.Airliner.Type));

        }
        //returns the cargo price
        public double getCargoPrice()
        {
            return ((CargoRoute)this.Entry.TimeTable.Route).PricePerUnit;
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
            return this.Classes.Find(c=>c.AirlinerClass.Type == type); 
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
        //returns if it is a passenger flight
        public Boolean isPassengerFlight()
        {
            return this.Entry.TimeTable.Route.Type == Route.RouteType.Mixed || this.Entry.TimeTable.Route.Type == Route.RouteType.Passenger;
        }
        //returns if it is a cargo flight
        public Boolean isCargoFlight()
        {
            return this.Entry.TimeTable.Route.Type == Route.RouteType.Mixed || this.Entry.TimeTable.Route.Type == Route.RouteType.Cargo;
  
        }

    }
}
