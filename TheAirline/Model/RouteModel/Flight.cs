using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using TheAirline.Model.AirlinerModel;
using TheAirline.Model.AirportModel;
using TheAirline.Model.GeneralModel;
using TheAirline.Model.GeneralModel.Helpers;

namespace TheAirline.Model.RouteModel
{
    /*! Flight.
  * This is used for a actually flight.
  * The class needs parameter for the time table entry which the flight flights after
  */

    [Serializable]
    public class Flight : BaseModel
    {
        #region Constructors and Destructors

        public Flight(RouteTimeTableEntry entry)
        {
            Entry = entry;

            if (Entry.TimeTable.Route.Type == Route.RouteType.Passenger
                || Entry.TimeTable.Route.Type == Route.RouteType.Mixed || Entry.TimeTable.Route.Type == Route.RouteType.Helicopter)
            {
                Classes = new List<FlightAirlinerClass>();

                if (Entry != null)
                {
                    Airliner = Entry.Airliner;
                    FlightTime = MathHelpers.ConvertEntryToDate(Entry);
                    ScheduledFlightTime = FlightTime;
                }

                IsOnTime = true;
            }
            if (Entry.TimeTable.Route.Type == Route.RouteType.Cargo
                || Entry.TimeTable.Route.Type == Route.RouteType.Mixed)
            {
                if (Entry != null)
                {
                    Airliner = Entry.Airliner;
                    FlightTime = MathHelpers.ConvertEntryToDate(Entry);
                    ScheduledFlightTime = FlightTime;
                }

                IsOnTime = true;
            }

            DistanceToDestination =
                MathHelpers.GetDistance(
                    Entry.Destination.Airport.Profile.Coordinates.ConvertToGeoCoordinate(),
                    Entry.DepartureAirport.Profile.Coordinates.ConvertToGeoCoordinate());
        }

        protected Flight(SerializationInfo info, StreamingContext ctxt) : base(info, ctxt)
        {
        }

        #endregion

        #region Public Properties

        [Versioning("airliner")]
        public FleetAirliner Airliner { get; set; }

        [Versioning("cargo")]
        public double Cargo { get; set; }

        [Versioning("classes")]
        public List<FlightAirlinerClass> Classes { get; set; }

        [Versioning("distance")]
        public double DistanceToDestination { get; set; }

        [Versioning("entry")]
        public RouteTimeTableEntry Entry { get; set; }

        public DateTime ExpectedLanding
        {
            get { return GetExpectedLandingTime(); }
        }

        [Versioning("flighttime")]
        public DateTime FlightTime { get; set; }

        [Versioning("isontime")]
        public Boolean IsOnTime { get; set; }

        [Versioning("scheduled")]
        public DateTime ScheduledFlightTime { get; set; }

        #endregion

        #region Public Methods and Operators

        public override void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            info.AddValue("version", 1);

            base.GetObjectData(info, context);
        }

        //adds some delay minutes to the flight
        public virtual void AddDelayMinutes(int minutes)
        {
            FlightTime = FlightTime.AddMinutes(minutes);
        }

        //returns the scheduled expected landing time

        //returns the cargo price
        public double GetCargoPrice()
        {
            var route = Entry.TimeTable.Route as CargoRoute;
            if (route != null)
                return route.PricePerUnit;
            return ((CombiRoute) Entry.TimeTable.Route).PricePerUnit;
        }

        public Airport GetDepartureAirport()
        {
            return GetNextDestination();
        }

        public DateTime GetExpectedLandingTime()
        {
            return
                FlightTime.Add(
                    MathHelpers.GetFlightTime(
                        Entry.DepartureAirport.Profile.Coordinates.ConvertToGeoCoordinate(),
                        Entry.Destination.Airport.Profile.Coordinates.ConvertToGeoCoordinate(),
                        Airliner.Airliner.Type));
        }

        //returns the total number of passengers

        //returns the flight airliner class for a specific class type
        public FlightAirlinerClass GetFlightAirlinerClass(AirlinerClass.ClassType type)
        {
            return Classes.Find(c => c.AirlinerClass.Type == type);
        }

        //returns the next destination
        public Airport GetNextDestination()
        {
            return Entry.Destination.Airport == Entry.TimeTable.Route.Destination1
                       ? Entry.TimeTable.Route.Destination2
                       : Entry.TimeTable.Route.Destination1;
        }

        public DateTime GetScheduledLandingTime()
        {
            return
                ScheduledFlightTime.Add(
                    MathHelpers.GetFlightTime(
                        Entry.DepartureAirport.Profile.Coordinates.ConvertToGeoCoordinate(),
                        Entry.Destination.Airport.Profile.Coordinates.ConvertToGeoCoordinate(),
                        Airliner.Airliner.Type));
        }

        public int GetTotalPassengers()
        {
            int passengers = Classes.Sum(aClass => aClass.Passengers);

            if (passengers < 0)
            {
                passengers = 0;
            }

            return passengers;
        }

        //returns the departure location

        //returns if it is a cargo flight
        public Boolean IsCargoFlight()
        {
            return Entry.TimeTable.Route.Type == Route.RouteType.Mixed
                   || Entry.TimeTable.Route.Type == Route.RouteType.Cargo;
        }

        public Boolean IsPassengerFlight()
        {
            return Entry.TimeTable.Route.Type == Route.RouteType.Mixed
                   || Entry.TimeTable.Route.Type == Route.RouteType.Passenger
                   || Entry.TimeTable.Route.Type == Route.RouteType.Helicopter;
        }

        #endregion
    }
}